using ETLSimulator.Controller;
using ETLSimulator.Handler;

Console.WriteLine("ETL, World!");

const string BROKER_ADDRESS = "220.90.135.5";
const int PORT = 1883;
const bool USE_TLS = false;

MqttClientHandler mqttClientHandlerForTotal
    = new(BROKER_ADDRESS, PORT, USE_TLS);
IntegrationController totalDataController
    = new(mqttClientHandlerForTotal.Client);

//const int MILLI_SECONDS = 100;
//const int MAX_COUNT = 1;

//_ = Task.Run(async () =>
//{
//    await totalDataController.SendAGVLoop(
//       topic: "XR/data/f55ae730a09fc737efd02cec80440b73/AGV",
//       milliseconds: MILLI_SECONDS,
//       maxCount: MAX_COUNT);
//});

//const int TRANSPORT_MILLI_SECONDS = 1_000;
//const int TRANSPORT_MAX_COUNT = 1;

//_ = Task.Run(async () =>
//{
//    await totalDataController.SendTransportLoop(
//       topic: "XR/data/b0fce4b111ab01ab148c94de81120404/TRANSPORT_JOB",
//       milliseconds: TRANSPORT_MILLI_SECONDS,
//       maxCount: TRANSPORT_MAX_COUNT);
//});

const int REDIS_MILLI_SECONDS = 5000;
const int REDIS_MAX_COUNT = 5;
const string MQTT_TOPIC = "testData/MyFactory";

_ = Task.Run(async () =>
{
    await totalDataController.SendRedisBulkTestLoop(
       topic: MQTT_TOPIC,
       milliseconds: REDIS_MILLI_SECONDS,
       maxCount: REDIS_MAX_COUNT);
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