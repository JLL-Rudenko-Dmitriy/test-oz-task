namespace ValueObjects;

public record struct UserViews
{
    public UserViews(ulong value)
    {
        Count = value;
    }
    
    public ulong Count { get; }
}