using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Protocol;

/// <summary>Inner ActionCable channel selector — serialized as JSON then quoted into the outer frame's <c>identifier</c> field.</summary>
[JsonConverter(typeof(ActionCableIdentifierConverter))]
public sealed class ActionCableIdentifier
{
    /// <summary>Channel name, e.g. <c>"CGSimplePrice"</c>.</summary>
    [JsonPropertyName("channel")] public string Channel { get; init; } = string.Empty;
}
