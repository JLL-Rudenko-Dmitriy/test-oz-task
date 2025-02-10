using Abstractions;
using Abstractions.Repositories;
using Abstractions.ResultTypes;
using Enums;
using Plugins;
using StackExchange.Redis;

namespace Repositories.ReportQueryRepositories;

public class RedisReportQueryRepository : ICachingReportQueryRepository
{
    private readonly IDatabase _database;
    private readonly string _sortedSetKey;

    public RedisReportQueryRepository(string connectionString, string sortedSetKey)
    {
        _database = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
        _sortedSetKey = sortedSetKey;
    }

    public async  Task<ReportDbQueryResult> TryGetReportStatusByIdAsync(Guid caseId)
    {
        RedisValue statusValue = await _database.StringGetAsync($"{caseId}");
        
        return !statusValue.HasValue 
            ? new ReportDbQueryResult.DoesNotExist()
            : new ReportDbQueryResult.SuccessesFindOneReportStatus((ProcessStatuses)(short)statusValue);
    }

    public async Task<ReportDbQueryResult> InsertReportAsync(IReportQuery reportQuery)
    {
        double timestamp =  (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;

        try
        {
            await _database.SortedSetAddAsync(_sortedSetKey, reportQuery.ToRedisString(), timestamp);
            await _database.StringSetAsync(reportQuery.ToRedisString(), (short)reportQuery.Status);
            return new ReportDbQueryResult.SuccessCreate();
        }
        catch (Exception)
        {
            return new ReportDbQueryResult.Failing(reportQuery.CaseId, "Failed to insert report status");
        }
    }

    public async Task<ReportDbQueryResult> UpdateStatusAsync(IReportQuery reportQuery)
    {
        try
        {
            await _database.StringSetAsync(reportQuery.ToRedisString(), (short)reportQuery.Status);
            return new ReportDbQueryResult.SuccessUpdate();
        }
        catch (Exception)
        {
            return new ReportDbQueryResult.Failing(reportQuery.CaseId, "Failed to update report status");
        }

       
    }

    public async Task DeleteExpiredReportQueriesAsync()
    {
        double timestamp =  (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds; 
        await _database.SortedSetRemoveRangeByScoreAsync(_sortedSetKey, double.NegativeInfinity, timestamp+1);
    }
}