using mqttpetproject.Domain.Enums;

namespace mqttpetproject.Domain.Entities;

public sealed class GatewayData
{
    public string Topic { get; set; } = string.Empty;
    public TopicType TopicType { get; set; } = TopicType.Unknown;
    public List<DeviceData> Data_Devices { get; set; } = new();
}
