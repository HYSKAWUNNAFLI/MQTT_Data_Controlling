namespace mqttpetproject.RabbitMqAdapter.Configuration;

public sealed class RabbitMqQueueDefinition
{
    public string Name { get; set; } = string.Empty;
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; }
    public bool AutoDelete { get; set; }
    public string? DeadLetterExchange { get; set; }
    public string? DeadLetterRoutingKey { get; set; }
    public ushort PrefetchCount { get; set; } = 1;
    public int ConsumerCount { get; set; } = 1;
    public int MaxRetryAttempts { get; set; } = 3;
    public List<RabbitMqBindingDefinition> Bindings { get; set; } = new();
}
