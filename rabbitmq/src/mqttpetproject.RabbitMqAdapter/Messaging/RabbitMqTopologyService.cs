using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using mqttpetproject.RabbitMqAdapter.Configuration;

namespace mqttpetproject.RabbitMqAdapter.Messaging;

public sealed class RabbitMqTopologyService
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

        var exchanges = _options.Exchanges
            .Where(exchange => !string.IsNullOrWhiteSpace(exchange.Name))
            .GroupBy(exchange => exchange.Name, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();

        var exchangeBindings = _options.ExchangeBindings
            .Where(binding =>
                !string.IsNullOrWhiteSpace(binding.SourceExchange)
                && !string.IsNullOrWhiteSpace(binding.DestinationExchange))
            .ToList();

        var queues = _options.Queues
            .Where(queue => !string.IsNullOrWhiteSpace(queue.Name))
            .GroupBy(queue => queue.Name, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();

        if (exchanges.Count == 0)
        {
            throw new InvalidOperationException("RabbitMQ topology requires at least one exchange.");
        }

        if (queues.Count == 0)
        {
            throw new InvalidOperationException("RabbitMQ topology requires at least one queue.");
        }

        foreach (var exchange in exchanges)
        {
            if (exchange.Declare)
            {
                channel.ExchangeDeclare(
                    exchange: exchange.Name,
                    type: exchange.Type,
                    durable: exchange.Durable,
                    autoDelete: exchange.AutoDelete);
            }
        }

        var exchangeNames = exchanges
            .Select(exchange => exchange.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var exchangeBinding in exchangeBindings)
        {
            if (!exchangeNames.Contains(exchangeBinding.SourceExchange))
            {
                throw new InvalidOperationException(
                    $"Exchange binding references unknown source exchange '{exchangeBinding.SourceExchange}'.");
            }

            if (!exchangeNames.Contains(exchangeBinding.DestinationExchange))
            {
                throw new InvalidOperationException(
                    $"Exchange binding references unknown destination exchange '{exchangeBinding.DestinationExchange}'.");
            }

            channel.ExchangeBind(
                destination: exchangeBinding.DestinationExchange,
                source: exchangeBinding.SourceExchange,
                routingKey: exchangeBinding.RoutingKey);
        }

        foreach (var queue in queues)
        {
            var arguments = BuildQueueArguments(queue);

            channel.QueueDeclare(
                queue: queue.Name,
                durable: queue.Durable,
                exclusive: queue.Exclusive,
                autoDelete: queue.AutoDelete,
                arguments: arguments);

            foreach (var binding in queue.Bindings)
            {
                if (!exchangeNames.Contains(binding.Exchange))
                {
                    throw new InvalidOperationException(
                        $"Queue '{queue.Name}' references unknown exchange '{binding.Exchange}'.");
                }

                channel.QueueBind(queue.Name, binding.Exchange, binding.RoutingKey);
            }
        }

        _logger.LogInformation(
            "RabbitMQ topology ensured with {ExchangeCount} exchanges, {ExchangeBindingCount} exchange bindings and {QueueCount} queues.",
            exchanges.Count,
            exchangeBindings.Count,
            queues.Count);

        return Task.CompletedTask;
    }

    private static IDictionary<string, object>? BuildQueueArguments(RabbitMqQueueDefinition queue)
    {
        var hasDeadLetterExchange = !string.IsNullOrWhiteSpace(queue.DeadLetterExchange);
        var hasDeadLetterRoutingKey = !string.IsNullOrWhiteSpace(queue.DeadLetterRoutingKey);

        if (!hasDeadLetterExchange && !hasDeadLetterRoutingKey)
        {
            return null;
        }

        var arguments = new Dictionary<string, object>(StringComparer.Ordinal);

        if (hasDeadLetterExchange)
        {
            arguments["x-dead-letter-exchange"] = queue.DeadLetterExchange!;
        }

        if (hasDeadLetterRoutingKey)
        {
            arguments["x-dead-letter-routing-key"] = queue.DeadLetterRoutingKey!;
        }

        return arguments;
    }
}
