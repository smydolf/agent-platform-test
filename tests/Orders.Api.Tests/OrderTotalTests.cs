using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Orders.Api.Tests;

public class OrderTotalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private sealed class FakeStore : IOrderTotalStore
    {
        public Task<decimal?> GetTotalAsync(int id) => Task.FromResult<decimal?>(id switch { 1 => 100m, 2 => 250.5m, _ => null });
    }

    private readonly HttpClient client;

    public OrderTotalTests(WebApplicationFactory<Program> factory) =>
        client = factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
            s.AddScoped<IOrderTotalStore, FakeStore>())).CreateClient();

    [Theory]
    [InlineData(1, "100.00")]
    [InlineData(2, "250.50")]
    public async Task Existing_order_returns_total_with_two_decimals(int id, string expected)
    {
        var response = await client.GetAsync($"/orders/{id}/total");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(id, doc.RootElement.GetProperty("id").GetInt32());
        Assert.Equal(JsonValueKind.String, doc.RootElement.GetProperty("total").ValueKind);
        Assert.Equal(expected, doc.RootElement.GetProperty("total").GetString());
    }

    [Fact]
    public async Task Missing_order_returns_404()
    {
        var response = await client.GetAsync("/orders/99/total");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
