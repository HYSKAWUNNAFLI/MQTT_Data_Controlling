using mqttpetproject.Application.Abstractions.Validation;
using mqttpetproject.Domain.Entities;
using mqttpetproject.Domain.Enums;

namespace mqttpetproject.Application.Validation.Rules;

public sealed class GasTopicRuleValidator : ITopicRuleValidator
{
    public TopicType SupportedTopic => TopicType.Gas;

    public ValidationResult Validate(GatewayData gatewayData)
    {
        return FlowTopicRuleValidatorHelper.Validate(gatewayData, "Gas");
    }
}
