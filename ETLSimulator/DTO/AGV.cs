using System;
using System.Text.Json.Serialization;

namespace ETLSimulator.DTO;

public readonly record struct AGV
{
    [JsonPropertyName("VHL_NAME")]
    public string? VhlName { get; init; }
    [JsonPropertyName("X")]
    public int? X { get; init; }
    [JsonPropertyName("Y")]
    public int? Y { get; init; }
    [JsonPropertyName("VHL_STATE")]
    public string? VhlState { get; init; }
    [JsonPropertyName("BATT")]
    public int? Batt { get; init; }

    [JsonPropertyName("SUB_GOAL")]
    public int? SubGoal { get; init; }
    [JsonPropertyName("FINAL_GOAL")]
    public int? FinalGoal { get; init; }
    [JsonPropertyName("TIMESTAMP")]
    public DateTime? SendTime { get; init; }

    [JsonPropertyName("DEGREE")]
    public string? Degree { get; init; }
}
