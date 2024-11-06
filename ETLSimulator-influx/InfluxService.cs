using System.Collections.Concurrent;
using System.Data;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
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

        List<PointData> pointList = [.. points];
        await BatchWritePointData(pointList);
    }

    private async Task BatchWritePointData(List<PointData> points)
    {
        const int INFLUX_BATCH_SIZE = 5000;
        try
        {
            for (int i = 0; i < points.Count; i += INFLUX_BATCH_SIZE)
            {
                int chunkSize = Math.Min(INFLUX_BATCH_SIZE, points.Count - i);
                List<PointData> chunk = points.GetRange(i, chunkSize);
                await WritePointData(chunk);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "InfluxDB Write Error : {Message}", ex.Message);
        }
    }

    private async Task WritePointData(List<PointData> points)
    {
        try
        {
            await influxClient
                    .GetWriteApiAsync()
                    .WritePointsAsync(points, bucket, org);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex,
                "InfluxDB Write Task Canceled: {Message}", ex.Message);
        }
        catch (UnprocessableEntityException ex)
        {
            logger.LogError(ex, "InfluxDB Type Error : {Message}", ex.Message);
            LogInvalidPoints(points);
        }
        catch (HttpException ex)
        {
            logger.LogError(ex, "InfluxDB Http Error : {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "InfluxDB Write Error : {Message}", ex.Message);
        }
    }

    private void LogInvalidPoints(List<PointData> points)
    {
        // Limit the number of points to log to prevent excessive logging
        int maxPointsToLog = 5;
        var pointsToLog = points.Take(maxPointsToLog);

        foreach (var point in pointsToLog)
        {
            string lineProtocol = point.ToLineProtocol();

            // Parse the Line Protocol to extract measurement and timestamp
            ParseLineProtocol(lineProtocol, out string measurement, out string timestamp);

            logger.LogInformation(
                "Invalid Point - Measurement: {Measurement}, Timestamp: {Timestamp}",
                measurement,
                timestamp);
        }

        if (points.Count > maxPointsToLog)
        {
            logger.LogInformation(
                "Total invalid points: {TotalInvalidPoints}. Logged first {LoggedPointsCount} points.",
                points.Count,
                maxPointsToLog);
        }
    }

    private void ParseLineProtocol(string lineProtocol, out string measurement, out string timestamp)
    {
        // Initialize output variables
        measurement = string.Empty;
        timestamp = string.Empty;

        try
        {
            // Line Protocol format: measurement[,tags] fields [timestamp]
            // Split the Line Protocol into components
            // We need to handle cases where tags are present or absent

            // Split by space to separate the main components
            string[] spaceParts = lineProtocol.Split(' ');

            if (spaceParts.Length >= 2)
            {
                // The first part contains measurement and tags
                string measurementAndTags = spaceParts[0];

                // The last part may be the timestamp
                if (spaceParts.Length == 3)
                {
                    timestamp = spaceParts[2];
                }

                // Extract measurement name (it's before the first comma)
                int commaIndex = measurementAndTags.IndexOf(',');

                if (commaIndex >= 0)
                {
                    // Measurement with tags
                    measurement = measurementAndTags.Substring(0, commaIndex);
                }
                else
                {
                    // Measurement without tags
                    measurement = measurementAndTags;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse Line Protocol: {LineProtocol}", lineProtocol);
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
