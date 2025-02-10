using Abstractions.ResultTypes;
using Dto;

namespace Abstractions.Repositories;

public interface IReportRepository
{ 
    public Task<ReportDbQueryResult> TryGetReportByIdAsync(Guid id);
    
    public Task InsertReportAsync(IReport report);

    public Task<ConversionReportDataDto> CalculateConversionRatioAsync(IReportQuery reportQuery);
}