using Enums;
using ValueObjects;

namespace Abstractions;

public interface IReportQuery
{
    public ProcessStatuses Status { get; }
    
    public Period Period { get; }
    
    public Guid CaseId { get; }
    
    public Guid ProductId { get; }

    public void UpdateStatus(ProcessStatuses status);
}