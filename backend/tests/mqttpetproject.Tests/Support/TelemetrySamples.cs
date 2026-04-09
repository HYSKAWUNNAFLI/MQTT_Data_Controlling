using System.Text.Json;
using mqttpetproject.Application.DTOs;
using mqttpetproject.Domain.Entities;
using mqttpetproject.Domain.Enums;

namespace mqttpetproject.Tests.Support;

internal static class TelemetrySamples
{
    public static string ValidElectricityJson(bool noSave = false, bool forceDbError = false)
    {
        return $$"""
        {
          "MessageId": "msg-001",
          "CorrelationId": "corr-001",
          "Source": "NodeRed",
          "SchemaVersion": "1.0",
          "MessageType": "telemetry",
          "ID_Gateway": "GatewayFactory04",
          "Timestamp_Gateway": 1710000000,
          "Data_Gateway": [
            {
              "Topic": "Electricity",
              "Data_Devices": [
                {
                  "ID_Device": "PM02MSB",
                  "Type_Device": "PM2130",
                  "Timestamp_Device": 1710000000,
                  "Reading_Device": {
                    "CurrentA": 1000.5,
                    "VoltageAN": 220.1,
                    "Frequency": 50.0
                  }
                }
              ]
            }
          ],
          "Meta": {
            "IsTest": true,
            "TestCase": "VALID_SAVE_DB"
          },
          "Simulate": {
            "NoSave": {{(noSave ? "true" : "false")}},
            "ForceDbError": {{(forceDbError ? "true" : "false")}}
          }
        }
        """;
    }

    public static string MissingGatewayIdJson()
    {
        return """
        {
          "MessageId": "msg-002",
          "CorrelationId": "corr-002",
          "Source": "NodeRed",
          "SchemaVersion": "1.0",
          "MessageType": "telemetry",
          "ID_Gateway": "",
          "Timestamp_Gateway": 1710000000,
          "Data_Gateway": [
            {
              "Topic": "Electricity",
              "Data_Devices": [
                {
                  "ID_Device": "PM02MSB",
                  "Type_Device": "PM2130",
                  "Timestamp_Device": 1710000000,
                  "Reading_Device": {
                    "CurrentA": 1000.5
                  }
                }
              ]
            }
          ]
        }
        """;
    }

    public static string InvalidTimestampTypeJson()
    {
        return """
        {
          "MessageId": "msg-003",
          "CorrelationId": "corr-003",
          "Source": "NodeRed",
          "SchemaVersion": "1.0",
          "MessageType": "telemetry",
          "ID_Gateway": "GatewayFactory04",
          "Timestamp_Gateway": "1710000000",
          "Data_Gateway": [
            {
              "Topic": "Electricity",
              "Data_Devices": [
                {
                  "ID_Device": "PM02MSB",
                  "Type_Device": "PM2130",
                  "Timestamp_Device": 1710000000,
                  "Reading_Device": {
                    "CurrentA": 1000.5
                  }
                }
              ]
            }
          ]
        }
        """;
    }

    public static string SteamMissingPressureJson()
    {
        return """
        {
          "MessageId": "msg-004",
          "CorrelationId": "corr-004",
          "Source": "NodeRed",
          "SchemaVersion": "1.0",
          "MessageType": "telemetry",
          "ID_Gateway": "GatewayFactory04",
          "Timestamp_Gateway": 1710000000,
          "Data_Gateway": [
            {
              "Topic": "Steam",
              "Data_Devices": [
                {
                  "ID_Device": "Steam01",
                  "Type_Device": "SteamSensor",
                  "Timestamp_Device": 1710000000,
                  "Reading_Device": {
                    "FlowRate": 10.5,
                    "Temperature": 90.0
                  }
                }
              ]
            }
          ]
        }
        """;
    }

    public static TelemetryEnvelopeDto DeserializeEnvelope(string json)
    {
        return JsonSerializer.Deserialize<TelemetryEnvelopeDto>(json)!;
    }

    public static GatewayData BuildElectricityGateway(double frequency = 50.0)
    {
        return new GatewayData
        {
            Topic = "Electricity",
            TopicType = TopicType.Electricity,
            Data_Devices =
            {
                new DeviceData
                {
                    ID_Device = "PM02MSB",
                    Type_Device = "PM2130",
                    Timestamp_Device = 1710000000,
                    Reading_Device = new Dictionary<string, double>
                    {
                        ["CurrentA"] = 10.0,
                        ["VoltageAN"] = 220.0,
                        ["Frequency"] = frequency
                    }
                }
            }
        };
    }

    public static GatewayData BuildSteamGateway(bool includePressure)
    {
        var readings = new Dictionary<string, double>
        {
            ["FlowRate"] = 10.0,
            ["Temperature"] = 120.0
        };

        if (includePressure)
        {
            readings["Pressure"] = 2.0;
        }

        return new GatewayData
        {
            Topic = "Steam",
            TopicType = TopicType.Steam,
            Data_Devices =
            {
                new DeviceData
                {
                    ID_Device = "Steam01",
                    Type_Device = "SteamSensor",
                    Timestamp_Device = 1710000000,
                    Reading_Device = readings
                }
            }
        };
    }

    public static GatewayData BuildGasGateway(double flowRate)
    {
        return new GatewayData
        {
            Topic = "Gas",
            TopicType = TopicType.Gas,
            Data_Devices =
            {
                new DeviceData
                {
                    ID_Device = "Gas01",
                    Type_Device = "GasSensor",
                    Timestamp_Device = 1710000000,
                    Reading_Device = new Dictionary<string, double>
                    {
                        ["FlowRate"] = flowRate,
                        ["Temperature"] = 30.0,
                        ["Pressure"] = 1.5
                    }
                }
            }
        };
    }
}
