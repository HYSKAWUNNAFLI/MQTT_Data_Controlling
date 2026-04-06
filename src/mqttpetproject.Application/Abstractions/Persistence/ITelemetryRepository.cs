using mqttpetproject.Domain.Entities;

namespace mqttpetproject.Application.Abstractions.Persistence;

public interface ITelemetryRepository
{
    Task SaveTelemetryAsync(TelemetryMessage message, CancellationToken cancellationToken = default);
    Task SaveDlqAuditAsync(DlqAuditMessage message, CancellationToken cancellationToken = default);
}
