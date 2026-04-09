using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using mqttpetproject.Application.Abstractions.Messaging;
using mqttpetproject.Infrastructure.Configuration;

namespace mqttpetproject.Infrastructure.Messaging;

public sealed class RabbitMqPublisher : IMessagePublisher
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;

    public RabbitMqPublisher(RabbitMqConnectionFactory connectionFactory, IOptions<RabbitMqOptions> options)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
    }

    public Task PublishAsync(string payload, string routingKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var channel = _connectionFactory.CreateModel();

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        channel.BasicPublish(
            exchange: _options.MainExchange,
            routingKey: string.IsNullOrWhiteSpace(routingKey) ? _options.MainRoutingKey : routingKey,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(payload));

        return Task.CompletedTask;
    }
}
