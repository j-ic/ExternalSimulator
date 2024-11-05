using MessagingContracts.ETL;
using MQTTnet.Extensions.ManagedClient;
using System.Text.Json.Nodes;
using System.Text.Json;
using MQTTnet;
using System.Timers;

namespace ETLSimulator.Controller;

public class RandomTransportDataController
{
    #region Constructor
    public RandomTransportDataController(IManagedMqttClient managedMqttClient)
    {
        _managedMqttClient = managedMqttClient;
        _managedMqttClient.ConnectionStateChangedAsync += async args =>
        {
            Console.WriteLine("Transport Client Connection state changed");
            await Task.CompletedTask;
        };
        _managedMqttClient.ConnectedAsync += async args =>
        {
            Console.WriteLine($"Transport Client Connected: {args.ConnectResult.ResultCode}");
            await Task.CompletedTask;
        };
        _managedMqttClient.DisconnectedAsync += async args =>
        {
            Console.WriteLine($"Transport Client Disconnected: {args.Reason}");
            await Task.CompletedTask;
        };

        _timer = new System.Timers.Timer(60000);
        _timer.Elapsed += PrintMessageCount;
    }
    #endregion

    #region Public Methods

    public async Task SendTransportLoop(string topic, string jobId, uint milliseconds, int maxCount)
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
            Dictionary<string, List<Transport>> transportList = new();

            int listCount = maxCount;
            transportList = await Task.Run(() => CreateTransportList(listCount));

            string payloadString = JsonSerializer.Serialize(transportList);
            SendMachineDto(topic, payloadString);
            transportList.Clear();
            Interlocked.Increment(ref _messageCount);

            await Task.Delay((int)milliseconds);
        }
    }

    #endregion

    #region Private Methods

    private Dictionary<string, List<Transport>> CreateTransportList(int count)
    {
        List<Transport> transportList = [];

        string[] mainCarrIdArray = [
            "2F38678", "2F49727", "3F84077", "3F16198", "2F27631", "3F48241",
            "3F40768", "2F13414", "3F54193", "2F49674", "3F00689", "3F11802",
            "2F41125", "3F94525", "3F53365", "3F91658", "2F84097", "2F66722",
            "2F19687", "2F18741", "2F62227", "2F39188", "2F20734", "2F79661",
            "2F90372", "3F79847", "2F08989", "2F38216", "3F62528", "3F90339"
        ];
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
        string[] sysNameArray = ["TC", "ADS", "MCS", ];
        string[] moveStsArray = ["MOVING", "COMPLETE", "RECEIVE"];

        for (int i = 0; i < count; i++)
        {
            string mainCarrId = mainCarrIdArray[Random.Shared.Next(0, mainCarrIdArray.Length - 1)];
            string fromEqpId = eqpIdArray[Random.Shared.Next(0, eqpIdArray.Length - 1)];
            string toEqpId = eqpIdArray[Random.Shared.Next(0, eqpIdArray.Length - 1)];
            string currEqpId = eqpIdArray[Random.Shared.Next(0, eqpIdArray.Length - 1)];

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

            transportList.Add(transportDto);
        }

        Dictionary<string, List<Transport>> dict = new();
        dict["TRANSPORT_JOB"] = transportList;

        return dict;
    }

    private void SendMachineDto(string topic, string payloadString)
    {
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payloadString)
            .WithRetainFlag()
            .Build();

        _managedMqttClient.EnqueueAsync(message);
    }

    private void PrintMessageCount(object? sender, ElapsedEventArgs e)
    {
        string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
        Console.WriteLine($"[{timestamp}] Published Transport Message Count : {_messageCount}");
        //Interlocked.Exchange(ref _messageCount, 0);
    }

    #endregion 

    #region Private Fields

    private readonly IManagedMqttClient _managedMqttClient;
    private volatile int _messageCount;
    private System.Timers.Timer _timer;

    #endregion
}
