using System.Data;
using System.Text.Json;
using Xunit;

namespace Orders.Api.Tests;

public class OrderMapperTests
{
    private static DataTableReader Reader(object currency)
    {
        var t = new DataTable();
        t.Columns.Add("Id", typeof(int));
        t.Columns.Add("Total", typeof(decimal));
        t.Columns.Add("Currency", typeof(string));
        t.Rows.Add(1, 10.5m, currency);
        var r = t.CreateDataReader();
        r.Read();
        return r;
    }

    [Fact]
    public void Null_currency_maps_to_null_and_serializes_as_null()
    {
        var dto = OrderMapper.Read(Reader(DBNull.Value));
        Assert.Null(dto.Currency);
        Assert.Contains("\"Currency\":null", JsonSerializer.Serialize(dto));
    }

    [Fact]
    public void Currency_is_kept()
    {
        var dto = OrderMapper.Read(Reader("EUR"));
        Assert.Equal(new OrderDto(1, 10.5m, "EUR"), dto);
    }
}
