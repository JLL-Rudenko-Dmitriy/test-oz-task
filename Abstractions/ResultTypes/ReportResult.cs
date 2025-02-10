namespace Abstractions.ResultTypes;

public abstract record ReportResult
{
    public sealed record SuccessResult(IReport Report) : ReportResult;
    
    public sealed record FailureResult(string Message) : ReportResult;
}