namespace ValueObjects;

public record struct PaymentsCount
{
    public PaymentsCount(ulong value)
    {
        Count = value;
    }
    
    public ulong Count { get; }
}