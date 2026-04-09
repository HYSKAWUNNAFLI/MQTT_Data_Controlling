using mqttpetproject.Domain.Constants;

namespace mqttpetproject.Infrastructure.Configuration;

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string MainExchange { get; set; } = RabbitMqConstants.MainExchange;
    public string Dlx { get; set; } = RabbitMqConstants.Dlx;
    public string MainQueue { get; set; } = RabbitMqConstants.MainQueue;
    public string Dlq { get; set; } = RabbitMqConstants.Dlq;
    public string MainRoutingKey { get; set; } = RabbitMqConstants.MainRoutingKey;
    public string DlqRoutingKey { get; set; } = RabbitMqConstants.DlqRoutingKey;
    public ushort PrefetchCount { get; set; } = 1;
}
