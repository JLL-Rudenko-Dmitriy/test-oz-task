using Contracts;
using Microsoft.Extensions.DependencyInjection;
using Services;

namespace Extension;

public static class DomainExtension
{
    public static IServiceCollection AddConversionServices(this IServiceCollection service)
    {
        service.AddScoped<IReportService, ReportService>();
        return service.AddScoped<IReportQueryService, ReportQueryService>();
    }
}