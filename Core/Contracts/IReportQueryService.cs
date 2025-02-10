using Abstractions;
using Abstractions.ResultTypes;
using Dto.RequestsDto;
using Dto.ResponsesDto;
using Enums;

namespace Contracts;

public interface IReportQueryService
{
    public Task<ReportStatusResponseDto> TryGetReportStatusAsync(Guid reportQueryId);

    public Task<ReportQueryResult> InsertReportQueryAsync(ReportRequestDto reportRequestDto);

    public Task<ReportQueryResult> UpdateReportQueryStatusAsync(IReportQuery reportQuery, ProcessStatuses newStatus);
}