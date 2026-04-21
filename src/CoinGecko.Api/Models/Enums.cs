using System.Runtime.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Sort order for <c>/coins/markets</c>.</summary>
public enum CoinMarketsOrder
{
    [EnumMember(Value = "market_cap_desc")] MarketCapDesc = 0,
    [EnumMember(Value = "market_cap_asc")]  MarketCapAsc,
    [EnumMember(Value = "volume_desc")]     VolumeDesc,
    [EnumMember(Value = "volume_asc")]      VolumeAsc,
    [EnumMember(Value = "id_asc")]          IdAsc,
    [EnumMember(Value = "id_desc")]         IdDesc,
}

/// <summary>Price-change windows for the <c>price_change_percentage</c> param.</summary>
public enum PriceChangeWindow
{
    [EnumMember(Value = "1h")]   OneHour = 0,
    [EnumMember(Value = "24h")]  TwentyFourHours,
    [EnumMember(Value = "7d")]   SevenDays,
    [EnumMember(Value = "14d")]  FourteenDays,
    [EnumMember(Value = "30d")]  ThirtyDays,
    [EnumMember(Value = "200d")] TwoHundredDays,
    [EnumMember(Value = "1y")]   OneYear,
}

/// <summary>Fixed time windows for <c>/coins/{id}/market_chart</c>.</summary>
public enum MarketChartRange
{
    [EnumMember(Value = "1")]   OneDay = 0,
    [EnumMember(Value = "7")]   SevenDays,
    [EnumMember(Value = "14")]  FourteenDays,
    [EnumMember(Value = "30")]  ThirtyDays,
    [EnumMember(Value = "90")]  NinetyDays,
    [EnumMember(Value = "180")] OneHundredEightyDays,
    [EnumMember(Value = "365")] OneYear,
    [EnumMember(Value = "max")] Max,
}
