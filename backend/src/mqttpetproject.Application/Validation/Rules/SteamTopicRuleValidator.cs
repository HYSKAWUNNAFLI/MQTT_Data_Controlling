using mqttpetproject.Application.Abstractions.Validation;
using mqttpetproject.Application.Validation;
using mqttpetproject.Domain.Entities;
using mqttpetproject.Domain.Enums;

namespace mqttpetproject.Application.Validation.Rules;

public sealed class SteamTopicRuleValidator : ITopicRuleValidator
{
    public TopicType SupportedTopic => TopicType.Steam;

    public ValidationResult Validate(GatewayData gatewayData)
    {
        return FlowTopicRuleValidatorHelper.Validate(gatewayData, "Steam");
    }
}
