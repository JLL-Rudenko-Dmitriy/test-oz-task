using Abstractions;
using Abstractions.Repositories;
using Abstractions.ResultTypes;
using Dto;
using Models.Reports.Builders;
using Npgsql;
using ValueObjects;

namespace Repositories.ReportRepositories;

public class NpgsqlReportRepository : IReportRepository
{
    private readonly NpgsqlDataSource _dataSource;
    
    public NpgsqlReportRepository(string connectionString)
    {
        _dataSource = NpgsqlDataSource.Create(connectionString);
    }
    
    public async Task<ReportDbQueryResult> TryGetReportByIdAsync(Guid id)
    {
        const string sqlCommand = """
                                        SELECT conversion_ratio, payments_count FROM Reports 
                                        WHERE case_id = @case_id;
                                    """;
        
        await using NpgsqlCommand command = _dataSource.CreateCommand(sqlCommand);
        command.Parameters.AddWithValue("@case_id", id);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
            return new ReportDbQueryResult.DoesNotExist();
        
        try
        {
            await reader.ReadAsync();
            var conversionRatio = new ConversionRatio(reader.GetDecimal(0));
            var paymentsCount = new PaymentsCount((ulong)reader.GetInt64(1));

            Abstractions.Builders.ReportBuilders.IReportBuilder reportBuilder = new ReportBuilder()
                .WithCaseId(id)
                .WithConversionRatio(conversionRatio)
                .WithPaymentsCount(paymentsCount);
            
            return new ReportDbQueryResult.SuccessesFindOneReport(
                reportBuilder.Build());
        }
        catch
        {
            return new ReportDbQueryResult.Failing(id, "Can't get report");
        }
    }

    public async Task InsertReportAsync(IReport report)
    {
        const string sqlCommand = """
                                        INSERT INTO reports 
                                           (case_id, conversion_ratio, payments_count)
                                         VALUES
                                            (@case_id, @conversion_ratio, @payments_count);
                                    """;
        
        await using NpgsqlCommand command = _dataSource.CreateCommand(sqlCommand);
        command.Parameters.AddWithValue("@case_id", report.CaseId);
        command.Parameters.AddWithValue("@conversion_ratio", report.ConversionRatio.Ratio);
        command.Parameters.AddWithValue("@payments_count", (long)report.PaymentsCount.Count);
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task<ConversionReportDataDto> CalculateConversionRatioAsync(IReportQuery reportQuery)
    {
        const string sqlCommand = """
                                       SELECT
                                            SUM(pv.views)/NULLIF(COUNT(tr.id), 0)  as conversion_ratio,
                                            COUNT(tr.id) as payment_count
                                        FROM product_views as pv
                                            INNER JOIN invoices as inv
                                                ON inv.product_id=pv.product_id
                                            INNER JOIN transactions as tr
                                                ON tr.invoice_id=inv.id
                                        WHERE
                                            pv.timestamp BETWEEN @start_date AND @end_date
                                                AND
                                            tr.timestamp BETWEEN  @start_date AND @end_date
                                                AND
                                            pv.product_id=@product_id
                                        GROUP BY
                                            pv.product_id;
                                   """;
        
        await using NpgsqlCommand command = _dataSource.CreateCommand(sqlCommand);

        try
        {
            command.Parameters.AddWithValue("@product_id", reportQuery.ProductId);
            command.Parameters.AddWithValue("@start_date", reportQuery.Period.DateStart);
            command.Parameters.AddWithValue("@end_date", reportQuery.Period.DateEnd);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new ConversionReportDataDto(
                    reader.GetDecimal(0),
                    (ulong)reader.GetInt64(1));
            }
            
            return new ConversionReportDataDto(null, 0);
        }
        catch (NpgsqlException)
        {
            throw new InvalidOperationException();
        }
    }
}