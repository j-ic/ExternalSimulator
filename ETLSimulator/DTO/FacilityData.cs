namespace ETLSimulator.DTO;

public readonly record struct FacilityData
{
    public string LineId { get; init; }
    public double Temperature { get; init; }
    public double Humidity { get; init; }
    public double LineSpeed { get; init; }
    // 설비 가동률
    public double UtilizationRate { get; init; }
    // 설비 생산성
    public double Productivity { get; init; }
}
