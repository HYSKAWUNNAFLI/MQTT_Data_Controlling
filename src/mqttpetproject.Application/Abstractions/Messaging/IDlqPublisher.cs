using mqttpetproject.Application.DTOs;

namespace mqttpetproject.Application.Abstractions.Messaging;

public interface IDlqPublisher
{
    Task PublishAsync(DlqMessageDto message, CancellationToken cancellationToken = default);
}
