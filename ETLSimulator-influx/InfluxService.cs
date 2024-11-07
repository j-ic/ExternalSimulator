using System.Collections.Concurrent;
using System.Data;
using System.Timers;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Writes;
using MessagingContracts.ETL;

namespace ETLSimulator_influx;

public class InfluxService: IHostedService
{
    #region Constructor

    public InfluxService(
        IInfluxDBClient influxClient,
        ILogger<InfluxService> logger,
        string bucket,
        string org,
        string measurementName,
        int pointDataCount,
        double delaySeconds)
    {
        _influxClient = influxClient;
        _logger = logger;
        _bucket = bucket;
        _org = org;
        _measurementName = measurementName;
        _pointDataCount = pointDataCount;
        _delaySeconds = delaySeconds;

        _timer = new System.Timers.Timer(TimeSpan.FromMinutes(1));
        _timer.Elapsed += PrintPointCount;
        _timer.Start();
    }

    #endregion

    #region public methods

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested == false)
        {
            try
            {
                await WriteDataAsync();
                await Task.Delay(TimeSpan.FromSeconds(_delaySeconds), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: {Message}", ex.Message);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _influxClient.Dispose();
        return Task.CompletedTask;
    }

    #endregion

    #region private methods

    private async Task WriteDataAsync()
    {
        Dictionary<string, List<AGV>> agvList = CreateAGVList(_pointDataCount);
        ConcurrentBag<PointData> points = [];
        Partitioner<List<AGV>> partitioner 
                = Partitioner.Create(agvList.Values.ToList(), true);

        Parallel.ForEach(partitioner, agv =>
        {
            foreach (var item in agv)
            {
                PointData.Builder point = PointData.Builder
                    .Measurement(_measurementName)
                    .Tag(nameof(item.VhlName), item.VhlName)
                    .Field(nameof(item.X), (double)item.X!)
                    .Field(nameof(item.Y), (double)item.Y!)
                    .Field(nameof(item.VhlState), item.VhlState)
                    .Field(nameof(item.Batt), (double)item.Batt!)
                    .Field(nameof(item.SubGoal), (double)item.SubGoal!)
                    .Field(nameof(item.FinalGoal), (double)item.FinalGoal!)
                    .Field(nameof(item.Degree), item.Degree)
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

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
            _logger.LogError(ex, "InfluxDB Write Error : {Message}", ex.Message);
        }
    }

    private async Task WritePointData(List<PointData> points)
    {
        try
        {
            await _influxClient
                    .GetWriteApiAsync()
                    .WritePointsAsync(points, _bucket, _org);

            // Increment the message count for each successfully written point
            Interlocked.Add(ref _pointCount, points.Count);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex,
                "InfluxDB Write Task Canceled: {Message}", ex.Message);
        }
        catch (UnprocessableEntityException ex)
        {
            _logger.LogError(ex, "InfluxDB Type Error : {Message}", ex.Message);
            LogInvalidPoints(points);
        }
        catch (HttpException ex)
        {
            _logger.LogError(ex, "InfluxDB Http Error : {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InfluxDB Write Error : {Message}", ex.Message);
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

            _logger.LogInformation(
                "Invalid Point - Measurement: {Measurement}, Timestamp: {Timestamp}",
                measurement,
                timestamp);
        }

        if (points.Count > maxPointsToLog)
        {
            _logger.LogInformation(
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
            _logger.LogWarning(ex, "Failed to parse Line Protocol: {LineProtocol}", lineProtocol);
        }
    }

    private static Dictionary<string, List<AGV>> CreateAGVList(int count)
    {
        List<AGV> agvList = [];
        const string VAL_NAME = "AGV_001";

        for (int i = 0; i < count; i++)
        {
            var agvDto = new AGV
            {
                VhlName = VAL_NAME,
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

    private void PrintPointCount(object? sender, ElapsedEventArgs e)
    {
        string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
        string formattedPointCount = _pointCount.ToString("N0");
        _logger.LogInformation(
            "Total Points: {PointCount} - {Timestamp}", 
            formattedPointCount, 
            timestamp);

        if (_pointCount >= int.MaxValue) { Interlocked.Exchange(ref _pointCount, 0); }
    }

    #endregion

    #region private fields

    private readonly IInfluxDBClient _influxClient;
    private readonly ILogger _logger;
    private readonly string _bucket;
    private readonly string _org;
    private readonly string _measurementName;
    private readonly int _pointDataCount;
    private readonly double _delaySeconds;

    private volatile int _pointCount;
    private readonly System.Timers.Timer _timer;


    #endregion
}
