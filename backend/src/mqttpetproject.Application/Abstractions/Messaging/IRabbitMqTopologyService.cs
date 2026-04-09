namespace mqttpetproject.Application.Abstractions.Messaging;

public interface IRabbitMqTopologyService
{
    Task EnsureTopologyAsync(CancellationToken cancellationToken = default);
}
