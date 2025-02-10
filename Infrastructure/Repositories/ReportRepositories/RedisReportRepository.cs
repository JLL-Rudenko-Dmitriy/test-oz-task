using Abstractions;
using Abstractions.Repositories;
using Abstractions.ResultTypes;
using Enums;
using Models.Reports.Builders;
using Plugins;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using ValueObjects;

namespace Repositories.ReportRepositories;

public class RedisReportRepository : ICachingReportRepository
{
    private readonly IDatabase _database;
    private readonly string _hashKey;

    public RedisReportRepository(string connectionString, string hashKey)
    {
        _database = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
        _hashKey = hashKey;
    }

    public async Task<ReportDbQueryResult> GetReportByIdAsync(Guid caseId)
    {
        HashEntry[] value = await _database.HashGetAllAsync(ReportHashKey(caseId.ToString()));
        
        if (value.Length == 0)
            return new ReportDbQueryResult.DoesNotExist();
        
        try{
            HashEntry conversionRatioValue = value.FirstOrDefault(x => x.Name == "conversion_ratio");
            HashEntry paymentsCountValue = value.FirstOrDefault(x => x.Name == "payments_count");
            
            const string decimalPattern = @"[1-9][0-9]*\.[0-9]+";
            Match getDecimalFromRedisValue = Regex.Match(conversionRatioValue.Value.ToString(), decimalPattern);
            
            const string unsignedLongPattern = @"^[0-9]+$";
            Match getUlongFromRedisValue = Regex.Match(paymentsCountValue.Value.ToString(), unsignedLongPattern);

            decimal conversionRatio = decimal.Parse(getDecimalFromRedisValue.Value);
            ulong paymentsCount = ulong.Parse(getUlongFromRedisValue.Value);

            Abstractions.Builders.ReportBuilders.IReportBuilder reportBuilder = new ReportBuilder()
                .WithCaseId(caseId)
                .WithConversionRatio(new ConversionRatio(conversionRatio))
                .WithPaymentsCount(new PaymentsCount(paymentsCount));

            return new ReportDbQueryResult.SuccessesFindOneReport(reportBuilder.Build());
        }
        catch (Exception e)
        {
            return new ReportDbQueryResult.Failing(caseId, "Can't get report: " + e.Message);
        }
    }

    public async Task<ReportDbQueryResult> InsertReportAsync(IReport report)
    {
        var reportFields = new HashEntry[]
        {
            new("case_id", report.CaseId.ToString()),
            new("conversion_ratio", report.ConversionRatio.ToString()),
            new("payments_count", report.PaymentsCount.ToString())
        };
        
        try
        {
            await _database.HashSetAsync(ReportHashKey(report.ToRedisString()), reportFields);
            await _database.StringSetAsync(report.ToRedisString(), (short)ProcessStatuses.Done);
        
            return new ReportDbQueryResult.SuccessCreate();
        }
        catch (Exception)
        {
            return new ReportDbQueryResult.Failing(report.CaseId, "Failed to insert report record");
        }
    }

    private string ReportHashKey(string caseId)
    {
        return $"{_hashKey}:{caseId}";
    }
}