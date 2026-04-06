using mqttpetproject.Application.DTOs;
using mqttpetproject.Application.Validation;

namespace mqttpetproject.Application.Abstractions.Validation;

public interface ITelemetryValidator
{
    ValidationResult Validate(TelemetryEnvelopeDto envelope, string rawPayload);
}
