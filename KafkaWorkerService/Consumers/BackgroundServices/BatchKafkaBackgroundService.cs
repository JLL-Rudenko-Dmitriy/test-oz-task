using KafkaWorkerService.Consumers.Decorator;

namespace KafkaWorkerService.Consumers.BackgroundServices;

public class BatchKafkaBackgroundService : KafkaBackgroundService<BatchKafkaBackgroundService>
{
    public BatchKafkaBackgroundService(
        IConsumerDecorator consumerDecorator,
        ILogger<BatchKafkaBackgroundService> logger)
        : base(consumerDecorator, logger)
    {
        logger.LogInformation("BatchKafkaBackgroundService initialized!");
    }
}
