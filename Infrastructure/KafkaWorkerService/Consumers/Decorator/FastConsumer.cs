using Confluent.Kafka;
using KafkaWorkerService.ConsumerObserver;
using KafkaWorkerService.Consumers.PureFabrication;
using ReportQueryManaging;

namespace KafkaWorkerService.Consumers.Decorator;

public class FastConsumer : IConsumerDecorator, IObservable
{
    private readonly ReportQueryManaging.ReportQueryService.ReportQueryServiceClient _grpcService;
    private readonly IConsumer<Null, byte[]> _consumer;
    private readonly List<IObserver> _observers;
    
    private readonly BatchSize _batchSizer;
    
    public FastConsumer(
        IConsumer<Null, byte[]> consumer,
        BatchSize batchSizer,
        ReportQueryManaging.ReportQueryService.ReportQueryServiceClient grpcService) 
    {
        _consumer = consumer;
        _batchSizer = batchSizer;
        _grpcService = grpcService;
        _observers = [];
    }

    public async Task ConsumingAsync()
    {
        while (true)
        {
            try
            {
                ConsumeResult<Null, byte[]> consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));
                if (consumeResult == null)
                {
                    await Task.Delay(500);
                    continue;
                }

                await _grpcService.CreateGenerationRequestAsync(
                    ReportRequest.Parser.ParseFrom(consumeResult.Message.Value));

                _batchSizer.Inc();

                if (_batchSizer.IsLimitReached)
                {
                    NotifyObservers(_batchSizer.Size);
                    _batchSizer.Flush();
                }
            }
            catch (Exception)
            {
                await Task.Delay(1000);
            }
        }
    }

    public void AddObserver(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void RemoveObserver(IObserver observer)
    {
        _observers.Remove(observer);
    }

    public void NotifyObservers(uint topicMessageCount)
    {
        foreach (IObserver observer in _observers)
        {
            observer.OnNotifyAsync(topicMessageCount);
        }
    }
}