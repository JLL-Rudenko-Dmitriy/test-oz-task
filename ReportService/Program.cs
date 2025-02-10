using Extension;
using Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ReportService.Interceptors.Validation;
using ReportService.Services;
using System.Net;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddGrpc()
    .AddServiceOptions<ReportGeneratorService>(options =>
    {
        options.Interceptors.Add<GetReportValidationInterceptor>();
    });

string? pgsqlConnectionString = string.Empty;
string? redisConnectionString = string.Empty;
string? redisSortedSetKey = string.Empty;
string? redisHashKey = string.Empty;

builder.Services.AddConversionServices();

if (builder.Environment.IsDevelopment())
{
    pgsqlConnectionString = builder.Configuration.GetConnectionString("PgsqlConnectionString") 
                                    ?? throw new InvalidOperationException("Pgsql connection string is not configured.");
    
    redisConnectionString = builder.Configuration.GetConnectionString("RedisConnectionString") 
                            ?? throw new InvalidOperationException("Redis connection string is not configured.");

    IConfigurationSection appSettingsSection = builder.Configuration.GetSection("AppSettings");
    
    redisSortedSetKey = appSettingsSection.GetValue<string>("RedisSortedSetKey") 
                                ?? throw new InvalidOperationException("RedisSortedSetKey is not configured.");
    
    redisHashKey = appSettingsSection.GetValue<string>("RedisHashKey") 
                           ?? throw new InvalidOperationException("RedisHashKey is not configured.");
}

if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(IPAddress.Any, 5039, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
    });
    
    pgsqlConnectionString = builder.Configuration["PGSQL_CONNECTION_STRING"]
                            ?? throw new InvalidOperationException("Pgsql connection string is not configured.");
    
    redisConnectionString = builder.Configuration["REDIS_CONNECTION_STRING"]
                            ?? throw new InvalidOperationException("Redis connection string is not configured.");
    
    redisSortedSetKey = builder.Configuration["REDIS_SORTEDSET_KEY_NAME"]
                        ?? throw new InvalidOperationException("RedisSortedSetKey is not configured.");
    
    redisHashKey = builder.Configuration["REDIS_HASH_KEY"]
                   ?? throw new InvalidOperationException("RedisHashKey is not configured.");
}

builder.Services.AddInfrastructureServices(
    redisConnectionString,
    pgsqlConnectionString, 
    redisHashKey, 
    redisSortedSetKey);

WebApplication app = builder.Build();

app.MapGrpcService<ReportGeneratorService>();

app.Run();
