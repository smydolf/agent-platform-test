using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Orders.Api.Tests;

// Needs the orders database (ConnectionStrings__Orders, set up with tools/DbSetup).
public class OrderTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Missing_order_returns_404_with_json_error()
    {
        var response = await factory.CreateClient().GetAsync("/orders/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Order not found", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Existing_order_is_returned()
    {
        var response = await factory.CreateClient().GetAsync("/orders/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetProperty("id").GetInt32());
    }
}
