namespace mqttpetproject.Application.DTOs;

public sealed class DlqMessageDto
{
    public string MessageId { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public string FailureReason { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();
    public string RawPayload { get; init; } = string.Empty;
}
