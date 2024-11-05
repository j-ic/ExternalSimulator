using System.Collections.Concurrent;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using MessagingContracts.ETL;

namespace ETLSimulator_influx;

public class InfluxService(
    IInfluxDBClient influxClient,
    ILogger<InfluxService> logger,
    string bucket,
    string org,
    double delaySeconds)
: IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested == false)
        {
            try
            {
                await WriteDataAsync();
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error: {Message}", ex.Message);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        influxClient.Dispose();
        return Task.CompletedTask;
    }

    private async Task WriteDataAsync()
    {
        Dictionary<string, List<AGV>> agvList = CreateAGVList(10_000);
        ConcurrentBag<PointData> points = [];
        Partitioner<List<AGV>> partitioner 
                = Partitioner.Create(agvList.Values.ToList(), true);

        Parallel.ForEach(partitioner, agv =>
        {
            foreach (var item in agv)
            {
                PointData.Builder point = PointData.Builder
                    .Measurement("AGV")
                    .Tag(nameof(item.VhlName), item.VhlName)
                    .Field(nameof(item.X), (double)item.X!)
                    .Field(nameof(item.Y), (double)item.Y!)
                    .Field(nameof(item.VhlState), item.VhlState)
                    .Field(nameof(item.Batt), (double)item.Batt!)
                    .Field(nameof(item.SubGoal), (double)item.SubGoal!)
                    .Field(nameof(item.FinalGoal), (double)item.FinalGoal!)
                    .Field(nameof(item.Degree), item.Degree)
                    .Timestamp(DateTime.UtcNow.AddHours(-3), WritePrecision.Ns);

                points.Add(point.ToPointData());
            }
        });

        const int INFLUX_BATCH_SIZE = 5000;
        List<PointData> pointList = [.. points];

        for (int i = 0; i < pointList.Count; i += INFLUX_BATCH_SIZE)
        {
            int chunkSize = Math.Min(INFLUX_BATCH_SIZE, pointList.Count - i);
            List<PointData> chunk = pointList.GetRange(i, chunkSize);
            await influxClient
                    .GetWriteApiAsync()
                    .WritePointsAsync(chunk, bucket, org);

            logger.LogInformation("{time}: {chunkSize} requested.",
                DateTime.Now.ToString("HH:mm:ss.fff"), chunkSize);
        }
    }

    private static Dictionary<string, List<AGV>> CreateAGVList(int count)
    {
        List<AGV> agvList = [];
        string[] vhlName = ["AGV_001", "AGV_002", "AGV_003", "AGV_004",
                            "AGV_005", "AGV_006", "AGV_007", "AGV_008"];

        for (int i = 0; i < count; i++)
        {
            var agvDto = new AGV
            {
                VhlName = vhlName[Random.Shared.Next(0, vhlName.Length-1)],
                X = Random.Shared.Next(0, 100),
                Y = Random.Shared.Next(0, 100),
                VhlState = "VhlState",
                Batt = Random.Shared.Next(0, 100),
                SubGoal = Random.Shared.Next(0, 100),
                FinalGoal = Random.Shared.Next(0, 100),
                SendTime = DateTime.Now,
                Degree = Random.Shared.Next(0, 100).ToString(),
            };

            agvList.Add(agvDto);
        }

        Dictionary<string, List<AGV>> dict = new() { ["AGV"] = agvList };

        return dict;
    }
}
