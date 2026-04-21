using CoinGecko.Api.Internal;

namespace CoinGecko.Api.Tests.Internal;

public class UriTemplateExpanderTests
{
    [Theory]
    [InlineData("/coins/{id}", "id=bitcoin", "/coins/bitcoin")]
    [InlineData("/coins/{id}/market_chart", "id=bitcoin", "/coins/bitcoin/market_chart")]
    [InlineData("/coins/{id}/contract/{contract_address}", "id=ethereum;contract_address=0xA0b86a", "/coins/ethereum/contract/0xA0b86a")]
    public void Expand_replaces_named_segments(string template, string pairs, string expected)
    {
        var kvs = pairs.Split(';').Select(p => p.Split('=')).Select(a => (a[0], a[1])).ToArray();
        var result = UriTemplateExpander.Expand(template, kvs);
        result.ShouldBe(expected);
    }

    [Fact]
    public void Unreplaced_placeholder_throws()
    {
        Should.Throw<ArgumentException>(() => UriTemplateExpander.Expand("/coins/{id}", Array.Empty<(string, string)>()));
    }

    [Fact]
    public void Values_are_url_escaped()
    {
        UriTemplateExpander.Expand("/search/{q}", new[] { ("q", "a b/c") })
            .ShouldBe("/search/a%20b%2Fc");
    }
}
