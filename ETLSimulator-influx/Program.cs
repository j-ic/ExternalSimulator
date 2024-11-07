using ETLSimulator_influx;
using InfluxDB.Client;

var builder = WebApplication.CreateSlimBuilder(args);

// Add logging services
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

builder.Services.AddSingleton<IInfluxDBClient>(provider =>
{
    const string URL = "http://220.90.135.6:8086";
    const string TOKEN = "Y6gOnnAyzQTAFJKm9R17hcOB3QbtNenwHVwbIlb8aubiivQcpUXjTYtfS08AVKih0NAwJt7nmd-IMTZJ17Yl_w==";
    InfluxDBClient influxClient = new(
        url: URL,
        token: TOKEN
    );
    influxClient.EnableGzip();
    return influxClient;
});

builder.Services.AddHostedService(provider =>
{
    IInfluxDBClient influxClient = provider.GetRequiredService<IInfluxDBClient>();
    ILogger<InfluxService> logger = provider.GetRequiredService<ILogger<InfluxService>>();
    const string BUCKET = "sample";
    const string ORG = "sample";
    const string MESUREMENT_NAME = "AGV";
    const int POINT_DATA_COUNT = 10_000;
    const double DELAY_SECONDS = 0.1;
    return new InfluxService(
        influxClient: influxClient, 
        logger: logger, 
        bucket: BUCKET,
        org: ORG,
        measurementName: MESUREMENT_NAME,
        pointDataCount: POINT_DATA_COUNT,
        delaySeconds: DELAY_SECONDS);
});

var app = builder.Build();

// Create a logger instance
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger<Program>();

// Log a message
logger.LogInformation("ETLSimulator-influx is starting......");
logger.LogInformation("prepare yourself for engaging.....");

app.MapGet("/", () => "HELL World!");

app.Run();