namespace KafkaWorkerService.Consumers.PureFabrication;

public class BatchSize
{
    private readonly uint _bathLimit;

    public BatchSize(uint bathLimit)
    {
        Size = 0;
        _bathLimit = bathLimit;
    }

    public void Flush()
    {
        Size = 0;
    }

    public void Inc()
    {
        Size++;
    }

    public bool IsLimitReached => Size == _bathLimit;
    
    public uint Size { get; private set; }
}