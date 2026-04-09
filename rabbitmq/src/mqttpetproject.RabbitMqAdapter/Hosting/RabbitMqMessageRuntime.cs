using Microsoft.Extensions.Logging;
using mqttpetproject.Application.Abstractions.Messaging;
using mqttpetproject.RabbitMqAdapter.Messaging;

namespace mqttpetproject.RabbitMqAdapter.Hosting;

public sealed class RabbitMqMessageRuntime : IInboundMessageRuntime
{
    private readonly RabbitMqTopologyService _topologyService;
    private readonly RabbitMqConsumer _consumer;
    private readonly ILogger<RabbitMqMessageRuntime> _logger;

    public RabbitMqMessageRuntime(
        RabbitMqTopologyService topologyService,
        RabbitMqConsumer consumer,
        ILogger<RabbitMqMessageRuntime> logger)
    {
        _topologyService = topologyService;
        _consumer = consumer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("RabbitMQ runtime starting.");
        await _topologyService.EnsureTopologyAsync(cancellationToken);
        await _consumer.StartConsumingAsync(cancellationToken);
    }
}
