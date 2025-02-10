using Abstractions;
using Abstractions.Repositories;
using Abstractions.ResultTypes;
using Contracts;
using Dto;
using Dto.RequestsDto;
using Dto.ResponsesDto;
using Enums;
using System.Globalization;
using System.Text.RegularExpressions;
using Moq;
using Plugins;
using Services;
using ValueObjects;

namespace ConversionServiceTests;

public partial class UnitTests
{
    [Fact]
    public void IsValid_Period_Assert_True()
    {
        var dateFirst = DateTime.Parse("2025-01-05 09:29:35");
        var dateSecond = DateTime.Parse("2025-05-05 00:20:30");

        Period period = new(dateFirst, dateSecond);

        Assert.True(
            IsValidDateRegex()
                .Match(
                    period.DateStart.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                .Success &&
            IsValidDateRegex()
                .Match(
                    period.DateEnd.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                .Success);
    }

    [Fact]
    public void Validate_Invalid_Period_Assert_ArgumentException()
    {
        var dateFirst = DateTime.Parse("2025-05-05 00:20:30");
        var dateSecond = DateTime.Parse("2025-01-05 09:29:35");

        Assert.Throws<ArgumentException>(() => new Period(dateFirst, dateSecond));
    }

    [Fact]
    public async Task Get_ReportStatus_Assert_Success()
    {
        var existingGuid = Guid.NewGuid();
        ReportStatusResponseDto expectedReportRequestDto = new()
        {
            IsSuccess = true,
            Status = ProcessStatuses.Pending.ToString(),
        };

        Mock<ICachingReportQueryRepository> mockCacheRepository = new();
        mockCacheRepository
            .Setup(repo => repo.TryGetReportStatusByIdAsync(existingGuid))
            .ReturnsAsync(new ReportDbQueryResult.SuccessesFindOneReportStatus(ProcessStatuses.Pending));

        Mock<IReportQueryRepository> mockReportQueryRepository = new();
        mockReportQueryRepository
            .Setup(repo => repo.TryGetReportStatusByIdAsync(existingGuid))
            .ReturnsAsync(new ReportDbQueryResult.SuccessesFindOneReportStatus(ProcessStatuses.Pending));

        IReportQueryService reportQueryService = new ReportQueryService(
            mockCacheRepository.Object,
            mockReportQueryRepository.Object
        );

        ReportStatusResponseDto actualReportRequestDto = await reportQueryService.TryGetReportStatusAsync(existingGuid);

        Assert.Equal(expectedReportRequestDto, actualReportRequestDto);
    }

    [Fact]
    public async Task Get_Unexist_ReportStatus_Assert_NotFound()
    {
        var existingGuid = Guid.NewGuid();
        ReportStatusResponseDto expectedReportRequestDto = new()
        {
            IsSuccess = false,
            Message = new ReportQueryResult.FailureResult("Status not found").ToString()
        };

        Mock<ICachingReportQueryRepository> mockCacheRepository = new();
        mockCacheRepository
            .Setup(repo => repo.TryGetReportStatusByIdAsync(existingGuid))
            .ReturnsAsync(new ReportDbQueryResult.DoesNotExist());

        Mock<IReportQueryRepository> mockReportQueryRepository = new();
        mockReportQueryRepository
            .Setup(repo => repo.TryGetReportStatusByIdAsync(existingGuid))
            .ReturnsAsync(new ReportDbQueryResult.DoesNotExist());

        IReportQueryService reportQueryService = new ReportQueryService(
            mockCacheRepository.Object,
            mockReportQueryRepository.Object
        );

        ReportStatusResponseDto actualReportRequestDto = await reportQueryService.TryGetReportStatusAsync(existingGuid);

        Assert.Equal(expectedReportRequestDto, actualReportRequestDto);
    }

    [Fact]
    public async Task TryCreate_New_ReportQuery_Assert_Success()
    {
        ReportQueryResult expectedResult = new ReportQueryResult.SuccessCreateResult();

        var dateFirst = DateTime.Parse("2025-01-05 09:29:35");
        var dateSecond = DateTime.Parse("2025-05-05 00:20:30");

        var productId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        Period period = new(dateFirst, dateSecond);

        var requestDto = new ReportRequestDto()
        {
            CaseId = caseId,
            ProductId = productId,
            DateStart = period.DateStart,
            DateEnd = period.DateEnd,
        };

        Mock<ICachingReportQueryRepository> mockCacheRepository = new();
        mockCacheRepository
            .Setup(repo => repo.InsertReportAsync(requestDto.ToReportQuery()))
            .ReturnsAsync(new ReportDbQueryResult.SuccessCreate());

        Mock<IReportQueryRepository> mockReportQueryRepository = new();
        mockReportQueryRepository
            .Setup(repo => repo.InsertReportQueryAsync(requestDto.ToReportQuery()));

        IReportQueryService reportQueryService = new ReportQueryService(
            mockCacheRepository.Object,
            mockReportQueryRepository.Object
        );

        ReportQueryResult actualResult = await reportQueryService.InsertReportQueryAsync(requestDto);

        Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public async Task TryCreate_Existing_ReportQuery_Assert_AlreadyExists()
    {
        string expectedMessage = "Can't insert reportQuery";

        var dateFirst = DateTime.Parse("2025-01-05 09:29:35");
        var dateSecond = DateTime.Parse("2025-05-05 00:20:30");

        var productId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        Period period = new(dateFirst, dateSecond);

        var requestDto = new ReportRequestDto()
        {
            CaseId = caseId,
            ProductId = productId,
            DateStart = period.DateStart,
            DateEnd = period.DateEnd,
        };

        Mock<ICachingReportQueryRepository> mockCacheRepository = new();
        mockCacheRepository
            .Setup(repo => repo.InsertReportAsync(It.IsAny<IReportQuery>()))
            .ThrowsAsync(new InvalidOperationException("Something went wrong"));

        Mock<IReportQueryRepository> mockReportQueryRepository = new();
        mockReportQueryRepository
            .Setup(repo => repo.InsertReportQueryAsync(It.IsAny<IReportQuery>()))
            .ThrowsAsync(new InvalidOperationException("Something went wrong"));

        IReportQueryService reportQueryService = new ReportQueryService(
            mockCacheRepository.Object,
            mockReportQueryRepository.Object
        );

        ReportQueryResult actualResult = await reportQueryService.InsertReportQueryAsync(requestDto);

        Assert.IsType<ReportQueryResult.FailureResult>(actualResult);
        Assert.Equal(expectedMessage, ((ReportQueryResult.FailureResult)actualResult).Message);

        mockReportQueryRepository
            .Verify(repo => repo.InsertReportQueryAsync(It.IsAny<IReportQuery>()), Times.Once);
        mockCacheRepository
            .Verify(repo => repo.InsertReportAsync(It.IsAny<IReportQuery>()), Times.Once);
    }

    [Fact]
    public async Task TryGenerate_New_Report_Assert_Success()
    {
        Mock<ICachingReportQueryRepository> mockCacheReportQueryRepository = new();
        mockCacheReportQueryRepository
            .Setup(repo => repo.UpdateStatusAsync(It.IsAny<IReportQuery>()));

        Mock<IReportQueryRepository> mockReportQueryRepository = new();
        mockReportQueryRepository
            .Setup(repo => repo.UpdateReportQueryAsync(It.IsAny<IReportQuery>()));

        Mock<ICachingReportRepository> mockCacheRepository = new();
        mockCacheRepository
            .Setup(repo => repo.InsertReportAsync(It.IsAny<IReport>()));

        Mock<IReportRepository> mockReportRepository = new();
        mockReportRepository
            .Setup(repo => repo.CalculateConversionRatioAsync(It.IsAny<IReportQuery>()))
            .ReturnsAsync(new ConversionReportDataDto(1.0m, 10));


        var dateFirst = DateTime.Parse("2025-01-05 09:29:35");
        var dateSecond = DateTime.Parse("2025-05-05 00:20:30");

        var productId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        CreateReportRequestDto createReportRequestDto = new()
        {
            CaseId = caseId,
            ProductId = productId,
            DateStart = dateFirst,
            DateEnd = dateSecond
        };

        IReportService reportService = new ReportService(
            mockReportRepository.Object,
            mockCacheRepository.Object,
            mockReportQueryRepository.Object,
            mockCacheReportQueryRepository.Object);

        ProcessStatuses expectedStatus = ProcessStatuses.Done;
        ProcessStatuses actualStatus = await reportService.CreateReportAsync(createReportRequestDto);

        mockCacheReportQueryRepository
            .Verify(repo => repo.UpdateStatusAsync(It.IsAny<IReportQuery>()), Times.Once);
        mockReportQueryRepository
            .Verify(repo => repo.UpdateReportQueryAsync(It.IsAny<IReportQuery>()), Times.Once);
        mockCacheRepository
            .Verify(repo => repo.InsertReportAsync(It.IsAny<IReport>()), Times.Once);
        mockReportRepository
            .Verify(repo => repo.CalculateConversionRatioAsync(It.IsAny<IReportQuery>()), Times.Once);

        Assert.Equal(expectedStatus, actualStatus);
    }

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}(\.\d{1,6})?$")]
    private static partial Regex IsValidDateRegex();
}