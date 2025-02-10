using Abstractions;
using Abstractions.Repositories;
using Abstractions.ResultTypes;
using Contracts;
using Dto.RequestsDto;
using Dto.ResponsesDto;
using Enums;
using Plugins;

namespace Services;

public class ReportQueryService : IReportQueryService
{
    private readonly IReportQueryRepository _reportQueryRepository;
    private readonly ICachingReportQueryRepository _cacheReportQueryRepository;

    public ReportQueryService(ICachingReportQueryRepository cacheReportQueryRepository, IReportQueryRepository reportQueryRepository)
    {
        _cacheReportQueryRepository = cacheReportQueryRepository;
        _reportQueryRepository = reportQueryRepository;
    }

    public async Task<ReportStatusResponseDto> TryGetReportStatusAsync(Guid reportQueryId)
    {
        ReportDbQueryResult reportQueryResultInCache = await _cacheReportQueryRepository.TryGetReportStatusByIdAsync(reportQueryId);

        if (reportQueryResultInCache is ReportDbQueryResult.SuccessesFindOneReportStatus cacheReportQueryResult)
        {
            return new ReportStatusResponseDto()
            {
                IsSuccess = true,
                Status = cacheReportQueryResult.Status.ToString()
            };
        }

        ReportDbQueryResult reportResultInDb = await _reportQueryRepository.TryGetReportStatusByIdAsync(reportQueryId);

        if (reportResultInDb is ReportDbQueryResult.SuccessesFindOneReportStatus dbReportResult)
        {
            return new ReportStatusResponseDto()
            {
                IsSuccess = true,
                Status = dbReportResult.Status.ToString()
            };
        }

        return new ReportStatusResponseDto()
        {
            IsSuccess = false,
            Message = new ReportQueryResult.FailureResult("Status not found").ToString()
        };
    }

    public async Task<ReportQueryResult> InsertReportQueryAsync(ReportRequestDto reportRequestDto)
    {
        IReportQuery reportQuery = reportRequestDto.ToReportQuery();
        
        try
        {
            await Task.WhenAll(
                _reportQueryRepository.InsertReportQueryAsync(reportQuery),
                _cacheReportQueryRepository.InsertReportAsync(reportQuery));
            return new ReportQueryResult.SuccessCreateResult();
        }
        catch (Exception)
        {
            return new ReportQueryResult.FailureResult("Can't insert reportQuery");
        }
    }

    public async Task<ReportQueryResult> UpdateReportQueryStatusAsync(IReportQuery reportQuery, ProcessStatuses newStatus)
    {
        reportQuery.UpdateStatus(newStatus);
        try
        {
            await _cacheReportQueryRepository.UpdateStatusAsync(reportQuery);
            return new ReportQueryResult.SuccessUpdateResult();
        }
        catch (Exception)
        {
            return new ReportQueryResult.FailureResult("Can't update report query status");
        }
    }
}