using mqttpetproject.Domain.Enums;

namespace mqttpetproject.Domain.Entities;

public sealed class DlqAuditMessage
{
    public string AuditId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public MessageProcessingStatus ProcessingStatus { get; set; } = MessageProcessingStatus.RejectToDlq;
    public string FailureReason { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public string RawPayload { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
