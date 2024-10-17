using System.Text.Json.Serialization;

namespace ETLSimulator.DTO;

public readonly record struct Facility
{
    [JsonPropertyName("LINE_ID")]
    public string LineId { get; init; }
    [JsonPropertyName("TEMPERATURE")]
    public float Temperature { get; init; }
    [JsonPropertyName("HUMIDITY")]
    public float Humidity { get; init; }
    // 단위 m/s
    [JsonPropertyName("LINE_SPEED")]
    public float LineSpeed { get; init; }
    // 설비 가동률
    [JsonPropertyName("UTILIZATION_RATE")]
    public float UtilizationRate { get; init; }
    // 설비 생산성
    [JsonPropertyName("PRODUCTIVITY")]
    public float Productivity { get; init; }
    [JsonPropertyName("TIMESTAMP")]
    public DateTime Timestamp { get; init; }
}
