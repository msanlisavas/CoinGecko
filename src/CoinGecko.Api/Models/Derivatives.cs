using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>A derivative ticker row from <c>/derivatives</c>.</summary>
public sealed class Derivative
{
    /// <summary>Market / exchange name.</summary>
    [JsonPropertyName("market")] public string? Market { get; init; }
    /// <summary>Ticker symbol (e.g. <c>"BTC-PERP"</c>).</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }
    /// <summary>Index id (e.g. <c>"BTC"</c>).</summary>
    [JsonPropertyName("index_id")] public string? IndexId { get; init; }
    /// <summary>Last price.</summary>
    [JsonPropertyName("price")] public string? Price { get; init; }
    /// <summary>24h price change percentage.</summary>
    [JsonPropertyName("price_percentage_change_24h")] public decimal? PricePercentageChange24h { get; init; }
    /// <summary>Contract type (<c>"perpetual"</c>, <c>"futures"</c>).</summary>
    [JsonPropertyName("contract_type")] public string? ContractType { get; init; }
    /// <summary>Current index price.</summary>
    [JsonPropertyName("index")] public decimal? Index { get; init; }
    /// <summary>Basis vs index (%).</summary>
    [JsonPropertyName("basis")] public decimal? Basis { get; init; }
    /// <summary>Spread (%).</summary>
    [JsonPropertyName("spread")] public decimal? Spread { get; init; }
    /// <summary>Funding rate (%, perpetuals only).</summary>
    [JsonPropertyName("funding_rate")] public decimal? FundingRate { get; init; }
    /// <summary>Open interest (USD).</summary>
    [JsonPropertyName("open_interest")] public decimal? OpenInterest { get; init; }
    /// <summary>24h volume (USD).</summary>
    [JsonPropertyName("volume_24h")] public decimal? Volume24h { get; init; }
    /// <summary>UNIX ms of last trade.</summary>
    [JsonPropertyName("last_traded_at")] public long? LastTradedAt { get; init; }
    /// <summary>Expiration timestamp (futures only); ms since epoch.</summary>
    [JsonPropertyName("expired_at")] public long? ExpiredAt { get; init; }
}

/// <summary>A derivatives exchange summary row.</summary>
public class DerivativeExchange
{
    /// <summary>Exchange name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Exchange id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>24h perpetuals volume in BTC.</summary>
    [JsonPropertyName("open_interest_btc")] public decimal? OpenInterestBtc { get; init; }
    /// <summary>24h total trade volume in BTC.</summary>
    [JsonPropertyName("trade_volume_24h_btc")] public string? TradeVolume24hBtc { get; init; }
    /// <summary>Total perpetuals pairs.</summary>
    [JsonPropertyName("number_of_perpetual_pairs")] public int? NumberOfPerpetualPairs { get; init; }
    /// <summary>Total futures pairs.</summary>
    [JsonPropertyName("number_of_futures_pairs")] public int? NumberOfFuturesPairs { get; init; }
    /// <summary>Exchange logo URL.</summary>
    [JsonPropertyName("image")] public string? Image { get; init; }
    /// <summary>ISO date the exchange was launched.</summary>
    [JsonPropertyName("year_established")] public int? YearEstablished { get; init; }
    /// <summary>HQ country.</summary>
    [JsonPropertyName("country")] public string? Country { get; init; }
    /// <summary>Description snippet.</summary>
    [JsonPropertyName("description")] public string? Description { get; init; }
    /// <summary>Exchange home URL.</summary>
    [JsonPropertyName("url")] public string? Url { get; init; }
}

/// <summary>Extended derivatives-exchange detail (includes tickers if requested).</summary>
public sealed class DerivativeExchangeDetail : DerivativeExchange
{
    /// <summary>Tickers on this exchange; populated when <c>include_tickers</c> is set.</summary>
    [JsonPropertyName("tickers")] public IReadOnlyList<Derivative>? Tickers { get; init; }
}

/// <summary>A row in <c>/derivatives/exchanges/list</c>.</summary>
public sealed class DerivativeExchangeListItem
{
    /// <summary>Exchange id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
}

/// <summary>Options for the <c>/derivatives/exchanges</c> list endpoint.</summary>
public sealed record DerivativeExchangesOptions
{
    /// <summary>Sort order.</summary>
    public string? Order { get; init; }
    /// <summary>Items per page (1–250).</summary>
    public int PerPage { get; init; } = 100;
    /// <summary>Page number (1-indexed).</summary>
    public int Page { get; init; } = 1;
}
