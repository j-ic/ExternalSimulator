﻿using ETLSimulator.Controller;
using ETLSimulator.Handler;

Console.WriteLine("ETL, World!");

var mqttClientHandlerForAGV 
    = new MqttClientHandler("220.90.135.6", 1883);
var randomDataController 
    = new RandomDataController(mqttClientHandlerForAGV.Client);

Task agv = Task.Run(async() =>
{
    await randomDataController.SendAGVLoop(
       topic: "XR/data/106a6c241b8797f52e1e77317b96a201/AGV",
       jobId: "106a6c241b8797f52e1e77317b96a201/AGV",
       milliseconds: 300,
       maxCount: 10000);
});

var mqttClientHandlerForTransport 
    = new MqttClientHandler("220.90.135.6", 1883);
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