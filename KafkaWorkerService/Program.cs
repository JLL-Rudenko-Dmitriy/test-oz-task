using Grpc.Core;
using Grpc.Net.Client;
using KafkaWorkerService.Consumers.BackgroundServices;
using KafkaWorkerService.Consumers.Builders.Models;
using KafkaWorkerService.Consumers.Decorator;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

string? kafkaConnectionString = string.Empty;
string? fastConsumerGroup = string.Empty;
string? batchConsumerGroup = string.Empty;

string consumerTopic = "report-requests";

uint batchTimeInterval = 12;
uint messageLimitPerConsumer = 512;

if (builder.Environment.IsDevelopment())
{
    string reportGeneratorChannelString = builder.Configuration.GetConnectionString("GrpcReportGeneratorService")
                                          ?? throw new InvalidOperationException("Can not set channel address");

    string reportQueriesChannelString = builder.Configuration.GetConnectionString("GrpcReportQueryService")
                                        ?? throw new InvalidOperationException("Can not set channel address");

    ChannelBase grpcGeneratorChannel = GrpcChannel.ForAddress(reportGeneratorChannelString);
    ChannelBase grpcReportQueriesChannel = GrpcChannel.ForAddress(reportQueriesChannelString);

    builder.Services.AddSingleton<ReportGenerator.ReportGeneratorService.ReportGeneratorServiceClient>(
        sp => new ReportGenerator.ReportGeneratorService.ReportGeneratorServiceClient(grpcGeneratorChannel));

    builder.Services.AddSingleton<ReportQueryManaging.ReportQueryService.ReportQueryServiceClient>(
        sp => new ReportQueryManaging.ReportQueryService.ReportQueryServiceClient(grpcReportQueriesChannel));

    kafkaConnectionString = builder.Configuration.GetConnectionString("Kafka")
                            ?? throw new InvalidOperationException("Can not set connection string");

    fastConsumerGroup = builder.Configuration["KafkaTopics:FastConsumerTopic"]
                        ?? throw new InvalidOperationException("Can not set connection fast consumer topic string");

    batchConsumerGroup = builder.Configuration["KafkaTopics:BatchConsumerTopic"]
                         ?? throw new InvalidOperationException("Can not set connection batch consumer topic string");
}

if (builder.Environment.IsProduction())
{
    string reportGeneratorChannelString = builder.Configuration["GRPC_SERVICES_REPORT_GENERATOR"]
                                          ?? throw new InvalidOperationException("Can not set channel address");

    string reportQueriesChannelString = builder.Configuration["GRPC_SERVICES_REPORT_QUERY"]
                                        ?? throw new InvalidOperationException("Can not set channel address");

    var grpcGeneratorChannel = GrpcChannel.ForAddress(reportGeneratorChannelString);
    var grpcReportQueriesChannel = GrpcChannel.ForAddress(reportQueriesChannelString);

    builder.Services.AddScoped<ReportGenerator.ReportGeneratorService.ReportGeneratorServiceClient>(
        sp => new ReportGenerator.ReportGeneratorService.ReportGeneratorServiceClient(grpcGeneratorChannel));

    builder.Services.AddScoped<ReportQueryManaging.ReportQueryService.ReportQueryServiceClient>(
        sp => new ReportQueryManaging.ReportQueryService.ReportQueryServiceClient(grpcReportQueriesChannel));

    kafkaConnectionString = builder.Configuration["KAFKA_CONNECTION_HOST_STRING"]
                            ?? throw new InvalidOperationException("Can not set connection string");

    fastConsumerGroup = builder.Configuration["KAFKA_TOPICS_FAST_CONSUMER_TOPIC"]
                        ?? throw new InvalidOperationException("Can not set connection fast consumer topic string");

    batchConsumerGroup = builder.Configuration["KAFKA_TOPICS_BATCH_CONSUMER_TOPIC"]
                         ?? throw new InvalidOperationException("Can not set connection batch consumer topic string");

    batchTimeInterval = uint.Parse(builder.Configuration["BATCH_CONSUMER_HOURS_INTERVAL"]
                                   ?? throw new InvalidOperationException(
                                       "Can not set connection batch time interval"));

    messageLimitPerConsumer = uint.Parse(builder.Configuration["MESSAGE_LIMIT_PER_CONSUMER"]
                                         ?? throw new InvalidOperationException(
                                             "Can not set connection batch time interval"));

    consumerTopic = builder.Configuration["KAFKA_TOPIC"]
                    ?? throw new InvalidOperationException("Can not set connection consumer topic");
}

builder.Services.AddSingleton<BaseConsumerFactory>(sp =>
{
    var grpcReportGeneratorClient =
        sp.GetRequiredService<ReportGenerator.ReportGeneratorService.ReportGeneratorServiceClient>();
    var grpcReportQueryClient =
        sp.GetRequiredService<ReportQueryManaging.ReportQueryService.ReportQueryServiceClient>();

    return new BaseConsumerFactory(messageLimitPerConsumer,
        batchTimeInterval,
        grpcReportQueryClient,
        grpcReportGeneratorClient);
});

builder.Services.AddSingleton<BatchConsumer>(sp =>
{
    BaseConsumerFactory factory = sp.GetRequiredService<BaseConsumerFactory>();
    return (BatchConsumer)factory.CreateBatchConsumer(consumerTopic, batchConsumerGroup, kafkaConnectionString);
});

builder.Services.AddSingleton<FastConsumer>(sp =>
{
    BaseConsumerFactory factory = sp.GetRequiredService<BaseConsumerFactory>();
    BatchConsumer batchConsumer = sp.GetRequiredService<BatchConsumer>();

    var fastConsumer =
        (FastConsumer)factory.CreateFastConsumer(consumerTopic, fastConsumerGroup, kafkaConnectionString);
    fastConsumer.AddObserver(batchConsumer);

    return fastConsumer;
});

builder.Services.AddHostedService<FastKafkaBackgroundService>(sp =>
{
    ILogger<FastKafkaBackgroundService> logger = sp.GetRequiredService<ILogger<FastKafkaBackgroundService>>();
    return new FastKafkaBackgroundService(sp.GetRequiredService<FastConsumer>(), logger);
});

builder.Services.AddHostedService<BatchKafkaBackgroundService>(sp =>
{
    ILogger<BatchKafkaBackgroundService> logger = sp.GetRequiredService<ILogger<BatchKafkaBackgroundService>>();
    return new BatchKafkaBackgroundService(sp.GetRequiredService<BatchConsumer>(), logger);
});

IHost host = builder.Build();

await host.RunAsync();