// See https://aka.ms/new-console-template for more information
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

Console.WriteLine("TAG_DATA, World!");

MqttClientTlsOptionsBuilder tlsOptions = new MqttClientTlsOptionsBuilder()
    .UseTls(false)
    .WithAllowUntrustedCertificates()
    .WithIgnoreCertificateChainErrors()
    .WithIgnoreCertificateRevocationErrors();

MqttClientOptions  mqttClientOptions
    = new MqttClientOptionsBuilder()
    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
    .WithClientId(Guid.NewGuid().ToString())
    .WithTcpServer("localhost", 1883)
    //.WithTlsOptions(new MqttClientTlsOptionsBuilder().Build())
    .WithCleanSession()
    .Build();

ManagedMqttClientOptions managedMqttClientOptions 
    = new ManagedMqttClientOptionsBuilder()
    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
    .WithClientOptions(mqttClientOptions)
    .Build();

var managedMqttClient = new MqttFactory().CreateManagedMqttClient();
managedMqttClient.StartAsync(managedMqttClientOptions).Wait();

TagDataSimulator.Controller.TagDataController TagDataController = new (managedMqttClient);

_ = Task.Run(() => TagDataController.SendTagData(100));
// _ = Task.Run(() => TagDataController.SendEdgeResource(3000));
// _ = Task.Run(() => TagDataController.SendEdgeState(5000));
// _ = Task.Run(() => TagDataController.SendDeviceState(7000));
_ = Task.Run(() => TagDataController.SendEdgeStorage(100));

// Set Exit Point
var quitEvent = new ManualResetEvent(false);
Console.CancelKeyPress += (sender, eArgs) =>
{
    quitEvent.Set();
    eArgs.Cancel = true;
};

// wait for timeout or Ctrl-C
quitEvent.WaitOne();

Console.WriteLine("Program ended.");
Console.WriteLine("Press any key to finish...");
_ = Console.ReadKey();