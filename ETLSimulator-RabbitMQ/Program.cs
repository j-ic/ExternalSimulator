using ETLSimulator_RabbitMQ;
using MessagingContracts.ETL;
using RabbitMQ.Client;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain
        .Insert(0, AGVJsonSerializerContext.Default);

    options.SerializerOptions.TypeInfoResolverChain
        .Insert(0, TrasportJsonSerializerContext.Default);
});

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// Register RabbitMQ connection and channel as services
builder.Services.AddSingleton(provider =>
{
    var factory = new ConnectionFactory
    {
        HostName = "localhost",
        UserName = "guest",
        Password = "guest"
    };
    IConnection connection
        = factory.CreateConnectionAsync().Result;
    return connection;
});

builder.Services.AddSingleton(provider =>
{
    IChannel channel = provider
        .GetRequiredService<IConnection>()
        .CreateChannelAsync().Result;
    return channel;
});


builder.Services.AddHostedService<RabbitMQService>(provider =>
{
    IConnection connection = provider.GetRequiredService<IConnection>();
    ILogger<RabbitMQService> logger = provider.GetRequiredService<ILogger<RabbitMQService>>();
    IChannel channel = provider.GetRequiredService<IChannel>();
    const int MAX_COUNT = 1800;
    const int MILLISECONDS = 7;
    const int MULTIPLIER = 4;
    return new RabbitMQService(MAX_COUNT, MILLISECONDS, MULTIPLIER, channel, logger);
});

var app = builder.Build();

app.Services.GetRequiredService<IHostApplicationLifetime>()
    .ApplicationStarted.Register(() =>
    {
        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<Program>();
        logger.LogInformation("ETLSimulator-RabbitMQ is starting......");
        logger.LogInformation("prepare yourself for engaging.....");
    });


app.MapGet("/", () => "HELL World!");

app.Run();

[JsonSerializable(typeof(Dictionary<string, List<AGV>>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class AGVJsonSerializerContext : JsonSerializerContext;


[JsonSerializable(typeof(Dictionary<string, List<Transport>>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class TrasportJsonSerializerContext : JsonSerializerContext;
