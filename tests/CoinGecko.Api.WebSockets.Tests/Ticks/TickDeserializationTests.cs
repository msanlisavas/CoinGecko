using System.Text.Json;
using CoinGecko.Api.WebSockets.Ticks;

namespace CoinGecko.Api.WebSockets.Tests.Ticks;

public class TickDeserializationTests
{
    [Fact]
    public void C1_sample_deserializes_with_float_seconds_timestamp()
    {
        const string json = """{"c":"C1","i":"ethereum","vs":"usd","p":2591.08,"pp":1.38,"m":312938652962.8,"v":20460612214.8,"t":1747808150.269}""";
        var tick = JsonSerializer.Deserialize(json, TicksJsonContext.Default.CoinPriceTick);
        tick.ShouldNotBeNull();
        tick!.CoinId.ShouldBe("ethereum");
        tick.Price.ShouldBe(2591.08m);
        tick.Timestamp.ToUnixTimeSeconds().ShouldBe(1747808150L);
    }

    [Fact]
    public void G2_sample_deserializes_with_millisecond_timestamp()
    {
        const string json = """{"ch":"G2","n":"bsc","pa":"0x172f","tx":"0xabc","ty":"b","to":1.5,"toq":3000,"vo":2500,"pc":0.01,"pu":1.67,"t":1747808150000}""";
        var trade = JsonSerializer.Deserialize(json, TicksJsonContext.Default.DexTrade);
        trade.ShouldNotBeNull();
        trade!.NetworkId.ShouldBe("bsc");
        trade.Timestamp.ToUnixTimeMilliseconds().ShouldBe(1747808150000L);
    }

    [Fact]
    public void G3_sample_deserializes_with_second_timestamp()
    {
        const string json = """{"ch":"G3","n":"eth","pa":"0xpool","to":"base","i":"1m","o":1,"h":2,"l":0.5,"c":1.5,"v":1000,"t":1747808100}""";
        var candle = JsonSerializer.Deserialize(json, TicksJsonContext.Default.OnchainOhlcvCandle);
        candle.ShouldNotBeNull();
        candle!.Interval.ShouldBe("1m");
        candle.CandleStart.ToUnixTimeSeconds().ShouldBe(1747808100L);
    }

    [Fact]
    public void G1_sample_deserializes()
    {
        const string json = """{"ch":"G1","n":"eth","ta":"0xabc","pu":1.23,"pn":0.0005,"fdv":100000000,"tr":50000,"t":1747808150.5}""";
        var tick = JsonSerializer.Deserialize(json, TicksJsonContext.Default.OnchainTokenPriceTick);
        tick.ShouldNotBeNull();
        tick!.PriceUsd.ShouldBe(1.23m);
    }
}
