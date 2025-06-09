using MessagingContracts.ETL;
using System.Text.Json;
using System.Timers;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MessagingContracts.Redis;

namespace ETLSimulator.Controller;

public class IntegrationController
{
    #region Constructor

    /// <summary>
    /// Initializes a managed MQTT client and event handlers
    /// </summary>
    public IntegrationController(IManagedMqttClient managedMqttClient)
    {
        _managedMqttClient = managedMqttClient;

        // Event handler for connection state changed
        _managedMqttClient.ConnectionStateChangedAsync += async args =>
        {
            Console.WriteLine("Integration Client Connection state changed");
            await Task.CompletedTask;
        };

        // Event handler for successful connection
        _managedMqttClient.ConnectedAsync += async args =>
        {
            Console.WriteLine($"Integration Client Connected: {args.ConnectResult.ResultCode}");
            await Task.CompletedTask;
        };

        // Event handler for disconnection
        _managedMqttClient.DisconnectedAsync += async args =>
        {
            Console.WriteLine($"Integration Client Disconnected: {args.Reason}");
            await Task.CompletedTask;
        };

        // Timer for printing message count
        _timer = new System.Timers.Timer(TimeSpan.FromMinutes(1));
        _timer.Elapsed += PrintMessageCount;
        _timer.Start();
    }

    #endregion

    /// <summary>
    /// Send AGV data to MQTT topic in a loop
    /// </summary>
    /// <param name="topic"> MQTT topic to publish AGV data </param>
    /// <param name="milliseconds"> Delay in milliseconds between each message </param>
    /// <param name="maxCount"> Maximum number of AGV data to send </param>
    public async Task SendAGVLoop(string topic, uint milliseconds, int maxCount)
    {
        // Convert milliseconds to int
        int millisecondInt = (int)milliseconds;

        // Loop to send AGV data
        while (true)
        {
            // Create AGV list
            Dictionary<string, List<AGV>> agvList
                = await Task.Run(() => CreateAGVList(maxCount));

            // Serialize AGV list to JSON string
            string agvPayload = JsonSerializer.Serialize(agvList);

            // Calculate payload size
            int payloadSize 
                = System.Text.Encoding.UTF8.GetByteCount(agvPayload);

            // Log payload size
            Interlocked.Add(ref _totalBytesSent, payloadSize);

            // Publish AGV data to MQTT
            await PublishMqtt(topic, agvPayload);

            // Increment message count
            Interlocked.Increment(ref _messageCount);

            // Delay for specified milliseconds
            await Task.Delay(millisecondInt);
        }
    }

    /// <summary>
    /// Send transport data to MQTT topic in a loop
    /// </summary>
    /// <param name="topic"> MQTT topic to publish transport data </param>
    /// <param name="milliseconds"> Delay in milliseconds between each message </param>
    /// <param name="maxCount"> Maximum number of transport data to send </param>
    public async Task SendTransportLoop(string topic, uint milliseconds, int maxCount)
    {
        // Convert milliseconds to int
        int millisecondInt = (int)milliseconds;

        // Loop to send transport data
        while (true)
        {
            // Create transport list
            Dictionary<string, List<Transport>> transportList
                = await Task.Run(() => CreateTransportList(maxCount));

            // Serialize transport list to JSON string
            string payloadString = JsonSerializer.Serialize(transportList);

            // Calculate payload size
            int payloadSize
                = System.Text.Encoding.UTF8.GetByteCount(payloadString);

            // Log payload size
            Interlocked.Add(ref _totalBytesSent, payloadSize);

            // Publish transport data to MQTT
            await PublishMqtt(topic, payloadString);

            // Increment message count
            Interlocked.Increment(ref _messageCount);

            // Delay for specified milliseconds
            await Task.Delay(millisecondInt);
        }
    }

    public async Task SendRedisTestLoop(string topic, uint milliseconds, int maxCount)
    {
        // Convert milliseconds to int
        int millisecondInt = (int)milliseconds;

        while (true)
        {
            var redisDto = new RedisTestDto(
                Temperature: Random.Shared.Next(0, 666),
                Humidity: Random.Shared.Next(0, 100),
                Vibration: Random.Shared.NextSingle() * 100f,
                Pressure: Random.Shared.NextSingle() * 100f,
                MotorCurrent: Random.Shared.Next(0, 100),
                MotorVoltage: Random.Shared.Next(0, 100),
                SpindleSpeed: Random.Shared.Next(0, 100),
                FeedRate: Random.Shared.Next(0, 100),
                OilLevel: Random.Shared.Next(0, 100),
                CoolantTemperature: Random.Shared.NextSingle() * 100f,
                ProductCount: Random.Shared.Next(0, 1000),
                ErrorCode: "E" + Random.Shared.Next(1000, 9999).ToString(),
                IsRunning: Random.Shared.Next(0, 2) == 1,
                DoorOpen: Random.Shared.Next(0, 2) == 1,
                OverHeatingWarning: Random.Shared.Next(0, 2) == 1,
                Hehe: Random.Shared.Next(0, 2) == 1);

            string payloadString = JsonSerializer.Serialize(redisDto);

            int lineNumber = Random.Shared.Next(1, 21);
            int machineNumber = Random.Shared.Next(1, 3126);
            string randomTopic
                = $"{topic}/Line{lineNumber:D2}/{machineNumber}";

            // Calculate payload size
            int payloadSize 
                = System.Text.Encoding.UTF8.GetByteCount(payloadString);
            // Log payload size
            Interlocked.Add(ref _totalBytesSent, payloadSize);

            await PublishMqtt(randomTopic, payloadString);

            Interlocked.Increment(ref _messageCount);

            await Task.Delay(millisecondInt);
        }
    }

    #region Private Methods

    private static Dictionary<string, List<Transport>> CreateTransportList(int count)
    {
        // Create transport list
        List<Transport> transportList = [];

        // Main carrier ID array
        string[] mainCarrIdArray = [
            "2F38678", "2F49727", "3F84077", "3F16198", "2F27631", "3F48241",
            "3F40768", "2F13414", "3F54193", "2F49674", "3F00689", "3F11802",
            "2F41125", "3F94525", "3F53365", "3F91658", "2F84097", "2F66722",
            "2F19687", "2F18741", "2F62227", "2F39188", "2F20734", "2F79661",
            "2F90372", "3F79847", "2F08989", "2F38216", "3F62528", "3F90339"
        ];

        // Equipment ID array
        string[] eqpIdArray = [
            "HFB09ICS0600", "HFF09AGN0200", "HFF09AGN0200", "HFF11AGN0400", "HFF11CNV0500",
            "HFF09AGM0100", "HFF09AGN0200", "HFF11AGC0100", "HFF11AGN0100", "HFF09AGM0200",
            "HFB09ICS0100", "HFF09AGC0200", "HFF09AGC0300", "HFF11AGN0600", "HFF09AGN0500",
            "HFF09AGN0700", "HFF11AGN0200", "HFF09ICS0800", "HFF11AGN0300", "HFF09AGN0900",
            "HFF09AGN0100", "HFB09ICS0700", "HFF09AGC0100", "HFF11CNV0100", "HFF09AGN0300",
            "HFF09AGN0400", "HFF09ICS0500", "HFB09ICS0300", "HFF09AGN0600", "HFF09AGN0700",
            "HFF11AGN0500", "HFF11CNV0200", "HFF09AGC0400", "HFF11AGN0200", "HFF09ICS0900",
            "HFF11AGC0200", "HFB09ICS0200", "HFF09AGN0800", "HFF09AGN0900", "HFF09AGC0300",
            "HFF09AGN0500", "HFF09ICS0400", "HFF11AGN0600", "HFF11CNV0300", "HFF11AGN0300",
            "HFF09AGC0500", "HFF11AGC0100", "HFB09ICS0500", "HFF09AGN0200", "HFF09AGN0600"
        ];

        // System name array
        string[] sysNameArray = ["TC", "ADS", "MCS",];

        // Move status array
        string[] moveStsArray = ["MOVING", "COMPLETE", "RECEIVE"];

        // Loop to create transport data
        for (int i = 0; i < count; i++)
        {
            // Randomly select main carrier ID
            string mainCarrId = mainCarrIdArray[Random.Shared.Next(0, mainCarrIdArray.Length - 1)];

            // Randomly select equipment ID
            string fromEqpId = eqpIdArray[Random.Shared.Next(0, eqpIdArray.Length - 1)];

            // Randomly select equipment ID
            string toEqpId = eqpIdArray[Random.Shared.Next(0, eqpIdArray.Length - 1)];

            // Randomly select equipment ID
            string currEqpId = eqpIdArray[Random.Shared.Next(0, eqpIdArray.Length - 1)];

            // Create transport data
            var transportDto = new Transport
            {
                JobId = mainCarrId + "_" + Random.Shared.Next(0, 1000000).ToString(),
                ReqTime = DateTime.Now,
                MainCarrId = mainCarrId,
                MovePrintNo = Random.Shared.Next(0, 100).ToString(),
                ReqSysName = sysNameArray[Random.Shared.Next(0, sysNameArray.Length - 1)],
                FromEqpId = fromEqpId,
                FromPortId = fromEqpId + "_" + Random.Shared.Next(0, 100).ToString(),
                FromRackId = fromEqpId + "_" + Random.Shared.Next(0, 100).ToString(),
                ToEqpId = toEqpId,
                ToPortId = toEqpId + "_" + Random.Shared.Next(0, 100).ToString(),
                ToRackId = toEqpId + "_" + Random.Shared.Next(0, 100).ToString(),
                CurEqpId = currEqpId,
                CurPortId = currEqpId + "_" + Random.Shared.Next(0, 100).ToString(),
                CurRackId = currEqpId + "_" + Random.Shared.Next(0, 100).ToString(),
                MoveSts = moveStsArray[Random.Shared.Next(0, moveStsArray.Length - 1)],
                CreateTime = DateTime.Now,
                TimeStamp = DateTime.Now,
                CreateUserId = sysNameArray[Random.Shared.Next(0, sysNameArray.Length - 1)],
                UpdateUserId = sysNameArray[Random.Shared.Next(0, sysNameArray.Length - 1)]
            };

            // Add transport data to list
            transportList.Add(transportDto);
        }

        // Create dictionary with transport list
        Dictionary<string, List<Transport>> dict = new() 
        {
            ["TRANSPORT_JOB"] = transportList
        };

        return dict;
    }

    /// <summary>
    /// Create AGV list with random data
    /// </summary>
    /// <param name="count"> Size of AGV list </param>
    private static Dictionary<string, List<AGV>> CreateAGVList(int count)
    {
        // Create AGV list
        List<AGV> agvList = [];

        // AGV name array
        string[] vhlName = ["AGV_001", "AGV_002", "AGV_003", "AGV_004",
                            "AGV_005", "AGV_006", "AGV_007", "AGV_008"];

        // Loop to create AGV data
        for (int i = 0; i < count; i++)
        {
            // Create AGV data
            var agvDto = new AGV
            {
                VhlName = vhlName[Random.Shared.Next(0, vhlName.Length - 1)],
                X = Random.Shared.Next(0, 100),
                Y = Random.Shared.Next(0, 100),
                VhlState = "VhlState",
                Batt = Random.Shared.Next(0, 100),
                SubGoal = Random.Shared.Next(0, 100),
                FinalGoal = Random.Shared.Next(0, 100),
                SendTime = DateTime.Now,
                Degree = Random.Shared.Next(0, 100).ToString(),
            };

            // Add AGV data to list
            agvList.Add(agvDto);
        }

        // Create dictionary with AGV list
        Dictionary<string, List<AGV>> dict = new() { ["AGV"] = agvList };

        return dict;
    }

    /// <summary>
    /// publish MQTT message
    /// </summary>
    private async Task PublishMqtt(string topic, string payloadString)
    {
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payloadString)
            .WithRetainFlag()
            .Build();

        await _managedMqttClient.EnqueueAsync(message);
    }

    /// <summary>
    /// Callback function that prints the total number of messages published
    /// </summary>
    private void PrintMessageCount(object? sender, ElapsedEventArgs e)
    {
        // Atomically reset the counters and get their previous values.
        // This prevents race conditions and data loss.
        long lastMinuteTotalBytes
            = Interlocked.Exchange(ref _totalBytesSent, 0);
        int lastMinuteMessageCount
            = Interlocked.Exchange(ref _messageCount, 0);

        // Now, perform calculations with the captured values for the last minute.
        double totalMegaBytes = lastMinuteTotalBytes / (1024.0 * 1024.0);

        // Print stats for the last minute. (Using HH for 24-hour format)
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.WriteLine(
            $"[{timestamp}] Stats (Last 1 Min) -> " +
            $"Messages: {lastMinuteMessageCount}, " +
            $"Data Sent: {totalMegaBytes:F2} MB");
    }

    #endregion

    #region Private Fields

    private readonly IManagedMqttClient _managedMqttClient;
    private int _messageCount;
    private long _totalBytesSent;
    private readonly System.Timers.Timer _timer;

    #endregion
}
