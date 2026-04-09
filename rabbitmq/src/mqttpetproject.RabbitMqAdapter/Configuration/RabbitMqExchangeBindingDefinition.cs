namespace mqttpetproject.RabbitMqAdapter.Configuration;

public sealed class RabbitMqExchangeBindingDefinition
{
    public string SourceExchange { get; set; } = string.Empty;
    public string DestinationExchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
}
