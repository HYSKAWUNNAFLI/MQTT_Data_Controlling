using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using mqttpetproject.Application.Abstractions.Messaging;
using mqttpetproject.Infrastructure.Configuration;

namespace mqttpetproject.Infrastructure.Messaging;

public sealed class RabbitMqTopologyService : IRabbitMqTopologyService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqTopologyService> _logger;

    public RabbitMqTopologyService(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqTopologyService> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
    }

    public Task EnsureTopologyAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var channel = _connectionFactory.CreateModel();

        channel.ExchangeDeclare(_options.MainExchange, ExchangeType.Direct, durable: true, autoDelete: false);
        channel.ExchangeDeclare(_options.Dlx, ExchangeType.Direct, durable: true, autoDelete: false);

        channel.QueueDeclare(
            queue: _options.MainQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = _options.Dlx,
                ["x-dead-letter-routing-key"] = _options.DlqRoutingKey
            });

        channel.QueueDeclare(
            queue: _options.Dlq,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.QueueBind(_options.MainQueue, _options.MainExchange, _options.MainRoutingKey);
        channel.QueueBind(_options.Dlq, _options.Dlx, _options.DlqRoutingKey);

        _logger.LogInformation(
            "RabbitMQ topology ensured. Main exchange: {MainExchange}, main queue: {MainQueue}, DLX: {Dlx}, DLQ: {Dlq}.",
            _options.MainExchange,
            _options.MainQueue,
            _options.Dlx,
            _options.Dlq);

        return Task.CompletedTask;
    }
}
