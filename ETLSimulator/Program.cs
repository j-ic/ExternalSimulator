﻿using ETLSimulator.Controller;
using ETLSimulator.Handler;

Console.WriteLine("ETL, World!");

const string BROKER_ADDRESS = "220.90.135.6";
const int PORT = 1883;
const bool USE_TLS = false;

MqttClientHandler mqttClientHandlerForTotal
    = new MqttClientHandler(BROKER_ADDRESS, PORT, USE_TLS);
IntegrationController totalDataController
    = new IntegrationController(mqttClientHandlerForTotal.Client);

const int MILLI_SECONDS = 1_000;
const int MAX_COUNT = 1;

_ = Task.Run(async () =>
{
    await totalDataController.SendAGVLoop(
       topic: "XR/data/4e7a17a46bbf09a94af971efe37a8340/AGV",
       jobId: "4e7a17a46bbf09a94af971efe37a8340/AGV",
       milliseconds: MILLI_SECONDS,
       maxCount: MAX_COUNT);
});

_ = Task.Run(async () =>
{
    await totalDataController.SendTransportLoop(
       topic: "XR/data/b0fce4b111ab01ab148c94de81120404/TRANSPORT_JOB",
       jobId: "b0fce4b111ab01ab148c94de81120404/TRANSPORT_JOB",
       milliseconds: MILLI_SECONDS,
       maxCount: MAX_COUNT);
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