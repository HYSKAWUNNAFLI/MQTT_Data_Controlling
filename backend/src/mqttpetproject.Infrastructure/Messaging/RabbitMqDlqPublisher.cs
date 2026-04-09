using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using mqttpetproject.Application.Abstractions.Messaging;
using mqttpetproject.Application.DTOs;
using mqttpetproject.Infrastructure.Configuration;

namespace mqttpetproject.Infrastructure.Messaging;

public sealed class RabbitMqDlqPublisher : IDlqPublisher
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;

    public RabbitMqDlqPublisher(RabbitMqConnectionFactory connectionFactory, IOptions<RabbitMqOptions> options)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
    }

    public Task PublishAsync(DlqMessageDto message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var channel = _connectionFactory.CreateModel();

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(message);

        channel.BasicPublish(
            exchange: _options.Dlx,
            routingKey: _options.DlqRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(payload));

        return Task.CompletedTask;
    }
}
