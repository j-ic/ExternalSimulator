using ETLSimulator.Controller;
using ETLSimulator.Handler;

Console.WriteLine("ETL, World!");

const string BROKER_ADDRESS = "localhost";
const int PORT = 1883;
const bool USE_TLS = false;

var mqttClientHandlerForFacility
    = new MqttClientHandler(BROKER_ADDRESS, PORT, USE_TLS);
var randomFacilityDataController
    = new RandomFacilityDataController(mqttClientHandlerForFacility.Client);

Task agv = Task.Run(async () =>
{
    await randomFacilityDataController.SendFacilityLoop(
       topic: "XR/data/exhibition/FACILITY",
       jobId: "exhibition/FACILITY",
       milliseconds: 300,
       maxCount: 5);
});

var mqttClientHandlerForAGV
    = new MqttClientHandler(BROKER_ADDRESS, PORT, USE_TLS);
var randomAGVDataController
    = new RandomAGVDataController(mqttClientHandlerForAGV.Client);

Task agv = Task.Run(async() =>
{
    await randomAGVDataController.SendAGVLoop(
       topic: "XR/data/106a6c241b8797f52e1e77317b96a201/AGV",
       jobId: "106a6c241b8797f52e1e77317b96a201/AGV",
       milliseconds: 300,
       maxCount: 10000);
});

var mqttClientHandlerForTransport 
    = new MqttClientHandler(BROKER_ADDRESS, PORT, USE_TLS);
var randomTransportDataController
    = new RandomTransportDataController(mqttClientHandlerForTransport.Client);

Task transport = Task.Run(async () =>
{
    await randomTransportDataController.SendTransportLoop(
       topic: "XR/data/b0fce4b111ab01ab148c94de81120404/TRANSPORT_JOB",
       jobId: "b0fce4b111ab01ab148c94de81120404/TRANSPORT_JOB",
       milliseconds: 300,
       maxCount: 10000);
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