namespace KafkaWorkerService.ConsumerObserver;

public interface IObserver
{
    public void OnNotify(uint topicMessageCount);

    public Task OnNotifyAsync(uint topicMessageCount);
}