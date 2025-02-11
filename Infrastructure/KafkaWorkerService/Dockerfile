FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5010

ENV APP_NAME=ReportQueryService \
    KAFKA_CONNECTION_HOST_STRING=kafka:29092 \
    KAFKA_TOPIC=report-requests \
    KAFKA_TOPICS_FAST_CONSUMER_GROUP=fast-consumer-group \
    KAFKA_TOPICS_BATCH_CONSUMER_GROUP=batch-consumer-group \
    GRPC_SERVICES_REPORT_GENERATOR=http://localhost:5039 \
    GRPC_SERVICES_REPORT_QUERY=http://localhost:5038 \
    MESSAGE_LIMIT_PER_CONSUMER=512 \
    BATCH_CONSUMER_HOURS_INTERVAL=12 \
    ASPNETCORE_ENVIRONMENT=Production \
    APP_UID=1000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Infrastructure/KafkaWorkerService/KafkaWorkerService.csproj", "Infrastructure/KafkaWorkerService/"]
COPY ["Infrastructure/Migrations/Migrations.csproj", "Infrastructure/Migrations/"]
RUN dotnet restore "Infrastructure/KafkaWorkerService/KafkaWorkerService.csproj"
COPY . .
WORKDIR "/src/Infrastructure/KafkaWorkerService"
RUN dotnet build "KafkaWorkerService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "KafkaWorkerService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KafkaWorkerService.dll"]