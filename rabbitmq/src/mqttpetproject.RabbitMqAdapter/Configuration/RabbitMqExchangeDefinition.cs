namespace mqttpetproject.RabbitMqAdapter.Configuration;

public sealed class RabbitMqExchangeDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "direct";
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; }
    public bool Declare { get; set; } = true;
}
