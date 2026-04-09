namespace mqttpetproject.RabbitMqAdapter.Configuration;

public sealed class RabbitMqOptions
{
    public const string ConfigurationSectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public List<RabbitMqExchangeDefinition> Exchanges { get; set; } = new();
    public List<RabbitMqExchangeBindingDefinition> ExchangeBindings { get; set; } = new();
    public List<RabbitMqQueueDefinition> Queues { get; set; } = new();
}
