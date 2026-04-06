namespace mqttpetproject.Application.Abstractions.Messaging;

public interface IMessageConsumer
{
    Task StartConsumingAsync(CancellationToken cancellationToken = default);
}
