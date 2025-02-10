using Npgsql;

namespace Migrations;

public static class NpgsqlMigrations
{
    public static async Task sqlUpAsync(string connectionString)
    {
        string sqlQuery = 
            """
                CREATE TABLE IF NOT EXISTS products (
                    id UUID PRIMARY KEY,
                    name VARCHAR(255) NOT NULL
                );
                
                CREATE TABLE IF NOT EXISTS report_queries (
                    case_id UUID PRIMARY KEY,
                    product_id UUID REFERENCES products(id) ON DELETE CASCADE,
                    status SMALLINT NOT NULL CHECK (status IN (1, 2, 4)),
                    start_date DATE NOT NULL,
                    end_date DATE NOT NULL
                );

                CREATE TABLE IF NOT EXISTS reports (
                    id SERIAL PRIMARY KEY,
                    case_id UUID REFERENCES report_queries(case_id) ON DELETE CASCADE,
                    conversion_ratio DECIMAL NOT NULL,
                    payments_count BIGINT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS product_views (
                    id SERIAL PRIMARY KEY,
                    timestamp TIMESTAMP NOT NULL,
                    product_id UUID REFERENCES products(id) ON DELETE CASCADE,
                    views BIGINT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ledger (
                    id SERIAL PRIMARY KEY,
                    balance DECIMAL NOT NULL,
                    timestamp TIMESTAMP NOT NULL
                );

                CREATE TABLE IF NOT EXISTS invoices (
                    id SERIAL PRIMARY KEY,
                    amount DECIMAL NOT NULL,
                    issued_date TIMESTAMP NOT NULL,
                    product_id UUID REFERENCES products(id) ON DELETE CASCADE
                );
            

                CREATE TABLE IF NOT EXISTS transactions (
                    id SERIAL PRIMARY KEY,
                    invoice_id SERIAL REFERENCES invoices(id) ON DELETE CASCADE,
                    amount DECIMAL NOT NULL,
                    timestamp TIMESTAMP NOT NULL 
                );

                CREATE INDEX IF NOT EXISTS idx_product_views_timestamp ON product_views(timestamp);

                CREATE INDEX IF NOT EXISTS idx_product_views_product_id ON product_views(product_id);

                CREATE INDEX IF NOT EXISTS idx_ledger_timestamp ON ledger(timestamp);

                CREATE INDEX IF NOT EXISTS idx_transactions_timestamp ON transactions(timestamp);

                CREATE INDEX IF NOT EXISTS idx_invoices_issued_date ON invoices(issued_date);

                CREATE INDEX IF NOT EXISTS idx_invoices_product_id ON invoices(product_id);
            """;
        
        await using (var dataSource = NpgsqlDataSource.Create(connectionString))
        {
            NpgsqlCommand sqlUpCommand = dataSource.CreateCommand(sqlQuery);
        
            await sqlUpCommand.ExecuteNonQueryAsync(); 
        };
    }

    public static async Task sqlGenerateRandomDataAsync(string connectionString)
    {
        string sqlQuery = """
                          CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
                          
                          INSERT INTO products (id, name)
                          SELECT uuid_generate_v4(), 'Product ' || generate_series
                          FROM generate_series(1, (15 + floor(random() * 10))::int);
                          
                          INSERT INTO product_views (timestamp, product_id, views)
                          SELECT NOW() - INTERVAL '1 day' * floor(random() * 30), 
                                 (SELECT id FROM products ORDER BY random() LIMIT 1),
                                 floor(random() * 1000 + 1)
                          FROM generate_series(1, (15 + floor(random() * 10))::int);
                          
                          INSERT INTO ledger (balance, timestamp)
                          SELECT round((random() * 20000 - 10000)::numeric, 2), 
                                 NOW() - INTERVAL '1 day' * floor(random() * 30)
                          FROM generate_series(1, (15 + floor(random() * 10))::int);
                          
                          WITH invoice_data AS (
                              INSERT INTO invoices (amount, issued_date, product_id)
                              SELECT round((random() * 2000 + 50)::numeric, 2), 
                                     NOW() - INTERVAL '1 day' * floor(random() * 30),
                                     (SELECT id FROM products ORDER BY random() LIMIT 1)
                              FROM generate_series(1, (15 + floor(random() * 10))::int)
                              RETURNING id, amount, issued_date
                          )
                          INSERT INTO transactions (invoice_id, amount, timestamp)
                          SELECT id, amount, issued_date + INTERVAL '1 hour' * floor(random() * 48)
                          FROM invoice_data
                          WHERE random() < 0.7;  -- Только для 70% счетов будут созданы транзакции
                          
                          """;
        
        await using (var dataSource = NpgsqlDataSource.Create(connectionString))
        {
            NpgsqlCommand sqlUpCommand = dataSource.CreateCommand(sqlQuery);
        
            await sqlUpCommand.ExecuteNonQueryAsync(); 
        };
        
    }
    
    public static async Task sqlDown(string connectionString)
    {
        string sqlQuery = 
            """
                DROP TABLE IF EXISTS invoices;
                DROP TABLE IF EXISTS transactions;
                DROP TABLE IF EXISTS ledger;
                DROP TABLE IF EXISTS product_views;
                DROP TABLE IF EXISTS reports;
                DROP TABLE IF EXISTS products;
                DROP TABLE IF EXISTS report_queries;
            """;
        
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        NpgsqlCommand sqlDownCommand = dataSource.CreateCommand(sqlQuery);
        
        await sqlDownCommand.ExecuteNonQueryAsync(); 
    }
}