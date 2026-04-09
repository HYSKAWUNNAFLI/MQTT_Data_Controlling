using mqttpetproject.Domain.Entities;
using mqttpetproject.Domain.Enums;

namespace mqttpetproject.Application.DTOs;

public sealed class ProcessingResultDto
{
    public MessageProcessingStatus Status { get; init; }
    public string Reason { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();
    public TelemetryMessage? Message { get; init; }
    public DlqMessageDto? DlqMessage { get; init; }
    public bool SavedToDatabase { get; init; }

    public static ProcessingResultDto Ack(
        TelemetryMessage? message,
        string reason,
        bool savedToDatabase)
    {
        return new ProcessingResultDto
        {
            Status = MessageProcessingStatus.Ack,
            Message = message,
            Reason = reason,
            SavedToDatabase = savedToDatabase
        };
    }

    public static ProcessingResultDto RejectToDlq(
        string reason,
        IEnumerable<string> errors,
        DlqMessageDto dlqMessage,
        TelemetryMessage? message = null)
    {
        return new ProcessingResultDto
        {
            Status = MessageProcessingStatus.RejectToDlq,
            Message = message,
            Reason = reason,
            Errors = errors.Distinct().ToArray(),
            DlqMessage = dlqMessage,
            SavedToDatabase = false
        };
    }

    public static ProcessingResultDto NackRequeue(
        string reason,
        TelemetryMessage? message = null)
    {
        return new ProcessingResultDto
        {
            Status = MessageProcessingStatus.NackRequeue,
            Message = message,
            Reason = reason,
            SavedToDatabase = false
        };
    }
}
