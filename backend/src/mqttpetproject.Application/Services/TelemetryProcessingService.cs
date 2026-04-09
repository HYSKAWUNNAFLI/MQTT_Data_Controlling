using System.Text.Json;
using Microsoft.Extensions.Logging;
using mqttpetproject.Application.Abstractions.Persistence;
using mqttpetproject.Application.Abstractions.Services;
using mqttpetproject.Application.Abstractions.Validation;
using mqttpetproject.Application.DTOs;
using mqttpetproject.Application.Exceptions;
using mqttpetproject.Domain.Entities;

namespace mqttpetproject.Application.Services;

public sealed class TelemetryProcessingService : ITelemetryProcessingService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    private readonly ITelemetryValidator _telemetryValidator;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly ILogger<TelemetryProcessingService> _logger;

    public TelemetryProcessingService(
        ITelemetryValidator telemetryValidator,
        ITelemetryRepository telemetryRepository,
        IIdGenerator idGenerator,
        ILogger<TelemetryProcessingService> logger)
    {
        _telemetryValidator = telemetryValidator;
        _telemetryRepository = telemetryRepository;
        _idGenerator = idGenerator;
        _logger = logger;
    }

    public async Task<ProcessingResultDto> ProcessAsync(string rawMessage, CancellationToken cancellationToken = default)
    {
        TelemetryEnvelopeDto? envelope;

        try
        {
            envelope = JsonSerializer.Deserialize<TelemetryEnvelopeDto>(rawMessage, SerializerOptions);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Failed to deserialize telemetry payload.");
            var dlqMessage = CreateDlqMessage(string.Empty, string.Empty, rawMessage, "Invalid JSON payload.", new[] { exception.Message });
            await TrySaveDlqAuditAsync(dlqMessage, cancellationToken);
            return ProcessingResultDto.RejectToDlq("Invalid JSON payload.", dlqMessage.Errors, dlqMessage);
        }

        if (envelope is null)
        {
            var dlqMessage = CreateDlqMessage(string.Empty, string.Empty, rawMessage, "Telemetry payload is empty.", new[] { "Payload could not be deserialized." });
            await TrySaveDlqAuditAsync(dlqMessage, cancellationToken);
            return ProcessingResultDto.RejectToDlq("Telemetry payload is empty.", dlqMessage.Errors, dlqMessage);
        }

        var validationResult = _telemetryValidator.Validate(envelope, rawMessage);
        if (!validationResult.IsValid || validationResult.Message is null)
        {
            var dlqMessage = CreateDlqMessage(
                envelope.MessageId ?? string.Empty,
                envelope.CorrelationId ?? string.Empty,
                rawMessage,
                "Telemetry validation failed.",
                validationResult.Errors);

            await TrySaveDlqAuditAsync(dlqMessage, cancellationToken);
            return ProcessingResultDto.RejectToDlq("Telemetry validation failed.", validationResult.Errors, dlqMessage);
        }

        var message = validationResult.Message;

        if (message.Simulate.ForceDbError)
        {
            _logger.LogWarning("Simulate.ForceDbError enabled for MessageId {MessageId}. Returning NackRequeue.", message.MessageId);
            return ProcessingResultDto.NackRequeue("Simulated transient database failure.", message);
        }

        if (message.Simulate.NoSave)
        {
            _logger.LogInformation("Simulate.NoSave enabled for MessageId {MessageId}. Skipping MongoDB save.", message.MessageId);
            return ProcessingResultDto.Ack(message, "Telemetry message acknowledged without persistence.", false);
        }

        try
        {
            await _telemetryRepository.SaveTelemetryAsync(message, cancellationToken);
            _logger.LogInformation("Telemetry message {MessageId} saved successfully.", message.MessageId);
            return ProcessingResultDto.Ack(message, "Telemetry message saved successfully.", true);
        }
        catch (TransientPersistenceException exception)
        {
            _logger.LogWarning(exception, "Transient persistence failure while saving MessageId {MessageId}.", message.MessageId);
            return ProcessingResultDto.NackRequeue("Transient persistence failure.", message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Non-transient processing failure while saving MessageId {MessageId}.", message.MessageId);

            var dlqMessage = CreateDlqMessage(
                message.MessageId,
                message.CorrelationId,
                rawMessage,
                "Non-transient persistence failure.",
                new[] { exception.Message });

            await TrySaveDlqAuditAsync(dlqMessage, cancellationToken);
            return ProcessingResultDto.RejectToDlq("Non-transient persistence failure.", dlqMessage.Errors, dlqMessage, message);
        }
    }

    private async Task TrySaveDlqAuditAsync(DlqMessageDto dlqMessage, CancellationToken cancellationToken)
    {
        try
        {
            await _telemetryRepository.SaveDlqAuditAsync(
                new DlqAuditMessage
                {
                    AuditId = _idGenerator.NewId(),
                    MessageId = dlqMessage.MessageId,
                    CorrelationId = dlqMessage.CorrelationId,
                    FailureReason = dlqMessage.FailureReason,
                    Errors = dlqMessage.Errors.ToList(),
                    RawPayload = dlqMessage.RawPayload
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to save DLQ audit record for MessageId {MessageId}.", dlqMessage.MessageId);
        }
    }

    private static DlqMessageDto CreateDlqMessage(
        string messageId,
        string correlationId,
        string rawPayload,
        string failureReason,
        IEnumerable<string> errors)
    {
        return new DlqMessageDto
        {
            MessageId = messageId,
            CorrelationId = correlationId,
            FailureReason = failureReason,
            Errors = errors.Distinct().ToArray(),
            RawPayload = rawPayload
        };
    }
}
