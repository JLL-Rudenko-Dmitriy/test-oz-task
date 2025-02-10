using Enums;
using ValueObjects;

namespace Abstractions.Builders.ReportQueryBuilders;

public interface IReportQueryBuilder
{
    public IReportQueryBuilder WithStatus(ProcessStatuses status);
    
    public IReportQueryBuilder WithPeriod(Period period);
    
    public IReportQueryBuilder WithCaseId(Guid caseId);
    
    public IReportQueryBuilder WithProductId(Guid productId);
    
    public IReportQuery Build();
}