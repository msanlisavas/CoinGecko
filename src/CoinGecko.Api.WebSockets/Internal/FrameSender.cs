using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CoinGecko.Api.WebSockets.Protocol;

namespace CoinGecko.Api.WebSockets.Internal;

internal static class FrameSender
{
    public static Task SendSubscribeAsync(WebSocket ws, string channel, CancellationToken ct)
        => SendCommandAsync(ws, "subscribe", channel, data: null, ct);

    public static Task SendUnsubscribeAsync(WebSocket ws, string channel, CancellationToken ct)
        => SendCommandAsync(ws, "unsubscribe", channel, data: null, ct);

    public static Task SendMessageAsync(WebSocket ws, string channel, string dataJson, CancellationToken ct)
        => SendCommandAsync(ws, "message", channel, data: dataJson, ct);

    // Builds the outer ActionCable frame directly with Utf8JsonWriter — no reflection, fully AOT-safe.
    // Wire format: {"command":"subscribe","identifier":"{\"channel\":\"CGSimplePrice\"}"}
    // For "message" frames: {"command":"message","identifier":"{\"channel\":\"...\"}","data":"<escaped-dataJson>"}
    private static async Task SendCommandAsync(
        WebSocket ws,
        string command,
        string channel,
        string? data,
        CancellationToken ct)
    {
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            writer.WriteStartObject();
            writer.WriteString("command", command);

            // identifier: the inner channel object serialised to a JSON string
            var identifierInner = JsonSerializer.Serialize(
                new ActionCableIdentifier { Channel = channel },
                ActionCableProtocolJsonContext.Default.ActionCableIdentifier);
            writer.WriteString("identifier", identifierInner);

            if (data is not null)
            {
                writer.WriteString("data", data);
            }

            writer.WriteEndObject();
        }

        var bytes = ms.ToArray();
        await ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct).ConfigureAwait(false);
    }
}
