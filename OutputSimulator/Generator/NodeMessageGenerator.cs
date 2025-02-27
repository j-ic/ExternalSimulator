using MQTTnet.Extensions.ManagedClient;
using System.Text.Json;

namespace OutputSimulator.Generator;

public class NodeMessageGenerator
{
	#region Constructor
	public NodeMessageGenerator(string jsonFilePath, IManagedMqttClient managedMqttClient)
	{
		_jsonFilePath = jsonFilePath;

		_managedMqttClient = managedMqttClient;

		_managedMqttClient.ConnectionStateChangedAsync += async args =>
		{
			Console.WriteLine("MQTT Client Connection state changed");
			await Task.CompletedTask;
		};

		_managedMqttClient.ConnectedAsync += async args =>
		{
			Console.WriteLine($"MQTT Client Connected: {args.ConnectResult.ResultCode}");
			await Task.CompletedTask;
		};

		_managedMqttClient.DisconnectedAsync += async args =>
		{
			Console.WriteLine($"MQTT Client Disconnected: {args.Reason}");
			await Task.CompletedTask;
		};
	}
	#endregion

	public List<NodeInfo> LoadNodes()
	{
		if (!File.Exists(_jsonFilePath))
		{
			throw new FileNotFoundException($"Error: '{_jsonFilePath}' file not found.");
		}

		var jsonContent = File.ReadAllText(_jsonFilePath);
		var nodeConfig = JsonSerializer.Deserialize<NodeConfig>(jsonContent);

		if (nodeConfig?.Nodes == null || nodeConfig.Nodes.Count == 0)
		{
			throw new Exception("Error: No Nodes found in the JSON file.");
		}

		return nodeConfig.Nodes;
	}

	public object GenerateValue(NodeInfo node)
	{
		return node.Value.Equals("RANDOM", StringComparison.OrdinalIgnoreCase)
			? GenerateRandomValue(node.DataType)
			: ConvertValue(node.Value, node.DataType);
	}

	private static object GenerateRandomValue(string dataType)
	{
		var random = new Random();
		return dataType.ToLower() switch
		{
			"int" => random.Next(100, 500),
			"string" => Guid.NewGuid().ToString(),
			"float" => (float)(random.NextDouble() * 400 + 100),
			_ => "UnsupportedType"
		};
	}

	private static object ConvertValue(string value, string dataType)
	{
		return dataType.ToLower() switch
		{
			"bool" => bool.TryParse(value.ToLower(), out bool boolValue) ? boolValue : false,
			"int" => int.TryParse(value, out int intValue) ? intValue : 0,
			"string" => value,
			"float" => float.TryParse(value, out float floatValue) ? floatValue : 0.0f,
			_ => value
		};
	}

	#region Private Fields
	private readonly IManagedMqttClient _managedMqttClient;
	private readonly string _jsonFilePath;
	#endregion
}

// JSON 파일 구조와 매핑할 클래스
public class NodeConfig
{
	public List<NodeInfo> Nodes { get; set; }
}

/// <summary>
/// 
/// DataType : int, string, float 가능
/// Value : "RANDOM" 인 경우 랜덤값 발행, 다른 값 입력 시 고정값 발행
/// Period 단위 : ms
/// </summary>
public class NodeInfo
{
	public required string NodeID { get; set; }
	public required string DataType { get; set; }
	public required string Value { get; set; }
	public int Period { get; set; } = 1000;
}
