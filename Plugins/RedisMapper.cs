using Abstractions;

namespace Plugins;

public static class RedisMapper
{
    public static string ToRedisString(this IReportQuery reportQuery)
    {
        return $"{reportQuery.CaseId}";
    }

    public static string ToRedisString(this IReport report)
    {
        return $"{report.CaseId}";
    }
}