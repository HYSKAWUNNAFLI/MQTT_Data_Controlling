using mqttpetproject.Application.Abstractions.Validation;
using mqttpetproject.Domain.Entities;
using mqttpetproject.Domain.Enums;

namespace mqttpetproject.Application.Validation.Rules;

public sealed class ElectricityTopicRuleValidator : ITopicRuleValidator
{
    private static readonly string[] PositiveVoltageFields = { "VoltageAN", "VoltageBN", "VoltageCN" };

    public TopicType SupportedTopic => TopicType.Electricity;

    public ValidationResult Validate(GatewayData gatewayData)
    {
        var errors = new List<string>();

        if (gatewayData.Data_Devices.Count == 0)
        {
            errors.Add("Electricity topic must contain at least one device.");
            return ValidationResult.Failure(errors);
        }

        foreach (var device in gatewayData.Data_Devices)
        {
            foreach (var key in device.Reading_Device.Keys)
            {
                if (key.Trim() != key)
                {
                    errors.Add($"Electricity reading key '{key}' must not contain leading or trailing whitespace.");
                }
            }

            var hasElectricalField = device.Reading_Device.Keys.Any(IsElectricalField);
            if (!hasElectricalField)
            {
                errors.Add($"Device '{device.ID_Device}' on Electricity topic must contain at least one electrical reading.");
            }

            if (TryGetReadingValue(device, "Frequency", out var frequency) && (frequency < 49 || frequency > 51))
            {
                errors.Add($"Device '{device.ID_Device}' has Frequency outside the valid range 49-51.");
            }

            foreach (var field in PositiveVoltageFields)
            {
                if (TryGetReadingValue(device, field, out var voltage) && voltage <= 0)
                {
                    errors.Add($"Device '{device.ID_Device}' has invalid {field}; value must be greater than 0.");
                }
            }

            foreach (var reading in device.Reading_Device)
            {
                if (RequiresNonNegativeValue(reading.Key) && reading.Value < 0)
                {
                    errors.Add($"Device '{device.ID_Device}' has invalid {reading.Key}; value must be non-negative.");
                }
            }
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    private static bool IsElectricalField(string key)
    {
        return key.Equals("Frequency", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("Current", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("Voltage", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("Power", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("Energy", StringComparison.OrdinalIgnoreCase);
    }

    private static bool RequiresNonNegativeValue(string key)
    {
        return key.StartsWith("Current", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("Power", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("Energy", StringComparison.OrdinalIgnoreCase);
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
