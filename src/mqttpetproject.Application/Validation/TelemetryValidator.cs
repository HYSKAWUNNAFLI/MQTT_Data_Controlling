using System.Text.Json;
using mqttpetproject.Application.Abstractions.Validation;
using mqttpetproject.Application.DTOs;
using mqttpetproject.Domain.Entities;
using mqttpetproject.Domain.Enums;

namespace mqttpetproject.Application.Validation;

public sealed class TelemetryValidator : ITelemetryValidator
{
    private readonly IReadOnlyDictionary<TopicType, ITopicRuleValidator> _topicRuleValidators;

    public TelemetryValidator(IEnumerable<ITopicRuleValidator> topicRuleValidators)
    {
        _topicRuleValidators = topicRuleValidators.ToDictionary(validator => validator.SupportedTopic);
    }

    public ValidationResult Validate(TelemetryEnvelopeDto envelope, string rawPayload)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(envelope.MessageId))
        {
            errors.Add("MessageId is required.");
        }

        if (string.IsNullOrWhiteSpace(envelope.ID_Gateway))
        {
            errors.Add("ID_Gateway is required.");
        }

        var hasGatewayTimestamp = TryReadInt64(
            envelope.Timestamp_Gateway,
            "Timestamp_Gateway",
            errors,
            out var gatewayTimestamp);

        var gatewayData = ParseGatewayData(envelope.Data_Gateway, errors);

        if (errors.Count > 0 || !hasGatewayTimestamp)
        {
            return ValidationResult.Failure(errors);
        }

        var message = new TelemetryMessage
        {
            MessageId = envelope.MessageId!,
            CorrelationId = envelope.CorrelationId ?? string.Empty,
            Source = envelope.Source ?? string.Empty,
            SchemaVersion = envelope.SchemaVersion ?? string.Empty,
            MessageType = envelope.MessageType ?? string.Empty,
            ID_Gateway = envelope.ID_Gateway!,
            Timestamp_Gateway = gatewayTimestamp,
            Data_Gateway = gatewayData,
            Meta = envelope.Meta ?? new MetaInfo(),
            Simulate = envelope.Simulate ?? new Domain.ValueObjects.SimulateOptions(),
            ReceivedAtUtc = DateTime.UtcNow,
            RawPayload = rawPayload
        };

        foreach (var gateway in gatewayData)
        {
            if (!_topicRuleValidators.TryGetValue(gateway.TopicType, out var validator))
            {
                errors.Add($"Unsupported topic '{gateway.Topic}'.");
                continue;
            }

            var topicValidation = validator.Validate(gateway);
            if (!topicValidation.IsValid)
            {
                errors.AddRange(topicValidation.Errors);
            }
        }

        return errors.Count == 0
            ? ValidationResult.Success(message)
            : ValidationResult.Failure(errors);
    }

    private static List<GatewayData> ParseGatewayData(JsonElement gatewayDataElement, ICollection<string> errors)
    {
        var result = new List<GatewayData>();

        if (gatewayDataElement.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            errors.Add("Data_Gateway is required.");
            return result;
        }

        if (gatewayDataElement.ValueKind != JsonValueKind.Array)
        {
            errors.Add("Data_Gateway must be an array.");
            return result;
        }

        var index = 0;
        foreach (var gatewayItem in gatewayDataElement.EnumerateArray())
        {
            if (gatewayItem.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"Data_Gateway[{index}] must be an object.");
                index++;
                continue;
            }

            var topic = ReadRequiredString(gatewayItem, "Topic", $"Data_Gateway[{index}].Topic", errors);
            var devices = ParseDevices(gatewayItem, index, errors);

            result.Add(new GatewayData
            {
                Topic = topic ?? string.Empty,
                TopicType = ResolveTopicType(topic),
                Data_Devices = devices
            });

            index++;
        }

        if (result.Count == 0)
        {
            errors.Add("Data_Gateway must contain at least one topic entry.");
        }

        return result;
    }

    private static List<DeviceData> ParseDevices(JsonElement gatewayItem, int gatewayIndex, ICollection<string> errors)
    {
        var result = new List<DeviceData>();

        if (!gatewayItem.TryGetProperty("Data_Devices", out var devicesElement)
            || devicesElement.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            errors.Add($"Data_Gateway[{gatewayIndex}].Data_Devices is required.");
            return result;
        }

        if (devicesElement.ValueKind != JsonValueKind.Array)
        {
            errors.Add($"Data_Gateway[{gatewayIndex}].Data_Devices must be an array.");
            return result;
        }

        var deviceIndex = 0;
        foreach (var deviceElement in devicesElement.EnumerateArray())
        {
            if (deviceElement.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"Data_Gateway[{gatewayIndex}].Data_Devices[{deviceIndex}] must be an object.");
                deviceIndex++;
                continue;
            }

            var deviceId = ReadRequiredString(
                deviceElement,
                "ID_Device",
                $"Data_Gateway[{gatewayIndex}].Data_Devices[{deviceIndex}].ID_Device",
                errors);

            var deviceType = ReadRequiredString(
                deviceElement,
                "Type_Device",
                $"Data_Gateway[{gatewayIndex}].Data_Devices[{deviceIndex}].Type_Device",
                errors);

            long deviceTimestamp = 0;
            var hasDeviceTimestamp = deviceElement.TryGetProperty("Timestamp_Device", out var deviceTimestampElement);
            var hasValidTimestamp = hasDeviceTimestamp
                                    && TryReadInt64(
                                        deviceTimestampElement,
                                        $"Data_Gateway[{gatewayIndex}].Data_Devices[{deviceIndex}].Timestamp_Device",
                                        errors,
                                        out deviceTimestamp);

            if (!hasDeviceTimestamp)
            {
                errors.Add($"Data_Gateway[{gatewayIndex}].Data_Devices[{deviceIndex}].Timestamp_Device is required.");
            }

            var readings = ParseReadings(deviceElement, gatewayIndex, deviceIndex, errors);

            result.Add(new DeviceData
            {
                ID_Device = deviceId ?? string.Empty,
                Type_Device = deviceType ?? string.Empty,
                Timestamp_Device = hasValidTimestamp ? deviceTimestamp : 0,
                Reading_Device = readings
            });

            deviceIndex++;
        }

        if (result.Count == 0)
        {
            errors.Add($"Data_Gateway[{gatewayIndex}].Data_Devices must contain at least one device.");
        }

        return result;
    }

    private static Dictionary<string, double> ParseReadings(
        JsonElement deviceElement,
        int gatewayIndex,
        int deviceIndex,
        ICollection<string> errors)
    {
        var result = new Dictionary<string, double>(StringComparer.Ordinal);
        var prefix = $"Data_Gateway[{gatewayIndex}].Data_Devices[{deviceIndex}].Reading_Device";

        if (!deviceElement.TryGetProperty("Reading_Device", out var readingsElement)
            || readingsElement.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            errors.Add($"{prefix} is required.");
            return result;
        }

        if (readingsElement.ValueKind != JsonValueKind.Object)
        {
            errors.Add($"{prefix} must be an object.");
            return result;
        }

        foreach (var property in readingsElement.EnumerateObject())
        {
            if (property.Name.Trim() != property.Name)
            {
                errors.Add($"Reading key '{property.Name}' must not contain leading or trailing whitespace.");
            }

            if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetDouble(out var value))
            {
                errors.Add($"{prefix}.{property.Name} must be numeric.");
                continue;
            }

            result[property.Name] = value;
        }

        return result;
    }

    private static string? ReadRequiredString(
        JsonElement parent,
        string propertyName,
        string errorPath,
        ICollection<string> errors)
    {
        if (!parent.TryGetProperty(propertyName, out var propertyValue)
            || propertyValue.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            errors.Add($"{errorPath} is required.");
            return null;
        }

        if (propertyValue.ValueKind != JsonValueKind.String)
        {
            errors.Add($"{errorPath} must be a string.");
            return null;
        }

        var value = propertyValue.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{errorPath} is required.");
            return null;
        }

        return value;
    }

    private static bool TryReadInt64(
        JsonElement element,
        string errorPath,
        ICollection<string> errors,
        out long value)
    {
        if (element.ValueKind != JsonValueKind.Number || !element.TryGetInt64(out value))
        {
            errors.Add($"{errorPath} must be numeric.");
            value = default;
            return false;
        }

        return true;
    }

    private static TopicType ResolveTopicType(string? topic)
    {
        if (string.Equals(topic, "Electricity", StringComparison.OrdinalIgnoreCase))
        {
            return TopicType.Electricity;
        }

        if (string.Equals(topic, "Steam", StringComparison.OrdinalIgnoreCase))
        {
            return TopicType.Steam;
        }

        if (string.Equals(topic, "Gas", StringComparison.OrdinalIgnoreCase))
        {
            return TopicType.Gas;
        }

        return TopicType.Unknown;
    }
}
