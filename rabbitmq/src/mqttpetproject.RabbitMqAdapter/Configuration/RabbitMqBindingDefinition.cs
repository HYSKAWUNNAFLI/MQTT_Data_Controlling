namespace mqttpetproject.RabbitMqAdapter.Configuration;

public sealed class RabbitMqBindingDefinition
{
    public string Exchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
}
