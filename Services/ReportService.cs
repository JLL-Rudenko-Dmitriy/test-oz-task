using Abstractions;
using Abstractions.Repositories;
using Abstractions.ResultTypes;
using Contracts;
using Dto.RequestsDto;
using Dto.ResponsesDto;
using Enums;
using Models.Reports.Builders;
using Plugins;
using ValueObjects;

namespace Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly ICachingReportRepository _cacheReportRepository;
    private readonly IReportQueryRepository _reportQueryRepository;
    private readonly ICachingReportQueryRepository _cacheReportQueryRepository;

    public ReportService(
        IReportRepository reportRepository,
        ICachingReportRepository cacheReportRepository,
        IReportQueryRepository reportQueryRepository,
        ICachingReportQueryRepository cacheReportQueryRepository)
    {
        _reportRepository = reportRepository;
        _cacheReportRepository = cacheReportRepository;
        _reportQueryRepository = reportQueryRepository;
        _cacheReportQueryRepository = cacheReportQueryRepository;
    }

    public async Task<ReportResponseDto> TryGetReportAsync(Guid reportId)
    {
        ReportDbQueryResult reportResultInCache = await _cacheReportRepository.GetReportByIdAsync(reportId);

        if (reportResultInCache is ReportDbQueryResult.SuccessesFindOneReport cacheReportResult)
            return cacheReportResult.Report.ToResponseDto();

        ReportDbQueryResult reportResultInDb = await _reportRepository.TryGetReportByIdAsync(reportId);

        if (reportResultInDb is ReportDbQueryResult.SuccessesFindOneReport dbReportResult)
            return dbReportResult.Report.ToResponseDto();

        return new ReportResponseDto()
        {
            IsSuccess = false,
            ErrorMessage = new ReportResult.FailureResult("Report not found").ToString()
        };
    }

    public IEnumerable<Task<ReportResponseDto>> TryGetReportsAsync(IEnumerable<Guid> reportIds)
    {
        foreach (Guid reportId in reportIds)
        {
            yield return TryGetReportAsync(reportId);
        }
    }

    public async Task<ProcessStatuses> CreateReportAsync(CreateReportRequestDto requestDto)
    {
        IReportQuery reportQuery = requestDto.ToReportQuery();

        Dto.ConversionReportDataDto calculateConversionTask = await _reportRepository.CalculateConversionRatioAsync(reportQuery);

        if (calculateConversionTask.ConversionRatio is null)
        {
            return ProcessStatuses.Failed;
        }
        
        decimal ratioValue = calculateConversionTask.ConversionRatio.Value;
        
        var conversionRatio = new ConversionRatio(ratioValue);
        var paymentsCount = new PaymentsCount(calculateConversionTask.PaymentsCount);

        Abstractions.Builders.ReportBuilders.IReportBuilder reportBuilder = new ReportBuilder()
            .WithCaseId(reportQuery.CaseId)
            .WithConversionRatio(conversionRatio)
            .WithPaymentsCount(paymentsCount);

        IReport report;
        try
        {
            report = reportBuilder.Build();
        }
        catch (Exception)
        {
            return ProcessStatuses.Failed;
        }

        try
        {
            Task<ReportDbQueryResult> cacheCreatingTask = _cacheReportRepository.InsertReportAsync(report);
            Task reportCreationTask = _reportRepository.InsertReportAsync(report);

            await Task.WhenAll(cacheCreatingTask, reportCreationTask);

            reportQuery.UpdateStatus(ProcessStatuses.Done);
            
            Task<ReportDbQueryResult> cacheReportQueryUpdateTask = _cacheReportQueryRepository.UpdateStatusAsync(reportQuery);
            Task<ReportDbQueryResult> reportQueryUpdateTask = _reportQueryRepository.UpdateReportQueryAsync(reportQuery);

            await Task.WhenAll(cacheReportQueryUpdateTask, reportQueryUpdateTask);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create report", ex);
        }

        return ProcessStatuses.Done;
    }
}