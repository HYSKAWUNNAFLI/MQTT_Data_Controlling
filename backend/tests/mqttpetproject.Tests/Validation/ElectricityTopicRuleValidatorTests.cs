using mqttpetproject.Application.Validation.Rules;
using mqttpetproject.Tests.Support;

namespace mqttpetproject.Tests.Validation;

public sealed class ElectricityTopicRuleValidatorTests
{
    private readonly ElectricityTopicRuleValidator _validator = new();

    [Fact]
    public void Validate_ValidElectricityGateway_ShouldPass()
    {
        var gateway = TelemetrySamples.BuildElectricityGateway();

        var result = _validator.Validate(gateway);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FrequencyOutOfRange_ShouldFail()
    {
        var gateway = TelemetrySamples.BuildElectricityGateway(55.0);

        var result = _validator.Validate(gateway);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Frequency"));
    }
}
