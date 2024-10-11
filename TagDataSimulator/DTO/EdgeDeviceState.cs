using System;
using System.Text.Json.Serialization;

namespace TagDataSimulator.DTO;

public struct EdgeDeviceState
{
    [JsonPropertyName("EDGE_ID")]
    public string EdgeId { get; init; }
    [JsonPropertyName("DEVICE_ID")]
    public string DeviceId { get; init; }
    [JsonPropertyName("STATE")]
    public string State { get; init; }
    [JsonPropertyName("DATE_TIME")]
    public string DateTime { get; init; }
}
