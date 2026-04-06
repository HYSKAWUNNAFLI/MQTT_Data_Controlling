using mqttpetproject.Application.Abstractions.Validation;
using mqttpetproject.Application.Validation;
using mqttpetproject.Application.Validation.Rules;
using mqttpetproject.Tests.Support;

namespace mqttpetproject.Tests.Validation;

public sealed class TelemetryValidatorTests
{
    private readonly TelemetryValidator _validator;

    public TelemetryValidatorTests()
    {
        _validator = new TelemetryValidator(new ITopicRuleValidator[]
        {
            new ElectricityTopicRuleValidator(),
            new SteamTopicRuleValidator(),
            new GasTopicRuleValidator()
        });
    }

    [Fact]
    public void Validate_ValidElectricityPayload_ShouldPass()
    {
        var payload = TelemetrySamples.ValidElectricityJson();
        var envelope = TelemetrySamples.DeserializeEnvelope(payload);

        var result = _validator.Validate(envelope, payload);

        result.IsValid.Should().BeTrue();
        result.Message.Should().NotBeNull();
    }

    [Fact]
    public void Validate_MissingGatewayId_ShouldFail()
    {
        var payload = TelemetrySamples.MissingGatewayIdJson();
        var envelope = TelemetrySamples.DeserializeEnvelope(payload);

        var result = _validator.Validate(envelope, payload);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("ID_Gateway"));
    }

    [Fact]
    public void Validate_InvalidTimestampType_ShouldFail()
    {
        var payload = TelemetrySamples.InvalidTimestampTypeJson();
        var envelope = TelemetrySamples.DeserializeEnvelope(payload);

        var result = _validator.Validate(envelope, payload);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Timestamp_Gateway"));
    }

    [Fact]
    public void Validate_SteamMissingPressure_ShouldFail()
    {
        var payload = TelemetrySamples.SteamMissingPressureJson();
        var envelope = TelemetrySamples.DeserializeEnvelope(payload);

        var result = _validator.Validate(envelope, payload);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Pressure"));
    }
}
