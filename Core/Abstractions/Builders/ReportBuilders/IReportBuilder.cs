using ValueObjects;

namespace Abstractions.Builders.ReportBuilders;

public interface IReportBuilder
{
    public IReportBuilder WithCaseId(Guid caseId);
    
    public IReportBuilder WithConversionRatio(ConversionRatio conversionRatio);
    
    public IReportBuilder WithPaymentsCount(PaymentsCount paymentsCount);
    
    public IReport Build();
}