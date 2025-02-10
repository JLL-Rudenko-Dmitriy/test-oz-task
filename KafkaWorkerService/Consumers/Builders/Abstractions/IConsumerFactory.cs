using KafkaWorkerService.Consumers.Decorator;

namespace KafkaWorkerService.Consumers.Builders.Abstractions;

public interface IConsumerFactory
{
    public IConsumerDecorator CreateFastConsumer(string topic, string consumerGroup, string stringServerConnection);
    
    public IConsumerDecorator CreateBatchConsumer(string topic, string consumerGroup, string stringServerConnection);
}