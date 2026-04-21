using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;

namespace CoinGecko.Api.Tests.Internal;

public class QueryStringBuilderTests
{
    private static readonly string[] CryptoIds = ["bitcoin", "ethereum", "ripple"];
    [Fact]
    public void Empty_builder_returns_empty_string()
        => new QueryStringBuilder().ToString().ShouldBe(string.Empty);

    [Fact]
    public void Appends_string_values_url_escaped()
    {
        new QueryStringBuilder()
            .Add("vs_currency", "usd")
            .Add("q", "a b/c")
            .ToString()
            .ShouldBe("?vs_currency=usd&q=a%20b%2Fc");
    }

    [Fact]
    public void Skips_null_values()
    {
        new QueryStringBuilder()
            .Add("a", "1")
            .Add("b", (string?)null)
            .Add("c", "3")
            .ToString()
            .ShouldBe("?a=1&c=3");
    }

    [Fact]
    public void Formats_numbers_with_invariant_culture()
    {
        new QueryStringBuilder()
            .Add("precision", 4)
            .Add("threshold", 1.5m)
            .ToString()
            .ShouldBe("?precision=4&threshold=1.5");
    }

    [Fact]
    public void Formats_bool_as_lowercase()
    {
        new QueryStringBuilder()
            .Add("sparkline", true)
            .Add("localization", false)
            .ToString()
            .ShouldBe("?sparkline=true&localization=false");
    }

    [Fact]
    public void Formats_date_as_dd_mm_yyyy_for_coingecko_history()
    {
        new QueryStringBuilder()
            .AddCoinGeckoDate("date", new DateOnly(2024, 1, 2))
            .ToString()
            .ShouldBe("?date=02-01-2024");
    }

    [Fact]
    public void Formats_enum_via_EnumMember_value()
    {
        new QueryStringBuilder()
            .AddEnum("order", CoinMarketsOrder.MarketCapDesc)
            .ToString()
            .ShouldBe("?order=market_cap_desc");
    }

    [Fact]
    public void Formats_enumerable_as_comma_separated()
    {
        new QueryStringBuilder()
            .AddList("ids", CryptoIds)
            .ToString()
            .ShouldBe("?ids=bitcoin%2Cethereum%2Cripple");
    }
}
