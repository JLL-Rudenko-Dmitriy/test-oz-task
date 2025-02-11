FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5038

ENV APP_NAME=ReportQueryService \
    PGSQL_CONNECTION_STRING='Host=localhost;Username=postgrs;Password=postgrs;Host=5432;Database=postgrs;'\
    REDIS_CONNECTION_STRING=localhost:6379\
    REDIS_SORTEDSET_KEY_NAME=report_queries \
    REDIS_HASH_KEY=report_query \
    ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Endpoints/GrpcServices/ReportQueryService/ReportQueryService.csproj", "Endpoints/GrpcServices/ReportQueryService/"]
COPY ["Core/ValueObjects/ValueObjects.csproj", "Core/ValueObjects/"]
COPY ["Core/Services/Services.csproj", "Core/Services/"]
COPY ["Core/Abstractions/Abstractions.csproj", "Core/Abstractions/"]
COPY ["Infrastructure/Dto/Dto.csproj", "Infrastructure/Dto/"]
COPY ["Core/Enums/Enums.csproj", "Core/Enums/"]
COPY ["Core/Contracts/Contracts.csproj", "Core/Contracts/"]
COPY ["Core/Models/Models.csproj", "Core/Models/"]
COPY ["Infrastructure/Plugins/Plugins.csproj", "Infrastructure/Plugins/"]
COPY ["Infrastructure/Repositories/Repositories.csproj", "Infrastructure/Repositories/"]
COPY ["Infrastructure/InfrastructureExtensions/InfrastructureExtensions.csproj", "Infrastructure/InfrastructureExtensions/"]
COPY ["Core/DomainExtensions/DomainExtensions.csproj", "Core/DomainExtensions/"]
RUN dotnet restore "Endpoints/GrpcServices/ReportQueryService/ReportQueryService.csproj"
COPY . .
WORKDIR "/src/Endpoints/GrpcServices/ReportQueryService"
RUN dotnet build "ReportQueryService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ReportQueryService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReportQueryService.dll"]
