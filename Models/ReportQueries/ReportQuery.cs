using Abstractions;
using Enums;
using ValueObjects;

namespace Models.ReportQueries;

public class ReportQuery : IReportQuery
{

    public ReportQuery(short status, Period period, Guid caseId, Guid productId)
    {
        Status = (ProcessStatuses)status;
        Period = period;
        CaseId = caseId;
        ProductId = productId;
    }
    
    public ProcessStatuses Status { get; private set; }
    
    public Period Period { get; }
    
    public Guid CaseId { get; }
    
    public Guid ProductId { get; }
    
    public void UpdateStatus(ProcessStatuses status)
    {
        Status = status;
    }
}