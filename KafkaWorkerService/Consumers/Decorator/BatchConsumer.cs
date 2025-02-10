using Confluent.Kafka;
using KafkaWorkerService.ConsumerObserver;
using KafkaWorkerService.Consumers.PureFabrication;
using ReportGenerator;

namespace KafkaWorkerService.Consumers.Decorator;

public class BatchConsumer : IConsumerDecorator, IObserver
{
    private readonly uint _hoursLimit;
    private readonly BatchSize _batchSizer;
    private readonly IConsumer<Null, byte[]> _consumer;
    private readonly List<ReportRequest> _messageBatch = new();
    private readonly ReportGenerator.ReportGeneratorService.ReportGeneratorServiceClient _grpcService;

    public BatchConsumer(
        IConsumer<Null, byte[]> consumer,
        BatchSize batchSizer,
        ReportGenerator.ReportGeneratorService.ReportGeneratorServiceClient grpcService,
        uint hoursLimit)
    {
        _batchSizer = batchSizer;
        _consumer = consumer;
        _grpcService = grpcService;
        _hoursLimit = hoursLimit;
    }

    public async Task ConsumingAsync()
{

    while (true)
    {

        var messageBatch = new List<ReportRequest>();
        DateTime startTime = DateTime.UtcNow;
        
        while ((DateTime.Now - startTime) < TimeSpan.FromHours(_hoursLimit))
        {
            ConsumeResult<Null, byte[]> consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));
            
            if (consumeResult == null)
            {
                continue;
            }

            ReportRequest message = ReportRequest.Parser.ParseFrom(consumeResult.Message.Value);
            
            messageBatch.Add(message);
            _batchSizer.Inc();

            if (_batchSizer.IsLimitReached)
            {
                break;
            }
        }

        if (messageBatch.Count > 0)
        {
            var createReportsRequest = new CreateReportsRequest
            {
                Reports = { messageBatch }
            };
            
            await _grpcService.GenerateReportsAsync(createReportsRequest);
        }

        _batchSizer.Flush();
        
        await Task.Delay(TimeSpan.FromHours(_hoursLimit));
    }
}

    public async Task OnNotifyAsync(uint topicMessageCount)
    {
        while (!_batchSizer.IsLimitReached)
        {
            ConsumeResult<Null, byte[]> consumeResult = _consumer.Consume();
            _messageBatch.Add(ReportRequest.Parser.ParseFrom(consumeResult.Message.Value));
            
            _batchSizer.Inc();
        }
        
        var createReportsRequest = new CreateReportsRequest
        {
            Reports = { _messageBatch }
        };
        
        await _grpcService.GenerateReportsAsync(createReportsRequest);
        
        _batchSizer.Flush();
    }

    public void OnNotify(uint topicMessageCount)
    {
        while (!_batchSizer.IsLimitReached)
        {
            ConsumeResult<Null, byte[]> consumeResult = _consumer.Consume();
            _messageBatch.Add(ReportRequest.Parser.ParseFrom(consumeResult.Message.Value));
            
            _batchSizer.Inc();
        }
        
        var createReportsRequest = new CreateReportsRequest
        {
            Reports = { _messageBatch }
        };
        
        _grpcService.GenerateReports(createReportsRequest);
        
        _batchSizer.Flush();
    }
}