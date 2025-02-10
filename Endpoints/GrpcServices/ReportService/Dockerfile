FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5039

ENV APP_NAME=ReportQueryService \
    PGSQL_CONNECTION_STRING='Host=localhost;Username=postgrs;Password=postgrs;Host=5432;Database=postgrs;'\
    REDIS_CONNECTION_STRING=localhost:6379\
    REDIS_SORTEDSET_KEY_NAME=report_queries \
    REDIS_HASH_KEY=report_query \
    ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ReportService/ReportService.csproj", "ReportService/"]
COPY ["DomainExtensions/DomainExtensions.csproj", "DomainExtensions/"]
COPY ["Dto/Dto.csproj", "Dto/"]
COPY ["Models/Models.csproj", "Models/"]
COPY ["Abstractions/Abstractions.csproj", "Abstractions/"]
COPY ["ValueObjects/ValueObjects.csproj", "ValueObjects/"]
COPY ["Enums/Enums.csproj", "Enums/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Plugins/Plugins.csproj", "Plugins/"]
COPY ["InfrastructureExtensions/InfrastructureExtensions.csproj", "InfrastructureExtensions/"]
COPY ["Repositories/Repositories.csproj", "Repositories/"]
RUN dotnet restore "ReportService/ReportService.csproj"
COPY . .
WORKDIR "/src/ReportService"
RUN dotnet build "ReportService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ReportService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReportService.dll"]
