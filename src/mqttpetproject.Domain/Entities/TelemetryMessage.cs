using mqttpetproject.Domain.ValueObjects;

namespace mqttpetproject.Domain.Entities;

public sealed class TelemetryMessage
{
    public string MessageId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string ID_Gateway { get; set; } = string.Empty;
    public long Timestamp_Gateway { get; set; }
    public List<GatewayData> Data_Gateway { get; set; } = new();
    public MetaInfo Meta { get; set; } = new();
    public SimulateOptions Simulate { get; set; } = new();
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public string RawPayload { get; set; } = string.Empty;
}
