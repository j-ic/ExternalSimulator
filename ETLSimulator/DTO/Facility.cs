using System.Text.Json.Serialization;

namespace ETLSimulator.DTO;

public readonly record struct Facility
{
    [JsonPropertyName("LINE_ID")]
    public string LineId { get; init; }
    [JsonPropertyName("TEMPERATURE")]
    public double Temperature { get; init; }
    [JsonPropertyName("HUMIDITY")]
    public double Humidity { get; init; }
    [JsonPropertyName("LINE_SPEED")]
    public double LineSpeed { get; init; }
    [JsonPropertyName("UTILIZATION_RATE")]
    // 설비 가동률
    public double UtilizationRate { get; init; }
    [JsonPropertyName("PRODUCTIVITY")]
    // 설비 생산성
    public double Productivity { get; init; }
    [JsonPropertyName("TIMESTAMP")]
    public DateTime Timestamp { get; init; }
}
