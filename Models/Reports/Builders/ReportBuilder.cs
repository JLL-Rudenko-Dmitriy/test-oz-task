using Abstractions;
using Abstractions.Builders.ReportBuilders;
using ValueObjects;

namespace Models.Reports.Builders;

public class ReportBuilder : IReportBuilder
{
    private Guid? _caseId;

    private ConversionRatio? _conversionRatio;
    
    private PaymentsCount? _paymentsCount;

    public IReportBuilder WithCaseId(Guid caseId)
    {
        _caseId = caseId;
        return this;
    }

    public IReportBuilder WithConversionRatio(ConversionRatio conversionRatio)
    {
        _conversionRatio = conversionRatio;
        return this;
    }

    public IReportBuilder WithPaymentsCount(PaymentsCount paymentsCount)
    {
        _paymentsCount = paymentsCount;
        return this;
    }

    public IReport Build()
    {
        return new Report(
            _caseId ?? throw new ArgumentNullException(nameof(_caseId)),
            _conversionRatio ?? throw new ArgumentNullException(nameof(_conversionRatio)),
            _paymentsCount ?? throw new ArgumentNullException(nameof(_paymentsCount)));
    }
}