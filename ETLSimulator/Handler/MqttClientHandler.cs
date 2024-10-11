using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLSimulator.Handler;

public class MqttClientHandler
{
    public IManagedMqttClient Client { get; private set; }

    public MqttClientHandler(string brokerAddress, int port)
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer(brokerAddress, port)
            .WithCleanSession()
            .Build();

        var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(mqttClientOptions)
            .Build();

        Client = new MqttFactory().CreateManagedMqttClient();
        Client.StartAsync(managedMqttClientOptions).Wait();
    }
}
