using mqttpetproject.Application.Abstractions.Messaging;

namespace mqttpetproject.Api.BackgroundServices;

public sealed class RabbitMqConsumerBackgroundService : BackgroundService
{
    private readonly IRabbitMqTopologyService _topologyService;
    private readonly IMessageConsumer _messageConsumer;
    private readonly ILogger<RabbitMqConsumerBackgroundService> _logger;

    public RabbitMqConsumerBackgroundService(
        IRabbitMqTopologyService topologyService,
        IMessageConsumer messageConsumer,
        ILogger<RabbitMqConsumerBackgroundService> logger)
    {
        _topologyService = topologyService;
        _messageConsumer = messageConsumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ consumer background service starting.");

        await _topologyService.EnsureTopologyAsync(stoppingToken);
        await _messageConsumer.StartConsumingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ consumer background service stopping.");
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("RabbitMQ consumer background service stopped.");
    }
}
