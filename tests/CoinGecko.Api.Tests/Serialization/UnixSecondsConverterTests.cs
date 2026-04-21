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
    public void Writes_as_iso8601_roundtrip()
    {
        var dt = DateTimeOffset.FromUnixTimeSeconds(1700000000);
        var json = JsonSerializer.Serialize(dt, Opts);
        // Round-trip format "o" emits full precision with offset, e.g. "2023-11-14T22:13:20.0000000+00:00".
        json.ShouldStartWith("\"");
        json.ShouldEndWith("\"");
        JsonSerializer.Deserialize<DateTimeOffset>(json, Opts).ShouldBe(dt);
    }

    [Fact]
    public void Reads_iso8601_string_returned_by_coingecko()
    {
        var json = "\"2021-11-10T14:24:11.849Z\"";
        var dt = JsonSerializer.Deserialize<DateTimeOffset>(json, Opts);
        dt.Year.ShouldBe(2021);
        dt.Month.ShouldBe(11);
        dt.Day.ShouldBe(10);
        dt.Hour.ShouldBe(14);
    }

    [Fact]
    public void Reads_numeric_string_as_unix_seconds()
    {
        var dt = JsonSerializer.Deserialize<DateTimeOffset>("\"1700000000\"", Opts);
        dt.ShouldBe(DateTimeOffset.FromUnixTimeSeconds(1700000000));
    }
}
