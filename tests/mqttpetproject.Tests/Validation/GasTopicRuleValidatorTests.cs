using mqttpetproject.Application.Validation.Rules;
using mqttpetproject.Tests.Support;

namespace mqttpetproject.Tests.Validation;

public sealed class GasTopicRuleValidatorTests
{
    private readonly GasTopicRuleValidator _validator = new();

    [Fact]
    public void Validate_NegativeFlowRate_ShouldFail()
    {
        var gateway = TelemetrySamples.BuildGasGateway(-1.0);

        var result = _validator.Validate(gateway);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("FlowRate"));
    }
}
