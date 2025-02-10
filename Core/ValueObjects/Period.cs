namespace ValueObjects;

public record struct Period
{
    public Period(DateTime dateStart, DateTime dateEnd)
    {
        DateStart = dateStart;
        DateEnd = dateEnd;
        
        if (DateStart > DateEnd)
            throw new ArgumentException("The start date must be before the end of the period.");
    }
    
    public DateTime DateStart { get; }

    public DateTime DateEnd { get; }
}