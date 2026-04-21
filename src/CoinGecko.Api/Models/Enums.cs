using System.Runtime.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Sort order for <c>/coins/markets</c>.</summary>
public enum CoinMarketsOrder
{
    /// <summary>Descending by market capitalization (default).</summary>
    [EnumMember(Value = "market_cap_desc")] MarketCapDesc = 0,

    /// <summary>Ascending by market capitalization.</summary>
    [EnumMember(Value = "market_cap_asc")] MarketCapAsc,

    /// <summary>Descending by trading volume.</summary>
    [EnumMember(Value = "volume_desc")] VolumeDesc,

    /// <summary>Ascending by trading volume.</summary>
    [EnumMember(Value = "volume_asc")] VolumeAsc,

    /// <summary>Ascending alphabetically by coin id.</summary>
    [EnumMember(Value = "id_asc")] IdAsc,

    /// <summary>Descending alphabetically by coin id.</summary>
    [EnumMember(Value = "id_desc")] IdDesc,
}

/// <summary>Price-change windows for the <c>price_change_percentage</c> param.</summary>
public enum PriceChangeWindow
{
    /// <summary>1 hour.</summary>
    [EnumMember(Value = "1h")] OneHour = 0,

    /// <summary>24 hours (1 day).</summary>
    [EnumMember(Value = "24h")] TwentyFourHours,

    /// <summary>7 days.</summary>
    [EnumMember(Value = "7d")] SevenDays,

    /// <summary>14 days.</summary>
    [EnumMember(Value = "14d")] FourteenDays,

    /// <summary>30 days.</summary>
    [EnumMember(Value = "30d")] ThirtyDays,

    /// <summary>200 days.</summary>
    [EnumMember(Value = "200d")] TwoHundredDays,

    /// <summary>1 year.</summary>
    [EnumMember(Value = "1y")] OneYear,
}

/// <summary>Fixed time windows for <c>/coins/{id}/market_chart</c>.</summary>
public enum MarketChartRange
{
    /// <summary>1 day.</summary>
    [EnumMember(Value = "1")] OneDay = 0,

    /// <summary>7 days.</summary>
    [EnumMember(Value = "7")] SevenDays,

    /// <summary>14 days.</summary>
    [EnumMember(Value = "14")] FourteenDays,

    /// <summary>30 days.</summary>
    [EnumMember(Value = "30")] ThirtyDays,

    /// <summary>90 days.</summary>
    [EnumMember(Value = "90")] NinetyDays,

    /// <summary>180 days.</summary>
    [EnumMember(Value = "180")] OneHundredEightyDays,

    /// <summary>365 days (1 year).</summary>
    [EnumMember(Value = "365")] OneYear,

    /// <summary>Entire available history.</summary>
    [EnumMember(Value = "max")] Max,
}
