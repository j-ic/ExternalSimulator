using ETLSimulator.Controller;
using ETLSimulator.Handler;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

Console.WriteLine("ETL, World!");

var mqttClientHandlerForAGV 
    = new MqttClientHandler("mqtt-input.flexing.ai", 1890);
var randomDataController 
    = new RandomDataController(mqttClientHandlerForAGV.Client);

Task agv = Task.Run(async() =>
{
    await randomDataController.SendAGVLoop(
       topic: "XR/data/106a6c241b8797f52e1e77317b96a201/AGV",
       jobId: "106a6c241b8797f52e1e77317b96a201/AGV",
       milliseconds: 300);
});

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