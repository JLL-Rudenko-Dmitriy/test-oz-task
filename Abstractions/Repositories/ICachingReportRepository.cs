using Abstractions.ResultTypes;

namespace Abstractions.Repositories;

public interface ICachingReportRepository
{
    public Task<ReportDbQueryResult> GetReportByIdAsync(Guid caseId);
    
    public Task<ReportDbQueryResult> InsertReportAsync(IReport report);
}