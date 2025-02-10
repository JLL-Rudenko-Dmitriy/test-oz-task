using Abstractions.ResultTypes;

namespace Abstractions.Repositories;

public interface ICachingReportQueryRepository
{
    public Task<ReportDbQueryResult> TryGetReportStatusByIdAsync(Guid caseId);
    
    public Task<ReportDbQueryResult> InsertReportAsync(IReportQuery reportQuery);
    
    public Task<ReportDbQueryResult> UpdateStatusAsync(IReportQuery reportQuery);
    
    public Task DeleteExpiredReportQueriesAsync();
}