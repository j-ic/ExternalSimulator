using System.Text.Json.Serialization;

namespace TagDataSimulator.DTO;

public readonly record struct EdgeResourceDto
{
    [JsonPropertyName("EDGE_ID")]
    public string EdgeId { get; init; }
    [JsonPropertyName("IP")]
    public string Ip { get; init; }
    [JsonPropertyName("CPU")]
    public string CpuUseage { get; init; }
    [JsonPropertyName("RAM")]
    public string RamUseage { get; init; }
    [JsonPropertyName("NETWORK_RECV")]
    public string NetworkRecv { get; init; }
    [JsonPropertyName("NETWORK_SEND")]
    public string NetworkSend { get; init; }
    [JsonPropertyName("DATE_TIME")]
    public string DateTime { get; init; }
}
