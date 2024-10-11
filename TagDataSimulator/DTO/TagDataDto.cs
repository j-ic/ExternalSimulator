using System;
using System.Text.Json.Serialization;

namespace TagDataSimulator.DTO;

public readonly struct EdgeMessageDto
{
    [JsonPropertyName("MessageId")]
    public string MessageId { get; init; }
    [JsonPropertyName("MessageType")]
    public string MessageType { get; init; }
    [JsonPropertyName("Messages")]
    public TagDataDto Messages { get; init; }
}

public readonly struct TagDataDto
{
    [JsonPropertyName("PublisherId")]
    public string EdgeId {  get; init; }
    [JsonPropertyName("Timestamp")]
    public DateTime Timestamp { get; init; }
    [JsonPropertyName("Payload")]
    public Tag[] Tags { get; init; }
};

public readonly struct Tag
{
    [JsonPropertyName("NodeIdentifier")]
    public string NodeIdentifier { get; init; }
    [JsonPropertyName("Value")]
    public object Value { get; init; }
    [JsonPropertyName("SourceTime")]
    public DateTime SourceTime { get; init; }
};
