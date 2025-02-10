using Dto.RequestsDto;
using Dto.ResponsesDto;
using Enums;

namespace Contracts;

public interface IReportService
{
    public Task<ReportResponseDto> TryGetReportAsync(Guid reportId);

    public IEnumerable<Task<ReportResponseDto>> TryGetReportsAsync(IEnumerable<Guid> reportIds);

    public Task<ProcessStatuses> CreateReportAsync(CreateReportRequestDto requestDto);
}