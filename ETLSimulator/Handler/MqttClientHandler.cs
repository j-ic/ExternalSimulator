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

    public MqttClientHandler(string brokerAddress, int port, bool useTls)
    {

        var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer(brokerAddress, port)
            .WithCleanSession();

        if (useTls)
        {
            var tlsOptions = new MqttClientTlsOptionsBuilder()
                .UseTls(true)
                .WithIgnoreCertificateChainErrors(true)
                .WithIgnoreCertificateRevocationErrors(true)
                .WithAllowUntrustedCertificates(true)
                .Build();

            mqttClientOptionsBuilder.WithTlsOptions(tlsOptions);
        }

        var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(mqttClientOptionsBuilder.Build())
            .Build();

        Client = new MqttFactory().CreateManagedMqttClient();
        Client.StartAsync(managedMqttClientOptions).Wait();
    }
}
