namespace mqttpetproject.Application.Abstractions.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(string payload, string routingKey, CancellationToken cancellationToken = default);
}
