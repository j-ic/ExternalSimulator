using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using ETLSimulator.DTO;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;

namespace ETLSimulator.Controller;

public class RandomDataController
{
    #region Constructor

    public RandomDataController(IManagedMqttClient  managedMqttClient)
    {
        _managedMqttClient = managedMqttClient;
        _managedMqttClient.ConnectionStateChangedAsync += async args =>
        {
            Console.WriteLine("Connection state changed");
            await Task.CompletedTask;
        };
        _managedMqttClient.ConnectedAsync += async args =>
        {
            Console.WriteLine($"Connected: {args.ConnectResult.ResultCode}");
            await Task.CompletedTask;
        };
        _managedMqttClient.DisconnectedAsync += async args =>
        {
            Console.WriteLine($"Disconnected: {args.Reason}");
            await Task.CompletedTask;
        };
        _random = new Random();
    }

    #endregion

    #region Public Methods

    public async Task SendAGVLoop(string topic, string jobId, uint miliseconds)
    {
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

            int listCount = _random.Next(1, 10000);
            agvList = await Task.Run(() => CreateAGVList(listCount));


            string payloadString = JsonSerializer.Serialize(agvList);
            SendMachineDto(topic, payloadString);
            agvList.Clear();

            await Task.Delay((int)miliseconds);
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
        string[] vhlName = ["aa", "bb", "cc", "dd", "ee", "ff", "gg"];
        for (int i = 0; i < count; i++)
        {
            var agvDto = new AGV
            {
                VhlName = vhlName[_random.Next(0, vhlName.Length-1)],
                X = _random.Next(0, 100),
                Y = _random.Next(0, 100),
                VhlState = "VhlState",
                Batt = _random.Next(0, 100),
                SubGoal = _random.Next(0, 100),
                FinalGoal = _random.Next(0, 100),
                SendTime = DateTime.Now,
                Degree = _random.Next(0, 100).ToString(),
            };

            agvList.Add(agvDto);
        }

        Dictionary<string, List<AGV>> dict = new();
        dict["AGV"] = agvList;

        return dict;
    }

    #endregion

    #region Private Fields

    private readonly IManagedMqttClient _managedMqttClient;
    private readonly Random _random;

    #endregion
}