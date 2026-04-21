# CoinGecko.Api.AiAgentHub.Mcp — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development.

**Goal:** Ship `CoinGecko.Api.AiAgentHub.Mcp` v0.1.0-preview — a thin .NET client for CoinGecko's hosted MCP (Model Context Protocol) endpoints. Consumers get `IReadOnlyList<AIFunction>` (identical to the shape from Plan 3's `CoinGecko.Api.AiAgentHub`) that they can feed into any `IChatClient`. Swapping between "REST-backed tools" (Plan 3) and "MCP-fetched tools" (Plan 4) is a one-line change.

**Architecture:** Sibling package (does NOT depend on `CoinGecko.Api`). Uses the official `ModelContextProtocol` .NET SDK to connect to CoinGecko's hosted MCP server. Resolves the endpoint from the configured plan (Demo → `mcp.api.coingecko.com/mcp`, any paid plan → `mcp.pro-api.coingecko.com/mcp`). Authenticates with `Authorization: Bearer {apiKey}`. Converts MCP tool definitions to `AIFunction` objects via `Microsoft.Extensions.AI`'s MCP↔AIFunction adapter.

**Tech Stack:** `ModelContextProtocol.Client` · `Microsoft.Extensions.AI.Abstractions` · xUnit v3 + Shouldly + NSubstitute.

**Spec reference:** [`docs/superpowers/specs/2026-04-21-coingecko-wrapper-design.md`](../specs/2026-04-21-coingecko-wrapper-design.md) §7.2.
**Research:** [`docs/coingecko-api-research.md`](../../coingecko-api-research.md) §3 (MCP endpoints + transport).

---

## Global conventions (same as prior plans)

- Working dir `D:/repos/CoinGecko/`, branch `main`.
- Every commit: `git -c commit.gpgsign=false commit -m "..." --author="msanlisavas <muratsanlisavas@gmail.com>"`. No `Co-Authored-By`.
- XML `/// <summary>` on public/protected; internal exempt. CS1591 fatal in Release.
- xUnit v3: `TestContext.Current.CancellationToken`.
- Release build 0 warnings 0 errors.
- `CoinGeckoPlan` enum is defined in `CoinGecko.Api`. This package is a sibling and deliberately **redefines a minimal `CoinGeckoPlan` enum of its own** to avoid a hard dependency on the REST client. That duplication is justified — see Appendix A.

## Phase index

| # | Phase | Produces |
|---|---|---|
| 0 | Scaffold | `CoinGecko.Api.AiAgentHub.Mcp` project + test project. |
| 1 | Options + enums | `CoinGeckoMcpOptions`, `McpTransport` enum, plan-redeclaration or shared. |
| 2 | `CoinGeckoMcp.CreateClientAsync` | Low-level: configures transport + auth header + base URL, returns `IMcpClient`. |
| 3 | `CoinGeckoMcp.ConnectAsync` | High-level: calls `CreateClientAsync`, discovers tools, returns `IReadOnlyList<AIFunction>`. |
| 4 | Tests | Unit tests for URL resolution, auth header, transport selection. NO live MCP integration (deferred to follow-up). |
| 5 | Sample + README + pack + tag v0.1.0-preview-mcp |

Total: ~6 commits (one per phase + scaffold split).

---

## Phase 0 — Scaffold

### Task 0.1: Create `CoinGecko.Api.AiAgentHub.Mcp` project

**Files:**
- Create: `src/CoinGecko.Api.AiAgentHub.Mcp/CoinGecko.Api.AiAgentHub.Mcp.csproj`
- Create: `src/CoinGecko.Api.AiAgentHub.Mcp/README.md`

- [ ] **Step 1: Scaffold**

```bash
dotnet new classlib -n CoinGecko.Api.AiAgentHub.Mcp -o src/CoinGecko.Api.AiAgentHub.Mcp -f net9.0 --force
rm src/CoinGecko.Api.AiAgentHub.Mcp/Class1.cs
```

- [ ] **Step 2: Overwrite csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net9.0;net8.0</TargetFrameworks>
    <RootNamespace>CoinGecko.Api.AiAgentHub.Mcp</RootNamespace>
    <AssemblyName>CoinGecko.Api.AiAgentHub.Mcp</AssemblyName>
    <IsPackable>true</IsPackable>
    <PackageId>CoinGecko.Api.AiAgentHub.Mcp</PackageId>
    <Description>MCP (Model Context Protocol) client for CoinGecko's hosted MCP servers (mcp.api.coingecko.com + mcp.pro-api.coingecko.com). Returns IReadOnlyList&lt;AIFunction&gt; for drop-in use with any Microsoft.Extensions.AI IChatClient. Sibling of CoinGecko.Api.AiAgentHub — produces the same tool-list shape but fetched from CoinGecko's hosted MCP.</Description>
    <PackageTags>coingecko crypto cryptocurrency mcp model-context-protocol ai agent llm tools microsoft-extensions-ai</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ModelContextProtocol" />
    <PackageReference Include="Microsoft.Extensions.AI.Abstractions" />
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

> **Why no `ProjectReference` to `CoinGecko.Api`:** by design. The MCP package fetches tool definitions from CoinGecko's hosted server — it never calls the REST API directly. Users who want both packages just install both. Keeping them independent lets consumers use MCP without dragging the full REST client graph.

- [ ] **Step 3: Add `ModelContextProtocol` pin to `Directory.Packages.props`**

Under the Runtime group, add:

```xml
<PackageVersion Include="ModelContextProtocol" Version="0.3.0-preview.1" />
```

**Version-check note:** `ModelContextProtocol` is in active development. Before committing, run `dotnet list package ModelContextProtocol --source https://api.nuget.org/v3/index.json` or browse nuget.org to find the latest stable-ish version. The SDK uses preview semantics (`0.x-preview.N`). Pick the latest `-preview.*` available at execution time. Report the version pinned.

- [ ] **Step 4: Pack-time README**

`src/CoinGecko.Api.AiAgentHub.Mcp/README.md`:

```markdown
# CoinGecko.Api.AiAgentHub.Mcp

MCP (Model Context Protocol) client for CoinGecko's hosted MCP servers. Returns `IReadOnlyList<AIFunction>` you can plug into any `Microsoft.Extensions.AI` `IChatClient`.

## Install

```powershell
dotnet add package CoinGecko.Api.AiAgentHub.Mcp
```

## Quickstart

```csharp
using CoinGecko.Api.AiAgentHub.Mcp;
using Microsoft.Extensions.AI;

var tools = await CoinGeckoMcp.ConnectAsync(
    apiKey: Environment.GetEnvironmentVariable("COINGECKO_API_KEY")!,
    plan: CoinGeckoPlan.Pro);

IChatClient chat = /* OpenAI / Anthropic / Azure / Ollama / Gemini — any IChatClient */;
var response = await chat.GetResponseAsync(
    "What's BTC at right now?",
    new ChatOptions { Tools = tools.Cast<AITool>().ToArray() });
```

## Relationship to `CoinGecko.Api.AiAgentHub`

Both packages return `IReadOnlyList<AIFunction>`.

- **`CoinGecko.Api.AiAgentHub`** — tools are thin wrappers over the REST client you control. Zero MCP dependency; deterministic shape.
- **`CoinGecko.Api.AiAgentHub.Mcp`** — tools are fetched from CoinGecko's hosted MCP server. Definitions evolve upstream without a client-side package bump.

Swap between them by changing one line: `CoinGeckoAiTools.Create(gecko)` ↔ `await CoinGeckoMcp.ConnectAsync(apiKey, plan)`.

## Preview status

MCP is a young protocol and the CoinGecko MCP server + the `ModelContextProtocol` .NET SDK are both pre-1.0. This package ships in 0.x-preview and may break with SDK updates.

## License

MIT © msanlisavas
```

- [ ] **Step 5: Add to solution and build**

```bash
dotnet sln CoinGecko.sln add src/CoinGecko.Api.AiAgentHub.Mcp/CoinGecko.Api.AiAgentHub.Mcp.csproj
dotnet build src/CoinGecko.Api.AiAgentHub.Mcp -c Release
```

Expected: 0 warnings, 0 errors.

- [ ] **Step 6: Commit**

```bash
git add src/CoinGecko.Api.AiAgentHub.Mcp/ CoinGecko.sln Directory.Packages.props
git -c commit.gpgsign=false commit -m "feat(mcp): scaffold CoinGecko.Api.AiAgentHub.Mcp project" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 0.2: Test project scaffold

Mirror `CoinGecko.Api.AiAgentHub.Tests` with:
- `Microsoft.NET.Sdk` (not .Web).
- ProjectReference to `CoinGecko.Api.AiAgentHub.Mcp`.
- Same xUnit v3 / Shouldly / NSubstitute / Microsoft.NET.Test.Sdk / xunit.runner.visualstudio / coverlet deps.
- `<GenerateDocumentationFile>false</GenerateDocumentationFile>`.

`tests/CoinGecko.Api.AiAgentHub.Mcp.Tests/GlobalUsings.cs` with System / collections / linq / threading / Shouldly / Xunit.

Add to solution. Build Debug.

Commit: `test(mcp): scaffold CoinGecko.Api.AiAgentHub.Mcp.Tests project`

---

## Phase 1 — Options + enums

### Task 1.1: `CoinGeckoPlan` (local redeclaration), `McpTransport`, `CoinGeckoMcpOptions`

**Files:**
- Create: `src/CoinGecko.Api.AiAgentHub.Mcp/CoinGeckoPlan.cs`
- Create: `src/CoinGecko.Api.AiAgentHub.Mcp/McpTransport.cs`
- Create: `src/CoinGecko.Api.AiAgentHub.Mcp/CoinGeckoMcpOptions.cs`
- Create: `tests/CoinGecko.Api.AiAgentHub.Mcp.Tests/OptionsTests.cs`

- [ ] **Step 1: Failing tests**

```csharp
using CoinGecko.Api.AiAgentHub.Mcp;

namespace CoinGecko.Api.AiAgentHub.Mcp.Tests;

public class OptionsTests
{
    [Fact]
    public void Plan_is_ordered_ascending()
    {
        ((int)CoinGeckoPlan.Demo).ShouldBe(0);
        ((int)CoinGeckoPlan.Basic).ShouldBeGreaterThan((int)CoinGeckoPlan.Demo);
        ((int)CoinGeckoPlan.Enterprise).ShouldBeGreaterThan((int)CoinGeckoPlan.Pro);
    }

    [Fact]
    public void Transport_defaults_to_streamable_http()
    {
        default(McpTransport).ShouldBe(McpTransport.StreamableHttp);
        Enum.GetValues<McpTransport>().ShouldContain(McpTransport.Sse);
    }

    [Fact]
    public void Options_defaults()
    {
        var o = new CoinGeckoMcpOptions();
        o.BaseAddress.ShouldBeNull();
        o.Transport.ShouldBe(McpTransport.StreamableHttp);
        o.CallTimeout.ShouldBe(TimeSpan.FromSeconds(60));
    }
}
```

- [ ] **Step 2: Run — expect compile fail.**

- [ ] **Step 3: Implement**

`src/CoinGecko.Api.AiAgentHub.Mcp/CoinGeckoPlan.cs`:

```csharp
namespace CoinGecko.Api.AiAgentHub.Mcp;

/// <summary>
/// CoinGecko plan tier — mirror of the enum in <c>CoinGecko.Api</c>. Redeclared locally so this
/// package stays independent of the REST client. See the package README for rationale.
/// </summary>
public enum CoinGeckoPlan
{
    /// <summary>Free / Demo tier.</summary>
    Demo = 0,
    /// <summary>Paid Basic tier.</summary>
    Basic = 1,
    /// <summary>Paid Analyst tier.</summary>
    Analyst = 2,
    /// <summary>Paid Lite tier.</summary>
    Lite = 3,
    /// <summary>Paid Pro tier.</summary>
    Pro = 4,
    /// <summary>Paid Pro+ tier.</summary>
    ProPlus = 5,
    /// <summary>Enterprise.</summary>
    Enterprise = 6,
}
```

`src/CoinGecko.Api.AiAgentHub.Mcp/McpTransport.cs`:

```csharp
namespace CoinGecko.Api.AiAgentHub.Mcp;

/// <summary>MCP transport options exposed by the hosted CoinGecko MCP server.</summary>
public enum McpTransport
{
    /// <summary>Streamable HTTP (default, recommended for most callers).</summary>
    StreamableHttp = 0,

    /// <summary>Server-sent events. Use where Streamable HTTP is blocked by an intermediary.</summary>
    Sse = 1,
}
```

`src/CoinGecko.Api.AiAgentHub.Mcp/CoinGeckoMcpOptions.cs`:

```csharp
namespace CoinGecko.Api.AiAgentHub.Mcp;

/// <summary>Configuration for <see cref="CoinGeckoMcp"/>.</summary>
public sealed class CoinGeckoMcpOptions
{
    /// <summary>Override the MCP endpoint. Leave null to use the plan-default (<c>mcp.api.coingecko.com/mcp</c> or <c>mcp.pro-api.coingecko.com/mcp</c>).</summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>Transport mechanism.</summary>
    public McpTransport Transport { get; set; } = McpTransport.StreamableHttp;

    /// <summary>Per-call timeout. Applied to tool invocations.</summary>
    public TimeSpan CallTimeout { get; set; } = TimeSpan.FromSeconds(60);
}
```

- [ ] **Step 4: Pass. Commit.**

```bash
git -c commit.gpgsign=false commit -m "feat(mcp): add CoinGeckoPlan (local), McpTransport, CoinGeckoMcpOptions" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 2 — `CoinGeckoMcp.CreateClientAsync`

### Task 2.1: Static factory for low-level `IMcpClient`

**Files:**
- Create: `src/CoinGecko.Api.AiAgentHub.Mcp/CoinGeckoMcp.cs` (initial skeleton + `CreateClientAsync`; `ConnectAsync` added in Phase 3)

- [ ] **Implementation**

The exact `ModelContextProtocol.Client` public API depends on the installed SDK version. The typical shape (verify against installed SDK XML docs at execution time):

```csharp
using System.Diagnostics.CodeAnalysis;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

namespace CoinGecko.Api.AiAgentHub.Mcp;

/// <summary>Entry points for connecting to CoinGecko's hosted MCP server.</summary>
public static class CoinGeckoMcp
{
    private const string DemoHost = "https://mcp.api.coingecko.com";
    private const string ProHost  = "https://mcp.pro-api.coingecko.com";

    /// <summary>
    /// Create a low-level <see cref="IMcpClient"/> connected to CoinGecko's hosted MCP server.
    /// Use when you want fine-grained control over the MCP session (e.g., progress notifications,
    /// custom handlers). For the common case of "give me tools," use <c>ConnectAsync</c> instead.
    /// </summary>
    /// <param name="apiKey">CoinGecko API key — sent as <c>Authorization: Bearer {apiKey}</c>.</param>
    /// <param name="plan">Plan tier (picks between the demo and pro MCP hosts).</param>
    /// <param name="options">Transport + timeout overrides.</param>
    /// <param name="ct">Cancellation.</param>
    public static async Task<IMcpClient> CreateClientAsync(
        string apiKey,
        CoinGeckoPlan plan = CoinGeckoPlan.Demo,
        CoinGeckoMcpOptions? options = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);
        var opts = options ?? new CoinGeckoMcpOptions();

        var baseUri = ResolveBaseUri(opts.BaseAddress, plan, opts.Transport);

        var transportOptions = new SseClientTransportOptions
        {
            Endpoint = baseUri,
            Name = "CoinGecko MCP",
            TransportMode = opts.Transport switch
            {
                McpTransport.Sse => HttpTransportMode.Sse,
                _                => HttpTransportMode.StreamableHttp,
            },
            AdditionalHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {apiKey}",
            },
        };

        var transport = new SseClientTransport(transportOptions);
        return await McpClientFactory.CreateAsync(transport, cancellationToken: ct).ConfigureAwait(false);
    }

    internal static Uri ResolveBaseUri(Uri? userBase, CoinGeckoPlan plan, McpTransport transport)
    {
        if (userBase is not null)
        {
            return userBase;
        }
        var host = plan == CoinGeckoPlan.Demo ? DemoHost : ProHost;
        var path = transport == McpTransport.Sse ? "/sse" : "/mcp";
        return new Uri(host + path);
    }
}
```

> **Important:** The `ModelContextProtocol` SDK's exact type names (`SseClientTransport`, `SseClientTransportOptions`, `HttpTransportMode`, `McpClientFactory`) are subject to SDK version. Before implementing, **run `find ~/.nuget/packages/modelcontextprotocol* -name '*.xml'` to locate the installed package's XML docs, or inspect `/ref/net*/*.dll`** to discover the actual public surface. Adapt the code to the real API.

### Tests for `ResolveBaseUri` (Phase 2 has no integration test — all unit)

`tests/CoinGecko.Api.AiAgentHub.Mcp.Tests/ResolveBaseUriTests.cs`:

```csharp
using CoinGecko.Api.AiAgentHub.Mcp;

namespace CoinGecko.Api.AiAgentHub.Mcp.Tests;

public class ResolveBaseUriTests
{
    [Fact]
    public void Demo_plan_streamable_routes_to_mcp_api()
    {
        var uri = CoinGeckoMcp.ResolveBaseUri(null, CoinGeckoPlan.Demo, McpTransport.StreamableHttp);
        uri.ToString().ShouldBe("https://mcp.api.coingecko.com/mcp");
    }

    [Fact]
    public void Paid_plan_streamable_routes_to_mcp_pro_api()
    {
        var uri = CoinGeckoMcp.ResolveBaseUri(null, CoinGeckoPlan.Pro, McpTransport.StreamableHttp);
        uri.ToString().ShouldBe("https://mcp.pro-api.coingecko.com/mcp");
    }

    [Fact]
    public void Sse_transport_uses_sse_path()
    {
        var uri = CoinGeckoMcp.ResolveBaseUri(null, CoinGeckoPlan.Demo, McpTransport.Sse);
        uri.ToString().ShouldBe("https://mcp.api.coingecko.com/sse");
    }

    [Fact]
    public void User_override_wins()
    {
        var uri = CoinGeckoMcp.ResolveBaseUri(new Uri("https://proxy.example/mcp"), CoinGeckoPlan.Enterprise, McpTransport.Sse);
        uri.ToString().ShouldBe("https://proxy.example/mcp");
    }
}
```

`ResolveBaseUri` is `internal` — add `InternalsVisibleTo` from the Mcp csproj to the Mcp.Tests assembly:

```xml
<ItemGroup>
  <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleTo">
    <_Parameter1>CoinGecko.Api.AiAgentHub.Mcp.Tests</_Parameter1>
  </AssemblyAttribute>
</ItemGroup>
```

Release build green. Commit:

```bash
git -c commit.gpgsign=false commit -m "feat(mcp): add CoinGeckoMcp.CreateClientAsync + URL resolution" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 3 — `CoinGeckoMcp.ConnectAsync`

### Task 3.1: High-level entry point returning `IReadOnlyList<AIFunction>`

Extend `CoinGeckoMcp`:

```csharp
using Microsoft.Extensions.AI;

public static partial class CoinGeckoMcp
{
    /// <summary>
    /// Connect to CoinGecko's hosted MCP server, list its tools, and wrap them as
    /// <see cref="AIFunction"/>s for use with any <c>Microsoft.Extensions.AI</c> <c>IChatClient</c>.
    /// </summary>
    /// <param name="apiKey">API key.</param>
    /// <param name="plan">Plan tier.</param>
    /// <param name="options">Transport + timeout overrides.</param>
    /// <param name="ct">Cancellation.</param>
    public static async Task<IReadOnlyList<AIFunction>> ConnectAsync(
        string apiKey,
        CoinGeckoPlan plan = CoinGeckoPlan.Demo,
        CoinGeckoMcpOptions? options = null,
        CancellationToken ct = default)
    {
        var client = await CreateClientAsync(apiKey, plan, options, ct).ConfigureAwait(false);
        var tools = await client.ListToolsAsync(cancellationToken: ct).ConfigureAwait(false);
        // Each McpClientTool from the MCP SDK implements AIFunction via MEAI's adapter.
        return tools.Cast<AIFunction>().ToArray();
    }
}
```

**Note on SDK shape:**
- `IMcpClient.ListToolsAsync` may return `IList<McpClientTool>`, `IReadOnlyList<McpClientTool>`, or similar. Adjust the cast and return accordingly.
- `McpClientTool` typically extends / implements `AIFunction`. If it doesn't in the pinned SDK version, use `.AsAIFunction()` extension or construct `AIFunction` via MEAI's adapter. Verify against the real API.

Commit:

```bash
git -c commit.gpgsign=false commit -m "feat(mcp): add CoinGeckoMcp.ConnectAsync returning AIFunction tools" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 4 — Tests (unit only; live-MCP integration deferred)

The `ResolveBaseUri` tests from Phase 2 cover the pure logic. Add one more registration-shape test to confirm the public API exists and compiles as expected:

`tests/CoinGecko.Api.AiAgentHub.Mcp.Tests/PublicApiShapeTests.cs`:

```csharp
using System.Reflection;
using CoinGecko.Api.AiAgentHub.Mcp;

namespace CoinGecko.Api.AiAgentHub.Mcp.Tests;

public class PublicApiShapeTests
{
    [Fact]
    public void CoinGeckoMcp_exposes_ConnectAsync_and_CreateClientAsync()
    {
        var t = typeof(CoinGeckoMcp);
        t.GetMethod("ConnectAsync", BindingFlags.Public | BindingFlags.Static).ShouldNotBeNull();
        t.GetMethod("CreateClientAsync", BindingFlags.Public | BindingFlags.Static).ShouldNotBeNull();
    }

    [Fact]
    public void CoinGeckoMcpOptions_exposes_BaseAddress_Transport_CallTimeout()
    {
        var t = typeof(CoinGeckoMcpOptions);
        t.GetProperty("BaseAddress").ShouldNotBeNull();
        t.GetProperty("Transport").ShouldNotBeNull();
        t.GetProperty("CallTimeout").ShouldNotBeNull();
    }
}
```

Commit (bundled with Phase 3's test-adds if small; otherwise separate):

```bash
git -c commit.gpgsign=false commit -m "test(mcp): public API shape + URL resolution unit tests" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

> **Why no live-MCP integration tests:** running a real MCP server requires either (a) an MCP server mock (adds heavy test infrastructure — MCP server implementation complexity), or (b) live network to CoinGecko (flaky + key-gated). For v0.1-preview we verify URL + header logic via unit tests and defer protocol-level integration tests to a follow-up that builds a small in-process MCP server fixture.

---

## Phase 5 — Sample + README + pack + tag

### Task 5.1: Sample

`samples/CoinGecko.Api.Samples.McpAgent/CoinGecko.Api.Samples.McpAgent.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <RootNamespace>CoinGecko.Api.Samples.McpAgent</RootNamespace>
    <IsPackable>false</IsPackable>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\CoinGecko.Api.AiAgentHub.Mcp\CoinGecko.Api.AiAgentHub.Mcp.csproj" />
  </ItemGroup>

</Project>
```

`Program.cs`:

```csharp
using CoinGecko.Api.AiAgentHub.Mcp;

var apiKey = Environment.GetEnvironmentVariable("COINGECKO_API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Console.Error.WriteLine("Set COINGECKO_API_KEY first.");
    return 1;
}

try
{
    var tools = await CoinGeckoMcp.ConnectAsync(apiKey, CoinGeckoPlan.Demo);
    Console.WriteLine($"Connected. Fetched {tools.Count} MCP tools from CoinGecko:");
    foreach (var t in tools)
    {
        Console.WriteLine($"  • {t.Name}");
        Console.WriteLine($"    {t.Description}");
        Console.WriteLine();
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"MCP connect failed: {ex.Message}");
    return 2;
}

return 0;
```

Add to solution. Verify build.

### Task 5.2: README update

Update repo-root `README.md`:
- Packages table row for `.AiAgentHub.Mcp`: status `Planned` → `v0.1.0-preview`.
- Add a new section `## AI tools via MCP (preview)` with the quickstart.

### Task 5.3: Pack smoke

```bash
dotnet pack src/CoinGecko.Api.AiAgentHub.Mcp/CoinGecko.Api.AiAgentHub.Mcp.csproj -c Release -p:Version=0.1.0-preview -p:MinVerSkip=true -o artifacts/mcp-preview
```

Verify nupkg contents: `lib/net9.0/` + `lib/net8.0/` DLLs, nuspec metadata, dependencies on `ModelContextProtocol` and `Microsoft.Extensions.AI.Abstractions`.

Clean: `rm -rf artifacts/mcp-preview`.

### Task 5.4: Commit + tag

```bash
git add samples/CoinGecko.Api.Samples.McpAgent/ CoinGecko.sln README.md
git -c commit.gpgsign=false commit -m "docs: MCP sample + README update + v0.1.0-preview" --author="msanlisavas <muratsanlisavas@gmail.com>"

git tag -a v0.1.0-preview-mcp -m "CoinGecko.Api.AiAgentHub.Mcp 0.1.0-preview — hosted MCP client returning AIFunction tools"
git tag --list
```

DO NOT push.

### Final verification

```bash
dotnet build CoinGecko.sln -c Release
dotnet test CoinGecko.sln -c Release
git log --oneline | head -20
git tag --list
```

Expected:
- Build: 0 warnings, 0 errors
- Tests: 195 (Plan 3) + ~6 new MCP = ~201 per TFM
- Tags: `v0.1.0-api`, `v0.1.0-preview-websockets`, `v0.1.0-preview-aiagenthub`, `v0.1.0-preview-mcp`

---

## Appendix A — Why `CoinGeckoPlan` is redeclared (not shared)

The design spec §2 specifies `CoinGecko.Api.AiAgentHub.Mcp` as a sibling package with NO dependency on `CoinGecko.Api`. Redeclaring a 7-tier enum is a tiny duplication for a large decoupling benefit:

- Consumers who install **only** the MCP package don't drag the REST client's dependency graph.
- The MCP package can version independently from the REST client.
- Plan-tier evolution (if CoinGecko adds a tier) is handled independently in each package.

The two enums have the same underlying integer values for the same names, so casting between them is safe: `(CoinGecko.Api.CoinGeckoPlan)(int)mcpPlan`.

## Appendix B — Commit style

`feat(mcp): …`, `fix(mcp): …`, `test(mcp): …`, `docs: …`. Subject ≤72 chars. No AI co-author trailers.

## Appendix C — Deferred to v0.2

- In-process fake MCP server fixture for integration tests that validate the full handshake + tool-invocation path.
- SSE-transport smoke test against the real endpoint (opt-in, gated on `COINGECKO_API_KEY`).
- `ResourcesAsync` / `PromptsAsync` pass-through for MCP resources + prompts (not just tools).
- Tool-filter / tool-allow-list options (mirroring `CoinGeckoAiToolsOptions.ToolFilter`).
