using mqttpetproject.Application.Validation.Rules;
using mqttpetproject.Tests.Support;

namespace mqttpetproject.Tests.Validation;

public sealed class SteamTopicRuleValidatorTests
{
    private readonly SteamTopicRuleValidator _validator = new();

    [Fact]
    public void Validate_MissingPressure_ShouldFail()
    {
        var gateway = TelemetrySamples.BuildSteamGateway(includePressure: false);

        var result = _validator.Validate(gateway);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Pressure"));
    }
}
