using Abstractions;
using Dto.RequestsDto;
using Dto.ResponsesDto;
using Enums;
using Models.ReportQueries.Builders;
using ValueObjects;

namespace Plugins;

public static class DtoMapper
{
    public static IReportQuery ToReportQuery(this CreateReportRequestDto requestDto)
    {
        return new ReportQueryBuilder()
            .WithPeriod(new Period(requestDto.DateStart, requestDto.DateEnd))
            .WithStatus(ProcessStatuses.Pending)
            .WithCaseId(requestDto.CaseId)
            .WithProductId(requestDto.ProductId)
            .Build();
    }
    
    public static IReportQuery ToReportQuery(this ReportRequestDto requestDto)
    {
        return new ReportQueryBuilder()
            .WithPeriod(new Period(requestDto.DateStart, requestDto.DateEnd))
            .WithStatus(ProcessStatuses.Pending)
            .WithCaseId(requestDto.CaseId)
            .WithProductId(requestDto.ProductId)
            .Build();
    }
    
    public static ReportResponseDto ToResponseDto(this IReport report)
    {
        return new ReportResponseDto()
        {
            IsSuccess = true,
            ConversionRatio = report.ConversionRatio.Ratio,
            PaymentsCount = report.PaymentsCount.Count,
        };
    }
}