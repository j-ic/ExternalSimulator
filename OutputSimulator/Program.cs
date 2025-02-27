using OutputSimulator.Generator;
using OutputSimulator.MQTT;
using System.Text.Json;

Console.WriteLine("Output Simulator SETUP...");

// 설정정보 읽고 mqtt client set
string settingFilePath = "appsettings.json";
var mqttClientHandler	= new MqttClientHandler(settingFilePath);

// JSON 데이터 읽기 및 메시지 생성
string jsonFilePath = "nodeConfig.json";
var generator = new NodeMessageGenerator(jsonFilePath, mqttClientHandler.GetManagedClient());

// node 별 task 생성
var nodes = generator.LoadNodes();
var tasks = new List<Task>();

foreach (var node in nodes)
{
	tasks.Add(Task.Run(async () =>
	{
		while (true)
		{
			string topic = $"1/data/{node.NodeID.Trim()}";
			object value = generator.GenerateValue(node);

			// 페이로드 생성
			var payload = new Dictionary<string, object>
			{
				{ node.NodeID, value },
				{ "Timestamp", DateTime.UtcNow.ToString("o") }
			};

			// MQTT 메시지 발행
			string payloadJson = JsonSerializer.Serialize(payload);
			await mqttClientHandler.PublishMqtt(topic, payloadJson);

			// 주기적으로 발행
			await Task.Delay(node.Period);
		}
	}));
}

Console.WriteLine("Output Simulator START!");
// 모든 작업이 종료되지 않도록 유지
await Task.WhenAll(tasks);