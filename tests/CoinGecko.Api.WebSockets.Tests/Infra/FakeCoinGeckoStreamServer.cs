using System.Net;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace CoinGecko.Api.WebSockets.Tests.Infra;

/// <summary>
/// In-process Kestrel app that acts as a minimal ActionCable server for integration tests.
/// Supports: upgrading requests to WebSockets, recording inbound frames, and pushing
/// canned outbound frames on demand.
/// </summary>
public sealed class FakeCoinGeckoStreamServer : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly List<string> _receivedFrames = new();
    private readonly object _lock = new();
    private WebSocket? _currentSocket;

    /// <summary>WebSocket endpoint URI (<c>ws://127.0.0.1:{port}/v1</c>).</summary>
    public Uri Uri { get; }

    private FakeCoinGeckoStreamServer(WebApplication app, Uri uri)
    {
        _app = app;
        Uri = uri;
    }

    /// <summary>All text frames received from the client since startup.</summary>
    public IReadOnlyList<string> ReceivedFrames
    {
        get
        {
            lock (_lock)
            {
                return _receivedFrames.ToArray();
            }
        }
    }

    /// <summary>Create, configure, and start a new fake server bound to an ephemeral loopback port.</summary>
    public static async Task<FakeCoinGeckoStreamServer> StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel(k => k.Listen(IPAddress.Loopback, 0));
        var app = builder.Build();
        app.UseWebSockets();
        FakeCoinGeckoStreamServer? instance = null;

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/v1")
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                var ws = await context.WebSockets.AcceptWebSocketAsync();
                instance!._currentSocket = ws;
                await instance.SendAsync("{\"type\":\"welcome\"}", context.RequestAborted);
                var buffer = new byte[16 * 1024];
                while (ws.State == WebSocketState.Open)
                {
                    var sb = new StringBuilder();
                    WebSocketReceiveResult r;
                    do
                    {
                        r = await ws.ReceiveAsync(buffer, context.RequestAborted);
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, r.Count));
                    }
                    while (!r.EndOfMessage);

                    if (r.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    instance._Record(sb.ToString());
                }
            }
            else
            {
                await next();
            }
        });

        await app.StartAsync();
        var serverUri = app.Urls.First();
        var uri = new Uri(serverUri.Replace("http://", "ws://", StringComparison.Ordinal) + "/v1");
        instance = new FakeCoinGeckoStreamServer(app, uri);
        return instance;
    }

    private void _Record(string frame)
    {
        lock (_lock)
        {
            _receivedFrames.Add(frame);
        }
    }

    /// <summary>Push a canned frame to the currently connected client.</summary>
    public async Task PushAsync(string frame, CancellationToken ct = default)
    {
        if (_currentSocket is null || _currentSocket.State != WebSocketState.Open)
        {
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(frame);
        await _currentSocket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
    }

    /// <summary>Send a frame to the currently connected client (internal helper; also public for tests).</summary>
    public async Task SendAsync(string frame, CancellationToken ct)
    {
        if (_currentSocket is null)
        {
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(frame);
        await _currentSocket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_currentSocket?.State == WebSocketState.Open)
        {
            await _currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "test cleanup", CancellationToken.None);
        }

        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
