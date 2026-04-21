using System.Reflection;
using System.Runtime.Serialization;
using CoinGecko.Api.Models;

namespace CoinGecko.Api.Tests;

public class EnumSerializationTests
{
    public static TheoryData<Enum, string> WireFormats() => new()
    {
        { CoinMarketsOrder.MarketCapDesc, "market_cap_desc" },
        { CoinMarketsOrder.MarketCapAsc,  "market_cap_asc" },
        { CoinMarketsOrder.VolumeDesc,    "volume_desc" },
        { CoinMarketsOrder.VolumeAsc,     "volume_asc" },
        { CoinMarketsOrder.IdAsc,         "id_asc" },
        { CoinMarketsOrder.IdDesc,        "id_desc" },

        { PriceChangeWindow.OneHour,      "1h" },
        { PriceChangeWindow.TwentyFourHours, "24h" },
        { PriceChangeWindow.SevenDays,    "7d" },
        { PriceChangeWindow.FourteenDays, "14d" },
        { PriceChangeWindow.ThirtyDays,   "30d" },
        { PriceChangeWindow.TwoHundredDays, "200d" },
        { PriceChangeWindow.OneYear,      "1y" },

        { MarketChartRange.OneDay,    "1" },
        { MarketChartRange.SevenDays, "7" },
        { MarketChartRange.FourteenDays, "14" },
        { MarketChartRange.ThirtyDays, "30" },
        { MarketChartRange.NinetyDays, "90" },
        { MarketChartRange.OneHundredEightyDays, "180" },
        { MarketChartRange.OneYear,   "365" },
        { MarketChartRange.Max,       "max" },
    };

    [Theory, MemberData(nameof(WireFormats))]
    public void Enum_member_carries_its_wire_format(Enum value, string expected)
    {
        var member = value.GetType().GetField(value.ToString());
        member.ShouldNotBeNull();
        var em = member!.GetCustomAttribute<EnumMemberAttribute>();
        em.ShouldNotBeNull();
        em!.Value.ShouldBe(expected);
    }
}
