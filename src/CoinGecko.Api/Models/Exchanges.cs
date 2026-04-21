using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Exchange summary row from <c>/exchanges</c>.</summary>
public class Exchange
{
    /// <summary>Exchange id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Year established.</summary>
    [JsonPropertyName("year_established")] public int? YearEstablished { get; init; }
    /// <summary>HQ country.</summary>
    [JsonPropertyName("country")] public string? Country { get; init; }
    /// <summary>Description.</summary>
    [JsonPropertyName("description")] public string? Description { get; init; }
    /// <summary>Exchange home URL.</summary>
    [JsonPropertyName("url")] public string? Url { get; init; }
    /// <summary>Logo URL.</summary>
    [JsonPropertyName("image")] public string? Image { get; init; }
    /// <summary>Whether CoinGecko has verified trust in this exchange.</summary>
    [JsonPropertyName("has_trading_incentive")] public bool? HasTradingIncentive { get; init; }
    /// <summary>Trust score (0–10).</summary>
    [JsonPropertyName("trust_score")] public int? TrustScore { get; init; }
    /// <summary>Trust score rank.</summary>
    [JsonPropertyName("trust_score_rank")] public int? TrustScoreRank { get; init; }
    /// <summary>24h trade volume in BTC.</summary>
    [JsonPropertyName("trade_volume_24h_btc")] public decimal? TradeVolume24hBtc { get; init; }
    /// <summary>24h trade volume normalized in BTC.</summary>
    [JsonPropertyName("trade_volume_24h_btc_normalized")] public decimal? TradeVolume24hBtcNormalized { get; init; }
}

/// <summary>Exchange id + name from <c>/exchanges/list</c>.</summary>
public sealed class ExchangeListItem
{
    /// <summary>Exchange id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
}

/// <summary>Exchange detail with embedded top-100 tickers.</summary>
public sealed class ExchangeDetail : Exchange
{
    /// <summary>Exchange's top-100 tickers (CoinGecko embeds these in the detail response).</summary>
    [JsonPropertyName("tickers")] public IReadOnlyList<Ticker>? Tickers { get; init; }
    /// <summary>Status updates feed.</summary>
    [JsonPropertyName("status_updates")] public IReadOnlyList<StatusUpdate>? StatusUpdates { get; init; }
    /// <summary>Whether the exchange is centralized.</summary>
    [JsonPropertyName("centralized")] public bool? Centralized { get; init; }
    /// <summary>Alexa rank.</summary>
    [JsonPropertyName("alexa_rank")] public int? AlexaRank { get; init; }
    /// <summary>Facebook URL.</summary>
    [JsonPropertyName("facebook_url")] public string? FacebookUrl { get; init; }
    /// <summary>Reddit URL.</summary>
    [JsonPropertyName("reddit_url")] public string? RedditUrl { get; init; }
    /// <summary>Telegram URL.</summary>
    [JsonPropertyName("telegram_url")] public string? TelegramUrl { get; init; }
    /// <summary>Slack URL.</summary>
    [JsonPropertyName("slack_url")] public string? SlackUrl { get; init; }
    /// <summary>Other social URLs.</summary>
    [JsonPropertyName("other_url_1")] public string? OtherUrl1 { get; init; }
    /// <summary>Other social URLs.</summary>
    [JsonPropertyName("other_url_2")] public string? OtherUrl2 { get; init; }
    /// <summary>Twitter handle.</summary>
    [JsonPropertyName("twitter_handle")] public string? TwitterHandle { get; init; }
    /// <summary>Public notice.</summary>
    [JsonPropertyName("public_notice")] public string? PublicNotice { get; init; }
    /// <summary>Extra notices.</summary>
    [JsonPropertyName("alert_notice")] public string? AlertNotice { get; init; }
}

/// <summary>A single trading pair listing on an exchange.</summary>
public sealed class Ticker
{
    /// <summary>Base asset code.</summary>
    [JsonPropertyName("base")] public string? Base { get; init; }
    /// <summary>Target / quote asset code.</summary>
    [JsonPropertyName("target")] public string? Target { get; init; }
    /// <summary>Exchange summary.</summary>
    [JsonPropertyName("market")] public TickerMarket? Market { get; init; }
    /// <summary>Last price in base asset.</summary>
    [JsonPropertyName("last")] public decimal? Last { get; init; }
    /// <summary>24h trading volume (base).</summary>
    [JsonPropertyName("volume")] public decimal? Volume { get; init; }
    /// <summary>Converted last price (USD / BTC / ETH keys).</summary>
    [JsonPropertyName("converted_last")] public IReadOnlyDictionary<string, decimal>? ConvertedLast { get; init; }
    /// <summary>Converted 24h volume (USD / BTC / ETH keys).</summary>
    [JsonPropertyName("converted_volume")] public IReadOnlyDictionary<string, decimal>? ConvertedVolume { get; init; }
    /// <summary>Trust score for this specific ticker.</summary>
    [JsonPropertyName("trust_score")] public string? TrustScore { get; init; }
    /// <summary>Bid-ask spread percentage.</summary>
    [JsonPropertyName("bid_ask_spread_percentage")] public decimal? BidAskSpreadPercentage { get; init; }
    /// <summary>UNIX timestamp (ISO string).</summary>
    [JsonPropertyName("timestamp")] public string? Timestamp { get; init; }
    /// <summary>Last trade timestamp (ISO).</summary>
    [JsonPropertyName("last_traded_at")] public string? LastTradedAt { get; init; }
    /// <summary>Last fetch timestamp (ISO).</summary>
    [JsonPropertyName("last_fetch_at")] public string? LastFetchAt { get; init; }
    /// <summary>Whether the pair is anomalous / stale.</summary>
    [JsonPropertyName("is_anomaly")] public bool? IsAnomaly { get; init; }
    /// <summary>Whether trading is currently stale.</summary>
    [JsonPropertyName("is_stale")] public bool? IsStale { get; init; }
    /// <summary>Trade URL.</summary>
    [JsonPropertyName("trade_url")] public string? TradeUrl { get; init; }
    /// <summary>Token info (contract address if applicable).</summary>
    [JsonPropertyName("token_info_url")] public string? TokenInfoUrl { get; init; }
    /// <summary>CoinGecko coin id of the base.</summary>
    [JsonPropertyName("coin_id")] public string? CoinId { get; init; }
    /// <summary>CoinGecko coin id of the target.</summary>
    [JsonPropertyName("target_coin_id")] public string? TargetCoinId { get; init; }
}

/// <summary>Summary of the exchange carrying a ticker.</summary>
public sealed class TickerMarket
{
    /// <summary>Exchange display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Exchange id.</summary>
    [JsonPropertyName("identifier")] public string? Identifier { get; init; }
    /// <summary>Whether this market has trading incentives.</summary>
    [JsonPropertyName("has_trading_incentive")] public bool? HasTradingIncentive { get; init; }
    /// <summary>Logo URL.</summary>
    [JsonPropertyName("logo")] public string? Logo { get; init; }
}

/// <summary>Exchange tickers paged response.</summary>
public sealed class ExchangeTickers
{
    /// <summary>Exchange display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Tickers on this page.</summary>
    [JsonPropertyName("tickers")] public IReadOnlyList<Ticker> Tickers { get; init; } = Array.Empty<Ticker>();
}

/// <summary>Status update item (used by several detail endpoints).</summary>
public sealed class StatusUpdate
{
    /// <summary>Free-form description.</summary>
    [JsonPropertyName("description")] public string? Description { get; init; }
    /// <summary>Category (<c>"general"</c>, <c>"milestone"</c>, etc.).</summary>
    [JsonPropertyName("category")] public string? Category { get; init; }
    /// <summary>Creation timestamp (ISO).</summary>
    [JsonPropertyName("created_at")] public string? CreatedAt { get; init; }
    /// <summary>Name of user / bot that posted.</summary>
    [JsonPropertyName("user")] public string? User { get; init; }
    /// <summary>Title of the update.</summary>
    [JsonPropertyName("user_title")] public string? UserTitle { get; init; }
    /// <summary>Pinned flag.</summary>
    [JsonPropertyName("pin")] public bool? Pin { get; init; }
    /// <summary>Referenced project info.</summary>
    [JsonPropertyName("project")] public System.Text.Json.Nodes.JsonObject? Project { get; init; }
}

/// <summary>One (timestamp, btc-volume) pair from <c>/exchanges/{id}/volume_chart</c>.</summary>
public sealed record ExchangeVolumeChartPoint
{
    /// <summary>UTC timestamp (parsed from ms-since-epoch).</summary>
    public DateTimeOffset Timestamp { get; init; }
    /// <summary>24h BTC volume at that point.</summary>
    public decimal BtcVolume { get; init; }
}

/// <summary>Options for <c>/exchanges</c> list.</summary>
public sealed record ExchangesOptions
{
    /// <summary>Items per page (1–250).</summary>
    public int PerPage { get; init; } = 100;
    /// <summary>Page number.</summary>
    public int Page { get; init; } = 1;
}

/// <summary>Options for <c>/exchanges/{id}/tickers</c>.</summary>
public sealed record ExchangeTickersOptions
{
    /// <summary>Filter by coin ids (CSV).</summary>
    public IReadOnlyList<string>? CoinIds { get; init; }
    /// <summary>Include exchange logo in each ticker's market property.</summary>
    public bool IncludeExchangeLogo { get; init; }
    /// <summary>Page.</summary>
    public int Page { get; init; } = 1;
    /// <summary>Include order-book depth.</summary>
    public bool Depth { get; init; }
    /// <summary>Sort order string (e.g. <c>"trust_score_desc"</c>, <c>"volume_desc"</c>).</summary>
    public string? Order { get; init; }
    /// <summary>DEX pair format (<c>"contract_address"</c>, <c>"symbol"</c>).</summary>
    public string? DexPairFormat { get; init; }
}
