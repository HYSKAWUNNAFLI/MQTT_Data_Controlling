using mqttpetproject.Domain.Entities;

namespace mqttpetproject.Application.Validation;

public sealed class ValidationResult
{
    private ValidationResult(bool isValid, IReadOnlyCollection<string> errors, TelemetryMessage? message)
    {
        IsValid = isValid;
        Errors = errors;
        Message = message;
    }

    public bool IsValid { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public TelemetryMessage? Message { get; }

    public static ValidationResult Success(TelemetryMessage? message = null)
    {
        return new ValidationResult(true, Array.Empty<string>(), message);
    }

    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        return new ValidationResult(false, errors.Distinct().ToArray(), null);
    }

    public static ValidationResult Failure(params string[] errors)
    {
        return Failure((IEnumerable<string>)errors);
    }
}
