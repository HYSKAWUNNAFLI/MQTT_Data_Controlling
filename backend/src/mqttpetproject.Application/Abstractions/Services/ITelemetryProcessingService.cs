using mqttpetproject.Application.DTOs;

namespace mqttpetproject.Application.Abstractions.Services;

public interface ITelemetryProcessingService
{
    Task<ProcessingResultDto> ProcessAsync(string rawMessage, CancellationToken cancellationToken = default);
}
