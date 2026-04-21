using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Ticks;

[JsonSourceGenerationOptions(NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(CoinPriceTick))]
[JsonSerializable(typeof(OnchainTokenPriceTick))]
[JsonSerializable(typeof(DexTrade))]
[JsonSerializable(typeof(OnchainOhlcvCandle))]
internal sealed partial class TicksJsonContext : JsonSerializerContext
{
}
