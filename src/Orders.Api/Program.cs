using Azure.Storage.Blobs;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddScoped(_ => new SqlConnection(config.GetConnectionString("Orders")));
builder.Services.AddSingleton(_ => new BlobServiceClient(new Uri(config["InvoiceStorage:AccountUrl"]!)));
builder.Services.AddHttpClient("rates", c => c.BaseAddress = new Uri(config["ExchangeRates:BaseUrl"]!));

builder.Services.AddScoped<IOrderTotalStore, SqlOrderTotalStore>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok("ok"));

app.MapGet("/orders/{id:int}", async (int id, SqlConnection db) =>
{
    await db.OpenAsync();
    using var cmd = new SqlCommand("SELECT Id, Total, Currency FROM Orders WHERE Id = @id", db);
    cmd.Parameters.AddWithValue("@id", id);
    using var r = await cmd.ExecuteReaderAsync();
    return await r.ReadAsync() ? Results.Ok(new { Id = r.GetInt32(0), Total = r.GetDecimal(1), Currency = r.GetString(2) }) : Results.NotFound();
});

app.MapGet("/orders/{id:int}/total", async (int id, IOrderTotalStore store) =>
{
    var total = await store.GetTotalAsync(id);
    return total is null
        ? Results.NotFound()
        : Results.Ok(new { id, total = total.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) });
});

app.MapGet("/orders/{id:int}/invoice", async (int id, BlobServiceClient blobs) =>
{
    var blob = blobs.GetBlobContainerClient(config["InvoiceStorage:Container"] ?? "invoices").GetBlobClient($"{id}.pdf");
    return await blob.ExistsAsync() ? Results.Stream(await blob.OpenReadAsync(), "application/pdf") : Results.NotFound();
});

app.MapGet("/rates/{currency}", async (string currency, IHttpClientFactory http) =>
    Results.Content(await http.CreateClient("rates").GetStringAsync($"latest?base={currency}"), "application/json"));

app.Run();

public interface IOrderTotalStore
{
    Task<decimal?> GetTotalAsync(int id);
}

public class SqlOrderTotalStore(SqlConnection db) : IOrderTotalStore
{
    public async Task<decimal?> GetTotalAsync(int id)
    {
        await db.OpenAsync();
        using var cmd = new SqlCommand("SELECT Total FROM Orders WHERE Id = @id", db);
        cmd.Parameters.AddWithValue("@id", id);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? null : Convert.ToDecimal(v);
    }
}

public partial class Program;
