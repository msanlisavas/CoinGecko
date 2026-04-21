using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Protocol;

/// <summary>Outer ActionCable frame. The <see cref="Identifier"/> field is a JSON-encoded string (not a nested object).</summary>
public sealed class ActionCableFrame
{
    /// <summary>Command (<c>"subscribe"</c>, <c>"unsubscribe"</c>, <c>"message"</c>), or null on server-sent frames.</summary>
    [JsonPropertyName("command")] public string? Command { get; init; }

    /// <summary>Server-sent type marker (<c>"ping"</c>, <c>"welcome"</c>, <c>"confirm_subscription"</c>, etc.), or null on client frames.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }

    /// <summary>JSON-encoded channel identifier (deserialize separately via <see cref="ActionCableIdentifierConverter"/>).</summary>
    [JsonPropertyName("identifier")] public ActionCableIdentifier? Identifier { get; init; }

    /// <summary>Inner payload — another JSON-encoded string when <see cref="Command"/> is <c>"message"</c>; a typed object for server pushes.</summary>
    [JsonPropertyName("data")] public string? DataRaw { get; init; }

    /// <summary>Parsed server-pushed message body (only populated on server frames where <c>message</c> is a nested object).</summary>
    [JsonPropertyName("message")] public System.Text.Json.JsonElement Message { get; init; }
}
