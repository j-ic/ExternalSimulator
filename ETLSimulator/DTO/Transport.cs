using System.Text.Json.Serialization;

namespace ETLSimulator.DTO;
public readonly record struct Transport
{
    [JsonPropertyName("JOB_ID")]
    public string? JobId { get; init; }
    [JsonPropertyName("REQ_TIME")]
    public DateTime? ReqTime { get; init; }
    [JsonPropertyName("MAIN_CARR_ID")]
    public string? MainCarrId { get; init; }
    [JsonPropertyName("MOVE_PRIRT_NO")]
    public string? MovePrintNo { get; init; }
    [JsonPropertyName("REQ_SYS_NAME")]
    public string? ReqSysName { get; init; }
    [JsonPropertyName("FROM_EQP_ID")]
    public string? FromEqpId { get; init; }
    [JsonPropertyName("FROM_PORT_ID")]
    public string? FromPortId { get; init; }
    [JsonPropertyName("FROM_RACK_ID")]
    public string? FromRackId { get; init; }
    [JsonPropertyName("TO_EQP_ID")]
    public string? ToEqpId { get; init; }
    [JsonPropertyName("TO_PORT_ID")]
    public string? ToPortId { get; init; }
    [JsonPropertyName("TO_RACK_ID")]
    public string? ToRackId { get; init; }
    [JsonPropertyName("CUR_EQP_ID")]
    public string? CurEqpId { get; init; }
    [JsonPropertyName("CUR_PORT_ID")]
    public string? CurPortId { get; init; }
    [JsonPropertyName("CUR_RACK_ID")]
    public string? CurRackId { get; init; }
    [JsonPropertyName("MOVE_STS")]
    public string? MoveSts { get; init; }
    [JsonPropertyName("CREATE_TIME")]
    public DateTime? CreateTime { get; init; }
    [JsonPropertyName("TIMESTAMP")]
    public DateTime? TimeStamp { get; init; }
    [JsonPropertyName("CREATE_USER_ID")]
    public string? CreateUserId{ get; init; }
    [JsonPropertyName("UPDATE_USER_ID")]
    public string? UpdateUserId { get; init; }
}
