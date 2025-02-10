using Abstractions;
using Abstractions.Repositories;
using Abstractions.ResultTypes;
using Enums;
using Npgsql;

namespace Repositories.ReportQueryRepositories;

public class NpgsqlReportQueryRepository : IReportQueryRepository
{
    private readonly NpgsqlDataSource _dataSource;
    
    public NpgsqlReportQueryRepository(string connectionString)
    {
        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public async Task<ReportDbQueryResult> TryGetReportStatusByIdAsync(Guid reportQueryId)
    {
        const string sqlCommand = """
                                    SELECT status
                                    FROM report_query
                                    WHERE case_id = @case_id;
                                  """;
        await using NpgsqlCommand command = _dataSource.CreateCommand(sqlCommand);
        command.Parameters.AddWithValue("@case_id", reportQueryId);

        try
        {
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            
            if(await reader.ReadAsync())
            {
                return new ReportDbQueryResult.SuccessesFindOneReportStatus(
                    (ProcessStatuses)reader.GetInt16(0));
            }
        }
        catch (Exception)
        {
            return new ReportDbQueryResult.DoesNotExist();
        }
        
        return new ReportDbQueryResult.DoesNotExist();
    }

    public async Task InsertReportQueryAsync(IReportQuery reportQuery)
    {
        const string sqlCommand = """
                                    INSERT INTO report_queries
                                        (case_id, product_id, status, start_date, end_date)
                                    VALUES 
                                        (@case_id, @product_id, @status, @start_date, @end_date);
                                  """;

        await using NpgsqlCommand command = _dataSource.CreateCommand(sqlCommand);
        command.Parameters.AddWithValue("@case_id", reportQuery.CaseId);
        command.Parameters.AddWithValue("@product_id", reportQuery.ProductId);
        command.Parameters.AddWithValue("@status", (int)reportQuery.Status);
        command.Parameters.AddWithValue("@start_date", reportQuery.Period.DateStart);
        command.Parameters.AddWithValue("@end_date", reportQuery.Period.DateEnd);
            
        await command.ExecuteNonQueryAsync();
    }

    public async Task<ReportDbQueryResult> UpdateReportQueryAsync(IReportQuery updatedReportQuery)
    {
        const string sqlCommand = """
                                    UPDATE TABLE 
                                        report_queries
                                    SET
                                        case_id = @case_id,
                                        product_id = @product_id,
                                        status = @status,
                                        date_start = @date_start,
                                        date_end = @date_end
                                    WHERE
                                        case_id = @case_id;
                                  """;

        await using NpgsqlCommand command = _dataSource.CreateCommand(sqlCommand);
        command.Parameters.AddWithValue("@case_id", updatedReportQuery.CaseId);
        command.Parameters.AddWithValue("@product_id", updatedReportQuery.ProductId);
        command.Parameters.AddWithValue("@status", (int)updatedReportQuery.Status);
        command.Parameters.AddWithValue("@date_start", updatedReportQuery.Period.DateStart);
        command.Parameters.AddWithValue("@date_end", updatedReportQuery.Period.DateEnd);

        Task<int> execution = command.ExecuteNonQueryAsync();

        try
        {
            await execution;
            return new ReportDbQueryResult.SuccessUpdate();
        }
        catch
        {
            return new ReportDbQueryResult.Failing(updatedReportQuery.CaseId, "Could not update report query");
        }
    }
}