using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mqttpetproject.Application.Abstractions.Services;
using mqttpetproject.Domain.Enums;
using mqttpetproject.RabbitMqAdapter.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace mqttpetproject.RabbitMqAdapter.Messaging;

public sealed class RabbitMqConsumer
{
    private const string RetryCountHeaderName = "x-app-retry-count";

    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(
        RabbitMqConnectionFactory connectionFactory,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
    {
        var queuesToConsume = _options.Queues
            .Where(queue => !string.IsNullOrWhiteSpace(queue.Name) && queue.ConsumerCount > 0)
            .ToList();

        if (queuesToConsume.Count == 0)
        {
            _logger.LogWarning("No RabbitMQ queues are configured for consumption.");
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return;
        }

        var registrations = new List<ConsumerRegistration>();

        foreach (var queue in queuesToConsume)
        {
            for (var consumerIndex = 1; consumerIndex <= queue.ConsumerCount; consumerIndex++)
            {
                var channel = _connectionFactory.CreateModel();
                channel.BasicQos(0, queue.PrefetchCount, global: false);

                var capturedQueue = queue;
                var capturedConsumerIndex = consumerIndex;
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (_, eventArgs) =>
                    await HandleMessageAsync(channel, capturedQueue, capturedConsumerIndex, eventArgs, cancellationToken);

                var consumerTag = channel.BasicConsume(capturedQueue.Name, autoAck: false, consumer: consumer);
                registrations.Add(new ConsumerRegistration(channel, consumerTag, capturedQueue.Name, capturedConsumerIndex));

                _logger.LogInformation(
                    "Started RabbitMQ consumer {ConsumerIndex} on queue {Queue} with prefetch {PrefetchCount}.",
                    capturedConsumerIndex,
                    capturedQueue.Name,
                    capturedQueue.PrefetchCount);
            }
        }

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RabbitMQ consumer cancellation requested.");
        }
        finally
        {
            foreach (var registration in registrations)
            {
                try
                {
                    if (registration.Channel.IsOpen)
                    {
                        registration.Channel.BasicCancel(registration.ConsumerTag);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "Failed to cancel RabbitMQ consumer {ConsumerIndex} for queue {Queue}.",
                        registration.ConsumerIndex,
                        registration.QueueName);
                }
                finally
                {
                    registration.Channel.Dispose();
                }
            }

            _logger.LogInformation("RabbitMQ consumers stopped.");
        }
    }

    private async Task HandleMessageAsync(
        IModel channel,
        RabbitMqQueueDefinition queue,
        int consumerIndex,
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        var rawMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var processingService = scope.ServiceProvider.GetRequiredService<ITelemetryProcessingService>();
            var result = await processingService.ProcessAsync(rawMessage, cancellationToken);

            switch (result.Status)
            {
                case MessageProcessingStatus.Ack:
                    channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    break;

                case MessageProcessingStatus.RejectToDlq:
                    channel.BasicReject(eventArgs.DeliveryTag, requeue: false);
                    break;

                case MessageProcessingStatus.NackRequeue:
                    RetryOrRejectMessage(channel, queue, consumerIndex, eventArgs, result.Reason, exception: null);
                    break;
            }

            _logger.LogInformation(
                "Processed delivery tag {DeliveryTag} from exchange {Exchange} with routing key {RoutingKey} on queue {Queue} and consumer {ConsumerIndex} with status {Status}. Reason: {Reason}",
                eventArgs.DeliveryTag,
                eventArgs.Exchange,
                eventArgs.RoutingKey,
                queue.Name,
                consumerIndex,
                result.Status,
                result.Reason);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (channel.IsOpen)
            {
                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unexpected failure while consuming RabbitMQ delivery tag {DeliveryTag} from exchange {Exchange} with routing key {RoutingKey} on queue {Queue} and consumer {ConsumerIndex}.",
                eventArgs.DeliveryTag,
                eventArgs.Exchange,
                eventArgs.RoutingKey,
                queue.Name,
                consumerIndex);

            if (channel.IsOpen)
            {
                RetryOrRejectMessage(channel, queue, consumerIndex, eventArgs, "Unexpected consumer failure.", exception);
            }
        }
    }

    private void RetryOrRejectMessage(
        IModel channel,
        RabbitMqQueueDefinition queue,
        int consumerIndex,
        BasicDeliverEventArgs eventArgs,
        string reason,
        Exception? exception)
    {
        var currentRetryCount = GetRetryCount(eventArgs.BasicProperties);
        var retryLimit = Math.Max(queue.MaxRetryAttempts, 0);

        if (currentRetryCount >= retryLimit)
        {
            channel.BasicReject(eventArgs.DeliveryTag, requeue: false);

            _logger.LogWarning(
                exception,
                "Retry limit reached for delivery tag {DeliveryTag} from exchange {Exchange} with routing key {RoutingKey} on queue {Queue} and consumer {ConsumerIndex}. Rejecting to DLQ after {RetryAttempts} retry attempts. Reason: {Reason}",
                eventArgs.DeliveryTag,
                eventArgs.Exchange,
                eventArgs.RoutingKey,
                queue.Name,
                consumerIndex,
                currentRetryCount,
                reason);

            return;
        }

        try
        {
            var retryAttempt = currentRetryCount + 1;
            var properties = CreateRetryProperties(channel, eventArgs.BasicProperties, retryAttempt);

            channel.BasicPublish(
                exchange: eventArgs.Exchange,
                routingKey: eventArgs.RoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: eventArgs.Body);

            channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

            _logger.LogWarning(
                exception,
                "Republished delivery tag {DeliveryTag} from exchange {Exchange} with routing key {RoutingKey} on queue {Queue} and consumer {ConsumerIndex} for immediate retry {RetryAttempt}/{RetryLimit}. Reason: {Reason}",
                eventArgs.DeliveryTag,
                eventArgs.Exchange,
                eventArgs.RoutingKey,
                queue.Name,
                consumerIndex,
                retryAttempt,
                retryLimit,
                reason);
        }
        catch (Exception republishException)
        {
            _logger.LogError(
                republishException,
                "Failed to republish delivery tag {DeliveryTag} from exchange {Exchange} with routing key {RoutingKey} on queue {Queue} for retry. Rejecting to DLQ instead.",
                eventArgs.DeliveryTag,
                eventArgs.Exchange,
                eventArgs.RoutingKey,
                queue.Name);

            channel.BasicReject(eventArgs.DeliveryTag, requeue: false);
        }
    }

    private IBasicProperties CreateRetryProperties(IModel channel, IBasicProperties sourceProperties, int retryAttempt)
    {
        var properties = channel.CreateBasicProperties();

        properties.ContentType = sourceProperties.ContentType;
        properties.ContentEncoding = sourceProperties.ContentEncoding;
        properties.DeliveryMode = sourceProperties.DeliveryMode;
        properties.Priority = sourceProperties.Priority;
        properties.CorrelationId = sourceProperties.CorrelationId;
        properties.ReplyTo = sourceProperties.ReplyTo;
        properties.Expiration = sourceProperties.Expiration;
        properties.MessageId = sourceProperties.MessageId;
        properties.Timestamp = sourceProperties.Timestamp;
        properties.Type = sourceProperties.Type;
        properties.UserId = sourceProperties.UserId;
        properties.AppId = sourceProperties.AppId;
        properties.ClusterId = sourceProperties.ClusterId;

        var headers = CloneHeaders(sourceProperties.Headers);
        headers[RetryCountHeaderName] = retryAttempt;
        properties.Headers = headers;

        return properties;
    }

    private static Dictionary<string, object> CloneHeaders(IDictionary<string, object>? headers)
    {
        if (headers is null || headers.Count == 0)
        {
            return new Dictionary<string, object>(StringComparer.Ordinal);
        }

        return headers.ToDictionary(
            header => header.Key,
            header => header.Value,
            StringComparer.Ordinal);
    }

    private static int GetRetryCount(IBasicProperties? properties)
    {
        if (properties?.Headers is null || !properties.Headers.TryGetValue(RetryCountHeaderName, out var value) || value is null)
        {
            return 0;
        }

        return value switch
        {
            byte retryCount => retryCount,
            sbyte retryCount => retryCount,
            short retryCount => retryCount,
            ushort retryCount => retryCount,
            int retryCount => retryCount,
            long retryCount => retryCount > int.MaxValue ? int.MaxValue : (int)retryCount,
            byte[] retryCountBytes when int.TryParse(Encoding.UTF8.GetString(retryCountBytes), out var parsedRetryCount) => parsedRetryCount,
            string retryCountText when int.TryParse(retryCountText, out var parsedRetryCount) => parsedRetryCount,
            _ => 0
        };
    }

    private sealed record ConsumerRegistration(IModel Channel, string ConsumerTag, string QueueName, int ConsumerIndex);
}
