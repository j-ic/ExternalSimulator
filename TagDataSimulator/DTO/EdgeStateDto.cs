
using System.Text.Json.Serialization;

namespace TagDataSimulator.DTO;

public readonly record struct EdgeStateDto
{
    [JsonPropertyName("EDGE_ID")]
    public string EdgeId { get; init; }
    [JsonPropertyName("STATE")]
    public string State { get; init; }
    [JsonPropertyName("DATE_TIME")]
    public string DateTime { get; init; }
}
