namespace Migrations;

public static class Program
{
    public static async Task Main()
    {
        const string connectionString = "Host=localhost;Username=postgrs;Password=postgrs;Port=5432;Database=postgrs;";
        
        await Migrations.NpgsqlMigrations.sqlUpAsync(connectionString);
        await Migrations.NpgsqlMigrations.sqlGenerateRandomDataAsync(connectionString);
    }
}