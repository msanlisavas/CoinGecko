# CoinGecko.Api.WebSockets — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development. Checkbox (`- [ ]`) steps are task-granularity.

**Goal:** Ship `CoinGecko.Api.WebSockets` v0.1.0-preview to NuGet — streaming client for CoinGecko's beta WebSocket endpoint at `wss://stream.coingecko.com/v1`. Covers all four channels (C1 CGSimplePrice, G1 OnchainSimpleTokenPrice, G2 OnchainTrade, G3 OnchainOHLCV), ActionCable JSON-in-JSON envelope, auto-reconnect, heartbeat, and subscription-cap enforcement. Stays in 0.x while upstream protocol is beta.

**Architecture:** One package depending on `CoinGecko.Api`. `System.Net.WebSockets.ClientWebSocket` under the hood. Custom two-layer JSON serializer for the ActionCable envelope. Dedicated receive loop + per-topic `Channel<T>` dispatch. DI-first `AddCoinGeckoStream(...)` returning a configuration builder.

**Tech Stack:** `System.Net.WebSockets.Client` · `System.Threading.Channels` · System.Text.Json source-gen · xUnit v3 + Shouldly + NSubstitute · ASP.NET Core Kestrel (fake WS server for integration tests) · `Microsoft.Extensions.Hosting` (DI + lifetime hooks).

**Spec reference:** [`docs/superpowers/specs/2026-04-21-coingecko-wrapper-design.md`](../specs/2026-04-21-coingecko-wrapper-design.md) §6.
**Protocol reference:** [`docs/coingecko-api-research.md`](../../coingecko-api-research.md) §5.

---

## Global conventions (same as Plan 1)

- Working dir `D:/repos/CoinGecko/`, branch `main`.
- Every commit: `git -c commit.gpgsign=false commit -m "..." --author="msanlisavas <muratsanlisavas@gmail.com>"`. **No `Co-Authored-By` trailer.**
- XML `/// <summary>` on every public / protected member. `internal` types don't need docs. CS1591 is fatal in Release.
- xUnit v3: test async calls pass `TestContext.Current.CancellationToken`.
- CA1860: `.Count == 0` over `.Any()`.
- Every DTO registered in the package's own source-gen JSON context.
- TDD where tests are listed. Implement → Red → Green → Commit.

## Phase index

| # | Phase | Produces |
|---|---|---|
| 0 | Package scaffold | `src/CoinGecko.Api.WebSockets/` csproj + root README + solution add. |
| 1 | ActionCable envelope types + custom converter | `ActionCableFrame`, `ActionCableIdentifier`, JSON-in-JSON converter. |
| 2 | Options + enums + stream state | `CoinGeckoStreamOptions`, `StreamState`, `StreamStateChangedEventArgs`, `CoinGeckoStreamException`. |
| 3 | Channel topic registry + subscription handle | Internal `SubscriptionHandle`, per-topic `Channel<T>` dispatch. |
| 4 | Typed push-message DTOs (C1 / G1 / G2 / G3) | `CoinPriceTick`, `OnchainTokenPriceTick`, `DexTrade`, `OnchainOhlcvCandle`. |
| 5 | `CoinGeckoStream` core (connect, receive loop, subscribe/unsubscribe) | `ICoinGeckoStream` + `CoinGeckoStream` + receive loop task + dispatcher. |
| 6 | Reconnect + heartbeat supervision | Exponential backoff, subscription restoration, heartbeat timeout. |
| 7 | DI registration | `AddCoinGeckoStream(...)` extension returning builder. |
| 8 | Fake WS server for tests | `tests/CoinGecko.Api.WebSockets.Tests/Infra/FakeCoinGeckoStreamServer.cs` (Kestrel). |
| 9 | Integration tests | connect, subscribe C1/G1/G2/G3, heartbeat timeout, reconnect, subscription cap, state transitions. |
| 10 | Sample + README + pack + tag | `samples/CoinGecko.Api.Samples.StreamConsole`, package README, `dotnet pack` smoke, `v0.1.0-websockets` local tag. |

---

## Phase 0 — Package scaffold

### Task 0.1: Create `CoinGecko.Api.WebSockets` project

**Files:**
- Create: `src/CoinGecko.Api.WebSockets/CoinGecko.Api.WebSockets.csproj`
- Create: `src/CoinGecko.Api.WebSockets/README.md`

- [ ] **Step 1: Scaffold**

```bash
dotnet new classlib -n CoinGecko.Api.WebSockets -o src/CoinGecko.Api.WebSockets -f net9.0 --force
rm src/CoinGecko.Api.WebSockets/Class1.cs
```

- [ ] **Step 2: Overwrite csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net9.0;net8.0</TargetFrameworks>
    <RootNamespace>CoinGecko.Api.WebSockets</RootNamespace>
    <AssemblyName>CoinGecko.Api.WebSockets</AssemblyName>
    <IsPackable>true</IsPackable>
    <IsAotCompatible>true</IsAotCompatible>
    <IsTrimmable>true</IsTrimmable>
    <PackageId>CoinGecko.Api.WebSockets</PackageId>
    <Description>Streaming client for CoinGecko's beta WebSocket endpoint (wss://stream.coingecko.com/v1). Four typed channels: coin prices, onchain token prices, DEX trades, DEX OHLCV. Builds on CoinGecko.Api. In preview while upstream protocol is beta.</Description>
    <PackageTags>coingecko crypto cryptocurrency websocket streaming realtime geckoterminal dex</PackageTags>
    <VersionPrefix>0.1.0</VersionPrefix>
    <VersionSuffix>preview</VersionSuffix>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\CoinGecko.Api\CoinGecko.Api.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Options" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>

  <ItemGroup Label="Build-only">
    <PackageReference Include="MinVer" PrivateAssets="all" />
    <PackageReference Include="Microsoft.SourceLink.GitHub" PrivateAssets="all" />
  </ItemGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Write pack-time README**

`src/CoinGecko.Api.WebSockets/README.md`:

```markdown
# CoinGecko.Api.WebSockets

Streaming client for [CoinGecko](https://www.coingecko.com/)'s beta WebSocket API. Analyst+ plan required.

## Install

```powershell
dotnet add package CoinGecko.Api.WebSockets
```

## Quickstart

```csharp
builder.Services.AddCoinGeckoStream(opts =>
{
    opts.ApiKey = builder.Configuration["CoinGecko:ApiKey"];
    opts.AutoReconnect = true;
});

// Later, in a hosted service:
public sealed class PriceWatcher(ICoinGeckoStream stream)
{
    public async Task StartAsync(CancellationToken ct)
    {
        await stream.ConnectAsync(ct);

        var sub = await stream.SubscribeCoinPricesAsync(
            coinIds: ["bitcoin", "ethereum"],
            onTick: tick => Console.WriteLine($"{tick.CoinId}: ${tick.Price:N2}"),
            ct);
    }
}
```

## Preview status

CoinGecko's WebSocket API is itself in beta. This package ships in `0.x` until the upstream protocol stabilizes. Expect breaking changes across minor versions.

## License

MIT © msanlisavas
```

- [ ] **Step 4: Add to solution and build**

```bash
dotnet sln CoinGecko.sln add src/CoinGecko.Api.WebSockets/CoinGecko.Api.WebSockets.csproj
dotnet build src/CoinGecko.Api.WebSockets -c Release
```

Expected: 0 warnings, 0 errors.

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api.WebSockets/ CoinGecko.sln
git -c commit.gpgsign=false commit -m "feat(ws): scaffold CoinGecko.Api.WebSockets project" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 0.2: Test project scaffold

**Files:**
- Create: `tests/CoinGecko.Api.WebSockets.Tests/CoinGecko.Api.WebSockets.Tests.csproj`
- Create: `tests/CoinGecko.Api.WebSockets.Tests/GlobalUsings.cs`

- [ ] **Step 1: Scaffold**

```bash
dotnet new classlib -n CoinGecko.Api.WebSockets.Tests -o tests/CoinGecko.Api.WebSockets.Tests -f net9.0 --force
rm tests/CoinGecko.Api.WebSockets.Tests/Class1.cs
```

- [ ] **Step 2: Overwrite csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFrameworks>net9.0;net8.0</TargetFrameworks>
    <RootNamespace>CoinGecko.Api.WebSockets.Tests</RootNamespace>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <OutputType>Exe</OutputType>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
    <UserSecretsId>coingecko-ws-tests</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="Shouldly" />
    <PackageReference Include="NSubstitute" />
    <PackageReference Include="NSubstitute.Analyzers.CSharp">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\CoinGecko.Api.WebSockets\CoinGecko.Api.WebSockets.csproj" />
  </ItemGroup>

</Project>
```

> **Note on `Microsoft.NET.Sdk.Web`:** the test project uses the Web SDK so it can host a Kestrel-based fake WebSocket server directly in-process for integration tests. No other project in the solution does this — test-project-only.

- [ ] **Step 3: Global usings**

`tests/CoinGecko.Api.WebSockets.Tests/GlobalUsings.cs`:

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Net.WebSockets;
global using System.Threading;
global using System.Threading.Tasks;
global using Shouldly;
global using Xunit;
```

- [ ] **Step 4: Add to solution and build**

```bash
dotnet sln CoinGecko.sln add tests/CoinGecko.Api.WebSockets.Tests/CoinGecko.Api.WebSockets.Tests.csproj
dotnet build tests/CoinGecko.Api.WebSockets.Tests -c Debug
```

- [ ] **Step 5: Commit**

```bash
git add tests/CoinGecko.Api.WebSockets.Tests/ CoinGecko.sln
git -c commit.gpgsign=false commit -m "test(ws): scaffold CoinGecko.Api.WebSockets.Tests project" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 1 — ActionCable envelope

### Task 1.1: `ActionCableFrame` + `ActionCableIdentifier` types

**Files:**
- Create: `src/CoinGecko.Api.WebSockets/Protocol/ActionCableFrame.cs`
- Create: `src/CoinGecko.Api.WebSockets/Protocol/ActionCableIdentifier.cs`

- [ ] **Step 1: Implement**

`src/CoinGecko.Api.WebSockets/Protocol/ActionCableIdentifier.cs`:

```csharp
using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Protocol;

/// <summary>Inner ActionCable channel selector — serialized as JSON then quoted into the outer frame's <c>identifier</c> field.</summary>
public sealed class ActionCableIdentifier
{
    /// <summary>Channel name, e.g. <c>"CGSimplePrice"</c>.</summary>
    [JsonPropertyName("channel")] public string Channel { get; init; } = string.Empty;
}
```

`src/CoinGecko.Api.WebSockets/Protocol/ActionCableFrame.cs`:

```csharp
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
```

> The `Identifier` field carries *stringified* JSON on the wire (per ActionCable), but we model it as a typed object on the C# side. A custom `JsonConverter<ActionCableIdentifier>` (Task 1.2) handles the double-encoding.

- [ ] **Step 2: Commit** (after Task 1.2 — these land together)

---

### Task 1.2: `ActionCableIdentifierConverter`

**Files:**
- Create: `src/CoinGecko.Api.WebSockets/Protocol/ActionCableIdentifierConverter.cs`
- Create: `tests/CoinGecko.Api.WebSockets.Tests/Protocol/ActionCableEnvelopeTests.cs`

- [ ] **Step 1: Failing test**

`tests/CoinGecko.Api.WebSockets.Tests/Protocol/ActionCableEnvelopeTests.cs`:

```csharp
using System.Text.Json;
using CoinGecko.Api.WebSockets.Protocol;

namespace CoinGecko.Api.WebSockets.Tests.Protocol;

public class ActionCableEnvelopeTests
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        Converters = { new ActionCableIdentifierConverter() },
    };

    [Fact]
    public void Identifier_reads_double_encoded_json_string()
    {
        const string raw = "\"{\\\"channel\\\":\\\"CGSimplePrice\\\"}\"";
        var id = JsonSerializer.Deserialize<ActionCableIdentifier>(raw, Opts);
        id.ShouldNotBeNull();
        id!.Channel.ShouldBe("CGSimplePrice");
    }

    [Fact]
    public void Identifier_writes_as_json_string()
    {
        var id = new ActionCableIdentifier { Channel = "CGSimplePrice" };
        var s = JsonSerializer.Serialize(id, Opts);
        s.ShouldBe("\"{\\u0022channel\\u0022:\\u0022CGSimplePrice\\u0022}\"");
    }

    [Fact]
    public void Full_frame_subscribe_roundtrips()
    {
        const string raw = "{\"command\":\"subscribe\",\"identifier\":\"{\\\"channel\\\":\\\"CGSimplePrice\\\"}\"}";
        var frame = JsonSerializer.Deserialize<ActionCableFrame>(raw, Opts);
        frame.ShouldNotBeNull();
        frame!.Command.ShouldBe("subscribe");
        frame.Identifier!.Channel.ShouldBe("CGSimplePrice");
    }
}
```

- [ ] **Step 2: Run — expect compile fail.**

- [ ] **Step 3: Implement**

`src/CoinGecko.Api.WebSockets/Protocol/ActionCableIdentifierConverter.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Protocol;

/// <summary>Reads / writes <see cref="ActionCableIdentifier"/> as a JSON-encoded string (ActionCable's double-encoded wire format).</summary>
public sealed class ActionCableIdentifierConverter : JsonConverter<ActionCableIdentifier>
{
    /// <inheritdoc/>
    public override ActionCableIdentifier? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token for ActionCable identifier, got {reader.TokenType}.");
        }

        var json = reader.GetString();
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        // Inner JSON is a small, fixed-shape object — deserialize with plain STJ reflection on the
        // known simple type. This is AOT-safe because ActionCableIdentifier has a trivial shape.
        return JsonSerializer.Deserialize(json, ActionCableProtocolJsonContext.Default.ActionCableIdentifier);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ActionCableIdentifier value, JsonSerializerOptions options)
    {
        var inner = JsonSerializer.Serialize(value, ActionCableProtocolJsonContext.Default.ActionCableIdentifier);
        writer.WriteStringValue(inner);
    }
}
```

- [ ] **Step 4: Add protocol JsonContext**

`src/CoinGecko.Api.WebSockets/Protocol/ActionCableProtocolJsonContext.cs`:

```csharp
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
```

- [ ] **Step 5: Run — pass.**

- [ ] **Step 6: Commit**

```bash
git add src/CoinGecko.Api.WebSockets/Protocol/ tests/CoinGecko.Api.WebSockets.Tests/Protocol/
git -c commit.gpgsign=false commit -m "feat(ws): add ActionCable envelope + JSON-in-JSON identifier converter" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 2 — Options, enums, stream state

### Task 2.1: Enums + options + exception

**Files:**
- Create: `src/CoinGecko.Api.WebSockets/StreamState.cs`
- Create: `src/CoinGecko.Api.WebSockets/CoinGeckoStreamOptions.cs`
- Create: `src/CoinGecko.Api.WebSockets/StreamStateChangedEventArgs.cs`
- Create: `src/CoinGecko.Api.WebSockets/CoinGeckoStreamException.cs`
- Create: `src/CoinGecko.Api.WebSockets.Tests/OptionsAndStateTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using CoinGecko.Api.WebSockets;

namespace CoinGecko.Api.WebSockets.Tests;

public class OptionsAndStateTests
{
    [Fact]
    public void StreamState_values_cover_lifecycle_transitions()
    {
        var all = Enum.GetValues<StreamState>();
        all.ShouldContain(StreamState.Disconnected);
        all.ShouldContain(StreamState.Connecting);
        all.ShouldContain(StreamState.Connected);
        all.ShouldContain(StreamState.Reconnecting);
        all.ShouldContain(StreamState.Faulted);
        ((int)default(StreamState)).ShouldBe(0);
    }

    [Fact]
    public void Options_defaults()
    {
        var o = new CoinGeckoStreamOptions();
        o.ApiKey.ShouldBeNull();
        o.BaseAddress.ShouldBe(new Uri("wss://stream.coingecko.com/v1"));
        o.AutoReconnect.ShouldBeTrue();
        o.MaxReconnectAttempts.ShouldBe(10);
        o.HeartbeatTimeout.ShouldBe(TimeSpan.FromSeconds(25)); // server pings every ~10s; 25s covers two missed pings
        o.MaxSubscriptionsPerChannel.ShouldBe(100);
        o.ReceiveBufferSize.ShouldBe(16 * 1024);
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Implement**

`src/CoinGecko.Api.WebSockets/StreamState.cs`:

```csharp
namespace CoinGecko.Api.WebSockets;

/// <summary>Lifecycle state of an <see cref="ICoinGeckoStream"/>.</summary>
public enum StreamState
{
    /// <summary>Not connected. Initial state and terminal state after <c>DisconnectAsync</c>.</summary>
    Disconnected = 0,
    /// <summary>Opening the WebSocket handshake.</summary>
    Connecting = 1,
    /// <summary>Open and receiving frames.</summary>
    Connected = 2,
    /// <summary>Temporarily disconnected; attempting to reconnect.</summary>
    Reconnecting = 3,
    /// <summary>Terminal failure. Inspect <c>.Exception</c> for details.</summary>
    Faulted = 4,
}
```

`src/CoinGecko.Api.WebSockets/CoinGeckoStreamOptions.cs`:

```csharp
namespace CoinGecko.Api.WebSockets;

/// <summary>Configuration for <see cref="ICoinGeckoStream"/>.</summary>
public sealed class CoinGeckoStreamOptions
{
    /// <summary>Pro-tier API key (Analyst+ plan required by CoinGecko).</summary>
    public string? ApiKey { get; set; }

    /// <summary>WebSocket base URL. Override for tests or proxies.</summary>
    public Uri BaseAddress { get; set; } = new("wss://stream.coingecko.com/v1");

    /// <summary>Whether to reconnect automatically after a disconnect.</summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>Maximum reconnect attempts before transitioning to <see cref="StreamState.Faulted"/>.</summary>
    public int MaxReconnectAttempts { get; set; } = 10;

    /// <summary>If no server message / ping arrives within this window, the connection is considered dead.</summary>
    public TimeSpan HeartbeatTimeout { get; set; } = TimeSpan.FromSeconds(25);

    /// <summary>Client-side cap on subscriptions per channel (mirrors the upstream 100 per socket).</summary>
    public int MaxSubscriptionsPerChannel { get; set; } = 100;

    /// <summary>Receive buffer size for individual WebSocket frames.</summary>
    public int ReceiveBufferSize { get; set; } = 16 * 1024;

    /// <summary>Base WS KeepAliveInterval (the server pings every ~10s).</summary>
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);
}
```

`src/CoinGecko.Api.WebSockets/StreamStateChangedEventArgs.cs`:

```csharp
namespace CoinGecko.Api.WebSockets;

/// <summary>Arguments for <see cref="ICoinGeckoStream.StateChanged"/>.</summary>
public sealed class StreamStateChangedEventArgs(StreamState previous, StreamState current, Exception? error) : EventArgs
{
    /// <summary>Previous state.</summary>
    public StreamState Previous { get; } = previous;
    /// <summary>Current (new) state.</summary>
    public StreamState Current { get; } = current;
    /// <summary>Error that triggered the transition, if any.</summary>
    public Exception? Error { get; } = error;
}
```

`src/CoinGecko.Api.WebSockets/CoinGeckoStreamException.cs`:

```csharp
namespace CoinGecko.Api.WebSockets;

/// <summary>Thrown by <see cref="ICoinGeckoStream"/> for protocol / subscription errors.</summary>
public sealed class CoinGeckoStreamException : Exception
{
    /// <summary>Create an exception with a message.</summary>
    public CoinGeckoStreamException(string message) : base(message) { }
    /// <summary>Create an exception with a message and inner cause.</summary>
    public CoinGeckoStreamException(string message, Exception inner) : base(message, inner) { }
}
```

- [ ] **Step 4: Pass. Commit.**

```bash
git -c commit.gpgsign=false commit -m "feat(ws): add StreamState, CoinGeckoStreamOptions, event args, exception" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 3 — Subscription handle + topic registry (internal primitives)

### Task 3.1: `SubscriptionHandle`

**Files:**
- Create: `src/CoinGecko.Api.WebSockets/Internal/SubscriptionHandle.cs`
- Create: `src/CoinGecko.Api.WebSockets/Internal/ChannelDispatcher.cs`

Both types are `internal`. No XML docs required. Implement without tests (covered by Phase 9 integration tests).

`SubscriptionHandle.cs`:

```csharp
namespace CoinGecko.Api.WebSockets.Internal;

internal sealed class SubscriptionHandle(Func<ValueTask> onDispose) : IAsyncDisposable
{
    private int _disposed;

    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return ValueTask.CompletedTask;
        }
        return onDispose();
    }
}
```

`ChannelDispatcher.cs`:

```csharp
using System.Collections.Concurrent;

namespace CoinGecko.Api.WebSockets.Internal;

/// <summary>Per-channel dispatcher: routes decoded push-messages to subscriber callbacks.</summary>
internal sealed class ChannelDispatcher<TTick>
{
    private readonly ConcurrentDictionary<Guid, Action<TTick>> _subscribers = new();

    public Guid Subscribe(Action<TTick> onTick)
    {
        var id = Guid.NewGuid();
        _subscribers[id] = onTick;
        return id;
    }

    public void Unsubscribe(Guid id) => _subscribers.TryRemove(id, out _);

    public int Count => _subscribers.Count;

    public void Dispatch(TTick tick)
    {
        foreach (var kvp in _subscribers)
        {
            try { kvp.Value(tick); } catch { /* subscriber exceptions must not kill the receive loop */ }
        }
    }
}
```

Commit:

```bash
git -c commit.gpgsign=false commit -m "feat(ws): add internal SubscriptionHandle and per-channel dispatcher" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 4 — Typed push-message DTOs

### Task 4.1: C1, G1, G2, G3 ticks

**Files:**
- Create: `src/CoinGecko.Api.WebSockets/Ticks/CoinPriceTick.cs`
- Create: `src/CoinGecko.Api.WebSockets/Ticks/OnchainTokenPriceTick.cs`
- Create: `src/CoinGecko.Api.WebSockets/Ticks/DexTrade.cs`
- Create: `src/CoinGecko.Api.WebSockets/Ticks/OnchainOhlcvCandle.cs`

Per research: C1 field set is `{c, i, vs, p, pp, m, v, t}` (timestamps as float seconds); G2 is milliseconds int; G3 is seconds int.

`src/CoinGecko.Api.WebSockets/Ticks/CoinPriceTick.cs`:

```csharp
using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Ticks;

/// <summary>C1 channel tick — CoinGecko.com aggregated coin price.</summary>
public sealed class CoinPriceTick
{
    /// <summary>Channel code (always <c>"C1"</c>).</summary>
    [JsonPropertyName("c")] public string? ChannelCode { get; init; }
    /// <summary>Coin id.</summary>
    [JsonPropertyName("i")] public string? CoinId { get; init; }
    /// <summary>Quote currency code.</summary>
    [JsonPropertyName("vs")] public string? VsCurrency { get; init; }
    /// <summary>Current price in quote currency.</summary>
    [JsonPropertyName("p")] public decimal Price { get; init; }
    /// <summary>24-hour percentage change.</summary>
    [JsonPropertyName("pp")] public decimal? PricePercentChange24h { get; init; }
    /// <summary>Market cap.</summary>
    [JsonPropertyName("m")] public decimal? MarketCap { get; init; }
    /// <summary>24h volume.</summary>
    [JsonPropertyName("v")] public decimal? Volume24h { get; init; }
    /// <summary>UTC timestamp (parsed from unix seconds, possibly fractional).</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Raw <c>t</c> field (unix seconds as float, assigned by deserializer, translated to <see cref="Timestamp"/> on construction).</summary>
    [JsonPropertyName("t")]
    [JsonInclude]
    internal double RawT
    {
        get => ((DateTimeOffset)Timestamp).ToUnixTimeMilliseconds() / 1000.0;
        init => Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)(value * 1000));
    }
}
```

`src/CoinGecko.Api.WebSockets/Ticks/OnchainTokenPriceTick.cs`:

```csharp
using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Ticks;

/// <summary>G1 channel tick — GeckoTerminal onchain token price.</summary>
public sealed class OnchainTokenPriceTick
{
    /// <summary>Channel code (always <c>"G1"</c>).</summary>
    [JsonPropertyName("ch")] public string? ChannelCode { get; init; }
    /// <summary>Network id.</summary>
    [JsonPropertyName("n")] public string? NetworkId { get; init; }
    /// <summary>Token contract address.</summary>
    [JsonPropertyName("ta")] public string? TokenAddress { get; init; }
    /// <summary>Price in USD.</summary>
    [JsonPropertyName("pu")] public decimal? PriceUsd { get; init; }
    /// <summary>Price in native currency.</summary>
    [JsonPropertyName("pn")] public decimal? PriceNative { get; init; }
    /// <summary>Fully-diluted valuation USD.</summary>
    [JsonPropertyName("fdv")] public decimal? FdvUsd { get; init; }
    /// <summary>Total reserve USD across tracked pools.</summary>
    [JsonPropertyName("tr")] public decimal? TotalReserveUsd { get; init; }
    /// <summary>UTC timestamp.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Raw unix-seconds float.</summary>
    [JsonPropertyName("t")]
    [JsonInclude]
    internal double RawT
    {
        get => ((DateTimeOffset)Timestamp).ToUnixTimeMilliseconds() / 1000.0;
        init => Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)(value * 1000));
    }
}
```

`src/CoinGecko.Api.WebSockets/Ticks/DexTrade.cs`:

```csharp
using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Ticks;

/// <summary>G2 channel tick — DEX pool trade (swap).</summary>
public sealed class DexTrade
{
    /// <summary>Channel code (always <c>"G2"</c>).</summary>
    [JsonPropertyName("ch")] public string? ChannelCode { get; init; }
    /// <summary>Network id.</summary>
    [JsonPropertyName("n")] public string? NetworkId { get; init; }
    /// <summary>Pool contract address.</summary>
    [JsonPropertyName("pa")] public string? PoolAddress { get; init; }
    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("tx")] public string? TxHash { get; init; }
    /// <summary>Trade type — <c>"b"</c> (buy base) or <c>"s"</c> (sell base).</summary>
    [JsonPropertyName("ty")] public string? TradeType { get; init; }
    /// <summary>Base token amount.</summary>
    [JsonPropertyName("to")] public decimal? BaseTokenAmount { get; init; }
    /// <summary>Quote token amount.</summary>
    [JsonPropertyName("toq")] public decimal? QuoteTokenAmount { get; init; }
    /// <summary>Trade volume in USD.</summary>
    [JsonPropertyName("vo")] public decimal? VolumeUsd { get; init; }
    /// <summary>Base price in native currency.</summary>
    [JsonPropertyName("pc")] public decimal? PriceNative { get; init; }
    /// <summary>Base price in USD.</summary>
    [JsonPropertyName("pu")] public decimal? PriceUsd { get; init; }
    /// <summary>UTC timestamp (G2 uses ms-since-epoch).</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Raw unix milliseconds.</summary>
    [JsonPropertyName("t")]
    [JsonInclude]
    internal long RawT
    {
        get => Timestamp.ToUnixTimeMilliseconds();
        init => Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(value);
    }
}
```

`src/CoinGecko.Api.WebSockets/Ticks/OnchainOhlcvCandle.cs`:

```csharp
using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Ticks;

/// <summary>G3 channel tick — DEX pool OHLCV candle.</summary>
public sealed class OnchainOhlcvCandle
{
    /// <summary>Channel code (always <c>"G3"</c>).</summary>
    [JsonPropertyName("ch")] public string? ChannelCode { get; init; }
    /// <summary>Network id.</summary>
    [JsonPropertyName("n")] public string? NetworkId { get; init; }
    /// <summary>Pool address.</summary>
    [JsonPropertyName("pa")] public string? PoolAddress { get; init; }
    /// <summary>Token side — <c>"base"</c> or <c>"quote"</c>.</summary>
    [JsonPropertyName("to")] public string? TokenSide { get; init; }
    /// <summary>Interval (<c>"1m"</c>, <c>"5m"</c>, <c>"15m"</c>, <c>"1h"</c>, <c>"4h"</c>, <c>"1d"</c>).</summary>
    [JsonPropertyName("i")] public string? Interval { get; init; }
    /// <summary>Open price.</summary>
    [JsonPropertyName("o")] public decimal Open { get; init; }
    /// <summary>High price.</summary>
    [JsonPropertyName("h")] public decimal High { get; init; }
    /// <summary>Low price.</summary>
    [JsonPropertyName("l")] public decimal Low { get; init; }
    /// <summary>Close price.</summary>
    [JsonPropertyName("c")] public decimal Close { get; init; }
    /// <summary>Volume.</summary>
    [JsonPropertyName("v")] public decimal? Volume { get; init; }
    /// <summary>Candle start (UTC, parsed from unix seconds).</summary>
    public DateTimeOffset CandleStart { get; init; }

    /// <summary>Raw unix seconds (G3 uses integer seconds).</summary>
    [JsonPropertyName("t")]
    [JsonInclude]
    internal long RawT
    {
        get => CandleStart.ToUnixTimeSeconds();
        init => CandleStart = DateTimeOffset.FromUnixTimeSeconds(value);
    }
}
```

- [ ] Register all four ticks in a new `src/CoinGecko.Api.WebSockets/Ticks/TicksJsonContext.cs`:

```csharp
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
```

- [ ] Commit:

```bash
git -c commit.gpgsign=false commit -m "feat(ws): add C1/G1/G2/G3 push-message DTOs and source-gen JsonContext" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 5 — `CoinGeckoStream` core

### Task 5.1: `ICoinGeckoStream` public interface

`src/CoinGecko.Api.WebSockets/ICoinGeckoStream.cs`:

```csharp
using CoinGecko.Api.WebSockets.Ticks;

namespace CoinGecko.Api.WebSockets;

/// <summary>Streaming client for CoinGecko's WebSocket API (beta).</summary>
public interface ICoinGeckoStream : IAsyncDisposable
{
    /// <summary>Current state.</summary>
    StreamState State { get; }

    /// <summary>Last exception that caused a transition away from <see cref="StreamState.Connected"/>, or null if healthy.</summary>
    Exception? LastException { get; }

    /// <summary>Raised on every state transition.</summary>
    event EventHandler<StreamStateChangedEventArgs>? StateChanged;

    /// <summary>Open the WebSocket and enter <see cref="StreamState.Connected"/>.</summary>
    Task ConnectAsync(CancellationToken ct = default);

    /// <summary>Gracefully close. Enters <see cref="StreamState.Disconnected"/>.</summary>
    Task DisconnectAsync(CancellationToken ct = default);

    /// <summary>C1: subscribe to coin prices. Disposing the returned handle unsubscribes just this subscription.</summary>
    Task<IAsyncDisposable> SubscribeCoinPricesAsync(
        IReadOnlyList<string> coinIds,
        IReadOnlyList<string> vsCurrencies,
        Action<CoinPriceTick> onTick,
        CancellationToken ct = default);

    /// <summary>G1: subscribe to onchain token prices. Tokens are <c>network_id:address</c> pairs.</summary>
    Task<IAsyncDisposable> SubscribeOnchainTokenPricesAsync(
        IReadOnlyList<string> networkAndTokenAddresses,
        Action<OnchainTokenPriceTick> onTick,
        CancellationToken ct = default);

    /// <summary>G2: subscribe to DEX pool trades. Pools are <c>network_id:pool_address</c> pairs.</summary>
    Task<IAsyncDisposable> SubscribeDexTradesAsync(
        IReadOnlyList<string> networkAndPoolAddresses,
        Action<DexTrade> onTrade,
        CancellationToken ct = default);

    /// <summary>G3: subscribe to DEX OHLCV candles.</summary>
    Task<IAsyncDisposable> SubscribeDexOhlcvAsync(
        IReadOnlyList<string> networkAndPoolAddresses,
        string interval,
        string token,
        Action<OnchainOhlcvCandle> onCandle,
        CancellationToken ct = default);
}
```

### Task 5.2: `CoinGeckoStream` implementation (core — connect + receive loop + subscribe skeleton)

This is the largest file. Implement in full in `src/CoinGecko.Api.WebSockets/CoinGeckoStream.cs`. Because of length, the implementer should follow these design points closely:

1. **Fields:**
   - `ClientWebSocket? _ws`
   - `CancellationTokenSource? _receiveCts`
   - `Task? _receiveLoop`
   - `TaskCompletionSource? _closeTcs`
   - `DateTimeOffset _lastMessageAt`
   - `Timer? _heartbeatTimer`
   - `ChannelDispatcher<CoinPriceTick> _c1 = new()`
   - `ChannelDispatcher<OnchainTokenPriceTick> _g1 = new()`
   - `ChannelDispatcher<DexTrade> _g2 = new()`
   - `ChannelDispatcher<OnchainOhlcvCandle> _g3 = new()`
   - `Dictionary<string, List<RestorableSubscription>> _activeSubs` — keyed by channel name; used for reconnect restoration.
   - `SemaphoreSlim _sendLock = new(1, 1)` — serializes outbound frames.
   - `SemaphoreSlim _stateLock = new(1, 1)` — serializes state transitions.

2. **`ConnectAsync` flow:**
   - Transition `Disconnected → Connecting`.
   - Build `ClientWebSocket`:
     - `Options.SetRequestHeader("x-cg-pro-api-key", _opts.ApiKey)` if set.
     - `Options.KeepAliveInterval = _opts.KeepAliveInterval`.
   - `ConnectAsync(_opts.BaseAddress, ct)`.
   - Spawn receive loop task.
   - Transition `Connecting → Connected`. Raise `StateChanged`.

3. **Receive loop:**
   - Allocate `byte[_opts.ReceiveBufferSize]` or pooled.
   - Loop: `WebSocket.ReceiveAsync(...)`. Assemble multi-frame messages into `MemoryStream` until `EndOfMessage == true`.
   - On each complete message: `_lastMessageAt = DateTimeOffset.UtcNow`. Deserialize the outer frame via `ActionCableProtocolJsonContext.Default.ActionCableFrame`.
   - If `frame.Type == "ping"` — ignore, just update timestamp.
   - If `frame.Type == "welcome"` — ignore, server-driven.
   - If `frame.Type == "confirm_subscription"` — log, no-op.
   - If `frame.Command == null` and `frame.Type == null` but `frame.Message` is present (server-pushed push-message) — dispatch based on `identifier.Channel`:
     - `"CGSimplePrice"` → deserialize `frame.Message` as `CoinPriceTick` via `TicksJsonContext`, `_c1.Dispatch(tick)`.
     - `"OnchainSimpleTokenPrice"` → `OnchainTokenPriceTick` → `_g1.Dispatch`.
     - `"OnchainTrade"` → `DexTrade` → `_g2.Dispatch`.
     - `"OnchainOHLCV"` → `OnchainOhlcvCandle` → `_g3.Dispatch`.
   - On `WebSocketException` or `OperationCanceledException` in the loop: if cancel was caller-initiated, exit cleanly; else trigger reconnect.

4. **`SubscribeXxxAsync` flow** (shared pattern):
   - Validate `State == Connected`; otherwise throw `CoinGeckoStreamException`.
   - Check `dispatcher.Count < _opts.MaxSubscriptionsPerChannel` — else throw.
   - Register callback on the dispatcher → get `Guid`.
   - Build a subscribe frame: outer `{command:"subscribe", identifier: {channel: "..."}}`.
   - Send it (await subscribe confirmation? or fire-and-forget? For v0.1 fire-and-forget; consumers see ticks arrive shortly).
   - Build a `message` frame with the `action: "set_tokens"` (C1, G1) or `"set_pools"` (G2, G3) payload.
   - Track in `_activeSubs[channel]` for restoration.
   - Return `SubscriptionHandle` whose dispose:
     - Sends an `"unset_tokens"` / `"unset_pools"` frame for just this subscription's resources.
     - Unregisters from dispatcher.
     - Removes from `_activeSubs`.

5. **DisposeAsync:** calls DisconnectAsync with a 5-second timeout.

The full implementation is large (~600 lines). The implementer produces it in one task with careful review. Release build must be green after.

### Task 5.3: Outbound frame helpers

Extract low-level send helpers to `src/CoinGecko.Api.WebSockets/Internal/FrameSender.cs`:

```csharp
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CoinGecko.Api.WebSockets.Protocol;

namespace CoinGecko.Api.WebSockets.Internal;

internal static class FrameSender
{
    private static readonly JsonSerializerOptions OuterOpts = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new ActionCableIdentifierConverter() },
    };

    public static Task SendSubscribeAsync(WebSocket ws, string channel, CancellationToken ct)
    {
        var frame = new ActionCableFrame
        {
            Command = "subscribe",
            Identifier = new ActionCableIdentifier { Channel = channel },
        };
        return SendAsync(ws, frame, ct);
    }

    public static Task SendUnsubscribeAsync(WebSocket ws, string channel, CancellationToken ct)
    {
        var frame = new ActionCableFrame
        {
            Command = "unsubscribe",
            Identifier = new ActionCableIdentifier { Channel = channel },
        };
        return SendAsync(ws, frame, ct);
    }

    public static Task SendMessageAsync(WebSocket ws, string channel, string dataJson, CancellationToken ct)
    {
        var frame = new ActionCableFrame
        {
            Command = "message",
            Identifier = new ActionCableIdentifier { Channel = channel },
            DataRaw = dataJson,
        };
        return SendAsync(ws, frame, ct);
    }

    private static async Task SendAsync(WebSocket ws, ActionCableFrame frame, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(frame, OuterOpts);
        var bytes = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct).ConfigureAwait(false);
    }
}
```

- [ ] Single commit for Phase 5:

```bash
git -c commit.gpgsign=false commit -m "feat(ws): implement ICoinGeckoStream core (connect, receive loop, subscribe)" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 6 — Reconnect + heartbeat supervision

### Task 6.1: Reconnect

Add to `CoinGeckoStream`:

- On receive-loop exception that is not caller-cancellation:
  - Transition `Connected → Reconnecting`.
  - Close the broken WebSocket.
  - Loop up to `MaxReconnectAttempts`:
    - Wait `ExponentialBackoff(attempt)` (base 1s, cap 30s, decorrelated jitter).
    - Retry `ConnectAsync`.
    - On success: re-send all subscriptions from `_activeSubs`. Transition `Reconnecting → Connected`.
  - On exhaustion: `Reconnecting → Faulted`.

Backoff helper (reuse pattern from Plan 1 `CoinGeckoRetryHandler`, but as stand-alone function — use decorrelated jitter).

### Task 6.2: Heartbeat watchdog

When `Connected`:
- `_heartbeatTimer` fires every `HeartbeatTimeout / 4`.
- If `DateTimeOffset.UtcNow - _lastMessageAt > HeartbeatTimeout`: trigger reconnect (same path as receive-loop exception).

Disable timer on state change away from `Connected`.

Single commit:

```bash
git -c commit.gpgsign=false commit -m "feat(ws): add reconnect + heartbeat supervision with subscription restoration" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 7 — DI registration

### Task 7.1: `AddCoinGeckoStream` extension

`src/CoinGecko.Api.WebSockets/ServiceCollectionExtensions.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.WebSockets;

/// <summary>DI extensions for <see cref="ICoinGeckoStream"/>.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Register a singleton <see cref="ICoinGeckoStream"/> with the given options.</summary>
    public static IServiceCollection AddCoinGeckoStream(
        this IServiceCollection services, Action<CoinGeckoStreamOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<CoinGeckoStreamOptions>();
        }

        services.AddSingleton<ICoinGeckoStream>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<CoinGeckoStreamOptions>>().Value;
            var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<CoinGeckoStream>>();
            return new CoinGeckoStream(opts, logger);
        });

        return services;
    }
}
```

- [ ] Commit:

```bash
git -c commit.gpgsign=false commit -m "feat(ws): add AddCoinGeckoStream DI extension" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 8 — Fake WS server (Kestrel, in-process)

### Task 8.1: `FakeCoinGeckoStreamServer`

`tests/CoinGecko.Api.WebSockets.Tests/Infra/FakeCoinGeckoStreamServer.cs`:

```csharp
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

    public Uri Uri { get; }

    private FakeCoinGeckoStreamServer(WebApplication app, Uri uri)
    {
        _app = app;
        Uri = uri;
    }

    public IReadOnlyList<string> ReceivedFrames
    {
        get { lock (_lock) return _receivedFrames.ToArray(); }
    }

    public static async Task<FakeCoinGeckoStreamServer> StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel(k => k.Listen(IPAddress.Loopback, 0));
        var app = builder.Build();
        app.UseWebSockets();
        FakeCoinGeckoStreamServer? instance = null;

        app.Map("/v1", async (HttpContext ctx) =>
        {
            if (!ctx.WebSockets.IsWebSocketRequest) { ctx.Response.StatusCode = 400; return; }
            var ws = await ctx.WebSockets.AcceptWebSocketAsync();
            instance!._currentSocket = ws;
            await instance.SendAsync("{\"type\":\"welcome\"}", ctx.RequestAborted);
            var buffer = new byte[16 * 1024];
            while (ws.State == WebSocketState.Open)
            {
                var sb = new StringBuilder();
                WebSocketReceiveResult r;
                do
                {
                    r = await ws.ReceiveAsync(buffer, ctx.RequestAborted);
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, r.Count));
                } while (!r.EndOfMessage);
                if (r.MessageType == WebSocketMessageType.Close) break;
                instance._Record(sb.ToString());
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
        lock (_lock) _receivedFrames.Add(frame);
    }

    public async Task PushAsync(string frame, CancellationToken ct = default)
    {
        if (_currentSocket is null || _currentSocket.State != WebSocketState.Open) return;
        var bytes = Encoding.UTF8.GetBytes(frame);
        await _currentSocket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
    }

    public async Task SendAsync(string frame, CancellationToken ct)
    {
        if (_currentSocket is null) return;
        var bytes = Encoding.UTF8.GetBytes(frame);
        await _currentSocket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
    }

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
```

- [ ] Commit after it compiles:

```bash
git -c commit.gpgsign=false commit -m "test(ws): add FakeCoinGeckoStreamServer (Kestrel-hosted) for integration tests" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 9 — Integration tests

Add at minimum these tests in `tests/CoinGecko.Api.WebSockets.Tests/StreamIntegrationTests.cs`:

1. **Connect_transitions_through_connecting_to_connected** — verify state machine and event fires.
2. **Disconnect_transitions_connected_to_disconnected**.
3. **Subscribe_coin_prices_receives_canned_push** — connect → subscribe → server pushes a canned CGSimplePrice frame → assertion fires on received tick.
4. **Subscribe_dex_trades_receives_canned_push** — G2 variant.
5. **Subscribe_dex_ohlcv_receives_canned_push** — G3 variant.
6. **Subscription_handle_dispose_sends_unset_tokens**.
7. **Subscription_cap_exceeded_throws**.
8. **Heartbeat_timeout_triggers_reconnect** — hold the server idle beyond `HeartbeatTimeout`, verify state transitions through `Reconnecting` then back to `Connected`.
9. **Max_reconnect_attempts_exhausted_transitions_to_faulted** — server refuses reconnect; verify terminal state.

Each test runs with a per-test `FakeCoinGeckoStreamServer`. Use `TestContext.Current.CancellationToken`. Give each test a short timeout (5–10 seconds).

Final Phase 9 commit:

```bash
git -c commit.gpgsign=false commit -m "test(ws): integration suite covering connect/subscribe/heartbeat/reconnect/cap" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 10 — Sample, README update, pack, tag

### Task 10.1: Stream console sample

`samples/CoinGecko.Api.Samples.StreamConsole/CoinGecko.Api.Samples.StreamConsole.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <RootNamespace>CoinGecko.Api.Samples.StreamConsole</RootNamespace>
    <IsPackable>false</IsPackable>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\CoinGecko.Api.WebSockets\CoinGecko.Api.WebSockets.csproj" />
  </ItemGroup>

</Project>
```

`samples/CoinGecko.Api.Samples.StreamConsole/Program.cs`:

```csharp
using CoinGecko.Api.WebSockets;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCoinGeckoStream(opts =>
{
    opts.ApiKey = Environment.GetEnvironmentVariable("COINGECKO_API_KEY");
});
using var sp = services.BuildServiceProvider();

var stream = sp.GetRequiredService<ICoinGeckoStream>();
stream.StateChanged += (_, e) => Console.WriteLine($"[state] {e.Previous} -> {e.Current}");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { cts.Cancel(); e.Cancel = true; };

await stream.ConnectAsync(cts.Token);

var sub = await stream.SubscribeCoinPricesAsync(
    ["bitcoin", "ethereum"], ["usd"],
    tick => Console.WriteLine($"{DateTimeOffset.Now:HH:mm:ss}  {tick.CoinId,-10}  ${tick.Price,12:N2}  ({tick.PricePercentChange24h,+6:N2}%)"),
    cts.Token);

try { await Task.Delay(Timeout.Infinite, cts.Token); } catch { }
await sub.DisposeAsync();
await stream.DisconnectAsync(CancellationToken.None);
```

Add to solution.

### Task 10.2: Update repo README

In `README.md`, the package matrix row for `.WebSockets` changes from `Planned` to `v0.1.0-preview`. Add a brief "Streaming quickstart" section referencing the sample.

### Task 10.3: Pack + tag

```bash
dotnet build CoinGecko.sln -c Release
dotnet test CoinGecko.sln -c Release
dotnet pack src/CoinGecko.Api.WebSockets/CoinGecko.Api.WebSockets.csproj -c Release -o artifacts/ws-preview
# inspect nupkg, confirm lib/net9 + lib/net8, README + icon, preview version suffix
rm -rf artifacts/ws-preview

git tag -a v0.1.0-preview-websockets -m "CoinGecko.Api.WebSockets 0.1.0-preview"
```

(The `.github/workflows/release.yml` already matches the prerelease tag pattern `v[0-9]+.[0-9]+.[0-9]+-api-*` which does NOT match this tag — the release workflow is currently REST-specific. A later plan extends it with a second pattern matching `v*-preview-websockets`; for now we publish manually via `dotnet nuget push` when the user approves.)

Commit + tag only; do NOT push.

```bash
git add samples/ README.md
git -c commit.gpgsign=false commit -m "docs: WebSockets sample + README update + v0.1.0-preview tag" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Appendix A — `JsonSerializerOptions` caveat

The `ActionCableIdentifierConverter` uses a small amount of reflection-style serialization of `ActionCableIdentifier`, but because `ActionCableIdentifier` has a **fixed, simple shape** (single `string Channel` property) the compiler's trimmer preserves it — no runtime reflection is invoked in the hot path. AOT validation (Blazor sample publish) should succeed with zero IL link warnings.

## Appendix B — Commit style

Same as Plan 1:
- `feat(ws): …`
- `fix(ws): …`
- `test(ws): …`
- `docs: …`
- `build: …`
- `chore(ws): …`

Subject ≤72 chars; no AI co-author trailers.

## Appendix C — Out of scope for this plan

- Public API analyzer baseline for the WebSockets package — deferred to 0.2.0 (the 0.x preview explicitly allows API churn).
- Compare-live-to-canned regression harness — useful before 1.0 but overkill for preview.
- Release workflow update to match the `-preview-websockets` tag — a small `.github/workflows/release.yml` patch; track as a follow-up commit after v0.1.0 of REST ships.
- Publishing to NuGet — manual `dotnet nuget push` until the workflow pattern is extended.
