using Confluent.Kafka;
using KafkaWorkerService.Consumers.Builders.Abstractions;
using KafkaWorkerService.Consumers.Decorator;
using KafkaWorkerService.Consumers.PureFabrication;

namespace KafkaWorkerService.Consumers.Builders.Models;

public class BaseConsumerFactory : IConsumerFactory
{
    private readonly ReportQueryManaging.ReportQueryService.ReportQueryServiceClient _reportQueryService;
    private readonly ReportGenerator.ReportGeneratorService.ReportGeneratorServiceClient _reportGeneratorService;
    private readonly uint _hoursLimit;
    private readonly uint _batchSize;

    public BaseConsumerFactory(
        uint batchSize,
        uint hoursLimit,
        ReportQueryManaging.ReportQueryService.ReportQueryServiceClient reportQueryService,
        ReportGenerator.ReportGeneratorService.ReportGeneratorServiceClient reportGeneratorService)
    {
        _batchSize = batchSize;
        _hoursLimit = hoursLimit;
        _reportQueryService = reportQueryService;
        _reportGeneratorService = reportGeneratorService;
    }

    public IConsumerDecorator CreateFastConsumer(string topic, string consumerGroup, string stringServerConnection)
    {
        ConsumerConfig consumerConfig = new()
        {
            BootstrapServers = stringServerConnection,
            GroupId = consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        
        BatchSize batchSizer = new(_batchSize);

        IConsumer<Null, byte[]> fastConsumer = new ConsumerBuilder<Null, byte[]>(consumerConfig).Build();
        
        fastConsumer.Subscribe(topic);
        
        return new FastConsumer(fastConsumer, batchSizer, _reportQueryService);
    }

    public IConsumerDecorator CreateBatchConsumer(string topic, string consumerGroup, string stringServerConnection)
    {
        ConsumerConfig consumerConfig = new()
        {
            BootstrapServers = stringServerConnection,
            GroupId = consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        BatchSize batchSizer = new(_batchSize);

        IConsumer<Null, byte[]> batchConsumer = new ConsumerBuilder<Null, byte[]>(consumerConfig).Build();
        
        batchConsumer.Subscribe(topic);
        
        return new BatchConsumer(batchConsumer, batchSizer, _reportGeneratorService, _hoursLimit);
    }
}