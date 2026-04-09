using System.Text.Json;
using mqttpetproject.Domain.Entities;
using mqttpetproject.Domain.ValueObjects;

namespace mqttpetproject.Application.DTOs;

public sealed class TelemetryEnvelopeDto
{
    public string? MessageId { get; init; }
    public string? CorrelationId { get; init; }
    public string? Source { get; init; }
    public string? SchemaVersion { get; init; }
    public string? MessageType { get; init; }
    public string? ID_Gateway { get; init; }
    public JsonElement Timestamp_Gateway { get; init; }
    public JsonElement Data_Gateway { get; init; }
    public MetaInfo? Meta { get; init; }
    public SimulateOptions? Simulate { get; init; }
}
