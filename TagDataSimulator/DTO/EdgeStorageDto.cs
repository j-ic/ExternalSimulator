using System.Text.Json.Serialization;

namespace TagDataSimulator.DTO;

public readonly record struct EdgeStorageDto
{
    [JsonPropertyName("EDGE_ID")]
    public string EdgeId { get; init; }
    [JsonPropertyName("OS_TYPE")]
    public string OsType { get; init; }
    [JsonPropertyName("STORAGES")]
    public StorageDto[] Storages { get; init; }
    [JsonPropertyName("DATE_TIME")]
    public string DateTime { get; init; }
}

public readonly record struct StorageDto
{
    [JsonPropertyName("DISK_NAME")]
    public string DiskName { get; init; }
    [JsonPropertyName("TOTAL_SIZE")]
    public string TotalSize { get; init; }
    [JsonPropertyName("USE_SIZE")]
    public string UseSize { get; init; }
    [JsonPropertyName("FREE_SIZE")]
    public string FreeSize { get; init; }
}