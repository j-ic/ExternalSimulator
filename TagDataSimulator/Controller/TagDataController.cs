using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using TagDataSimulator.DTO;

namespace TagDataSimulator.Controller;

public class TagDataController
{
    #region constructor

    public TagDataController(IManagedMqttClient managedMqttClient)
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

    #region public methods

    public Task SendTagData(uint miliseconds)
    {
        while (true)
        {
            TagDataDto tagData = new()
            {
                EdgeId = $"{EDGE_ID}",
                Tags =
                [
                    new Tag
                    {
                        NodeIdentifier = $"{EDGE_ID}/AVG/test2/CUMULATIVE_COUNT",
                        Value = _random.Next(short.MaxValue),
                        SourceTime = DateTime.Now
                    },
                    new Tag
                    {
                        NodeIdentifier = $"{EDGE_ID}/AVG/test2/CYCLE_TIME_SEC_L",
                        Value = _random.Next(short.MaxValue),
                        SourceTime = DateTime.Now
                    },
                    new Tag
                    {
                        NodeIdentifier = $"{EDGE_ID}/AVG/test2/DAILY_COUNT",
                        Value = _random.Next(short.MaxValue),
                        SourceTime = DateTime.Now
                    },
                    new Tag
                    {
                        NodeIdentifier = $"{EDGE_ID}/AVG/test2/FEED_RATE_L",
                        Value = _random.Next(short.MaxValue),
                        SourceTime = DateTime.Now
                    },
                    new Tag
                    {
                        NodeIdentifier = $"{EDGE_ID}/AVG/test2/DEVICE_STATUS",
                        Value = _random.Next(0, 2) == 0,
                        SourceTime = DateTime.Now
                    },
                    new Tag
                    {
                        NodeIdentifier = $"{EDGE_ID}/AVG/test2/LED_SWITCH_ON_L",
                        Value = _random.Next(short.MaxValue),
                        SourceTime = DateTime.Now
                    },
                    new Tag
                    {
                        NodeIdentifier = $"{EDGE_ID}/AVG/test2/MAIN_AIR_PRESSURE",
                        Value = _random.Next(short.MaxValue),
                        SourceTime = DateTime.Now },
                    new Tag
                    {
                        NodeIdentifier = $"{EDGE_ID}/AVG/test2/PARTS_COUNT_L",
                        Value = _random.Next(short.MaxValue),
                        SourceTime = DateTime.Now
                    },
                    new Tag
                    {
                        NodeIdentifier = $"{EDGE_ID}/AVG/test2/TEMPERATURE_L",
                        Value = _random.Next(short.MaxValue),
                        SourceTime = DateTime.Now },
                ],
                Timestamp = DateTime.UtcNow
            };

            EdgeMessageDto edgeMessage = new()
            {
                MessageId = Guid.NewGuid().ToString(),
                MessageType = "ua-data",
                Messages = tagData
            };

            Span<byte> payload = JsonSerializer.SerializeToUtf8Bytes(edgeMessage);
            PublishData(DATA_TOPIC , payload);
            Task.Delay((int)miliseconds).Wait();
        }
    }

    public Task SendEdgeResource(uint miliseconds)
    {
        while (true)
        {
            EdgeResourceDto edgeResource = new()
            {
                EdgeId = EDGE_ID,
                Ip = Guid.NewGuid().ToString(),
                CpuUseage = _random.Next(100).ToString(),
                RamUseage = _random.Next(100).ToString(),
                NetworkRecv = _random.Next(100).ToString(),
                NetworkSend = _random.Next(100).ToString(),
                DateTime = DateTime.UtcNow.ToString(DATE_TIME_FORMAT)
            };

            Span<byte> payload = JsonSerializer.SerializeToUtf8Bytes(edgeResource);
            PublishData(RESOURECE_TOPIC, payload);
            Task.Delay((int)miliseconds).Wait();
        }
    }

    public Task SendEdgeState(uint miliseconds)
    {
        while (true)
        {
            EdgeStateDto edgeState = new()
            {
                EdgeId = EDGE_ID,
                State = _random.Next(0, 2) == 0 ? "ONLINE" : "OFFLINE",
                DateTime = DateTime.UtcNow.ToString(DATE_TIME_FORMAT)
            };

            Span<byte> payload = JsonSerializer.SerializeToUtf8Bytes(edgeState);
            PublishData(EDGE_STATE_TOPIC, payload);
            Task.Delay((int)miliseconds).Wait();
        }
    }

    public Task SendDeviceState(uint miliseconds)
    {
        while (true)
        {
            EdgeDeviceState deviceState = new()
            {
                EdgeId = EDGE_ID,
                DeviceId = $"AVG/test2",
                State = _random.Next(0, 2) == 0 ? "ONLINE" : "OFFLINE",
                DateTime = DateTime.UtcNow.ToString(DATE_TIME_FORMAT)
            };

            Span<byte> payload = JsonSerializer.SerializeToUtf8Bytes(deviceState);
            PublishData(DEVICE_STATE_TOPIC, payload);
            Task.Delay((int)miliseconds).Wait();
        }
    }

    public Task SendEdgeStorage(uint miliseconds)
    {
        while (true)
        {
            EdgeStorageDto edgeStorage = new()
            {
                EdgeId = EDGE_ID,
                OsType = "Windows",
                Storages =
                [
                    new StorageDto
                    {
                        DiskName = "C:",
                        TotalSize = "500GB",
                        UseSize = $"{_random.Next(0, 500)}GB",
                        FreeSize = $"{_random.Next(0, 500)}GB"
                    },
                    new StorageDto
                    {
                        DiskName = "D:",
                        TotalSize = "100GB",
                        UseSize = $"{_random.Next(0, 100)}GB",
                        FreeSize = $"{_random.Next(0, 100)}GB"
                    }
                ],
                DateTime = DateTime.UtcNow.ToString(DATE_TIME_FORMAT)
            };

            Span<byte> payload = JsonSerializer.SerializeToUtf8Bytes(edgeStorage);
            PublishData(DEVICE_STORAGE, payload);
            Task.Delay((int)miliseconds).Wait();
        }
    }

    public Task SendPing(uint miliseconds)
    {
        while (true)
        {
            var ping = new
            {
                MessageId = Guid.NewGuid().ToString(),
                EdgeId = EDGE_ID,
                Timestamp = DateTime.UtcNow
            };

            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic("Edge/ping")
                .WithPayload(JsonSerializer.SerializeToUtf8Bytes(ping))
                .Build();

            _managedMqttClient.EnqueueAsync(message).Wait();
            Task.Delay((int)miliseconds).Wait();
        }
    }

    #endregion

    #region private methods

    private void PublishData(string topic, Span<byte> payload)
    {
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload.ToArray())
            .Build();

        _managedMqttClient.EnqueueAsync(message).Wait();
    }

    #endregion

    #region private fields

    private readonly IManagedMqttClient _managedMqttClient;
    private readonly Random _random;
    private const string EDGE_ID = "qwer2";
    private const string DATA_TOPIC = $"Edge/data/{EDGE_ID}";
    private const string RESOURECE_TOPIC = "Edge/resource";
    private const string EDGE_STATE_TOPIC = "Edge/edgestate";
    private const string DEVICE_STATE_TOPIC = "Edge/devicestate";
    private const string DEVICE_STORAGE = "Edge/storage";
    private const string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

    #endregion
}
