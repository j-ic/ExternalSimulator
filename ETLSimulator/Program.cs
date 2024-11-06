using ETLSimulator.Controller;
using ETLSimulator.Handler;

Console.WriteLine("ETL, World!");

const string BROKER_ADDRESS = "220.90.135.6";
const int PORT = 1883;
const bool USE_TLS = false;

MqttClientHandler mqttClientHandlerForTotal
    = new MqttClientHandler(BROKER_ADDRESS, PORT, USE_TLS);
IntegrationController totalDataController
    = new IntegrationController(mqttClientHandlerForTotal.Client);

_ = Task.Run(async () =>
{
    await totalDataController.SendAGVLoop(
       topic: "COUNT_TEST/data/106a6c241b8797f52e1e77317b96a201/AGV",
       jobId: "106a6c241b8797f52e1e77317b96a201/AGV",
       milliseconds: 10,
       maxCount: 10_000);
});

_ = Task.Run(async () =>
{
    await totalDataController.SendTransportLoop(
       topic: "COUNT_TEST/data/106a6c241b8797f52e1e77317b96a201/AGV",
       jobId: "106a6c241b8797f52e1e77317b96a201/AGV",
       milliseconds: 10,
       maxCount: 10_000);
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