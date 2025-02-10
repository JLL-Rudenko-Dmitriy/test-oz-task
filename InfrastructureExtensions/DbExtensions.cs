using Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Repositories.ReportQueryRepositories;
using Repositories.ReportRepositories;

namespace Extensions;

public static class DbExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection service,
        string redisConnectionString,
        string postgresConnectionString,
        string redisHashKey,
        string redisSortedSetKey)
    {
        service.AddSingleton<ICachingReportQueryRepository>(sp =>
            new RedisReportQueryRepository(redisConnectionString, redisSortedSetKey));
        
        service.AddSingleton<ICachingReportRepository>(sp =>
            new RedisReportRepository(redisConnectionString, redisHashKey));
        
        service.AddSingleton<IReportQueryRepository>(sp =>
            new NpgsqlReportQueryRepository(postgresConnectionString));
        
        service.AddSingleton<IReportRepository>(sp =>
            new NpgsqlReportRepository(postgresConnectionString));
        
        return service;
    }
}