using System.Text.Json;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Tests.Serialization;

public class SupplyPointConverterTests
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        Converters = { new SupplyPointConverter() },
    };

    [Fact]
    public void Reads_array_with_string_supply_into_supply_point()
    {
        var result = JsonSerializer.Deserialize<SupplyPoint>("[1713916800000,\"19650000.5\"]", Opts);

        result.ShouldNotBeNull();
        result!.Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L));
        result.Supply.ShouldBe(19650000.5m);
    }

    [Fact]
    public void Writes_supply_point_as_two_element_array_with_string_supply()
    {
        var value = new SupplyPoint(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L), 19650000.5m);
        var json = JsonSerializer.Serialize(value, Opts);

        json.ShouldBe("[1713916800000,\"19650000.5\"]");
    }

    [Fact]
    public void Read_throws_when_not_start_array()
    {
        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<SupplyPoint>("{\"ts\":1713916800000,\"supply\":\"19650000.5\"}", Opts));
    }
}
