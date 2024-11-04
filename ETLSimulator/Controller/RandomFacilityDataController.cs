using System.Text.Json;
using System.Text.Json.Nodes;
using ETLSimulator.DTO;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;

namespace ETLSimulator.Controller;

public class RandomFacilityDataController
{
    #region Constructor
    public RandomFacilityDataController(IManagedMqttClient managedMqttClient)
    {
        _managedMqttClient = managedMqttClient;
        _managedMqttClient.ConnectionStateChangedAsync += async args =>
        {
            Console.WriteLine("Facility Client Connection state changed");
            await Task.CompletedTask;
        };
        _managedMqttClient.ConnectedAsync += async args =>
        {
            Console.WriteLine($"Facility Client Connected: {args.ConnectResult.ResultCode}");
            await Task.CompletedTask;
        };
        _managedMqttClient.DisconnectedAsync += async args =>
        {
            Console.WriteLine($"Facility Client Disconnected: {args.Reason}");
            await Task.CompletedTask;
        };
    }
    #endregion

    #region Public Methods

    public async Task SendFacilityLoop(string topic, string jobId, uint milliseconds, int maxCount)
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
            Dictionary<string, List<Facility>> facilityList = new();

            int listCount = Random.Shared.Next(1, maxCount);
            facilityList = await Task.Run(() => CreateFacilityList(listCount));


            string payloadString = JsonSerializer.Serialize(facilityList);
            SendMachineDto(topic, payloadString);
            facilityList.Clear();

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

    private Dictionary<string, List<Facility>> CreateFacilityList(int count)
    {
        List<Facility> facilityList = [];
        string[] lineId = ["LINE01-ASSEMBLY-001", "LINE02-PACKAGING-002", 
                        "LINE03-WAREHOUSE-003", "LINE04-MATERIAL-004", 
                        "LINE05-MATERIAL-005", "LINE06-MATERIAL-006",
                        "LINE10-PAINTING-010", "LINE11-WAREHOUSE-011"];
        for (int i = 0; i < count; i++)
        {
            var facilityDto = new Facility
            {
                LineId = lineId[Random.Shared.Next(0, lineId.Length - 1)],
                Temperature = GetRandomFloat(16.5f, 17.5f),
                Humidity = GetRandomFloat(64.0f, 66.0f),
                LineSpeed = GetRandomFloat(0.45f, 0.55f),
                UtilizationRate = GetRandomFloat(88.5f, 90.5f),
                Productivity = GetRandomFloat(88.5f, 90.5f),
                Timestamp = DateTime.Now,
            };

            facilityList.Add(facilityDto);
        }

        Dictionary<string, List<Facility>> dict = new();
        dict["Facility"] = facilityList;

        return dict;
    }

    public float GetRandomFloat(float min, float max)
    {
        float randomFloat = (float)Math.Round(Random.Shared.NextDouble() * (max - min) + min, 1);
        return randomFloat;
    }

    #endregion

    #region Private Fields

    private readonly IManagedMqttClient _managedMqttClient;

    #endregion
}
