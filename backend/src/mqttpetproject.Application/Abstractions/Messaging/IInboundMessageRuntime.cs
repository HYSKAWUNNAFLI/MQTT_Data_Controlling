namespace mqttpetproject.Application.Abstractions.Messaging;

public interface IInboundMessageRuntime
{
    Task StartAsync(CancellationToken cancellationToken = default);
}
