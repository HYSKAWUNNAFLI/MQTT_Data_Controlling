using mqttpetproject.Application.Validation;
using mqttpetproject.Domain.Entities;
using mqttpetproject.Domain.Enums;

namespace mqttpetproject.Application.Abstractions.Validation;

public interface ITopicRuleValidator
{
    TopicType SupportedTopic { get; }
    ValidationResult Validate(GatewayData gatewayData);
}
