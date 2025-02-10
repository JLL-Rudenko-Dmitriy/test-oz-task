using Abstractions.ResultTypes;

namespace Abstractions.Repositories;

public interface IReportQueryRepository
{
    public Task<ReportDbQueryResult> TryGetReportStatusByIdAsync(Guid reportQueryId);
    
    public Task InsertReportQueryAsync(IReportQuery reportQuery);
    
    public Task<ReportDbQueryResult> UpdateReportQueryAsync(IReportQuery updatedReportQuery);
}