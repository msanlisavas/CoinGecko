using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Protocol;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ActionCableIdentifier))]
[JsonSerializable(typeof(ActionCableFrame))]
internal sealed partial class ActionCableProtocolJsonContext : JsonSerializerContext
{
}
