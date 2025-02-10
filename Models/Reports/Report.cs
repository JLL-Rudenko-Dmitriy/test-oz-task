using Abstractions;
using ValueObjects;

namespace Models.Reports;

public class Report : IReport
{
    
    public Report(Guid caseId, ConversionRatio conversionRatio, PaymentsCount paymentsCount)
    {
        CaseId = caseId;
        ConversionRatio = conversionRatio;
        PaymentsCount = paymentsCount;
    }

    public Guid CaseId { get; }

    public ConversionRatio ConversionRatio { get; }
    
    public PaymentsCount PaymentsCount { get; }
}