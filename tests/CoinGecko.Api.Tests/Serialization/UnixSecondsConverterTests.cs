using System.Text.Json;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Tests.Serialization;

public class UnixSecondsConverterTests
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        Converters = { new UnixSecondsConverter() },
    };

    [Fact]
    public void Reads_integer_unix_seconds()
    {
        var dt = JsonSerializer.Deserialize<DateTimeOffset>("1700000000", Opts);
        dt.ShouldBe(DateTimeOffset.FromUnixTimeSeconds(1700000000));
    }

    [Fact]
    public void Reads_fractional_unix_seconds_as_milliseconds_precision()
    {
        var dt = JsonSerializer.Deserialize<DateTimeOffset>("1700000000.5", Opts);
        dt.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1700000000500));
    }

    [Fact]
    public void Writes_as_integer_seconds()
    {
        var dt = DateTimeOffset.FromUnixTimeSeconds(1700000000);
        JsonSerializer.Serialize(dt, Opts).ShouldBe("1700000000");
    }
}
