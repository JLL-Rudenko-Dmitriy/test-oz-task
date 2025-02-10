using Enums;

namespace Abstractions.ResultTypes;

public abstract record ReportQueryResult
{
    public sealed record SuccessResult(IReportQuery Report) : ReportQueryResult;
    
    public sealed record SuccessStatusResult(ProcessStatuses Status) : ReportQueryResult;

    public sealed record SuccessCreateResult : ReportQueryResult;

    public sealed record SuccessUpdateResult : ReportQueryResult;
    
    public sealed record FailureResult(string Message) : ReportQueryResult;
} 