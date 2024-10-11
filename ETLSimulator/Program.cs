using ETLSimulator.Controller;
using ETLSimulator.Handler;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

Console.WriteLine("ETL, World!");

MqttClientOptions  mqttClientOptions
    = new MqttClientOptionsBuilder()
    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
    .WithClientId(Guid.NewGuid().ToString())
    .WithTcpServer("mqtt-input.flexing.ai", 1890)
    .WithCleanSession()
    .Build();

ManagedMqttClientOptions managedMqttClientOptions 
    = new ManagedMqttClientOptionsBuilder()
    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
    .WithClientOptions(mqttClientOptions)
    .Build();

var managedMqttClient = new MqttFactory().CreateManagedMqttClient();
managedMqttClient.StartAsync(managedMqttClientOptions).Wait();

var randomDataController = new RandomDataController(managedMqttClient);

Task task = randomDataController.SendAGVLoop(
    topic: "XR/data/106a6c241b8797f52e1e77317b96a201/AGV",
    jobId: "106a6c241b8797f52e1e77317b96a201/AGV",
    miliseconds: 300);

task.Wait();

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