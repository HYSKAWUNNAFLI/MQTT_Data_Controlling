using mqttpetproject.Domain.Entities;

namespace mqttpetproject.Application.Validation.Rules;

internal static class FlowTopicRuleValidatorHelper
{
    private static readonly string[] RequiredFields = { "FlowRate", "Temperature", "Pressure" };

    public static ValidationResult Validate(GatewayData gatewayData, string topicName)
    {
        var errors = new List<string>();

        if (gatewayData.Data_Devices.Count == 0)
        {
            errors.Add($"{topicName} topic must contain at least one device.");
            return ValidationResult.Failure(errors);
        }

        foreach (var device in gatewayData.Data_Devices)
        {
            foreach (var key in device.Reading_Device.Keys)
            {
                if (key.Trim() != key)
                {
                    errors.Add($"{topicName} reading key '{key}' must not contain leading or trailing whitespace.");
                }
            }

            foreach (var field in RequiredFields)
            {
                if (!TryGetReadingValue(device, field, out _))
                {
                    errors.Add($"Device '{device.ID_Device}' on {topicName} topic must contain {field}.");
                }
            }

            if (TryGetReadingValue(device, "FlowRate", out var flowRate) && flowRate < 0)
            {
                errors.Add($"Device '{device.ID_Device}' has invalid FlowRate; value must be non-negative.");
            }

            if (TryGetReadingValue(device, "Pressure", out var pressure) && pressure < 0)
            {
                errors.Add($"Device '{device.ID_Device}' has invalid Pressure; value must be non-negative.");
            }
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    private static bool TryGetReadingValue(DeviceData device, string fieldName, out double value)
    {
        foreach (var reading in device.Reading_Device)
        {
            if (string.Equals(reading.Key, fieldName, StringComparison.OrdinalIgnoreCase))
            {
                value = reading.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
