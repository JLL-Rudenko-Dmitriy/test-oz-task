namespace ValueObjects;

public record struct ConversionRatio
{
    public ConversionRatio(decimal conversionRatio)
    {
        Ratio = conversionRatio;
    }
    
    public decimal Ratio { get; }
}