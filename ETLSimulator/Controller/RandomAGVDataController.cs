using MessagingContracts.ETL;
using System.Text.Json;
using System.Text.Json.Nodes;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using System.Timers;

namespace ETLSimulator.Controller;

public class RandomAGVDataController
{
    #region Constructor

    public RandomAGVDataController(IManagedMqttClient managedMqttClient)
    {
        _managedMqttClient = managedMqttClient;
        _managedMqttClient.ConnectionStateChangedAsync += async args =>
        {
            Console.WriteLine("AGV Client Connection state changed");
            await Task.CompletedTask;
        };
        _managedMqttClient.ConnectedAsync += async args =>
        {
            Console.WriteLine($"AGV Client Connected: {args.ConnectResult.ResultCode}");
            await Task.CompletedTask;
        };
        _managedMqttClient.DisconnectedAsync += async args =>
        {
            Console.WriteLine($"AGV Client Disconnected: {args.Reason}");
            await Task.CompletedTask;
        };

        _timer = new System.Timers.Timer(60000);
        _timer.Elapsed += PrintMessageCount;
    }

    #endregion

    #region Public Methods

    public async Task SendAGVLoop(string topic, string jobId, uint milliseconds, int maxCount)
    {
        _timer.Start();

        while (true)
        {
            var jobMessage = new 
            {
                jobId = jobId,
            };

            JsonNode? jobMessageNode
                = JsonNode.Parse(JsonSerializer.Serialize(jobMessage));
            if (jobMessageNode is not JsonObject jobMessageObject) { continue; }
            Dictionary<string, List<AGV>> agvList = new();

            int listCount = maxCount;
            agvList = await Task.Run(() => CreateAGVList(listCount));

            string payloadString = JsonSerializer.Serialize(agvList);
            SendMachineDto(topic, payloadString);
            agvList.Clear();
            Interlocked.Increment(ref _messageCount);

            await Task.Delay((int)milliseconds);
        }
    }

    #endregion

    #region Private Methods

    private void SendMachineDto(string topic, string payloadString)
    {
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payloadString)
            .WithRetainFlag()
            .Build();

        _managedMqttClient.EnqueueAsync(message);
    }

    
    private Dictionary<string, List<AGV>> CreateAGVList(int count)
    {
        List<AGV> agvList = [];
        string[] vhlName = ["AGV_001", "AGV_002", "AGV_003", "AGV_004",
                            "AGV_005", "AGV_006", "AGV_007", "AGV_008"];

        for (int i = 0; i < count; i++)
        {
            var agvDto = new AGV
            {
                VhlName = vhlName[Random.Shared.Next(0, vhlName.Length-1)],
                X = Random.Shared.Next(0, 100),
                Y = Random.Shared.Next(0, 100),
                VhlState = "VhlState",
                Batt = Random.Shared.Next(0, 100),
                SubGoal = Random.Shared.Next(0, 100),
                FinalGoal = Random.Shared.Next(0, 100),
                SendTime = DateTime.Now,
                Degree = Random.Shared.Next(0, 100).ToString(),
            };

            agvList.Add(agvDto);
        }

        Dictionary<string, List<AGV>> dict = new();
        dict["AGV"] = agvList;

        return dict;
    }

    private void PrintMessageCount(object? sender, ElapsedEventArgs e)
    {
        string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
        Console.WriteLine($"[{timestamp}] Published AGV Message Count : {_messageCount}");
        //Interlocked.Exchange(ref _messageCount, 0);
    }

    #endregion

    #region Private Fields

    private readonly IManagedMqttClient _managedMqttClient;
    private volatile int _messageCount;
    private System.Timers.Timer _timer;

    #endregion
}
