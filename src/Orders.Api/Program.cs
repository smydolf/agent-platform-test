using Azure.Storage.Blobs;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddScoped(_ => new SqlConnection(config.GetConnectionString("Orders")));
builder.Services.AddSingleton(_ => new BlobServiceClient(new Uri(config["InvoiceStorage:AccountUrl"]!)));
builder.Services.AddHttpClient("rates", c => c.BaseAddress = new Uri(config["ExchangeRates:BaseUrl"]!));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok("ok"));

app.MapGet("/orders/{id:int}", async (int id, SqlConnection db) =>
{
    await db.OpenAsync();
    using var cmd = new SqlCommand("SELECT Id, Total, Currency FROM Orders WHERE Id = @id", db);
    cmd.Parameters.AddWithValue("@id", id);
    using var r = await cmd.ExecuteReaderAsync();
    return await r.ReadAsync() ? Results.Ok(OrderMapper.Read(r)) : Results.NotFound();
});

app.MapGet("/orders/{id:int}/invoice", async (int id, BlobServiceClient blobs) =>
{
    var blob = blobs.GetBlobContainerClient(config["InvoiceStorage:Container"] ?? "invoices").GetBlobClient($"{id}.pdf");
    return await blob.ExistsAsync() ? Results.Stream(await blob.OpenReadAsync(), "application/pdf") : Results.NotFound();
});

app.MapGet("/rates/{currency}", async (string currency, IHttpClientFactory http) =>
    Results.Content(await http.CreateClient("rates").GetStringAsync($"latest?base={currency}"), "application/json"));

app.Run();

public partial class Program;

public record OrderDto(int Id, decimal Total, string? Currency);

public static class OrderMapper
{
    public static OrderDto Read(System.Data.Common.DbDataReader r) =>
        new(r.GetInt32(0), r.GetDecimal(1), r.IsDBNull(2) ? null : r.GetString(2));
}
