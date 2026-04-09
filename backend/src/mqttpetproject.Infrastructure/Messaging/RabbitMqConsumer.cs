using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using mqttpetproject.Application.Abstractions.Messaging;
using mqttpetproject.Application.Abstractions.Services;
using mqttpetproject.Domain.Enums;
using mqttpetproject.Infrastructure.Configuration;

namespace mqttpetproject.Infrastructure.Messaging;

public sealed class RabbitMqConsumer : IMessageConsumer
{
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
        using var channel = _connectionFactory.CreateModel();

        channel.BasicQos(0, _options.PrefetchCount, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, eventArgs) => await HandleMessageAsync(channel, eventArgs, cancellationToken);

        var consumerTag = channel.BasicConsume(_options.MainQueue, autoAck: false, consumer: consumer);
        _logger.LogInformation("Started consuming RabbitMQ queue {Queue} with prefetch {PrefetchCount}.", _options.MainQueue, _options.PrefetchCount);

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
            if (channel.IsOpen)
            {
                channel.BasicCancel(consumerTag);
            }

            _logger.LogInformation("RabbitMQ consumer stopped.");
        }
    }

    private async Task HandleMessageAsync(IModel channel, BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
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
                    channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
                    break;
            }

            _logger.LogInformation(
                "Processed delivery tag {DeliveryTag} with status {Status}. Reason: {Reason}",
                eventArgs.DeliveryTag,
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
            _logger.LogError(exception, "Unexpected failure while consuming RabbitMQ delivery tag {DeliveryTag}.", eventArgs.DeliveryTag);

            if (channel.IsOpen)
            {
                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
        }
    }
}
