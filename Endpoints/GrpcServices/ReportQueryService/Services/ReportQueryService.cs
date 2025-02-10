using Contracts;
using Dto.RequestsDto;
using Dto.ResponsesDto;
using Enums;
using Grpc.Core;
using ReportQueryManaging;

namespace ReportQueryService.Services;


public class ReportQueryService : ReportQueryManaging.ReportQueryService.ReportQueryServiceBase
{
    private readonly IReportQueryService _reportQueryService;
    private readonly IReportService _reportService;

    public ReportQueryService(IReportQueryService reportQueryService, IReportService reportService)
    {
        _reportQueryService = reportQueryService;
        _reportService = reportService;
    }

    public override async Task<ReportResponse> GetReport(GetReportRequest request, ServerCallContext context)
    {
        var caseId = Guid.Parse(request.CaseId);

        ReportStatusResponseDto reportStatusDto = await _reportQueryService.TryGetReportStatusAsync(caseId);
        if (!reportStatusDto.IsSuccess)
        {
            return new ReportResponse() { Status = $"There were no requests for a report by ID: {caseId}"};
        }

        if (!string.Equals(reportStatusDto.Status, "Done",  StringComparison.OrdinalIgnoreCase))
        {
            return new ReportResponse() { Status = $"Report is not done! Report status is {reportStatusDto.Status}"};
        }
        
        ReportResponseDto responseDto = await _reportService.TryGetReportAsync(caseId);
        if (!responseDto.IsSuccess)
        {
            throw new RpcException(new Status(StatusCode.NotFound, responseDto.ErrorMessage));
        }

        return new ReportResponse()
        {
            Report = new ReportData()
            {
                ConversionRatio = (double)responseDto.ConversionRatio,
                PaymentsCount = responseDto.PaymentsCount,
            }
        };
    }

    public override async Task<ReportQueryResponse> CreateGenerationRequest(ReportRequest request, ServerCallContext context)
    {
        ReportRequestDto requestDto = new()
        {
            CaseId = Guid.Parse(request.CaseId),
            ProductId = Guid.Parse(request.ProductId),
            DateStart = DateTime.Parse(request.DateStart),
            DateEnd = DateTime.Parse(request.DateEnd)
        };

        ReportStatusResponseDto reportStatus = await _reportQueryService.TryGetReportStatusAsync(requestDto.CaseId);
        if (reportStatus.IsSuccess)
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"Report query with ID: {requestDto.CaseId} is already exists"));
        
        await _reportQueryService.InsertReportQueryAsync(requestDto);
        
        return new ReportQueryResponse()
        {
            CaseId = requestDto.CaseId.ToString(),
            Status = ProcessStatuses.Pending.ToString(),
        };
    }
}