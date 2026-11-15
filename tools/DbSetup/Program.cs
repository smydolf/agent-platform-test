// Makes the orders database for a local run and the agent platform: the table and three seed orders (order 3 has no currency).
// Reads the connection string from ConnectionStrings__Orders. Safe to run more than once.
using Microsoft.Data.SqlClient;

var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Orders")
    ?? throw new InvalidOperationException("Set ConnectionStrings__Orders.");
var orders = new SqlConnectionStringBuilder(cs);
var master = new SqlConnectionStringBuilder(cs) { InitialCatalog = "master" };

// The database server can need some seconds after its container starts.
for (var i = 0; ; i++)
{
    try
    {
        await using var c = new SqlConnection(master.ConnectionString);
        await c.OpenAsync();
        await Exec(c, $"IF DB_ID('{orders.InitialCatalog}') IS NULL CREATE DATABASE [{orders.InitialCatalog}]");
        break;
    }
    catch (SqlException) when (i < 30)
    {
        Console.WriteLine("Waiting for the database server...");
        await Task.Delay(2000);
    }
}

await using (var c = new SqlConnection(orders.ConnectionString))
{
    await c.OpenAsync();
    await Exec(c, """
        IF OBJECT_ID('dbo.Orders') IS NULL
            CREATE TABLE dbo.Orders (Id INT NOT NULL PRIMARY KEY, Total DECIMAL(18, 2) NOT NULL, Currency NVARCHAR(3) NULL);
        IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE Id = 1) INSERT dbo.Orders VALUES (1, 100.00, 'USD');
        IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE Id = 2) INSERT dbo.Orders VALUES (2, 250.50, 'EUR');
        IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE Id = 3) INSERT dbo.Orders VALUES (3, 75.00, NULL);
        """);
}
Console.WriteLine($"The database {orders.InitialCatalog} is ready.");

static async Task Exec(SqlConnection c, string sql)
{
    await using var cmd = new SqlCommand(sql, c);
    await cmd.ExecuteNonQueryAsync();
}
