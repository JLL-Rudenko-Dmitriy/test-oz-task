using Contracts;
using Dto.RequestsDto;
using Enums;
using Grpc.Core;
using ReportGenerator;

namespace ReportService.Services;

public class ReportGeneratorService : ReportGenerator.ReportGeneratorService.ReportGeneratorServiceBase
{
    private readonly IReportService _reportService;

    public ReportGeneratorService(IReportService reportService)
    {
        _reportService = reportService;
    }
    
    public override async Task<CreateReportsResponse> GenerateReports(CreateReportsRequest request, ServerCallContext context)
    {
        List<FailReportCreationData> failReports = [];
        
        foreach (ReportRequest? reportRequest in request.Reports)
        {
            CreateReportRequestDto requestDto = new()
            {
                CaseId = Guid.Parse(reportRequest.CaseId),
                ProductId = Guid.Parse(reportRequest.ProductId),
                DateStart = DateTime.Parse(reportRequest.DateStart),
                DateEnd = DateTime.Parse(reportRequest.DateEnd)
            };

            ProcessStatuses result = await _reportService.CreateReportAsync(requestDto);
           if (result is ProcessStatuses.Failed)
           {
               failReports.Add(new FailReportCreationData()
               {
                   CaseId = reportRequest.CaseId,
                   Message = "Was an error while creating reports!",
               });
           }
        }

        return new CreateReportsResponse()
        {
            SuccessfulCount = (ulong)request.Reports.Count - (ulong)failReports.Count,
            FailedReports =
            {
                failReports
            }
        };
    }
}