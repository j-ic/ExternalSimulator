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
    float CoolantTemperature,
    int ProductCount,
    string ErrorCode,
    bool IsRunning,
    bool DoorOpen,
    bool OverHeatingWarning,
    bool Hehe);
