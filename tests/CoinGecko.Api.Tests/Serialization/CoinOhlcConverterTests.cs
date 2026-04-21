using System.Text.Json;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Tests.Serialization;

public class CoinOhlcConverterTests
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        Converters = { new CoinOhlcConverter() },
    };

    [Fact]
    public void Reads_five_element_array_into_coin_ohlc()
    {
        var result = JsonSerializer.Deserialize<CoinOhlc>("[1713916800000,41000.0,43000.0,40500.0,42000.0]", Opts);

        result.ShouldNotBeNull();
        result!.Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L));
        result.Open.ShouldBe(41000.0m);
        result.High.ShouldBe(43000.0m);
        result.Low.ShouldBe(40500.0m);
        result.Close.ShouldBe(42000.0m);
    }

    [Fact]
    public void Writes_coin_ohlc_as_five_element_array()
    {
        var value = new CoinOhlc(
            DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L),
            41000.0m, 43000.0m, 40500.0m, 42000.0m);

        var json = JsonSerializer.Serialize(value, Opts);

        json.ShouldBe("[1713916800000,41000.0,43000.0,40500.0,42000.0]");
    }

    [Fact]
    public void Read_throws_when_not_start_array()
    {
        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<CoinOhlc>("{\"ts\":1713916800000}", Opts));
    }
}
