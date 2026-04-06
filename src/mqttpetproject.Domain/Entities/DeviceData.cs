namespace mqttpetproject.Domain.Entities;

public sealed class DeviceData
{
    public string ID_Device { get; set; } = string.Empty;
    public string Type_Device { get; set; } = string.Empty;
    public long Timestamp_Device { get; set; }
    public Dictionary<string, double> Reading_Device { get; set; } = new(StringComparer.Ordinal);
}
