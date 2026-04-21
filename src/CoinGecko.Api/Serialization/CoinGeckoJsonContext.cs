using System.Text.Json.Serialization;
using CoinGecko.Api.Serialization.JsonApi;

namespace CoinGecko.Api.Serialization;

/// <summary>
/// Source-generated JSON context. Every public DTO type used by the library is
/// registered here with a <c>[JsonSerializable]</c> attribute; later tasks add one
/// <c>[JsonSerializable(typeof(NewDto))]</c> entry each. AOT- and trim-safe.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy   = JsonKnownNamingPolicy.SnakeCaseLower,
    NumberHandling         = JsonNumberHandling.AllowReadingFromString,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters             = new[] { typeof(UnixSecondsConverter) })]
[JsonSerializable(typeof(CoinGecko.Api.Models.PingResponse))]
[JsonSerializable(typeof(CoinGecko.Api.Models.TrendingResults))]
[JsonSerializable(typeof(CoinGecko.Api.Models.TrendingCoinItem))]
[JsonSerializable(typeof(CoinGecko.Api.Models.TrendingCoinData))]
[JsonSerializable(typeof(CoinGecko.Api.Models.TrendingNftItem))]
[JsonSerializable(typeof(CoinGecko.Api.Models.TrendingCategoryItem))]
[JsonSerializable(typeof(CoinGecko.Api.Models.SearchResults))]
[JsonSerializable(typeof(CoinGecko.Api.Models.SearchCoinHit))]
[JsonSerializable(typeof(CoinGecko.Api.Models.SearchExchangeHit))]
[JsonSerializable(typeof(CoinGecko.Api.Models.SearchCategoryHit))]
[JsonSerializable(typeof(CoinGecko.Api.Models.SearchNftHit))]
[JsonSerializable(typeof(CoinGecko.Api.Models.CategoryListItem))]
[JsonSerializable(typeof(CoinGecko.Api.Models.CategoryListItem[]))]
[JsonSerializable(typeof(CoinGecko.Api.Models.CoinCategory))]
[JsonSerializable(typeof(CoinGecko.Api.Models.CoinCategory[]))]
[JsonSerializable(typeof(CoinGecko.Api.Models.AssetPlatform))]
[JsonSerializable(typeof(CoinGecko.Api.Models.AssetPlatform[]))]
[JsonSerializable(typeof(CoinGecko.Api.Models.AssetPlatformImage))]
[JsonSerializable(typeof(CoinGecko.Api.Models.TokenList))]
[JsonSerializable(typeof(CoinGecko.Api.Models.TokenListVersion))]
[JsonSerializable(typeof(CoinGecko.Api.Models.TokenListItem))]
[JsonSerializable(typeof(CoinGecko.Api.Models.CompanyTreasury))]
[JsonSerializable(typeof(CoinGecko.Api.Models.Company))]
[JsonSerializable(typeof(CoinGecko.Api.Models.ApiKeyUsage))]
[JsonSerializable(typeof(JsonApiResponse<JsonApiResource>))]
[JsonSerializable(typeof(JsonApiResponse<JsonApiResource[]>))]
[JsonSerializable(typeof(JsonApiResource))]
[JsonSerializable(typeof(JsonApiResource[]))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, decimal?>>))]
[JsonSerializable(typeof(CoinGecko.Api.Models.GlobalMarketEnvelope))]
[JsonSerializable(typeof(CoinGecko.Api.Models.GlobalMarket))]
[JsonSerializable(typeof(CoinGecko.Api.Models.DefiGlobalEnvelope))]
[JsonSerializable(typeof(CoinGecko.Api.Models.DefiGlobal))]
[JsonSerializable(typeof(CoinGecko.Api.Models.GlobalMarketCapPoint))]
[JsonSerializable(typeof(CoinGecko.Api.Resources.GlobalClient.MarketCapChartEnvelope))]
[JsonSerializable(typeof(CoinGecko.Api.Resources.GlobalClient.MarketCapChartData))]
[JsonSerializable(typeof(CoinGecko.Api.Models.Derivative))]
[JsonSerializable(typeof(CoinGecko.Api.Models.Derivative[]))]
[JsonSerializable(typeof(CoinGecko.Api.Models.DerivativeExchange))]
[JsonSerializable(typeof(CoinGecko.Api.Models.DerivativeExchange[]))]
[JsonSerializable(typeof(CoinGecko.Api.Models.DerivativeExchangeDetail))]
[JsonSerializable(typeof(CoinGecko.Api.Models.DerivativeExchangeListItem))]
[JsonSerializable(typeof(CoinGecko.Api.Models.DerivativeExchangeListItem[]))]
internal sealed partial class CoinGeckoJsonContext : JsonSerializerContext
{
}
