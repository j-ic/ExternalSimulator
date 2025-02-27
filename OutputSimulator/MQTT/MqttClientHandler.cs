using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace OutputSimulator.MQTT;

public class MqttClientHandler
{
	public MqttClientHandler(string configFilePath)
	{
		if (string.IsNullOrWhiteSpace(configFilePath))
		{
			throw new ArgumentNullException(nameof(configFilePath), "Configuration file path cannot be null or empty.");
		}

		// 설정파일 로드
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile(configFilePath, optional: false, reloadOnChange: true)
			.Build();

		var brokerConfig = configuration.GetSection("OutputBroker").Get<BrokerConfig>();

		var mqttOptions = new MqttClientOptionsBuilder()
			.WithTcpServer(brokerConfig.Ip, brokerConfig.Port)
			.Build();

		var managedOptions = new ManagedMqttClientOptionsBuilder()
			.WithClientOptions(mqttOptions)
			.Build();

		// TBD 
		if (brokerConfig.UseTls)
		{

		}

		_managedMqttClient = new MqttFactory().CreateManagedMqttClient();
		_managedMqttClient.StartAsync(managedOptions).Wait();
	}

	public IManagedMqttClient GetManagedClient() => _managedMqttClient;

	public async Task PublishMqtt(string topic, string payloadString)
	{
		if (!_managedMqttClient.IsStarted)
		{
			throw new InvalidOperationException("MQTT client is not connected.");
		}

		var message = new MqttApplicationMessageBuilder()
			.WithTopic(topic)
			.WithPayload(payloadString)
			.WithRetainFlag()
			.Build();

		await _managedMqttClient.EnqueueAsync(message);
	}

	public async Task DisconnectAsync()
	{
		if (_managedMqttClient.IsStarted)
		{
			await _managedMqttClient.StopAsync();
		}
	}

	private readonly IManagedMqttClient _managedMqttClient;
}
