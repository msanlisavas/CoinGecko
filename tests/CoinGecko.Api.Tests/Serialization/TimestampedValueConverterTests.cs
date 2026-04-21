using System.Text.Json;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Tests.Serialization;

public class TimestampedValueConverterTests
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        Converters = { new TimestampedValueConverter() },
    };

    [Fact]
    public void Reads_two_element_array_into_timestamped_value()
    {
        var result = JsonSerializer.Deserialize<TimestampedValue>("[1713916800000,42000.5]", Opts);

        result.ShouldNotBeNull();
        result!.Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L));
        result.Value.ShouldBe(42000.5m);
    }

    [Fact]
    public void Writes_timestamped_value_as_two_element_array()
    {
        var value = new TimestampedValue(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L), 42000.5m);
        var json = JsonSerializer.Serialize(value, Opts);

        json.ShouldBe("[1713916800000,42000.5]");
    }

    [Fact]
    public void Read_throws_when_not_start_array()
    {
        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<TimestampedValue>("{\"ts\":1713916800000,\"value\":42000.5}", Opts));
    }
}
