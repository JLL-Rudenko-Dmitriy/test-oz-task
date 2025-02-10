using KafkaWorkerService.Consumers.Decorator;

namespace KafkaWorkerService.Consumers.BackgroundServices;

public class FastKafkaBackgroundService : KafkaBackgroundService<FastKafkaBackgroundService>
{
    public FastKafkaBackgroundService(IConsumerDecorator consumerDecorator, ILogger<FastKafkaBackgroundService> logger)
        : base(consumerDecorator, logger)
    {
        logger.LogInformation("BatchKafkaBackgroundService initialized!");
    }
}