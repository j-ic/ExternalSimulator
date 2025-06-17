namespace MessagingContracts.Redis;

public record RedisTestDto(
    int Temperature,
    int Humidity,
    float Vibration,
    float Pressure,
    float MotorCurrent,
    int MotorVoltage,
    int SpindleSpeed,
    int FeedRate,
    int OilLevel,
    float CoolantTemp,
    int ProductCount,
    string ErrorCode,
    bool IsRunning,
    bool DoorOpen,
    bool OverheatWarning,
    bool Hehe);
