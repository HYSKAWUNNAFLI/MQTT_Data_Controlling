using mqttpetproject.Application.Abstractions.Messaging;

namespace mqttpetproject.Api.BackgroundServices;

public sealed class InboundMessageRuntimeBackgroundService : BackgroundService
{
    private readonly IInboundMessageRuntime _messageRuntime;
    private readonly ILogger<InboundMessageRuntimeBackgroundService> _logger;

    public InboundMessageRuntimeBackgroundService(
        IInboundMessageRuntime messageRuntime,
        ILogger<InboundMessageRuntimeBackgroundService> logger)
    {
        _messageRuntime = messageRuntime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inbound message runtime starting.");

        await _messageRuntime.StartAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inbound message runtime stopping.");
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Inbound message runtime stopped.");
    }
}
