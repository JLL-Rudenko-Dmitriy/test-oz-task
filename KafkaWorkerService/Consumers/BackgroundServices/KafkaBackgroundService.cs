using KafkaWorkerService.Consumers.Decorator;

namespace KafkaWorkerService.Consumers.BackgroundServices;

public class KafkaBackgroundService<T> : BackgroundService
{
    private readonly IConsumerDecorator _consumer;
    private readonly ILogger<T> _logger;

    public KafkaBackgroundService(IConsumerDecorator consumerDecorator, ILogger<T> logger)
    {
        _consumer = consumerDecorator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            Task consumingTask = _consumer.ConsumingAsync();

            await Task.WhenAny(consumingTask, Task.Delay(1000, stoppingToken));
        }
    }
}