using Enums;

namespace Abstractions.ResultTypes;

public abstract record ReportDbQueryResult
{
    public sealed record SuccessesFindOneReport(IReport Report) : ReportDbQueryResult;
    
    public sealed record SuccessesFindOneReportStatus(ProcessStatuses Status) : ReportDbQueryResult;
    
    public sealed record SuccessesFindOneReportQuery(IReportQuery ReportQuery) : ReportDbQueryResult;
    
    public sealed record SuccessesFindAllReport(IEnumerable<IReport> Reports) : ReportDbQueryResult;
    
    public sealed record SuccessesFindAllReportQueries(IEnumerable<IReportQuery> ReportQueries) : ReportDbQueryResult;
    
    public sealed record SuccessUpdate : ReportDbQueryResult;
    
    public sealed record SuccessCreate : ReportDbQueryResult;
    
    public sealed record Failing(Guid Id, string Message) : ReportDbQueryResult;
    
    public sealed record DoesNotExist : ReportDbQueryResult;
}