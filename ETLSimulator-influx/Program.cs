using ETLSimulator_influx;
using InfluxDB.Client;

var builder = WebApplication.CreateSlimBuilder(args);

// Add logging services
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    // Add other logging providers as needed
});

builder.Services.AddSingleton<IInfluxDBClient>(provider =>
{
    InfluxDBClient influxClient = new(
        url: "http://220.90.135.6:8086",
        token: "Y6gOnnAyzQTAFJKm9R17hcOB3QbtNenwHVwbIlb8aubiivQcpUXjTYtfS08AVKih0NAwJt7nmd-IMTZJ17Yl_w=="
    );
    influxClient.EnableGzip();
    return influxClient;
});

builder.Services.AddHostedService(provider =>
{
    var influxClient = provider.GetRequiredService<IInfluxDBClient>();
    var logger = provider.GetRequiredService<ILogger<InfluxService>>();
    var bucket = "sample";
    var org = "sample";
    var delaySeconds = 1;
    return new InfluxService(influxClient, logger, bucket, org, delaySeconds);
});

var app = builder.Build();

app.MapGet("/", () => "HELL World!");

app.Run();