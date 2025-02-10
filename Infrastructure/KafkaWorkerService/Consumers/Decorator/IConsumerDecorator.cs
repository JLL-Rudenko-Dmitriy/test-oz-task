namespace KafkaWorkerService.Consumers.Decorator;

public interface IConsumerDecorator
{
    public Task ConsumingAsync();
}