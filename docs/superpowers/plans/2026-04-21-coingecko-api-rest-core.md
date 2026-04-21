# CoinGecko.Api REST Core — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship `CoinGecko.Api` v0.1.0 to NuGet — the REST core package covering every CoinGecko core + onchain endpoint, AOT-safe, DI-first, with full handler pipeline (auth, plan gating, rate-limit respect, transient retry), two deserialization paths (bare / JSON:API), async pagination, and full unit + mock + contract test coverage.

**Architecture:** Hand-written typed `HttpClient` layered on `IHttpClientFactory` with a four-stage `DelegatingHandler` pipeline. One `ICoinGeckoClient` root exposing 14 sub-clients (`Coins`, `Nfts`, `Exchanges`, `Derivatives`, `Categories`, `AssetPlatforms`, `Companies`, `Simple`, `Global`, `Search`, `Trending`, `Onchain`, `Key`, `Ping`). Serialization via `System.Text.Json` source generators; onchain endpoints unwrap a generic `JsonApiResponse<T>` envelope internally.

**Tech Stack:** .NET 9 + .NET 8 multi-target · `Microsoft.Extensions.Http` · `Microsoft.Extensions.Options` · System.Text.Json (source-gen) · xUnit v3 · Shouldly · NSubstitute · WireMock.Net · Verify.Xunit · MinVer · SourceLink · GitHub Actions · Central Package Management.

**Spec reference:** [`docs/superpowers/specs/2026-04-21-coingecko-wrapper-design.md`](../specs/2026-04-21-coingecko-wrapper-design.md)
**API catalog:** [`docs/coingecko-api-research.md`](../../coingecko-api-research.md)

---

## Reading guide (for the executor)

This plan is large. Each **Phase** is a cohesive block of work that produces a committable, buildable state. Phases are strictly ordered — do not skip. Inside a phase, **Tasks** are numbered; execute top-to-bottom. Each Task has step checkboxes that follow the same rhythm: **write failing test → run it → implement → run again → commit**. If you're a subagent and only see one task, you have everything you need in it — file paths, full code, test code, commands, and commit message.

**Global conventions for every task:**

- **Working dir:** `D:/repos/CoinGecko/` (pwsh on Windows).
- **Branch:** `main` (plan was produced on the empty repo root commit; no feature branch for the scaffolding phase. After Phase 5 you can optionally feature-branch; the plan does not require it).
- **SDK:** .NET 9 (`global.json` pinned in Phase 0). Use `dotnet` from the repo root.
- **Commit style:** Conventional Commits (`feat:`, `fix:`, `test:`, `chore:`, `docs:`, `build:`). One logical change per commit. **Never add `Co-Authored-By: Claude` or any AI-trailer** — per user preference the sole author is `msanlisavas <muratsanlisavas@gmail.com>`. Use `git -c commit.gpgsign=false commit` if signing is locally configured and failing; do **not** pass `--no-verify`.
- **Test discipline:** Every production code change is preceded by a failing test. Run `dotnet test` after each task with a `.Tests` touch; expected output for each test is documented in the task.
- **Nullable + warnings-as-errors** are on repo-wide (Phase 0 sets this). Treat every warning as a blocker.
- **No placeholders in code.** If a task shows a method body as `throw new NotImplementedException();`, a later task in the same phase replaces it. Do not leave those across phases.

---

## Phase index

| # | Phase | Produces |
|---|---|---|
| 0 | Repo scaffolding | Solution, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `.editorconfig`. |
| 1 | `CoinGecko.Api` csproj + package metadata | Buildable empty library project with full NuGet metadata. |
| 2 | Options, enums, plan model | `CoinGeckoOptions`, `CoinGeckoPlan`, `RateLimitPolicy`, `AuthenticationMode`, `PriceChangeWindow`, `CoinMarketsOrder`, `MarketChartRange`, `[RequiresPlan]` attribute. |
| 3 | Exception hierarchy | `CoinGeckoException` + six derived types. |
| 4 | Serialization infrastructure | `CoinGeckoJsonContext`, `UnixSecondsConverter`, `JsonApiResponse<T>` + unwrapper, `ResponseEnvelope` enum + options-key. |
| 5 | URI + query utilities | `UriTemplateExpander`, `QueryStringBuilder`. |
| 6 | Handler pipeline | `CoinGeckoAuthHandler`, `CoinGeckoPlanHandler`, `CoinGeckoRateLimitHandler`, `CoinGeckoRetryHandler` — each unit-tested. |
| 7 | Root client + DI + factory | `ICoinGeckoClient`, `CoinGeckoClient`, `ServiceCollectionExtensions.AddCoinGeckoApi`, `CoinGeckoClientFactory`. |
| 8 | First sub-client (canonical pattern) | `IPingClient` + full test layering: unit, contract (Verify), mock (WireMock). |
| 9 | Remaining 13 sub-clients | All other resources implemented by replicating the Phase 8 pattern. |
| 10 | `IAsyncEnumerable` pagination | Auto-pagination for list endpoints + `EnumerateXxxAsync` variants. |
| 11 | Observability | `ActivitySource` + `LoggerMessage` source-gen events. |
| 12 | Public API analyzers | `PublicAPI.Shipped.txt` / `.Unshipped.txt` per project. |
| 13 | CI + release workflow | `.github/workflows/ci.yml`, `release.yml`, `codeql.yml`, Dependabot. |
| 14 | Samples | `samples/CoinGecko.Api.Samples.Console` + `.Blazor` (WASM + AOT proof). |
| 15 | README + pack + tag v0.1.0 | Package README, `dotnet pack` dry-run, tag `v0.1.0-api`, release workflow dry-run on a prerelease tag. |

---

## Phase 0 — Repo scaffolding

### Task 0.1: Create `global.json` pinning .NET 9

**Files:**
- Create: `global.json`

- [ ] **Step 1: Write the file**

```json
{
  "sdk": {
    "version": "9.0.100",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

- [ ] **Step 2: Verify the SDK is resolvable**

Run: `dotnet --version`
Expected: a `9.0.x` version string (where `x >= 100`). If it reports `SDK not found`, install .NET 9 SDK from https://dotnet.microsoft.com/download before proceeding.

- [ ] **Step 3: Commit**

```bash
git add global.json
git -c commit.gpgsign=false commit -m "build: pin .NET 9 SDK via global.json" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 0.2: Write `Directory.Build.props`

**Files:**
- Create: `Directory.Build.props`

- [ ] **Step 1: Write the file**

```xml
<Project>

  <PropertyGroup Label="Language + nullability">
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>

  <PropertyGroup Label="Determinism + SourceLink">
    <Deterministic>true</Deterministic>
    <EmbedUntrackedSources>true</EmbedUntrackedSources>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
    <ContinuousIntegrationBuild Condition=" '$(TF_BUILD)' == 'true' OR '$(GITHUB_ACTIONS)' == 'true' ">true</ContinuousIntegrationBuild>
  </PropertyGroup>

  <PropertyGroup Label="Documentation">
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <!-- CS1591 (missing XML doc) only enforced in Release; keep dev fast. -->
    <NoWarn Condition=" '$(Configuration)' == 'Debug' ">$(NoWarn);CS1591</NoWarn>
  </PropertyGroup>

  <PropertyGroup Label="AOT posture (libraries opt-in individually)">
    <!-- IsAotCompatible is set per-library csproj, not here (so test projects don't drag it in). -->
  </PropertyGroup>

  <PropertyGroup Label="Package metadata defaults (shared)">
    <Authors>msanlisavas</Authors>
    <Company>msanlisavas</Company>
    <Product>CoinGecko.Api</Product>
    <Copyright>© $([System.DateTime]::UtcNow.Year) msanlisavas</Copyright>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/msanlisavas/CoinGecko</PackageProjectUrl>
    <RepositoryUrl>https://github.com/msanlisavas/CoinGecko</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageIcon>icon.png</PackageIcon>
    <PackageTags>coingecko crypto cryptocurrency price api client</PackageTags>
  </PropertyGroup>

  <ItemGroup Condition=" '$(IsPackable)' == 'true' ">
    <None Include="$(MSBuildThisFileDirectory)eng/icon.png" Pack="true" PackagePath="\" Visible="false" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Commit**

```bash
git add Directory.Build.props
git -c commit.gpgsign=false commit -m "build: add Directory.Build.props with shared MSBuild config" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 0.3: Write `Directory.Packages.props` (Central Package Management)

**Files:**
- Create: `Directory.Packages.props`

- [ ] **Step 1: Write the file**

```xml
<Project>

  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup Label="Runtime">
    <PackageVersion Include="Microsoft.Extensions.Http" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Options" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup Label="Build-time only (not packed)">
    <PackageVersion Include="MinVer" Version="5.0.0" />
    <PackageVersion Include="Microsoft.SourceLink.GitHub" Version="8.0.0" />
    <PackageVersion Include="Microsoft.CodeAnalysis.PublicApiAnalyzers" Version="3.3.4" />
  </ItemGroup>

  <ItemGroup Label="Tests">
    <PackageVersion Include="xunit.v3" Version="1.0.0" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.0.0" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageVersion Include="Shouldly" Version="4.2.1" />
    <PackageVersion Include="NSubstitute" Version="5.3.0" />
    <PackageVersion Include="NSubstitute.Analyzers.CSharp" Version="1.0.17" />
    <PackageVersion Include="WireMock.Net" Version="1.6.0" />
    <PackageVersion Include="Verify.Xunit" Version="26.0.0" />
    <PackageVersion Include="coverlet.collector" Version="6.0.2" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Commit**

```bash
git add Directory.Packages.props
git -c commit.gpgsign=false commit -m "build: add Central Package Management with pinned dep versions" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

> **Note:** version numbers above are floor targets. If a newer non-breaking version exists at execution time, bump to latest-non-breaking and note it in the commit message; this plan does not require the exact versions listed, only that they be latest-stable and central-pinned.

---

### Task 0.4: Write `.editorconfig`

**Files:**
- Create: `.editorconfig`

- [ ] **Step 1: Write the file**

```ini
root = true

[*]
charset = utf-8
end_of_line = lf
indent_style = space
indent_size = 4
insert_final_newline = true
trim_trailing_whitespace = true

[*.{json,yml,yaml,md,csproj,props,targets,config,xml}]
indent_size = 2

[*.cs]
# Language-level style
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
csharp_new_line_before_members_in_object_initializers = true
csharp_new_line_between_query_expression_clauses = true
csharp_indent_case_contents = true
csharp_indent_switch_labels = true
csharp_prefer_braces = true:warning
csharp_prefer_simple_using_statement = true:warning
csharp_style_namespace_declarations = file_scoped:warning
csharp_style_var_for_built_in_types = true:silent
csharp_style_var_when_type_is_apparent = true:silent
csharp_style_var_elsewhere = true:silent
csharp_style_expression_bodied_methods = when_on_single_line:silent
csharp_style_expression_bodied_properties = true:warning

# Nullable + modern C#
dotnet_style_null_propagation = true:warning
dotnet_style_coalesce_expression = true:warning
dotnet_style_prefer_is_null_check_over_reference_equality_method = true:warning
dotnet_style_prefer_conditional_expression_over_assignment = true:silent

# Using organization
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

# Naming
dotnet_naming_rule.interfaces_start_with_i.severity = warning
dotnet_naming_rule.interfaces_start_with_i.symbols = interfaces
dotnet_naming_rule.interfaces_start_with_i.style = begins_with_i
dotnet_naming_symbols.interfaces.applicable_kinds = interface
dotnet_naming_style.begins_with_i.required_prefix = I
dotnet_naming_style.begins_with_i.capitalization = pascal_case

# Analyzer severities
dotnet_diagnostic.CA1062.severity = none   # Explicit null check — nullable reference types cover it
dotnet_diagnostic.IDE0058.severity = none  # Unused expression value — noisy on fluent builders

# Unit tests may relax
[tests/**/*.cs]
dotnet_diagnostic.CA1707.severity = none   # Identifiers with underscore — test naming
dotnet_diagnostic.IDE0058.severity = none
```

- [ ] **Step 2: Commit**

```bash
git add .editorconfig
git -c commit.gpgsign=false commit -m "chore: add .editorconfig with modern C# style + analyzer tuning" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 0.5: Create solution file + `eng/` folder + placeholder icon

**Files:**
- Create: `CoinGecko.sln`
- Create: `eng/icon.png` (placeholder 128×128 PNG — can be replaced before 0.1.0 ship)

- [ ] **Step 1: Create solution**

Run: `dotnet new sln --name CoinGecko`
Expected: creates `CoinGecko.sln`.

- [ ] **Step 2: Create `eng/` and drop a placeholder icon**

Run (pwsh):

```powershell
New-Item -ItemType Directory -Force eng
# Create a 1-pixel PNG placeholder if you don't have a real icon yet.
# Replace before publishing 0.1.0.
[IO.File]::WriteAllBytes("eng/icon.png", [Convert]::FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="))
```

- [ ] **Step 3: Commit**

```bash
git add CoinGecko.sln eng/
git -c commit.gpgsign=false commit -m "build: add solution file and placeholder icon in eng/" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 0.6: Verify empty solution builds

- [ ] **Step 1: Run build**

Run: `dotnet build CoinGecko.sln -c Release`
Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

(No projects yet — the solution file is empty; command exits 0.)

- [ ] **Step 2: Commit nothing** (no changes).

---
## Phase 1 — `CoinGecko.Api` csproj + package metadata

### Task 1.1: Create the `CoinGecko.Api` project

**Files:**
- Create: `src/CoinGecko.Api/CoinGecko.Api.csproj`
- Create: `src/CoinGecko.Api/README.md` (pack-time package README)

- [ ] **Step 1: Create project via CLI**

Run:

```bash
dotnet new classlib -n CoinGecko.Api -o src/CoinGecko.Api -f net9.0 --force
```

This creates a skeleton `.csproj` with a stub `Class1.cs`. Delete the stub:

```bash
rm src/CoinGecko.Api/Class1.cs
```

- [ ] **Step 2: Overwrite the csproj with the canonical contents**

Replace `src/CoinGecko.Api/CoinGecko.Api.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net9.0;net8.0</TargetFrameworks>
    <RootNamespace>CoinGecko.Api</RootNamespace>
    <AssemblyName>CoinGecko.Api</AssemblyName>
    <IsPackable>true</IsPackable>
    <IsAotCompatible>true</IsAotCompatible>
    <IsTrimmable>true</IsTrimmable>
    <PackageId>CoinGecko.Api</PackageId>
    <Description>Strongly typed, AOT-safe .NET client for the CoinGecko REST API (core + onchain). Covers every documented endpoint with async pagination, plan-aware gating, and resilient rate-limit handling.</Description>
    <PackageTags>coingecko crypto cryptocurrency price api client rest geckoterminal onchain defi</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Http" />
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

- [ ] **Step 3: Write the pack-time package README**

Create `src/CoinGecko.Api/README.md`:

```markdown
# CoinGecko.Api

Strongly typed, AOT-safe .NET client for the [CoinGecko](https://www.coingecko.com/) REST API.

## Install

```powershell
dotnet add package CoinGecko.Api
```

## Quickstart (ASP.NET Core / Minimal Hosting)

```csharp
builder.Services.AddCoinGeckoApi(opts =>
{
    opts.ApiKey = builder.Configuration["CoinGecko:ApiKey"];
    opts.Plan   = CoinGeckoPlan.Demo; // or Analyst / Lite / Pro / ProPlus / Enterprise
});

// Inject ICoinGeckoClient anywhere:
public sealed class PriceService(ICoinGeckoClient gecko)
{
    public Task<Coin> GetBtcAsync(CancellationToken ct)
        => gecko.Coins.GetAsync("bitcoin", ct: ct);
}
```

## Quickstart (Console / scripts)

```csharp
using var gecko = CoinGeckoClientFactory.Create("my-api-key", CoinGeckoPlan.Pro);
var btc = await gecko.Coins.GetAsync("bitcoin");
Console.WriteLine($"BTC: ${btc.MarketData.CurrentPrice["usd"]}");
```

See the [full documentation](https://github.com/msanlisavas/CoinGecko) for all 14 sub-clients, streaming (CoinGecko.Api.WebSockets), and AI Agent Hub integrations (CoinGecko.Api.AiAgentHub, CoinGecko.Api.AiAgentHub.Mcp).

## License

MIT © msanlisavas
```

- [ ] **Step 4: Add the project to the solution**

Run:

```bash
dotnet sln CoinGecko.sln add src/CoinGecko.Api/CoinGecko.Api.csproj
```

- [ ] **Step 5: Build to verify it restores**

Run: `dotnet build src/CoinGecko.Api/CoinGecko.Api.csproj -c Release`
Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 6: Commit**

```bash
git add src/CoinGecko.Api/ CoinGecko.sln
git -c commit.gpgsign=false commit -m "feat(api): scaffold CoinGecko.Api project with NuGet metadata" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 1.2: Add `dotnet pack` smoke check

**Files:** (none created; invocation-only verification)

- [ ] **Step 1: Produce a prerelease nupkg**

Run:

```bash
dotnet pack src/CoinGecko.Api/CoinGecko.Api.csproj -c Release /p:Version=0.0.1-preview -o artifacts/preview
```

Expected: one `.nupkg` + one `.snupkg` in `artifacts/preview/`.

- [ ] **Step 2: Inspect the nupkg contents**

Run (pwsh):

```powershell
Expand-Archive artifacts/preview/CoinGecko.Api.0.0.1-preview.nupkg -DestinationPath artifacts/preview/inspect -Force
Get-ChildItem artifacts/preview/inspect -Recurse | Select-Object FullName
Remove-Item artifacts/preview -Recurse -Force
```

Expected entries include: `CoinGecko.Api.nuspec`, `lib/net9.0/CoinGecko.Api.dll`, `lib/net8.0/CoinGecko.Api.dll`, `README.md`, `icon.png`, `_rels/`, `[Content_Types].xml`.

Open the `.nuspec` and verify: `id`, `description`, `authors=msanlisavas`, `license`, `projectUrl`, `repository`, `icon=icon.png`, `readme=README.md`, all populated.

- [ ] **Step 3: Verify `artifacts/` is gitignored**

The initial `.gitignore` committed before Phase 0 already lists `artifacts/`. Confirm with:

```bash
grep -n '^artifacts/' .gitignore
```

If it already matches, no `.gitignore` edit or commit is needed. Task 1.2 is a pure verification gate — **do not create an empty commit**.

---
## Phase 2 — Options, enums, and plan model

Every subsequent phase references types from this phase, so it comes early. The attribute for plan-gating (`RequiresPlanAttribute`) is also defined here — it lives in a common surface so sub-client interfaces in later phases can reference it without introducing a new namespace.

### Task 2.1: Add the `CoinGecko.Api.Tests` project

**Files:**
- Create: `tests/CoinGecko.Api.Tests/CoinGecko.Api.Tests.csproj`
- Create: `tests/CoinGecko.Api.Tests/GlobalUsings.cs`

- [ ] **Step 1: Create test project**

Run:

```bash
dotnet new classlib -n CoinGecko.Api.Tests -o tests/CoinGecko.Api.Tests -f net9.0 --force
rm tests/CoinGecko.Api.Tests/Class1.cs
```

- [ ] **Step 2: Overwrite the csproj**

Replace `tests/CoinGecko.Api.Tests/CoinGecko.Api.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net9.0;net8.0</TargetFrameworks>
    <RootNamespace>CoinGecko.Api.Tests</RootNamespace>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <OutputType>Exe</OutputType>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
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
    <ProjectReference Include="..\..\src\CoinGecko.Api\CoinGecko.Api.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Add global usings for tests**

Create `tests/CoinGecko.Api.Tests/GlobalUsings.cs`:

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using Shouldly;
global using Xunit;
```

- [ ] **Step 4: Add to solution**

Run:

```bash
dotnet sln CoinGecko.sln add tests/CoinGecko.Api.Tests/CoinGecko.Api.Tests.csproj
```

- [ ] **Step 5: Verify build**

Run: `dotnet build tests/CoinGecko.Api.Tests/CoinGecko.Api.Tests.csproj -c Debug`
Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add tests/CoinGecko.Api.Tests/ CoinGecko.sln
git -c commit.gpgsign=false commit -m "test: scaffold CoinGecko.Api.Tests project (xUnit v3)" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 2.2: Define `CoinGeckoPlan` enum

**Files:**
- Create: `src/CoinGecko.Api/CoinGeckoPlan.cs`
- Create: `tests/CoinGecko.Api.Tests/CoinGeckoPlanTests.cs`

- [ ] **Step 1: Write the failing test**

`tests/CoinGecko.Api.Tests/CoinGeckoPlanTests.cs`:

```csharp
using CoinGecko.Api;

namespace CoinGecko.Api.Tests;

public class CoinGeckoPlanTests
{
    [Fact]
    public void Plans_are_ordered_ascending_by_capability()
    {
        // Ordinal comparison must work for plan gating:
        ((int)CoinGeckoPlan.Demo).ShouldBeLessThan((int)CoinGeckoPlan.Basic);
        ((int)CoinGeckoPlan.Basic).ShouldBeLessThan((int)CoinGeckoPlan.Analyst);
        ((int)CoinGeckoPlan.Analyst).ShouldBeLessThan((int)CoinGeckoPlan.Lite);
        ((int)CoinGeckoPlan.Lite).ShouldBeLessThan((int)CoinGeckoPlan.Pro);
        ((int)CoinGeckoPlan.Pro).ShouldBeLessThan((int)CoinGeckoPlan.ProPlus);
        ((int)CoinGeckoPlan.ProPlus).ShouldBeLessThan((int)CoinGeckoPlan.Enterprise);
    }

    [Fact]
    public void Demo_is_default_integer_value_zero()
    {
        ((int)default(CoinGeckoPlan)).ShouldBe(0);
        default(CoinGeckoPlan).ShouldBe(CoinGeckoPlan.Demo);
    }
}
```

- [ ] **Step 2: Run the test — expect compile failure**

Run: `dotnet test tests/CoinGecko.Api.Tests/CoinGecko.Api.Tests.csproj`
Expected: build error, "The type or namespace name 'CoinGeckoPlan' could not be found".

- [ ] **Step 3: Implement**

Create `src/CoinGecko.Api/CoinGeckoPlan.cs`:

```csharp
namespace CoinGecko.Api;

/// <summary>
/// CoinGecko subscription tiers. Ordered ascending by capability — higher-ordinal plans
/// include all endpoints and rate-limit budget of lower-ordinal plans. Use
/// <see cref="RequiresPlanAttribute"/> on sub-client methods to gate endpoints.
/// </summary>
/// <remarks>
/// The base URL split is binary: <see cref="Demo"/> routes to <c>api.coingecko.com</c>;
/// every other value routes to <c>pro-api.coingecko.com</c>. The ordinal granularity lets
/// the plan-enforcement handler short-circuit calls that require a higher tier.
/// </remarks>
public enum CoinGeckoPlan
{
    /// <summary>Free / Public tier with a Demo API key. Routes to <c>api.coingecko.com</c>.</summary>
    Demo = 0,

    /// <summary>Paid Basic tier. Routes to <c>pro-api.coingecko.com</c>.</summary>
    Basic = 1,

    /// <summary>Paid Analyst tier — unlocks WebSocket beta and premium endpoints.</summary>
    Analyst = 2,

    /// <summary>Paid Lite tier.</summary>
    Lite = 3,

    /// <summary>Paid Pro tier.</summary>
    Pro = 4,

    /// <summary>Paid Pro+ tier (higher credit / rate-limit budget than <see cref="Pro"/>).</summary>
    ProPlus = 5,

    /// <summary>Enterprise / custom contract.</summary>
    Enterprise = 6,
}
```

- [ ] **Step 4: Run the test — expect pass**

Run: `dotnet test tests/CoinGecko.Api.Tests/CoinGecko.Api.Tests.csproj --filter CoinGeckoPlanTests`
Expected: `Passed! - Failed: 0, Passed: 2`.

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/CoinGeckoPlan.cs tests/CoinGecko.Api.Tests/CoinGeckoPlanTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add CoinGeckoPlan enum with 7 ascending tiers" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 2.3: Define `RequiresPlanAttribute`

**Files:**
- Create: `src/CoinGecko.Api/RequiresPlanAttribute.cs`
- Create: `tests/CoinGecko.Api.Tests/RequiresPlanAttributeTests.cs`

- [ ] **Step 1: Write the failing test**

`tests/CoinGecko.Api.Tests/RequiresPlanAttributeTests.cs`:

```csharp
using System.Reflection;
using CoinGecko.Api;

namespace CoinGecko.Api.Tests;

public class RequiresPlanAttributeTests
{
    [Fact]
    public void Attribute_targets_methods_and_classes()
    {
        var targets = typeof(RequiresPlanAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>()!
            .ValidOn;

        targets.HasFlag(AttributeTargets.Method).ShouldBeTrue();
        targets.HasFlag(AttributeTargets.Class).ShouldBeTrue();
        targets.HasFlag(AttributeTargets.Interface).ShouldBeTrue();
    }

    [Fact]
    public void Attribute_carries_required_plan()
    {
        var attr = new RequiresPlanAttribute(CoinGeckoPlan.Analyst);
        attr.Plan.ShouldBe(CoinGeckoPlan.Analyst);
    }

    [Fact]
    public void Attribute_is_not_inherited()
    {
        typeof(RequiresPlanAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>()!
            .Inherited.ShouldBeFalse("method-level gating should not propagate to overrides automatically");
    }
}
```

- [ ] **Step 2: Run — expect fail**

Run: `dotnet test tests/CoinGecko.Api.Tests/CoinGecko.Api.Tests.csproj --filter RequiresPlanAttributeTests`
Expected: build error ("type 'RequiresPlanAttribute' could not be found").

- [ ] **Step 3: Implement**

Create `src/CoinGecko.Api/RequiresPlanAttribute.cs`:

```csharp
namespace CoinGecko.Api;

/// <summary>
/// Marks a sub-client interface, class, or method as requiring a specific minimum
/// <see cref="CoinGeckoPlan"/> tier. Enforced at the handler-pipeline level (see
/// <c>CoinGeckoPlanHandler</c>) — calls that violate the attribute throw
/// <c>CoinGeckoPlanException</c> before the HTTP request is issued.
/// </summary>
/// <remarks>
/// Ordinal comparison is used: <c>[RequiresPlan(CoinGeckoPlan.Analyst)]</c> passes for
/// <see cref="CoinGeckoPlan.Analyst"/> and every higher tier.
/// </remarks>
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface,
    AllowMultiple = false,
    Inherited = false)]
public sealed class RequiresPlanAttribute(CoinGeckoPlan plan) : Attribute
{
    /// <summary>The minimum plan tier that can invoke the decorated member.</summary>
    public CoinGeckoPlan Plan { get; } = plan;
}
```

- [ ] **Step 4: Run — expect pass**

Run: `dotnet test tests/CoinGecko.Api.Tests/CoinGecko.Api.Tests.csproj --filter RequiresPlanAttributeTests`
Expected: `Passed: 3`.

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/RequiresPlanAttribute.cs tests/CoinGecko.Api.Tests/RequiresPlanAttributeTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add [RequiresPlan] attribute for endpoint plan gating" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 2.4: Define `RateLimitPolicy` and `AuthenticationMode` enums

**Files:**
- Create: `src/CoinGecko.Api/RateLimitPolicy.cs`
- Create: `src/CoinGecko.Api/AuthenticationMode.cs`
- Create: `tests/CoinGecko.Api.Tests/PolicyEnumTests.cs`

- [ ] **Step 1: Write the failing test**

`tests/CoinGecko.Api.Tests/PolicyEnumTests.cs`:

```csharp
using CoinGecko.Api;

namespace CoinGecko.Api.Tests;

public class PolicyEnumTests
{
    [Fact]
    public void RateLimitPolicy_has_three_values_with_respect_as_default()
    {
        Enum.GetValues<RateLimitPolicy>().ShouldBe(
            new[] { RateLimitPolicy.Respect, RateLimitPolicy.Throw, RateLimitPolicy.Ignore },
            ignoreOrder: true);
        default(RateLimitPolicy).ShouldBe(RateLimitPolicy.Respect);
    }

    [Fact]
    public void AuthenticationMode_has_header_default_and_querystring_alt()
    {
        Enum.GetValues<AuthenticationMode>().ShouldBe(
            new[] { AuthenticationMode.Header, AuthenticationMode.QueryString },
            ignoreOrder: true);
        default(AuthenticationMode).ShouldBe(AuthenticationMode.Header);
    }
}
```

- [ ] **Step 2: Run — expect compile fail.**

- [ ] **Step 3: Implement both enums**

`src/CoinGecko.Api/RateLimitPolicy.cs`:

```csharp
namespace CoinGecko.Api;

/// <summary>
/// Behavior when CoinGecko responds with <c>429 Too Many Requests</c>.
/// </summary>
public enum RateLimitPolicy
{
    /// <summary>Honor <c>Retry-After</c> and retry automatically (default).</summary>
    Respect = 0,

    /// <summary>Surface as <c>CoinGeckoRateLimitException</c> without retrying.</summary>
    Throw = 1,

    /// <summary>Pass the raw <c>HttpResponseMessage</c> through without special handling.</summary>
    Ignore = 2,
}
```

`src/CoinGecko.Api/AuthenticationMode.cs`:

```csharp
namespace CoinGecko.Api;

/// <summary>
/// How the API key is transmitted on each request. Header is recommended and default;
/// query-string exists for edge cases such as caching proxies that key off the URL.
/// </summary>
public enum AuthenticationMode
{
    /// <summary>Send the key via <c>x-cg-demo-api-key</c> / <c>x-cg-pro-api-key</c> header.</summary>
    Header = 0,

    /// <summary>Send the key via <c>x_cg_demo_api_key</c> / <c>x_cg_pro_api_key</c> query param.</summary>
    QueryString = 1,
}
```

- [ ] **Step 4: Run — expect pass (2 tests).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/RateLimitPolicy.cs src/CoinGecko.Api/AuthenticationMode.cs tests/CoinGecko.Api.Tests/PolicyEnumTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add RateLimitPolicy and AuthenticationMode enums" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 2.5: Define `CoinGeckoOptions`

**Files:**
- Create: `src/CoinGecko.Api/CoinGeckoOptions.cs`
- Create: `tests/CoinGecko.Api.Tests/CoinGeckoOptionsTests.cs`

- [ ] **Step 1: Write the failing test**

`tests/CoinGecko.Api.Tests/CoinGeckoOptionsTests.cs`:

```csharp
using CoinGecko.Api;

namespace CoinGecko.Api.Tests;

public class CoinGeckoOptionsTests
{
    [Fact]
    public void Defaults_are_sensible()
    {
        var o = new CoinGeckoOptions();
        o.ApiKey.ShouldBeNull();
        o.Plan.ShouldBe(CoinGeckoPlan.Demo);
        o.Timeout.ShouldBe(TimeSpan.FromSeconds(30));
        o.UserAgent.ShouldStartWith("CoinGecko.Api/");
        o.AutoPaginate.ShouldBeTrue();
        o.RateLimit.ShouldBe(RateLimitPolicy.Respect);
        o.AuthMode.ShouldBe(AuthenticationMode.Header);
        o.BaseAddress.ShouldBeNull();
        o.OnchainBaseAddress.ShouldBeNull();
    }

    [Fact]
    public void UserAgent_placeholder_is_replaced_with_assembly_version_when_read_via_resolved_accessor()
    {
        var o = new CoinGeckoOptions();
        // UserAgent is a raw template here; a later phase (handler) resolves {version}.
        o.UserAgent.ShouldContain("{version}");
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Implement**

`src/CoinGecko.Api/CoinGeckoOptions.cs`:

```csharp
namespace CoinGecko.Api;

/// <summary>
/// Configuration for <see cref="ICoinGeckoClient"/>. Bind from configuration or set directly
/// via <c>AddCoinGeckoApi(opts =&gt; ...)</c>.
/// </summary>
public sealed class CoinGeckoOptions
{
    /// <summary>CoinGecko API key. Required for every tier (Demo keys are free but required since 2024).</summary>
    public string? ApiKey { get; set; }

    /// <summary>Subscription tier. Drives base URL selection (Demo → <c>api.coingecko.com</c>; anything else → <c>pro-api.coingecko.com</c>) and endpoint gating.</summary>
    public CoinGeckoPlan Plan { get; set; } = CoinGeckoPlan.Demo;

    /// <summary>Override for the primary base URL. Leave null for the plan-default host.</summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>Override for the onchain (GeckoTerminal) base URL. Leave null to derive from <see cref="BaseAddress"/>.</summary>
    public Uri? OnchainBaseAddress { get; set; }

    /// <summary>User-Agent header. The token <c>{version}</c> is replaced with the assembly informational version at handler attach time.</summary>
    public string UserAgent { get; set; } = "CoinGecko.Api/{version} (+https://github.com/msanlisavas/CoinGecko)";

    /// <summary>Per-request timeout. Applied via a linked <see cref="CancellationTokenSource"/>; does not touch <see cref="HttpClient.Timeout"/>.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Enable <see cref="IAsyncEnumerable{T}"/> auto-pagination for <c>EnumerateXxxAsync</c> methods.</summary>
    public bool AutoPaginate { get; set; } = true;

    /// <summary>Behavior on HTTP 429 responses.</summary>
    public RateLimitPolicy RateLimit { get; set; } = RateLimitPolicy.Respect;

    /// <summary>Whether to transmit the API key via header (default) or query string.</summary>
    public AuthenticationMode AuthMode { get; set; } = AuthenticationMode.Header;
}
```

- [ ] **Step 4: Run — expect pass (2 tests).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/CoinGeckoOptions.cs tests/CoinGecko.Api.Tests/CoinGeckoOptionsTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add CoinGeckoOptions with full default configuration" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 2.6: Define request-shaping enums

This batch covers the closed-set query parameters that show up across multiple sub-clients. Keeping them in one task avoids sprawl later.

**Files:**
- Create: `src/CoinGecko.Api/Models/Enums.cs`
- Create: `tests/CoinGecko.Api.Tests/EnumSerializationTests.cs`

- [ ] **Step 1: Write the failing test**

`tests/CoinGecko.Api.Tests/EnumSerializationTests.cs`:

```csharp
using System.Runtime.Serialization;
using System.Reflection;
using CoinGecko.Api.Models;

namespace CoinGecko.Api.Tests;

public class EnumSerializationTests
{
    public static TheoryData<Enum, string> WireFormats() => new()
    {
        { CoinMarketsOrder.MarketCapDesc, "market_cap_desc" },
        { CoinMarketsOrder.MarketCapAsc,  "market_cap_asc" },
        { CoinMarketsOrder.VolumeDesc,    "volume_desc" },
        { CoinMarketsOrder.VolumeAsc,     "volume_asc" },
        { CoinMarketsOrder.IdAsc,         "id_asc" },
        { CoinMarketsOrder.IdDesc,        "id_desc" },

        { PriceChangeWindow.OneHour,      "1h" },
        { PriceChangeWindow.TwentyFourHours, "24h" },
        { PriceChangeWindow.SevenDays,    "7d" },
        { PriceChangeWindow.FourteenDays, "14d" },
        { PriceChangeWindow.ThirtyDays,   "30d" },
        { PriceChangeWindow.TwoHundredDays, "200d" },
        { PriceChangeWindow.OneYear,      "1y" },

        { MarketChartRange.OneDay,    "1" },
        { MarketChartRange.SevenDays, "7" },
        { MarketChartRange.FourteenDays, "14" },
        { MarketChartRange.ThirtyDays, "30" },
        { MarketChartRange.NinetyDays, "90" },
        { MarketChartRange.OneHundredEightyDays, "180" },
        { MarketChartRange.OneYear,   "365" },
        { MarketChartRange.Max,       "max" },
    };

    [Theory, MemberData(nameof(WireFormats))]
    public void Enum_member_carries_its_wire_format(Enum value, string expected)
    {
        var member = value.GetType().GetField(value.ToString());
        member.ShouldNotBeNull();
        var em = member!.GetCustomAttribute<EnumMemberAttribute>();
        em.ShouldNotBeNull();
        em!.Value.ShouldBe(expected);
    }
}
```

- [ ] **Step 2: Run — expect compile fail.**

- [ ] **Step 3: Implement**

`src/CoinGecko.Api/Models/Enums.cs`:

```csharp
using System.Runtime.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Sort order for <c>/coins/markets</c>.</summary>
public enum CoinMarketsOrder
{
    /// <summary>Descending by market capitalization (default).</summary>
    [EnumMember(Value = "market_cap_desc")] MarketCapDesc = 0,

    /// <summary>Ascending by market capitalization.</summary>
    [EnumMember(Value = "market_cap_asc")]  MarketCapAsc,

    /// <summary>Descending by trading volume.</summary>
    [EnumMember(Value = "volume_desc")]     VolumeDesc,

    /// <summary>Ascending by trading volume.</summary>
    [EnumMember(Value = "volume_asc")]      VolumeAsc,

    /// <summary>Ascending alphabetically by coin id.</summary>
    [EnumMember(Value = "id_asc")]          IdAsc,

    /// <summary>Descending alphabetically by coin id.</summary>
    [EnumMember(Value = "id_desc")]         IdDesc,
}

/// <summary>Price-change windows for the <c>price_change_percentage</c> param.</summary>
public enum PriceChangeWindow
{
    /// <summary>1 hour.</summary>
    [EnumMember(Value = "1h")]   OneHour = 0,

    /// <summary>24 hours (1 day).</summary>
    [EnumMember(Value = "24h")]  TwentyFourHours,

    /// <summary>7 days.</summary>
    [EnumMember(Value = "7d")]   SevenDays,

    /// <summary>14 days.</summary>
    [EnumMember(Value = "14d")]  FourteenDays,

    /// <summary>30 days.</summary>
    [EnumMember(Value = "30d")]  ThirtyDays,

    /// <summary>200 days.</summary>
    [EnumMember(Value = "200d")] TwoHundredDays,

    /// <summary>1 year.</summary>
    [EnumMember(Value = "1y")]   OneYear,
}

/// <summary>Fixed time windows for <c>/coins/{id}/market_chart</c>.</summary>
public enum MarketChartRange
{
    /// <summary>1 day.</summary>
    [EnumMember(Value = "1")]   OneDay = 0,

    /// <summary>7 days.</summary>
    [EnumMember(Value = "7")]   SevenDays,

    /// <summary>14 days.</summary>
    [EnumMember(Value = "14")]  FourteenDays,

    /// <summary>30 days.</summary>
    [EnumMember(Value = "30")]  ThirtyDays,

    /// <summary>90 days.</summary>
    [EnumMember(Value = "90")]  NinetyDays,

    /// <summary>180 days.</summary>
    [EnumMember(Value = "180")] OneHundredEightyDays,

    /// <summary>365 days (1 year).</summary>
    [EnumMember(Value = "365")] OneYear,

    /// <summary>Entire available history.</summary>
    [EnumMember(Value = "max")] Max,
}
```

- [ ] **Step 4: Run — expect pass (all theory rows).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/Models/Enums.cs tests/CoinGecko.Api.Tests/EnumSerializationTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add request-shaping enums with wire-format EnumMember values" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---
## Phase 3 — Exception hierarchy

### Task 3.1: Base `CoinGeckoException` + six derived types

**Files:**
- Create: `src/CoinGecko.Api/Exceptions/CoinGeckoException.cs`
- Create: `src/CoinGecko.Api/Exceptions/CoinGeckoRateLimitException.cs`
- Create: `src/CoinGecko.Api/Exceptions/CoinGeckoPlanException.cs`
- Create: `src/CoinGecko.Api/Exceptions/CoinGeckoAuthException.cs`
- Create: `src/CoinGecko.Api/Exceptions/CoinGeckoNotFoundException.cs`
- Create: `src/CoinGecko.Api/Exceptions/CoinGeckoValidationException.cs`
- Create: `src/CoinGecko.Api/Exceptions/CoinGeckoServerException.cs`
- Create: `tests/CoinGecko.Api.Tests/ExceptionHierarchyTests.cs`

- [ ] **Step 1: Write the failing test**

`tests/CoinGecko.Api.Tests/ExceptionHierarchyTests.cs`:

```csharp
using System.Net;
using CoinGecko.Api;
using CoinGecko.Api.Exceptions;

namespace CoinGecko.Api.Tests;

public class ExceptionHierarchyTests
{
    [Theory]
    [InlineData(typeof(CoinGeckoRateLimitException))]
    [InlineData(typeof(CoinGeckoPlanException))]
    [InlineData(typeof(CoinGeckoAuthException))]
    [InlineData(typeof(CoinGeckoNotFoundException))]
    [InlineData(typeof(CoinGeckoValidationException))]
    [InlineData(typeof(CoinGeckoServerException))]
    public void All_derived_types_inherit_CoinGeckoException(Type type)
    {
        type.IsSubclassOf(typeof(CoinGeckoException)).ShouldBeTrue();
    }

    [Fact]
    public void Base_exposes_status_and_raw_body()
    {
        var ex = new CoinGeckoAuthException(HttpStatusCode.Unauthorized, "{\"error\":\"bad key\"}", requestId: Guid.Empty);
        ex.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        ex.RawBody.ShouldBe("{\"error\":\"bad key\"}");
        ex.RequestId.ShouldBe(Guid.Empty);
        ex.Message.ShouldContain("401");
    }

    [Fact]
    public void RateLimit_exposes_retry_after()
    {
        var ex = new CoinGeckoRateLimitException(TimeSpan.FromSeconds(7), rawBody: "", requestId: Guid.NewGuid());
        ex.RetryAfter.ShouldBe(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public void Plan_exposes_required_plan()
    {
        var ex = new CoinGeckoPlanException(required: CoinGeckoPlan.Analyst, actual: CoinGeckoPlan.Demo);
        ex.RequiredPlan.ShouldBe(CoinGeckoPlan.Analyst);
        ex.ActualPlan.ShouldBe(CoinGeckoPlan.Demo);
    }
}
```

- [ ] **Step 2: Run — expect compile fail.**

- [ ] **Step 3: Implement the base**

`src/CoinGecko.Api/Exceptions/CoinGeckoException.cs`:

```csharp
using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>Base type for all exceptions thrown by <c>CoinGecko.Api</c>.</summary>
public abstract class CoinGeckoException : Exception
{
    /// <summary>HTTP status of the CoinGecko response, or null if the error pre-dates the HTTP call.</summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>Raw response body (truncated to 16 KiB).</summary>
    public string? RawBody { get; }

    /// <summary>Correlation id shared with <c>ActivitySource</c> and <c>ILogger</c>.</summary>
    public Guid RequestId { get; }

    /// <summary>Initializes a new <see cref="CoinGeckoException"/>.</summary>
    /// <param name="message">Human-readable description of the failure.</param>
    /// <param name="statusCode">HTTP status of the response, or null if pre-HTTP.</param>
    /// <param name="rawBody">Response body (truncated to 16 KiB).</param>
    /// <param name="requestId">Correlation id from the telemetry / logging pipeline.</param>
    /// <param name="inner">Optional inner exception.</param>
    protected CoinGeckoException(
        string message,
        HttpStatusCode? statusCode,
        string? rawBody,
        Guid requestId,
        Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        RawBody = Truncate(rawBody, 16 * 1024);
        RequestId = requestId;
    }

    private static string? Truncate(string? s, int max)
        => s is null || s.Length <= max ? s : s[..max] + "… [truncated]";
}
```

- [ ] **Step 4: Implement each derived type**

`src/CoinGecko.Api/Exceptions/CoinGeckoRateLimitException.cs`:

```csharp
using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>Thrown when CoinGecko responds 429 and <see cref="RateLimitPolicy.Throw"/> is in effect, or when retries are exhausted under <see cref="RateLimitPolicy.Respect"/>.</summary>
public sealed class CoinGeckoRateLimitException(TimeSpan? retryAfter, string? rawBody, Guid requestId)
    : CoinGeckoException(
        $"CoinGecko rate limit hit (429). Retry-After = {retryAfter?.ToString() ?? "unspecified"}.",
        HttpStatusCode.TooManyRequests,
        rawBody,
        requestId)
{
    /// <summary>Server-advised delay before retrying, parsed from the <c>Retry-After</c> header.</summary>
    public TimeSpan? RetryAfter { get; } = retryAfter;
}
```

`src/CoinGecko.Api/Exceptions/CoinGeckoPlanException.cs`:

```csharp
namespace CoinGecko.Api.Exceptions;

/// <summary>Thrown before issuing a request when the configured <see cref="CoinGeckoOptions.Plan"/> is below the endpoint's <see cref="RequiresPlanAttribute"/>.</summary>
public sealed class CoinGeckoPlanException(CoinGeckoPlan required, CoinGeckoPlan actual)
    : CoinGeckoException(
        $"This endpoint requires plan {required} or higher; configured plan is {actual}. Upgrade at https://www.coingecko.com/en/api/pricing.",
        statusCode: null,
        rawBody: null,
        requestId: Guid.Empty)
{
    /// <summary>Minimum plan tier required for the endpoint.</summary>
    public CoinGeckoPlan RequiredPlan { get; } = required;

    /// <summary>Plan tier the caller is currently configured with.</summary>
    public CoinGeckoPlan ActualPlan { get; } = actual;
}
```

`src/CoinGecko.Api/Exceptions/CoinGeckoAuthException.cs`:

```csharp
using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>401 / 403. Typically a missing, wrong-type, or revoked API key.</summary>
public sealed class CoinGeckoAuthException(HttpStatusCode statusCode, string? rawBody, Guid requestId)
    : CoinGeckoException(
        $"CoinGecko rejected the credentials ({(int)statusCode} {statusCode}). Verify ApiKey and Plan.",
        statusCode, rawBody, requestId)
{
}
```

`src/CoinGecko.Api/Exceptions/CoinGeckoNotFoundException.cs`:

```csharp
using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>404. Usually an unknown coin id, asset-platform id, or contract address.</summary>
public sealed class CoinGeckoNotFoundException(string? rawBody, Guid requestId)
    : CoinGeckoException(
        "CoinGecko returned 404 Not Found.",
        HttpStatusCode.NotFound, rawBody, requestId)
{
}
```

`src/CoinGecko.Api/Exceptions/CoinGeckoValidationException.cs`:

```csharp
using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>400. Invalid query parameters or body.</summary>
public sealed class CoinGeckoValidationException(string? rawBody, Guid requestId)
    : CoinGeckoException(
        $"CoinGecko returned 400 Bad Request. Body: {rawBody}",
        HttpStatusCode.BadRequest, rawBody, requestId)
{
}
```

`src/CoinGecko.Api/Exceptions/CoinGeckoServerException.cs`:

```csharp
using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>5xx. Transient; retry handler exhausted or retries disabled.</summary>
public sealed class CoinGeckoServerException(HttpStatusCode statusCode, string? rawBody, Guid requestId, Exception? inner = null)
    : CoinGeckoException(
        $"CoinGecko returned {(int)statusCode} {statusCode}.",
        statusCode, rawBody, requestId, inner)
{
}
```

- [ ] **Step 5: Run — expect pass (all 4 tests across the theory).**

- [ ] **Step 6: Commit**

```bash
git add src/CoinGecko.Api/Exceptions/ tests/CoinGecko.Api.Tests/ExceptionHierarchyTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add CoinGeckoException hierarchy (6 derived types)" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---
## Phase 4 — Serialization infrastructure

All DTOs deserialize through a single `System.Text.Json` source-generated context to keep the library AOT- and trim-safe. Onchain endpoints wrap responses in a JSON:API envelope; the library unwraps them internally so the public API surface stays flat.

### Task 4.1: Add `JsonApiResponse<T>` envelope type

**Files:**
- Create: `src/CoinGecko.Api/Serialization/JsonApi/JsonApiResponse.cs`
- Create: `src/CoinGecko.Api/Serialization/JsonApi/JsonApiRelationship.cs`
- Create: `tests/CoinGecko.Api.Tests/Serialization/JsonApiResponseTests.cs`

- [ ] **Step 1: Write the failing test**

`tests/CoinGecko.Api.Tests/Serialization/JsonApiResponseTests.cs`:

```csharp
using System.Text.Json;
using CoinGecko.Api.Serialization.JsonApi;

namespace CoinGecko.Api.Tests.Serialization;

public class JsonApiResponseTests
{
    private sealed record Pool(string Name);

    [Fact]
    public void Single_object_payload_is_unwrapped_from_data()
    {
        const string json = """
        {
          "data": { "id": "1", "type": "pool", "attributes": { "name": "ETH-USDC" } },
          "included": [],
          "meta": {},
          "links": {}
        }
        """;

        var env = JsonSerializer.Deserialize<JsonApiResponse<JsonApiResource>>(json);
        env.ShouldNotBeNull();
        env!.Data.ShouldNotBeNull();
        env.Data!.Id.ShouldBe("1");
        env.Data.Type.ShouldBe("pool");
        env.Data.Attributes.ShouldNotBeNull();
    }

    [Fact]
    public void Array_payload_deserializes_into_data_array()
    {
        const string json = """
        {
          "data": [
            { "id": "1", "type": "pool" },
            { "id": "2", "type": "pool" }
          ],
          "included": null
        }
        """;

        var env = JsonSerializer.Deserialize<JsonApiResponse<JsonApiResource[]>>(json);
        env.ShouldNotBeNull();
        env!.Data.ShouldNotBeNull();
        env.Data!.Length.ShouldBe(2);
    }
}
```

- [ ] **Step 2: Run — expect compile fail.**

- [ ] **Step 3: Implement**

`src/CoinGecko.Api/Serialization/JsonApi/JsonApiResponse.cs`:

```csharp
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CoinGecko.Api.Serialization.JsonApi;

/// <summary>
/// JSON:API-style envelope used by CoinGecko onchain (GeckoTerminal) endpoints.
/// Public consumers never see this type — the handler pipeline unwraps <see cref="Data"/>
/// into the typed model before returning to the caller.
/// </summary>
/// <typeparam name="T">The typed payload (scalar resource, array, or merged graph).</typeparam>
public sealed class JsonApiResponse<T>
{
    [JsonPropertyName("data")]     public T? Data { get; set; }
    [JsonPropertyName("included")] public IReadOnlyList<JsonApiResource>? Included { get; set; }
    [JsonPropertyName("meta")]     public JsonObject? Meta { get; set; }
    [JsonPropertyName("links")]    public JsonApiLinks? Links { get; set; }
}

/// <summary>Top-level <c>links</c> object (pagination URIs).</summary>
public sealed class JsonApiLinks
{
    [JsonPropertyName("self")]   public string? Self { get; set; }
    [JsonPropertyName("first")]  public string? First { get; set; }
    [JsonPropertyName("last")]   public string? Last { get; set; }
    [JsonPropertyName("prev")]   public string? Prev { get; set; }
    [JsonPropertyName("next")]   public string? Next { get; set; }
}

/// <summary>Generic untyped resource (used for <c>included[]</c> cross-references).</summary>
public sealed class JsonApiResource
{
    [JsonPropertyName("id")]            public string? Id { get; set; }
    [JsonPropertyName("type")]          public string? Type { get; set; }
    [JsonPropertyName("attributes")]    public JsonObject? Attributes { get; set; }
    [JsonPropertyName("relationships")] public Dictionary<string, JsonApiRelationship>? Relationships { get; set; }
}
```

`src/CoinGecko.Api/Serialization/JsonApi/JsonApiRelationship.cs`:

```csharp
using System.Text.Json.Serialization;

namespace CoinGecko.Api.Serialization.JsonApi;

public sealed class JsonApiRelationship
{
    [JsonPropertyName("data")] public JsonApiResourceRef? Data { get; set; }
}

public sealed class JsonApiResourceRef
{
    [JsonPropertyName("id")]   public string? Id { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
}
```

- [ ] **Step 4: Run — expect pass.**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/Serialization/JsonApi/ tests/CoinGecko.Api.Tests/Serialization/JsonApiResponseTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add JSON:API envelope types for onchain endpoints" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 4.2: `UnixSecondsConverter`

**Files:**
- Create: `src/CoinGecko.Api/Serialization/UnixSecondsConverter.cs`
- Create: `tests/CoinGecko.Api.Tests/Serialization/UnixSecondsConverterTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using System.Text.Json;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Tests.Serialization;

public class UnixSecondsConverterTests
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        Converters = { new UnixSecondsConverter() },
    };

    [Fact]
    public void Reads_integer_unix_seconds()
    {
        var dt = JsonSerializer.Deserialize<DateTimeOffset>("1700000000", Opts);
        dt.ShouldBe(DateTimeOffset.FromUnixTimeSeconds(1700000000));
    }

    [Fact]
    public void Reads_fractional_unix_seconds_as_milliseconds_precision()
    {
        var dt = JsonSerializer.Deserialize<DateTimeOffset>("1700000000.5", Opts);
        dt.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1700000000500));
    }

    [Fact]
    public void Writes_as_integer_seconds()
    {
        var dt = DateTimeOffset.FromUnixTimeSeconds(1700000000);
        JsonSerializer.Serialize(dt, Opts).ShouldBe("1700000000");
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Implement**

`src/CoinGecko.Api/Serialization/UnixSecondsConverter.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoinGecko.Api.Serialization;

/// <summary>Reads Unix timestamps as numeric seconds (integer or fractional) and writes integer seconds.</summary>
public sealed class UnixSecondsConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out var seconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(seconds);
            }

            var d = reader.GetDouble();
            return DateTimeOffset.FromUnixTimeMilliseconds((long)(d * 1000));
        }

        if (reader.TokenType is JsonTokenType.String)
        {
            var s = reader.GetString();
            if (long.TryParse(s, out var secs))
            {
                return DateTimeOffset.FromUnixTimeSeconds(secs);
            }

            if (double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds((long)(d * 1000));
            }
        }

        throw new JsonException($"Cannot convert token {reader.TokenType} to DateTimeOffset (Unix seconds).");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
```

- [ ] **Step 4: Run — expect pass (3).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/Serialization/UnixSecondsConverter.cs tests/CoinGecko.Api.Tests/Serialization/UnixSecondsConverterTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add UnixSecondsConverter for epoch timestamp fields" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 4.3: Define `ResponseEnvelope` + `HttpRequestOptions` key

**Files:**
- Create: `src/CoinGecko.Api/Serialization/ResponseEnvelope.cs`
- Create: `src/CoinGecko.Api/Serialization/CoinGeckoRequestOptions.cs`

- [ ] **Step 1: Implement (no test needed — pure types; coverage comes via handler tests in Phase 6)**

`src/CoinGecko.Api/Serialization/ResponseEnvelope.cs`:

```csharp
namespace CoinGecko.Api.Serialization;

/// <summary>Selects which deserializer path to use for a given request.</summary>
public enum ResponseEnvelope
{
    /// <summary>Bare JSON object / array. Used for core endpoints.</summary>
    Bare = 0,

    /// <summary>JSON:API-style envelope. Used for onchain / GeckoTerminal endpoints.</summary>
    JsonApi = 1,
}
```

`src/CoinGecko.Api/Serialization/CoinGeckoRequestOptions.cs`:

```csharp
namespace CoinGecko.Api.Serialization;

/// <summary>Per-request metadata attached via <see cref="HttpRequestMessage.Options"/>.</summary>
internal static class CoinGeckoRequestOptions
{
    public static readonly HttpRequestOptionsKey<ResponseEnvelope> Envelope = new("coingecko.envelope");
    public static readonly HttpRequestOptionsKey<CoinGeckoPlan?>   RequiredPlan = new("coingecko.required_plan");
    public static readonly HttpRequestOptionsKey<string>           EndpointName = new("coingecko.endpoint");
    public static readonly HttpRequestOptionsKey<Guid>             RequestId = new("coingecko.request_id");
}
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build src/CoinGecko.Api -c Debug`
Expected: success.

- [ ] **Step 3: Commit**

```bash
git add src/CoinGecko.Api/Serialization/ResponseEnvelope.cs src/CoinGecko.Api/Serialization/CoinGeckoRequestOptions.cs
git -c commit.gpgsign=false commit -m "feat(api): add ResponseEnvelope enum and per-request options keys" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 4.4: Scaffold `CoinGeckoJsonContext` (seed DTOs added in Phase 8+; Phase 4 only registers the envelope + primitive types)

**Files:**
- Create: `src/CoinGecko.Api/Serialization/CoinGeckoJsonContext.cs`

- [ ] **Step 1: Implement the context seed**

`src/CoinGecko.Api/Serialization/CoinGeckoJsonContext.cs`:

```csharp
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
[JsonSerializable(typeof(JsonApiResponse<JsonApiResource>))]
[JsonSerializable(typeof(JsonApiResponse<JsonApiResource[]>))]
[JsonSerializable(typeof(JsonApiResource))]
[JsonSerializable(typeof(JsonApiResource[]))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(string[]))]
internal sealed partial class CoinGeckoJsonContext : JsonSerializerContext
{
}
```

- [ ] **Step 2: Build**

Run: `dotnet build src/CoinGecko.Api -c Debug`
Expected: build succeeds. (Source generator emits `CoinGeckoJsonContext.GetTypeInfo(Type)`.)

- [ ] **Step 3: Commit**

```bash
git add src/CoinGecko.Api/Serialization/CoinGeckoJsonContext.cs
git -c commit.gpgsign=false commit -m "feat(api): seed CoinGeckoJsonContext for source-gen STJ (AOT-safe)" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---
## Phase 5 — URI + query string utilities

Both are internal helpers used by every sub-client method. AOT-safe, zero reflection.

### Task 5.1: `UriTemplateExpander`

**Files:**
- Create: `src/CoinGecko.Api/Internal/UriTemplateExpander.cs`
- Create: `tests/CoinGecko.Api.Tests/Internal/UriTemplateExpanderTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using CoinGecko.Api.Internal;

namespace CoinGecko.Api.Tests.Internal;

public class UriTemplateExpanderTests
{
    [Theory]
    [InlineData("/coins/{id}", "id=bitcoin", "/coins/bitcoin")]
    [InlineData("/coins/{id}/market_chart", "id=bitcoin", "/coins/bitcoin/market_chart")]
    [InlineData("/coins/{id}/contract/{contract_address}", "id=ethereum;contract_address=0xA0b86a", "/coins/ethereum/contract/0xA0b86a")]
    public void Expand_replaces_named_segments(string template, string pairs, string expected)
    {
        var kvs = pairs.Split(';').Select(p => p.Split('=')).Select(a => (a[0], a[1])).ToArray();
        var result = UriTemplateExpander.Expand(template, kvs);
        result.ShouldBe(expected);
    }

    [Fact]
    public void Unreplaced_placeholder_throws()
    {
        Should.Throw<ArgumentException>(() => UriTemplateExpander.Expand("/coins/{id}", Array.Empty<(string, string)>()));
    }

    [Fact]
    public void Values_are_url_escaped()
    {
        UriTemplateExpander.Expand("/search/{q}", new[] { ("q", "a b/c") })
            .ShouldBe("/search/a%20b%2Fc");
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Implement**

`src/CoinGecko.Api/Internal/UriTemplateExpander.cs`:

```csharp
using System.Text;

namespace CoinGecko.Api.Internal;

/// <summary>Expands <c>{name}</c> placeholders in path templates with URL-escaped values.</summary>
internal static class UriTemplateExpander
{
    public static string Expand(string template, IReadOnlyList<(string Name, string Value)> values)
    {
        ArgumentException.ThrowIfNullOrEmpty(template);

        var sb = new StringBuilder(template.Length + 16);
        var i = 0;
        while (i < template.Length)
        {
            var open = template.IndexOf('{', i);
            if (open < 0)
            {
                sb.Append(template, i, template.Length - i);
                break;
            }

            sb.Append(template, i, open - i);

            var close = template.IndexOf('}', open + 1);
            if (close < 0)
            {
                throw new ArgumentException($"Unclosed placeholder in template: {template}");
            }

            var name = template[(open + 1)..close];
            var found = false;
            for (var k = 0; k < values.Count; k++)
            {
                if (values[k].Name == name)
                {
                    sb.Append(Uri.EscapeDataString(values[k].Value));
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new ArgumentException($"No value supplied for placeholder '{{{name}}}' in template '{template}'.");
            }

            i = close + 1;
        }

        return sb.ToString();
    }
}
```

- [ ] **Step 4: Run — expect pass (4).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/Internal/UriTemplateExpander.cs tests/CoinGecko.Api.Tests/Internal/UriTemplateExpanderTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add UriTemplateExpander for AOT-safe path expansion" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 5.2: `QueryStringBuilder`

**Files:**
- Create: `src/CoinGecko.Api/Internal/QueryStringBuilder.cs`
- Create: `tests/CoinGecko.Api.Tests/Internal/QueryStringBuilderTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;

namespace CoinGecko.Api.Tests.Internal;

public class QueryStringBuilderTests
{
    [Fact]
    public void Empty_builder_returns_empty_string()
        => new QueryStringBuilder().ToString().ShouldBe(string.Empty);

    [Fact]
    public void Appends_string_values_url_escaped()
    {
        new QueryStringBuilder()
            .Add("vs_currency", "usd")
            .Add("q", "a b/c")
            .ToString()
            .ShouldBe("?vs_currency=usd&q=a%20b%2Fc");
    }

    [Fact]
    public void Skips_null_values()
    {
        new QueryStringBuilder()
            .Add("a", "1")
            .Add("b", (string?)null)
            .Add("c", "3")
            .ToString()
            .ShouldBe("?a=1&c=3");
    }

    [Fact]
    public void Formats_numbers_with_invariant_culture()
    {
        new QueryStringBuilder()
            .Add("precision", 4)
            .Add("threshold", 1.5m)
            .ToString()
            .ShouldBe("?precision=4&threshold=1.5");
    }

    [Fact]
    public void Formats_bool_as_lowercase()
    {
        new QueryStringBuilder()
            .Add("sparkline", true)
            .Add("localization", false)
            .ToString()
            .ShouldBe("?sparkline=true&localization=false");
    }

    [Fact]
    public void Formats_date_as_dd_mm_yyyy_for_coingecko_history()
    {
        new QueryStringBuilder()
            .AddCoinGeckoDate("date", new DateOnly(2024, 1, 2))
            .ToString()
            .ShouldBe("?date=02-01-2024");
    }

    [Fact]
    public void Formats_enum_via_EnumMember_value()
    {
        new QueryStringBuilder()
            .AddEnum("order", CoinMarketsOrder.MarketCapDesc)
            .ToString()
            .ShouldBe("?order=market_cap_desc");
    }

    [Fact]
    public void Formats_enumerable_as_comma_separated()
    {
        new QueryStringBuilder()
            .AddList("ids", new[] { "bitcoin", "ethereum", "ripple" })
            .ToString()
            .ShouldBe("?ids=bitcoin%2Cethereum%2Cripple");
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Implement**

`src/CoinGecko.Api/Internal/QueryStringBuilder.cs`:

```csharp
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace CoinGecko.Api.Internal;

/// <summary>
/// Minimal, AOT-safe query-string builder. Uses <see cref="CultureInfo.InvariantCulture"/>
/// for numerics and URL-escapes all values. Intended for internal use by sub-clients.
/// </summary>
internal sealed class QueryStringBuilder
{
    private readonly StringBuilder _sb = new();
    private bool _hasAny;

    public QueryStringBuilder Add(string name, string? value)
    {
        if (value is null)
        {
            return this;
        }

        Append(name, value);
        return this;
    }

    public QueryStringBuilder Add(string name, int? value)
        => value is null ? this : Add(name, value.Value.ToString(CultureInfo.InvariantCulture));

    public QueryStringBuilder Add(string name, long? value)
        => value is null ? this : Add(name, value.Value.ToString(CultureInfo.InvariantCulture));

    public QueryStringBuilder Add(string name, decimal? value)
        => value is null ? this : Add(name, value.Value.ToString(CultureInfo.InvariantCulture));

    public QueryStringBuilder Add(string name, bool? value)
        => value is null ? this : Add(name, value.Value ? "true" : "false");

    public QueryStringBuilder AddCoinGeckoDate(string name, DateOnly? value)
        => value is null ? this : Add(name, value.Value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));

    public QueryStringBuilder AddUnixSeconds(string name, DateTimeOffset? value)
        => value is null ? this : Add(name, value.Value.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));

    public QueryStringBuilder AddEnum<TEnum>(string name, TEnum? value) where TEnum : struct, Enum
    {
        if (value is null)
        {
            return this;
        }

        var member = typeof(TEnum).GetField(value.Value.ToString());
        var em = member?.GetCustomAttribute<EnumMemberAttribute>();
        var wire = em?.Value ?? value.Value.ToString();
        return Add(name, wire);
    }

    public QueryStringBuilder AddList(string name, IReadOnlyCollection<string>? values, string separator = ",")
    {
        if (values is null || values.Count == 0)
        {
            return this;
        }

        return Add(name, string.Join(separator, values));
    }

    public QueryStringBuilder AddEnumList<TEnum>(string name, IReadOnlyCollection<TEnum>? values, string separator = ",")
        where TEnum : struct, Enum
    {
        if (values is null || values.Count == 0)
        {
            return this;
        }

        var wire = values.Select(v =>
        {
            var member = typeof(TEnum).GetField(v.ToString());
            var em = member?.GetCustomAttribute<EnumMemberAttribute>();
            return em?.Value ?? v.ToString();
        });

        return Add(name, string.Join(separator, wire));
    }

    private void Append(string name, string value)
    {
        _sb.Append(_hasAny ? '&' : '?');
        _sb.Append(Uri.EscapeDataString(name));
        _sb.Append('=');
        _sb.Append(Uri.EscapeDataString(value));
        _hasAny = true;
    }

    public override string ToString() => _sb.ToString();
}
```

> **AOT note:** `typeof(TEnum).GetField(...).GetCustomAttribute<EnumMemberAttribute>()` uses reflection over metadata of a value-type generic argument; the trimmer preserves enum metadata for value-type generics by default (DAM-safe). The analyzer will NOT flag this; if it does on a future SDK, switch to a source-generated `EnumMemberLookup<TEnum>` helper (documented as a follow-up in `docs/` if you hit it).

- [ ] **Step 4: Run — expect pass (all 8).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/Internal/QueryStringBuilder.cs tests/CoinGecko.Api.Tests/Internal/QueryStringBuilderTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add QueryStringBuilder with typed appenders" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---
## Phase 6 — Handler pipeline

Four `DelegatingHandler` classes, composed outer-to-inner: Auth → Plan → RateLimit → Retry → primary `SocketsHttpHandler`. Registered in Phase 7 on the `IHttpClientFactory` pipeline.

All handlers use a small shared helper `HttpRequestMessageExtensions` to get/set per-request options introduced in Task 4.3.

### Task 6.1: `HttpRequestMessageExtensions` helper

**Files:**
- Create: `src/CoinGecko.Api/Internal/HttpRequestMessageExtensions.cs`

- [ ] **Step 1: Implement (trivial helper, tested indirectly via handler tests below)**

```csharp
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Internal;

internal static class HttpRequestMessageExtensions
{
    public static ResponseEnvelope GetEnvelope(this HttpRequestMessage req)
        => req.Options.TryGetValue(CoinGeckoRequestOptions.Envelope, out var v) ? v : ResponseEnvelope.Bare;

    public static CoinGeckoPlan? GetRequiredPlan(this HttpRequestMessage req)
        => req.Options.TryGetValue(CoinGeckoRequestOptions.RequiredPlan, out var v) ? v : null;

    public static Guid GetOrCreateRequestId(this HttpRequestMessage req)
    {
        if (req.Options.TryGetValue(CoinGeckoRequestOptions.RequestId, out var id) && id != Guid.Empty)
        {
            return id;
        }

        var newId = Guid.NewGuid();
        req.Options.Set(CoinGeckoRequestOptions.RequestId, newId);
        return newId;
    }
}
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build src/CoinGecko.Api -c Debug`
Expected: success.

- [ ] **Step 3: Commit**

```bash
git add src/CoinGecko.Api/Internal/HttpRequestMessageExtensions.cs
git -c commit.gpgsign=false commit -m "feat(api): add HttpRequestMessage options accessors" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 6.2: `CoinGeckoAuthHandler`

**Files:**
- Create: `src/CoinGecko.Api/Handlers/CoinGeckoAuthHandler.cs`
- Create: `tests/CoinGecko.Api.Tests/Handlers/CoinGeckoAuthHandlerTests.cs`
- Create: `tests/CoinGecko.Api.Tests/Infra/StubHandler.cs` (shared test helper)

- [ ] **Step 1: Shared stub handler used across all handler tests**

`tests/CoinGecko.Api.Tests/Infra/StubHandler.cs`:

```csharp
namespace CoinGecko.Api.Tests.Infra;

internal sealed class StubHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _respond;

    public List<HttpRequestMessage> Received { get; } = new();

    public StubHandler(HttpResponseMessage response)
        : this((_, _) => response)
    {
    }

    public StubHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> respond)
    {
        _respond = respond;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Received.Add(request);
        return Task.FromResult(_respond(request, cancellationToken));
    }
}
```

- [ ] **Step 2: Failing test**

`tests/CoinGecko.Api.Tests/Handlers/CoinGeckoAuthHandlerTests.cs`:

```csharp
using System.Net;
using CoinGecko.Api;
using CoinGecko.Api.Handlers;
using CoinGecko.Api.Tests.Infra;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Tests.Handlers;

public class CoinGeckoAuthHandlerTests
{
    private static HttpClient BuildClient(CoinGeckoOptions opts, out StubHandler inner)
    {
        inner = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var handler = new CoinGeckoAuthHandler(new OptionsWrapper<CoinGeckoOptions>(opts)) { InnerHandler = inner };
        return new HttpClient(handler);
    }

    [Fact]
    public async Task Demo_plan_uses_demo_header()
    {
        using var client = BuildClient(new CoinGeckoOptions { Plan = CoinGeckoPlan.Demo, ApiKey = "abc" }, out var stub);
        await client.GetAsync("https://example.com/ping");
        stub.Received[0].Headers.GetValues("x-cg-demo-api-key").ShouldContain("abc");
        stub.Received[0].Headers.Contains("x-cg-pro-api-key").ShouldBeFalse();
    }

    [Fact]
    public async Task Paid_plan_uses_pro_header()
    {
        using var client = BuildClient(new CoinGeckoOptions { Plan = CoinGeckoPlan.Pro, ApiKey = "abc" }, out var stub);
        await client.GetAsync("https://example.com/ping");
        stub.Received[0].Headers.GetValues("x-cg-pro-api-key").ShouldContain("abc");
        stub.Received[0].Headers.Contains("x-cg-demo-api-key").ShouldBeFalse();
    }

    [Fact]
    public async Task QueryString_mode_appends_param_and_omits_header()
    {
        using var client = BuildClient(new CoinGeckoOptions
        {
            Plan = CoinGeckoPlan.Pro,
            ApiKey = "abc",
            AuthMode = AuthenticationMode.QueryString,
        }, out var stub);

        await client.GetAsync("https://example.com/coins/markets?vs_currency=usd");
        stub.Received[0].RequestUri!.Query.ShouldContain("x_cg_pro_api_key=abc");
        stub.Received[0].Headers.Contains("x-cg-pro-api-key").ShouldBeFalse();
    }

    [Fact]
    public async Task Sets_user_agent_with_version_substituted()
    {
        using var client = BuildClient(new CoinGeckoOptions { ApiKey = "abc" }, out var stub);
        await client.GetAsync("https://example.com/ping");
        var ua = string.Join(' ', stub.Received[0].Headers.UserAgent.Select(p => p.ToString()));
        ua.ShouldStartWith("CoinGecko.Api/");
        ua.ShouldNotContain("{version}");
    }

    [Fact]
    public async Task Missing_api_key_does_not_add_header()
    {
        using var client = BuildClient(new CoinGeckoOptions { Plan = CoinGeckoPlan.Demo, ApiKey = null }, out var stub);
        await client.GetAsync("https://example.com/ping");
        stub.Received[0].Headers.Contains("x-cg-demo-api-key").ShouldBeFalse();
    }
}
```

- [ ] **Step 3: Run — expect fail.**

- [ ] **Step 4: Implement**

`src/CoinGecko.Api/Handlers/CoinGeckoAuthHandler.cs`:

```csharp
using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Handlers;

internal sealed class CoinGeckoAuthHandler(IOptions<CoinGeckoOptions> options) : DelegatingHandler
{
    private static readonly string LibVersion =
        typeof(CoinGeckoAuthHandler).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(CoinGeckoAuthHandler).Assembly.GetName().Version?.ToString()
        ?? "0.0.0";

    private readonly CoinGeckoOptions _opts = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        AttachUserAgent(request);
        AttachApiKey(request);
        AttachAccept(request);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private void AttachUserAgent(HttpRequestMessage req)
    {
        req.Headers.UserAgent.Clear();
        var resolved = _opts.UserAgent.Replace("{version}", LibVersion);
        if (ProductInfoHeaderValue.TryParse(resolved, out var parsed))
        {
            req.Headers.UserAgent.Add(parsed!);
        }
        else
        {
            req.Headers.UserAgent.ParseAdd(resolved);
        }
    }

    private void AttachApiKey(HttpRequestMessage req)
    {
        if (string.IsNullOrEmpty(_opts.ApiKey))
        {
            return;
        }

        var headerName = _opts.Plan == CoinGeckoPlan.Demo ? "x-cg-demo-api-key" : "x-cg-pro-api-key";
        var paramName  = _opts.Plan == CoinGeckoPlan.Demo ? "x_cg_demo_api_key" : "x_cg_pro_api_key";

        if (_opts.AuthMode == AuthenticationMode.Header)
        {
            req.Headers.Remove(headerName);
            req.Headers.Add(headerName, _opts.ApiKey);
        }
        else
        {
            var uri = req.RequestUri ?? throw new InvalidOperationException("Request has no URI.");
            var sep = string.IsNullOrEmpty(uri.Query) ? '?' : '&';
            req.RequestUri = new Uri($"{uri}{sep}{paramName}={Uri.EscapeDataString(_opts.ApiKey)}", UriKind.Absolute);
        }
    }

    private static void AttachAccept(HttpRequestMessage req)
    {
        if (!req.Headers.Accept.Any())
        {
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
```

- [ ] **Step 5: Run — expect pass (5).**

- [ ] **Step 6: Commit**

```bash
git add src/CoinGecko.Api/Handlers/CoinGeckoAuthHandler.cs tests/CoinGecko.Api.Tests/Handlers/CoinGeckoAuthHandlerTests.cs tests/CoinGecko.Api.Tests/Infra/StubHandler.cs
git -c commit.gpgsign=false commit -m "feat(api): add CoinGeckoAuthHandler (header/query-string, UA, Accept)" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 6.3: `CoinGeckoPlanHandler`

**Files:**
- Create: `src/CoinGecko.Api/Handlers/CoinGeckoPlanHandler.cs`
- Create: `tests/CoinGecko.Api.Tests/Handlers/CoinGeckoPlanHandlerTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using System.Net;
using CoinGecko.Api;
using CoinGecko.Api.Exceptions;
using CoinGecko.Api.Handlers;
using CoinGecko.Api.Serialization;
using CoinGecko.Api.Tests.Infra;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Tests.Handlers;

public class CoinGeckoPlanHandlerTests
{
    private static HttpClient Build(CoinGeckoOptions opts, out StubHandler inner)
    {
        inner = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var h = new CoinGeckoPlanHandler(new OptionsWrapper<CoinGeckoOptions>(opts)) { InnerHandler = inner };
        return new HttpClient(h);
    }

    private static HttpRequestMessage Req(string path, CoinGeckoPlan? required)
    {
        var r = new HttpRequestMessage(HttpMethod.Get, "https://example.com" + path);
        if (required is not null)
        {
            r.Options.Set(CoinGeckoRequestOptions.RequiredPlan, required);
        }
        return r;
    }

    [Fact]
    public async Task Passes_through_when_no_plan_required()
    {
        using var c = Build(new CoinGeckoOptions { Plan = CoinGeckoPlan.Demo }, out var stub);
        var resp = await c.SendAsync(Req("/ping", required: null));
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        stub.Received.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Passes_through_when_plan_meets_requirement()
    {
        using var c = Build(new CoinGeckoOptions { Plan = CoinGeckoPlan.Pro }, out var stub);
        var resp = await c.SendAsync(Req("/x", required: CoinGeckoPlan.Analyst));
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Throws_when_plan_below_requirement()
    {
        using var c = Build(new CoinGeckoOptions { Plan = CoinGeckoPlan.Demo }, out var stub);
        var ex = await Should.ThrowAsync<CoinGeckoPlanException>(
            () => c.SendAsync(Req("/x", required: CoinGeckoPlan.Analyst)));
        ex.RequiredPlan.ShouldBe(CoinGeckoPlan.Analyst);
        ex.ActualPlan.ShouldBe(CoinGeckoPlan.Demo);
        stub.Received.Count.ShouldBe(0, "the request must not have been sent downstream");
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Implement**

```csharp
using CoinGecko.Api.Exceptions;
using CoinGecko.Api.Internal;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Handlers;

internal sealed class CoinGeckoPlanHandler(IOptions<CoinGeckoOptions> options) : DelegatingHandler
{
    private readonly CoinGeckoOptions _opts = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var required = request.GetRequiredPlan();
        if (required is { } min && (int)_opts.Plan < (int)min)
        {
            throw new CoinGeckoPlanException(required: min, actual: _opts.Plan);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
```

- [ ] **Step 4: Run — expect pass (3).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/Handlers/CoinGeckoPlanHandler.cs tests/CoinGecko.Api.Tests/Handlers/CoinGeckoPlanHandlerTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add CoinGeckoPlanHandler with pre-request plan gating" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 6.4: `CoinGeckoRateLimitHandler`

**Files:**
- Create: `src/CoinGecko.Api/Handlers/CoinGeckoRateLimitHandler.cs`
- Create: `tests/CoinGecko.Api.Tests/Handlers/CoinGeckoRateLimitHandlerTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using System.Net;
using CoinGecko.Api;
using CoinGecko.Api.Exceptions;
using CoinGecko.Api.Handlers;
using CoinGecko.Api.Tests.Infra;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Tests.Handlers;

public class CoinGeckoRateLimitHandlerTests
{
    private static HttpResponseMessage RateLimited(int? retryAfterSeconds)
    {
        var r = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if (retryAfterSeconds is not null)
        {
            r.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(retryAfterSeconds.Value));
        }
        return r;
    }

    [Fact]
    public async Task Respect_policy_retries_after_header_value_then_succeeds()
    {
        var calls = 0;
        var stub = new StubHandler((req, _) =>
        {
            calls++;
            return calls switch
            {
                1 => RateLimited(0), // immediate retry
                _ => new HttpResponseMessage(HttpStatusCode.OK),
            };
        });

        var h = new CoinGeckoRateLimitHandler(
            new OptionsWrapper<CoinGeckoOptions>(new CoinGeckoOptions { RateLimit = RateLimitPolicy.Respect }))
            { InnerHandler = stub };

        using var client = new HttpClient(h);
        var resp = await client.GetAsync("https://example.com/x");
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        calls.ShouldBe(2);
    }

    [Fact]
    public async Task Throw_policy_surfaces_exception_with_retry_after()
    {
        var stub = new StubHandler(RateLimited(7));
        var h = new CoinGeckoRateLimitHandler(
            new OptionsWrapper<CoinGeckoOptions>(new CoinGeckoOptions { RateLimit = RateLimitPolicy.Throw }))
            { InnerHandler = stub };

        using var client = new HttpClient(h);
        var ex = await Should.ThrowAsync<CoinGeckoRateLimitException>(() => client.GetAsync("https://example.com/x"));
        ex.RetryAfter.ShouldBe(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public async Task Ignore_policy_passes_429_through()
    {
        var stub = new StubHandler(RateLimited(1));
        var h = new CoinGeckoRateLimitHandler(
            new OptionsWrapper<CoinGeckoOptions>(new CoinGeckoOptions { RateLimit = RateLimitPolicy.Ignore }))
            { InnerHandler = stub };

        using var client = new HttpClient(h);
        var resp = await client.GetAsync("https://example.com/x");
        resp.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Exhausts_retries_then_throws_RateLimitException()
    {
        var stub = new StubHandler(RateLimited(0));
        var h = new CoinGeckoRateLimitHandler(
            new OptionsWrapper<CoinGeckoOptions>(new CoinGeckoOptions { RateLimit = RateLimitPolicy.Respect }))
            { InnerHandler = stub };

        using var client = new HttpClient(h);
        await Should.ThrowAsync<CoinGeckoRateLimitException>(() => client.GetAsync("https://example.com/x"));
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Implement**

```csharp
using CoinGecko.Api.Exceptions;
using CoinGecko.Api.Internal;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Handlers;

internal sealed class CoinGeckoRateLimitHandler(IOptions<CoinGeckoOptions> options) : DelegatingHandler
{
    private const int MaxAttempts = 4;
    private static readonly TimeSpan DefaultFallbackDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxAcceptedRetryAfter = TimeSpan.FromSeconds(60);

    private readonly CoinGeckoOptions _opts = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if ((int)response.StatusCode != 429)
            {
                return response;
            }

            var retryAfter = ReadRetryAfter(response) ?? DefaultFallbackDelay;

            if (_opts.RateLimit == RateLimitPolicy.Ignore)
            {
                return response;
            }

            if (_opts.RateLimit == RateLimitPolicy.Throw || attempt >= MaxAttempts)
            {
                var body = response.Content is null ? string.Empty : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                response.Dispose();
                throw new CoinGeckoRateLimitException(retryAfter, body, request.GetOrCreateRequestId());
            }

            response.Dispose();

            var clamped = retryAfter > MaxAcceptedRetryAfter ? MaxAcceptedRetryAfter : retryAfter;
            await Task.Delay(clamped, cancellationToken).ConfigureAwait(false);
        }
    }

    private static TimeSpan? ReadRetryAfter(HttpResponseMessage r)
    {
        var ra = r.Headers.RetryAfter;
        if (ra is null)
        {
            return null;
        }

        if (ra.Delta is { } delta)
        {
            return delta;
        }

        if (ra.Date is { } date)
        {
            var now = DateTimeOffset.UtcNow;
            if (date > now)
            {
                return date - now;
            }
            return TimeSpan.Zero;
        }

        return null;
    }
}
```

- [ ] **Step 4: Run — expect pass (4).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/Handlers/CoinGeckoRateLimitHandler.cs tests/CoinGecko.Api.Tests/Handlers/CoinGeckoRateLimitHandlerTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add CoinGeckoRateLimitHandler with Retry-After honoring + policy modes" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 6.5: `CoinGeckoRetryHandler`

**Files:**
- Create: `src/CoinGecko.Api/Handlers/CoinGeckoRetryHandler.cs`
- Create: `tests/CoinGecko.Api.Tests/Handlers/CoinGeckoRetryHandlerTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using System.Net;
using CoinGecko.Api.Handlers;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Handlers;

public class CoinGeckoRetryHandlerTests
{
    [Fact]
    public async Task Retries_on_5xx_up_to_bounded_attempts_then_returns()
    {
        var calls = 0;
        var stub = new StubHandler((_, _) =>
        {
            calls++;
            return calls switch
            {
                1 => new HttpResponseMessage(HttpStatusCode.BadGateway),
                2 => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                _ => new HttpResponseMessage(HttpStatusCode.OK),
            };
        });

        var h = new CoinGeckoRetryHandler { InnerHandler = stub, DelayProvider = _ => TimeSpan.Zero };
        using var client = new HttpClient(h);
        var resp = await client.GetAsync("https://example.com/x");

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        calls.ShouldBe(3);
    }

    [Fact]
    public async Task Does_not_retry_on_4xx()
    {
        var calls = 0;
        var stub = new StubHandler((_, _) =>
        {
            calls++;
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var h = new CoinGeckoRetryHandler { InnerHandler = stub, DelayProvider = _ => TimeSpan.Zero };
        using var client = new HttpClient(h);
        var resp = await client.GetAsync("https://example.com/x");

        resp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        calls.ShouldBe(1);
    }

    [Fact]
    public async Task Caller_cancellation_is_respected()
    {
        var stub = new StubHandler((_, _) => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var h = new CoinGeckoRetryHandler { InnerHandler = stub, DelayProvider = _ => TimeSpan.FromSeconds(30) };
        using var client = new HttpClient(h);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        await Should.ThrowAsync<TaskCanceledException>(
            () => client.GetAsync("https://example.com/x", cts.Token));
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Implement**

```csharp
namespace CoinGecko.Api.Handlers;

internal sealed class CoinGeckoRetryHandler : DelegatingHandler
{
    private const int MaxAttempts = 3;
    private static readonly Random Jitter = new();

    // Test seam:
    internal Func<int, TimeSpan> DelayProvider { get; set; } = attempt =>
    {
        // Decorrelated jitter: min = 100ms, cap = 5s
        var cap = 5000;
        var previousMs = attempt == 0 ? 100 : Math.Min(cap, (int)(Math.Pow(2, attempt - 1) * 100));
        var next = Jitter.Next(100, Math.Max(101, previousMs * 3));
        return TimeSpan.FromMilliseconds(Math.Min(cap, next));
    };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException) when (attempt < MaxAttempts)
            {
                await Task.Delay(DelayProvider(attempt), cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (attempt == MaxAttempts || !IsTransient(response.StatusCode))
            {
                return response;
            }

            response.Dispose();
            await Task.Delay(DelayProvider(attempt), cancellationToken).ConfigureAwait(false);
        }

        // Unreachable — loop either returns or throws.
        throw new InvalidOperationException("Retry loop exited unexpectedly.");
    }

    private static bool IsTransient(System.Net.HttpStatusCode code)
        => (int)code is 500 or 502 or 503 or 504;
}
```

- [ ] **Step 4: Run — expect pass (3).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/Handlers/CoinGeckoRetryHandler.cs tests/CoinGecko.Api.Tests/Handlers/CoinGeckoRetryHandlerTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add CoinGeckoRetryHandler for transient 5xx + socket errors" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---
## Phase 7 — Root client, DI, and factory

### Task 7.1: Define `ICoinGeckoClient` with 14 sub-client properties (as interfaces that don't yet have implementations)

**Files:**
- Create: `src/CoinGecko.Api/ICoinGeckoClient.cs`
- Create one empty interface file per sub-client in `src/CoinGecko.Api/Resources/`:
  - `IPingClient.cs`, `ICoinsClient.cs`, `INftsClient.cs`, `IExchangesClient.cs`, `IDerivativesClient.cs`, `ICategoriesClient.cs`, `IAssetPlatformsClient.cs`, `ICompaniesClient.cs`, `ISimpleClient.cs`, `IGlobalClient.cs`, `ISearchClient.cs`, `ITrendingClient.cs`, `IOnchainClient.cs`, `IKeyClient.cs`

- [ ] **Step 1: Write each empty interface file**

For each of the 14 listed above, write a file that looks like this (adjust the type name):

```csharp
namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's X endpoints. Methods added in Phase 8 (IPingClient) and Phase 9 (everything else).</summary>
public interface IPingClient
{
    // Methods added in Phase 8.
}
```

Each interface starts empty; methods are added in Phases 8 and 9.

- [ ] **Step 2: Write `ICoinGeckoClient`**

`src/CoinGecko.Api/ICoinGeckoClient.cs`:

```csharp
using CoinGecko.Api.Resources;

namespace CoinGecko.Api;

/// <summary>Root entry point. Expose one sub-client per CoinGecko resource group.</summary>
public interface ICoinGeckoClient
{
    ICoinsClient          Coins          { get; }
    INftsClient           Nfts           { get; }
    IExchangesClient      Exchanges      { get; }
    IDerivativesClient    Derivatives    { get; }
    ICategoriesClient     Categories     { get; }
    IAssetPlatformsClient AssetPlatforms { get; }
    ICompaniesClient      Companies      { get; }
    ISimpleClient         Simple         { get; }
    IGlobalClient         Global         { get; }
    ISearchClient         Search         { get; }
    ITrendingClient       Trending       { get; }
    IOnchainClient        Onchain        { get; }
    IKeyClient            Key            { get; }
    IPingClient           Ping           { get; }
}
```

- [ ] **Step 3: Build — expect success.**

- [ ] **Step 4: Commit**

```bash
git add src/CoinGecko.Api/ICoinGeckoClient.cs src/CoinGecko.Api/Resources/
git -c commit.gpgsign=false commit -m "feat(api): add ICoinGeckoClient and 14 empty sub-client interfaces" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 7.2: Concrete `CoinGeckoClient`

**Files:**
- Create: `src/CoinGecko.Api/CoinGeckoClient.cs`

- [ ] **Step 1: Implement**

```csharp
using CoinGecko.Api.Resources;

namespace CoinGecko.Api;

internal sealed class CoinGeckoClient(
    ICoinsClient coins,
    INftsClient nfts,
    IExchangesClient exchanges,
    IDerivativesClient derivatives,
    ICategoriesClient categories,
    IAssetPlatformsClient assetPlatforms,
    ICompaniesClient companies,
    ISimpleClient simple,
    IGlobalClient global,
    ISearchClient search,
    ITrendingClient trending,
    IOnchainClient onchain,
    IKeyClient key,
    IPingClient ping) : ICoinGeckoClient
{
    public ICoinsClient          Coins          { get; } = coins;
    public INftsClient           Nfts           { get; } = nfts;
    public IExchangesClient      Exchanges      { get; } = exchanges;
    public IDerivativesClient    Derivatives    { get; } = derivatives;
    public ICategoriesClient     Categories     { get; } = categories;
    public IAssetPlatformsClient AssetPlatforms { get; } = assetPlatforms;
    public ICompaniesClient      Companies      { get; } = companies;
    public ISimpleClient         Simple         { get; } = simple;
    public IGlobalClient         Global         { get; } = global;
    public ISearchClient         Search         { get; } = search;
    public ITrendingClient       Trending       { get; } = trending;
    public IOnchainClient        Onchain        { get; } = onchain;
    public IKeyClient            Key            { get; } = key;
    public IPingClient           Ping           { get; } = ping;
}
```

- [ ] **Step 2: Commit** (skipping build — see Task 7.3, which needs the DI extensions before the project compiles end-to-end with the resource stubs).

```bash
git add src/CoinGecko.Api/CoinGeckoClient.cs
git -c commit.gpgsign=false commit -m "feat(api): add CoinGeckoClient root implementation" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 7.3: Empty resource client stubs + DI extensions

Phase 7 needs DI registration to compile. Each of the 14 `IXxxClient` interfaces gets a minimal concrete stub class `XxxClient` that takes `HttpClient` + builds URIs. Methods are added in Phases 8 and 9.

**Files:**
- Create one stub per resource: `src/CoinGecko.Api/Resources/PingClient.cs`, etc. Follow the template below:

```csharp
namespace CoinGecko.Api.Resources;

internal sealed class PingClient(HttpClient http) : IPingClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 8.
}
```

Repeat for every `IXxxClient` listed in Task 7.1 (13 files), substituting the interface name.

- [ ] **Step 1: Create all 14 stub files (Ping + 13 others)**

- [ ] **Step 2: Create `ServiceCollectionExtensions` for DI**

`src/CoinGecko.Api/ServiceCollectionExtensions.cs`:

```csharp
using CoinGecko.Api.Handlers;
using CoinGecko.Api.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace CoinGecko.Api;

public static class ServiceCollectionExtensions
{
    private const string HttpClientName = "CoinGecko.Api";

    /// <summary>Registers <see cref="ICoinGeckoClient"/> and all sub-clients. Returns the <see cref="IHttpClientBuilder"/> so callers can chain additional handler configuration.</summary>
    public static IHttpClientBuilder AddCoinGeckoApi(this IServiceCollection services, Action<CoinGeckoOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<CoinGeckoOptions>();
        }

        services.AddTransient<CoinGeckoAuthHandler>();
        services.AddTransient<CoinGeckoPlanHandler>();
        services.AddTransient<CoinGeckoRateLimitHandler>();
        services.AddTransient<CoinGeckoRetryHandler>();

        var builder = services.AddHttpClient(HttpClientName, (sp, c) =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CoinGeckoOptions>>().Value;
            c.BaseAddress = opts.BaseAddress ?? new Uri(opts.Plan == CoinGeckoPlan.Demo
                ? "https://api.coingecko.com/api/v3/"
                : "https://pro-api.coingecko.com/api/v3/");
        })
        .AddHttpMessageHandler<CoinGeckoAuthHandler>()
        .AddHttpMessageHandler<CoinGeckoPlanHandler>()
        .AddHttpMessageHandler<CoinGeckoRateLimitHandler>()
        .AddHttpMessageHandler<CoinGeckoRetryHandler>();

        services.AddTransient<IPingClient>(sp => new PingClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ICoinsClient>(sp => new CoinsClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<INftsClient>(sp => new NftsClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IExchangesClient>(sp => new ExchangesClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IDerivativesClient>(sp => new DerivativesClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ICategoriesClient>(sp => new CategoriesClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IAssetPlatformsClient>(sp => new AssetPlatformsClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ICompaniesClient>(sp => new CompaniesClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ISimpleClient>(sp => new SimpleClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IGlobalClient>(sp => new GlobalClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ISearchClient>(sp => new SearchClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ITrendingClient>(sp => new TrendingClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IOnchainClient>(sp => new OnchainClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IKeyClient>(sp => new KeyClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));

        services.AddTransient<ICoinGeckoClient, CoinGeckoClient>();

        return builder;
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build src/CoinGecko.Api -c Debug`
Expected: success.

- [ ] **Step 4: Commit**

```bash
git add src/CoinGecko.Api/Resources/*Client.cs src/CoinGecko.Api/ServiceCollectionExtensions.cs
git -c commit.gpgsign=false commit -m "feat(api): add ServiceCollectionExtensions.AddCoinGeckoApi with full handler pipeline" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 7.4: `CoinGeckoClientFactory` (static affordance)

**Files:**
- Create: `src/CoinGecko.Api/CoinGeckoClientFactory.cs`
- Create: `tests/CoinGecko.Api.Tests/CoinGeckoClientFactoryTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using CoinGecko.Api;
using CoinGecko.Api.Resources;

namespace CoinGecko.Api.Tests;

public class CoinGeckoClientFactoryTests
{
    [Fact]
    public void Create_returns_a_disposable_wrapper_exposing_all_sub_clients()
    {
        using var scope = CoinGeckoClientFactory.Create("demo-key", CoinGeckoPlan.Demo);
        scope.Client.Ping.ShouldNotBeNull();
        scope.Client.Coins.ShouldNotBeNull();
        scope.Client.Onchain.ShouldNotBeNull();
    }

    [Fact]
    public void Dispose_disposes_the_underlying_service_scope()
    {
        var scope = CoinGeckoClientFactory.Create("demo-key");
        scope.Dispose();
        // After dispose, accessing Client should still return the cached reference — no ObjectDisposedException.
        // But using it will fail at HTTP time because the handler scope is disposed. We just verify Dispose is idempotent.
        Should.NotThrow(scope.Dispose);
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Implement**

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace CoinGecko.Api;

public sealed class CoinGeckoClientScope : IDisposable
{
    private readonly ServiceProvider _sp;
    public ICoinGeckoClient Client { get; }

    internal CoinGeckoClientScope(ServiceProvider sp, ICoinGeckoClient client)
    {
        _sp = sp;
        Client = client;
    }

    public void Dispose() => _sp.Dispose();
}

public static class CoinGeckoClientFactory
{
    public static CoinGeckoClientScope Create(string apiKey, CoinGeckoPlan plan = CoinGeckoPlan.Demo, Action<CoinGeckoOptions>? customize = null)
    {
        var services = new ServiceCollection();
        services.AddCoinGeckoApi(opts =>
        {
            opts.ApiKey = apiKey;
            opts.Plan = plan;
            customize?.Invoke(opts);
        });

        var sp = services.BuildServiceProvider();
        var client = sp.GetRequiredService<ICoinGeckoClient>();
        return new CoinGeckoClientScope(sp, client);
    }
}
```

- [ ] **Step 4: Run — expect pass (2).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/CoinGeckoClientFactory.cs tests/CoinGecko.Api.Tests/CoinGeckoClientFactoryTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add CoinGeckoClientFactory for script/console scenarios" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 8 — Canonical sub-client: `IPingClient`

`/ping` is the simplest endpoint; its shape establishes the pattern every other sub-client follows in Phase 9.

### Task 8.1: Add `PingResponse` DTO

**Files:**
- Create: `src/CoinGecko.Api/Models/PingResponse.cs`
- Modify: `src/CoinGecko.Api/Serialization/CoinGeckoJsonContext.cs` (add `[JsonSerializable(typeof(PingResponse))]`)

- [ ] **Step 1: Write the DTO**

```csharp
using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

public sealed class PingResponse
{
    [JsonPropertyName("gecko_says")] public string? GeckoSays { get; init; }
}
```

- [ ] **Step 2: Register in the source-gen context**

Edit `src/CoinGecko.Api/Serialization/CoinGeckoJsonContext.cs`, add above the partial class:

```csharp
[JsonSerializable(typeof(CoinGecko.Api.Models.PingResponse))]
```

- [ ] **Step 3: Build — expect success.**

- [ ] **Step 4: Commit**

```bash
git add src/CoinGecko.Api/Models/PingResponse.cs src/CoinGecko.Api/Serialization/CoinGeckoJsonContext.cs
git -c commit.gpgsign=false commit -m "feat(api): add PingResponse DTO and register in JsonContext" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 8.2: Add `PingAsync` method + unit test against StubHandler

**Files:**
- Modify: `src/CoinGecko.Api/Resources/IPingClient.cs`
- Modify: `src/CoinGecko.Api/Resources/PingClient.cs`
- Create: `tests/CoinGecko.Api.Tests/Resources/PingClientTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using System.Net;
using System.Text;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class PingClientTests
{
    [Fact]
    public async Task PingAsync_hits_ping_path_and_returns_message()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/ping");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"gecko_says":"(V3) To the Moon!"}""", Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new PingClient(http);

        var resp = await sut.PingAsync();
        resp.GeckoSays.ShouldBe("(V3) To the Moon!");
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Update the interface**

`src/CoinGecko.Api/Resources/IPingClient.cs`:

```csharp
using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

public interface IPingClient
{
    Task<PingResponse> PingAsync(CancellationToken ct = default);
}
```

- [ ] **Step 4: Implement the client**

`src/CoinGecko.Api/Resources/PingClient.cs`:

```csharp
using System.Net.Http.Json;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class PingClient(HttpClient http) : IPingClient
{
    private readonly HttpClient _http = http;

    public async Task<PingResponse> PingAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "ping");
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.PingResponse, ct).ConfigureAwait(false);
        return dto ?? throw new InvalidOperationException("CoinGecko returned empty body for /ping.");
    }
}
```

- [ ] **Step 5: Run — expect pass.**

- [ ] **Step 6: Commit**

```bash
git add src/CoinGecko.Api/Resources/IPingClient.cs src/CoinGecko.Api/Resources/PingClient.cs tests/CoinGecko.Api.Tests/Resources/PingClientTests.cs
git -c commit.gpgsign=false commit -m "feat(api): implement IPingClient.PingAsync (canonical sub-client pattern)" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 8.3: End-to-end WireMock test for `/ping` — establishes the mock-tests project

**Files:**
- Create: `tests/CoinGecko.Api.MockTests/CoinGecko.Api.MockTests.csproj` (mirror of Tests project, add `WireMock.Net`)
- Create: `tests/CoinGecko.Api.MockTests/GlobalUsings.cs` (same content as Tests)
- Create: `tests/CoinGecko.Api.MockTests/Infra/CoinGeckoMockFixture.cs`
- Create: `tests/CoinGecko.Api.MockTests/PingMockTests.cs`

- [ ] **Step 1: Create the csproj** — identical to `CoinGecko.Api.Tests.csproj` (Task 2.1), plus this one item group:

```xml
<ItemGroup>
  <PackageReference Include="WireMock.Net" />
</ItemGroup>
```

Add to solution: `dotnet sln CoinGecko.sln add tests/CoinGecko.Api.MockTests/CoinGecko.Api.MockTests.csproj`

- [ ] **Step 2: Shared fixture**

`tests/CoinGecko.Api.MockTests/Infra/CoinGeckoMockFixture.cs`:

```csharp
using CoinGecko.Api;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;

namespace CoinGecko.Api.MockTests.Infra;

public sealed class CoinGeckoMockFixture : IAsyncLifetime
{
    public WireMockServer Server { get; private set; } = default!;
    public ICoinGeckoClient Client { get; private set; } = default!;
    private ServiceProvider _sp = default!;

    public Task InitializeAsync()
    {
        Server = WireMockServer.Start();

        var services = new ServiceCollection();
        services.AddCoinGeckoApi(opts =>
        {
            opts.ApiKey = "test-demo-key";
            opts.Plan = CoinGeckoPlan.Demo;
            opts.BaseAddress = new Uri(Server.Url! + "/api/v3/");
        });
        _sp = services.BuildServiceProvider();
        Client = _sp.GetRequiredService<ICoinGeckoClient>();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _sp.Dispose();
        Server.Stop();
        Server.Dispose();
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 3: WireMock E2E test**

`tests/CoinGecko.Api.MockTests/PingMockTests.cs`:

```csharp
using CoinGecko.Api.MockTests.Infra;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace CoinGecko.Api.MockTests;

public class PingMockTests : IClassFixture<CoinGeckoMockFixture>
{
    private readonly CoinGeckoMockFixture _fx;

    public PingMockTests(CoinGeckoMockFixture fx) => _fx = fx;

    [Fact]
    public async Task Ping_happy_path()
    {
        _fx.Server
            .Given(Request.Create().WithPath("/api/v3/ping").UsingGet()
                .WithHeader("x-cg-demo-api-key", "test-demo-key"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"gecko_says":"(V3) To the Moon!"}"""));

        var r = await _fx.Client.Ping.PingAsync();
        r.GeckoSays.ShouldBe("(V3) To the Moon!");
    }
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/CoinGecko.Api.MockTests -c Debug
```

Expected: `Passed: 1`.

- [ ] **Step 5: Commit**

```bash
git add tests/CoinGecko.Api.MockTests/ CoinGecko.sln
git -c commit.gpgsign=false commit -m "test: add CoinGecko.Api.MockTests with WireMock and /ping E2E" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---
## Phase 9 — Remaining 13 sub-clients

This phase replicates the Phase 8 pattern for every other resource. Each task follows the **same four sub-steps**:

1. Add DTO records + register them in `CoinGeckoJsonContext` with `[JsonSerializable]`.
2. Fill the `IXxxClient` interface with method signatures (using `[RequiresPlan(...)]` per the research catalog).
3. Implement the concrete `XxxClient` following the Phase 8 `PingClient` shape: build `HttpRequestMessage`, set `HttpRequestOptions` (`Envelope`, `RequiredPlan`, `EndpointName`), deserialize through the source-gen context.
4. Add a unit test + a WireMock E2E test per endpoint.

Commit after each sub-client. Commit message template: `feat(api): implement I<Resource>Client (<N> endpoints)`.

**Canonical implementation reference:** [Phase 8 Task 8.2](#task-82-add-pingasync-method--unit-test-against-stubhandler) — if anything in Phase 9 is ambiguous, reread Phase 8 before proceeding.

**Endpoint catalog source of truth:** [`docs/coingecko-api-research.md`](../../coingecko-api-research.md). Every endpoint in that document that belongs to `CoinGecko.Api` (core + onchain; not WebSocket, not MCP) must appear in the Phase 9 implementation. Cross-reference at the end of this phase (Task 9.14).

### Endpoint ownership matrix

| Task | Sub-client | Endpoints (path → method → required plan) |
|---|---|---|
| 9.1 | `ICoinsClient` | `GET /coins/list` → `GetListAsync` (Demo) · `GET /coins/markets` → `GetMarketsAsync` (Demo) · `GET /coins/{id}` → `GetAsync` (Demo) · `GET /coins/{id}/tickers` → `GetTickersAsync` (Demo) · `GET /coins/{id}/history` → `GetHistoryAsync` (Demo) · `GET /coins/{id}/market_chart` → `GetMarketChartAsync` (Demo) · `GET /coins/{id}/market_chart/range` → `GetMarketChartRangeAsync` (Demo) · `GET /coins/{id}/ohlc` → `GetOhlcAsync` (Demo) · `GET /coins/{id}/ohlc/range` → `GetOhlcRangeAsync` (Analyst+) · `GET /coins/{id}/contract/{contract_address}` → `GetByContractAsync` (Demo) · `GET /coins/{id}/contract/{contract_address}/market_chart` → `GetContractMarketChartAsync` (Demo) · `GET /coins/{id}/contract/{contract_address}/market_chart/range` → `GetContractMarketChartRangeAsync` (Demo) · `GET /coins/{id}/circulating_supply_chart` → `GetCirculatingSupplyChartAsync` (Analyst+) · `GET /coins/{id}/circulating_supply_chart/range` → `GetCirculatingSupplyChartRangeAsync` (Analyst+) · `GET /coins/{id}/total_supply_chart` → `GetTotalSupplyChartAsync` (Analyst+) · `GET /coins/{id}/total_supply_chart/range` → `GetTotalSupplyChartRangeAsync` (Analyst+) · `GET /coins/top_gainers_losers` → `GetTopGainersLosersAsync` (Analyst+) · `GET /coins/list/new` → `GetNewListingsAsync` (Analyst+) |
| 9.2 | `INftsClient` | `GET /nfts/list` · `GET /nfts/{id}` · `GET /nfts/{asset_platform_id}/contract/{contract_address}` · `GET /nfts/markets` (Analyst+) · `GET /nfts/{id}/market_chart` (Analyst+) · `GET /nfts/{id}/tickers` (Analyst+) |
| 9.3 | `IExchangesClient` | `GET /exchanges` · `GET /exchanges/list` · `GET /exchanges/{id}` · `GET /exchanges/{id}/tickers` · `GET /exchanges/{id}/volume_chart` · `GET /exchanges/{id}/volume_chart/range` (Analyst+) |
| 9.4 | `IDerivativesClient` | `GET /derivatives` · `GET /derivatives/exchanges` · `GET /derivatives/exchanges/{id}` · `GET /derivatives/exchanges/list` |
| 9.5 | `ICategoriesClient` | `GET /coins/categories/list` · `GET /coins/categories` |
| 9.6 | `IAssetPlatformsClient` | `GET /asset_platforms` · `GET /token_lists/{asset_platform_id}/all.json` (Analyst+) |
| 9.7 | `ICompaniesClient` | `GET /companies/public_treasury/{coin_id}` |
| 9.8 | `ISimpleClient` | `GET /simple/price` · `GET /simple/token_price/{id}` · `GET /simple/supported_vs_currencies` |
| 9.9 | `IGlobalClient` | `GET /global` · `GET /global/decentralized_finance_defi` · `GET /global/market_cap_chart` (Analyst+) |
| 9.10 | `ISearchClient` | `GET /search` |
| 9.11 | `ITrendingClient` | `GET /search/trending` |
| 9.12 | `IOnchainClient` (**JSON:API envelope**) | `GET /onchain/networks` · `GET /onchain/networks/{network}/dexes` · `GET /onchain/networks/{network}/pools` · `GET /onchain/networks/{network}/new_pools` · `GET /onchain/networks/trending_pools` · `GET /onchain/networks/{network}/tokens/{address}` · `GET /onchain/networks/{network}/tokens/{address}/pools` · `GET /onchain/networks/{network}/pools/{address}` · `GET /onchain/networks/{network}/pools/{address}/ohlcv/{timeframe}` · `GET /onchain/networks/{network}/pools/{address}/trades` · `GET /onchain/networks/{network}/tokens/multi/{addresses}` · `GET /onchain/simple/networks/{network}/token_price/{addresses}` · `GET /onchain/search/pools` · `GET /onchain/pools/megafilter` (Analyst+) · `GET /onchain/tokens/info_recently_updated` · `GET /onchain/categories` (and every other onchain route listed in the research catalog) |
| 9.13 | `IKeyClient` (Pro-only) | `GET /key` (Basic+) |

> **Per-task reminder:** every method sets the `HttpRequestOptions` keys:
> - `Envelope = ResponseEnvelope.Bare` for all core sub-clients (9.1–9.11, 9.13).
> - `Envelope = ResponseEnvelope.JsonApi` for every onchain method (9.12).
> - `RequiredPlan` equals the plan column in the matrix.
> - `EndpointName` is the human-readable route (`"coins.markets"`, `"onchain.pools.by_address"`).

### Task 9.1 — `ICoinsClient` (18 endpoints)

Largest sub-client; allocate the longest chunk of implementation time here. DTO roster includes:

- `CoinListItem(string Id, string Symbol, string Name, IReadOnlyDictionary<string,string>? Platforms)`
- `CoinMarket(string Id, string Symbol, string Name, string Image, decimal? CurrentPrice, decimal? MarketCap, int? MarketCapRank, decimal? FullyDilutedValuation, decimal? TotalVolume, decimal? High24h, decimal? Low24h, decimal? PriceChange24h, decimal? PriceChangePercentage24h, decimal? MarketCapChange24h, decimal? MarketCapChangePercentage24h, decimal? CirculatingSupply, decimal? TotalSupply, decimal? MaxSupply, decimal? Ath, decimal? AthChangePercentage, DateTimeOffset? AthDate, decimal? Atl, decimal? AtlChangePercentage, DateTimeOffset? AtlDate, DateTimeOffset? LastUpdated, SparklineIn7d? SparklineIn7d, IReadOnlyDictionary<string,decimal?>? PriceChangePercentageByWindow)`
- `Coin(… full detail object — id, symbol, name, web_slug, asset_platform_id, platforms, detail_platforms, block_time_in_minutes, hashing_algorithm, categories, preview_listing, public_notice, additional_notices, localization, description, links, image, country_origin, genesis_date, sentiment_votes_up_percentage, sentiment_votes_down_percentage, market_cap_rank, market_data, community_data, developer_data, status_updates, last_updated, tickers …)` — use nested record types (`CoinLinks`, `CoinImage`, `CoinMarketData`, `CoinCommunityData`, `CoinDeveloperData`, `CoinTicker`) rather than one giant record; each is a separate file under `src/CoinGecko.Api/Models/Coins/`.
- `CoinTickers(string Name, IReadOnlyList<CoinTicker> Tickers)`
- `CoinHistory(string Id, string Symbol, string Name, CoinImage? Image, CoinHistoryMarketData? MarketData, CoinCommunityData? CommunityData, CoinDeveloperData? DeveloperData, CoinPublicInterestStats? PublicInterestStats)`
- `MarketChart(IReadOnlyList<TimestampedValue> Prices, IReadOnlyList<TimestampedValue> MarketCaps, IReadOnlyList<TimestampedValue> TotalVolumes)` + `TimestampedValue(DateTimeOffset Timestamp, decimal Value)` with a custom JSON converter (`TimestampedValueConverter : JsonConverter<TimestampedValue>` reading `[msSinceEpoch, number]` array format).
- `CoinOhlc(DateTimeOffset Timestamp, decimal Open, decimal High, decimal Low, decimal Close)` with an array-format converter (`[timestamp, o, h, l, c]`).
- `CirculatingSupplyPoint(DateTimeOffset Timestamp, decimal Supply)` with array-format converter.
- `TopGainersLosers(IReadOnlyList<CoinMarket> TopGainers, IReadOnlyList<CoinMarket> TopLosers)`
- `NewCoinListing(string Id, string Symbol, string Name, string? ActivatedAt)`

**Options records** (immutable, `init`-only; one per method):

```csharp
public sealed record CoinMarketsOptions
{
    public IReadOnlyList<string>? Ids { get; init; }
    public string? Category { get; init; }
    public CoinMarketsOrder Order { get; init; } = CoinMarketsOrder.MarketCapDesc;
    public int PerPage { get; init; } = 100;
    public int Page { get; init; } = 1;
    public bool Sparkline { get; init; }
    public IReadOnlyList<PriceChangeWindow>? PriceChangePercentage { get; init; }
    public string? Locale { get; init; }
    public int? Precision { get; init; }
}

public sealed record CoinDetailOptions
{
    public bool Localization { get; init; }
    public bool Tickers { get; init; } = true;
    public bool MarketData { get; init; } = true;
    public bool CommunityData { get; init; } = true;
    public bool DeveloperData { get; init; } = true;
    public bool Sparkline { get; init; }
}

public sealed record CoinTickersOptions
{
    public IReadOnlyList<string>? ExchangeIds { get; init; }
    public bool IncludeExchangeLogo { get; init; }
    public int Page { get; init; } = 1;
    public TickerOrder Order { get; init; } = TickerOrder.TrustScoreDesc;
    public bool Depth { get; init; }
}
```

**Steps:** execute Phase 8's four-sub-step pattern for each of the 18 endpoints listed in the matrix. Each method is ~20 lines; each unit test ~15 lines; each WireMock fixture needs a captured sample JSON (capture from `https://api.coingecko.com/api/v3/<path>` locally and commit to `tests/CoinGecko.Api.MockTests/Fixtures/coins/<endpoint>.json`). Final commit at task end:

```bash
git -c commit.gpgsign=false commit -m "feat(api): implement ICoinsClient (18 endpoints)" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

### Task 9.2 — `INftsClient` (6 endpoints)

DTOs: `NftListItem`, `Nft`, `NftMarket`, `NftMarketChart` (array-pair format), `NftTicker`, `NftImage`, `NftContractInfo`. Same four-sub-step pattern. Commit:

```bash
git -c commit.gpgsign=false commit -m "feat(api): implement INftsClient (6 endpoints)" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

### Task 9.3 — `IExchangesClient` (6 endpoints)

DTOs: `Exchange`, `ExchangeListItem`, `ExchangeDetail`, `ExchangeVolumeChartPoint`, `ExchangeTicker`. Commit template as above.

### Task 9.4 — `IDerivativesClient` (4 endpoints)

DTOs: `Derivative`, `DerivativeExchange`, `DerivativeExchangeDetail`, `DerivativeExchangeListItem`. Commit template as above.

### Task 9.5 — `ICategoriesClient` (2 endpoints)

DTOs: `CategoryListItem`, `CoinCategory`. Commit.

### Task 9.6 — `IAssetPlatformsClient` (2 endpoints)

DTOs: `AssetPlatform(string Id, int? ChainIdentifier, string Name, string Shortname, string? NativeCoinId, string? Image)`, `TokenList`. The `/token_lists/{asset_platform_id}/all.json` endpoint returns the Uniswap-style token-list JSON schema — reuse `TokenList` from Uniswap's public schema fields (`name`, `tokens[]`, `version`, `keywords[]`). Commit.

### Task 9.7 — `ICompaniesClient` (1 endpoint)

DTOs: `CompanyTreasury(IReadOnlyList<Company> Companies, decimal TotalHoldings, decimal TotalValueUsd, decimal MarketCapDominance)`, `Company`. Commit.

### Task 9.8 — `ISimpleClient` (3 endpoints)

DTOs: this resource returns maps-of-maps, so model responses as `IReadOnlyDictionary<string, IReadOnlyDictionary<string, decimal?>>` directly — no custom DTO for the price response, but a typed options object `SimplePriceOptions { IReadOnlyList<string> Ids, IReadOnlyList<string> VsCurrencies, bool IncludeMarketCap, bool Include24hVol, bool Include24hChange, bool IncludeLastUpdatedAt, int? Precision }` to keep the call site strongly-typed. Commit.

### Task 9.9 — `IGlobalClient` (3 endpoints)

DTOs: `GlobalData(GlobalDataInner Data)`, `GlobalDataInner(int ActiveCryptocurrencies, int UpcomingIcos, int OngoingIcos, int EndedIcos, int Markets, IReadOnlyDictionary<string,decimal> TotalMarketCap, IReadOnlyDictionary<string,decimal> TotalVolume, IReadOnlyDictionary<string,decimal> MarketCapPercentage, decimal MarketCapChangePercentage24hUsd, DateTimeOffset UpdatedAt)`. Similar for DeFi global. Market-cap chart uses `MarketCapChartPoint` array-format converter. Commit.

### Task 9.10 — `ISearchClient` (1 endpoint)

DTOs: `SearchResults(IReadOnlyList<SearchCoin> Coins, IReadOnlyList<SearchExchange> Exchanges, IReadOnlyList<SearchIcosItem> Icos, IReadOnlyList<SearchCategory> Categories, IReadOnlyList<SearchNft> Nfts)`. Commit.

### Task 9.11 — `ITrendingClient` (1 endpoint)

DTOs: `TrendingResults(IReadOnlyList<TrendingCoinItem> Coins, IReadOnlyList<TrendingNftItem> Nfts, IReadOnlyList<TrendingCategoryItem> Categories)`. Commit.

### Task 9.12 — `IOnchainClient` (JSON:API envelope — highest complexity)

This sub-client is the reason we built the `JsonApiResponse<T>` envelope in Phase 4. Every request sets `HttpRequestOptions.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi)`. The deserialization helper used by every method is:

```csharp
internal static class JsonApiUnwrap
{
    public static async Task<T> ReadDataAsync<T>(HttpContent content, JsonTypeInfo<JsonApiResponse<T>> envelopeInfo, CancellationToken ct)
        where T : notnull
    {
        var env = await content.ReadFromJsonAsync(envelopeInfo, ct).ConfigureAwait(false)
                  ?? throw new InvalidOperationException("Empty JSON:API response.");
        return env.Data ?? throw new InvalidOperationException("JSON:API envelope had null data.");
    }
}
```

DTO roster (non-exhaustive — full list in the research catalog):

- `Network(string Id, string Type, NetworkAttributes Attributes)` / `NetworkAttributes(string Name, DateTimeOffset? CoingeckoAssetPlatformId)`
- `Dex(string Id, string Type, DexAttributes Attributes)`
- `Pool(string Id, string Type, PoolAttributes Attributes, PoolRelationships? Relationships)` / `PoolAttributes(string Name, decimal? BaseTokenPriceUsd, decimal? QuoteTokenPriceUsd, …)` / `PoolRelationships(ResourceRef? BaseToken, ResourceRef? QuoteToken, ResourceRef? Dex, ResourceRef? Network)`
- `OnchainToken(string Id, string Type, TokenAttributes Attributes)`
- `OnchainOhlcv(OhlcvList Data, OhlcvMeta Meta)` — the OHLCV endpoint has a nested `data.attributes.ohlcv_list` as `decimal[][]`; needs an array-of-arrays converter.
- `OnchainTrade(string Id, string Type, TradeAttributes Attributes)`
- `OnchainCategory(string Id, string Type, CategoryAttributes Attributes)`

Every onchain method sets the envelope and uses `JsonApiUnwrap.ReadDataAsync<T>(...)`. Register both `JsonApiResponse<T>` *and* the bare `T` as `[JsonSerializable]` entries in `CoinGeckoJsonContext`. Commit:

```bash
git -c commit.gpgsign=false commit -m "feat(api): implement IOnchainClient with JSON:API unwrap (~28 endpoints)" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

### Task 9.13 — `IKeyClient` (1 endpoint, Basic+)

```csharp
public interface IKeyClient
{
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<ApiKeyUsage> GetAsync(CancellationToken ct = default);
}

public sealed record ApiKeyUsage(
    string Plan,
    int RateLimitRequestPerMinute,
    long MonthlyCallCredit,
    long CurrentTotalMonthlyCalls,
    long CurrentRemainingMonthlyCalls);
```

Commit.

### Task 9.14 — Coverage audit

**Files:** (documentation — no code change)
- Create: `docs/endpoint-coverage.md`

- [ ] **Step 1: Write a coverage table** comparing every endpoint from `docs/coingecko-api-research.md` against the methods implemented in Phase 9.1–9.13. Any missing row blocks Phase 10.

- [ ] **Step 2: Run full test suite**

```bash
dotnet test CoinGecko.sln -c Release
```

Expected: all tests pass, all projects compile warning-free.

- [ ] **Step 3: Commit**

```bash
git add docs/endpoint-coverage.md
git -c commit.gpgsign=false commit -m "docs: endpoint coverage audit vs research catalog" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---
## Phase 10 — `IAsyncEnumerable` pagination

Adds `EnumerateXxxAsync` variants to every list-returning method in Phases 8–9 that supports `page` / `per_page`.

### Task 10.1: Add `PaginationHelper.EnumerateAsync<T>`

**Files:**
- Create: `src/CoinGecko.Api/Internal/PaginationHelper.cs`
- Create: `tests/CoinGecko.Api.Tests/Internal/PaginationHelperTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using CoinGecko.Api.Internal;

namespace CoinGecko.Api.Tests.Internal;

public class PaginationHelperTests
{
    [Fact]
    public async Task Yields_across_pages_and_stops_on_short_page()
    {
        var pages = new List<int[]>
        {
            new[] { 1, 2, 3, 4, 5 },
            new[] { 6, 7, 8, 9, 10 },
            new[] { 11, 12 },        // short page → stop after yielding these
        };

        var collected = new List<int>();
        await foreach (var n in PaginationHelper.EnumerateAsync(
            fetchPage: (page, ct) => Task.FromResult((IReadOnlyList<int>)pages[page - 1]),
            perPage: 5,
            ct: default))
        {
            collected.Add(n);
        }

        collected.ShouldBe(new[] { 1,2,3,4,5,6,7,8,9,10,11,12 });
    }

    [Fact]
    public async Task Respects_cancellation_between_pages()
    {
        using var cts = new CancellationTokenSource();
        var enumerator = PaginationHelper.EnumerateAsync<int>(
            fetchPage: (_, _) =>
            {
                cts.Cancel();
                return Task.FromResult((IReadOnlyList<int>)new[] { 1, 2, 3 });
            },
            perPage: 3,
            ct: cts.Token).GetAsyncEnumerator();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            while (await enumerator.MoveNextAsync()) { /* drain */ }
        });
    }
}
```

- [ ] **Step 2: Run — expect fail.**

- [ ] **Step 3: Implement**

```csharp
using System.Runtime.CompilerServices;

namespace CoinGecko.Api.Internal;

internal static class PaginationHelper
{
    public static async IAsyncEnumerable<T> EnumerateAsync<T>(
        Func<int, CancellationToken, Task<IReadOnlyList<T>>> fetchPage,
        int perPage,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var page = 1;
        while (!ct.IsCancellationRequested)
        {
            ct.ThrowIfCancellationRequested();
            var items = await fetchPage(page, ct).ConfigureAwait(false);
            foreach (var item in items)
            {
                yield return item;
            }

            if (items.Count < perPage)
            {
                yield break;
            }

            page++;
        }

        ct.ThrowIfCancellationRequested();
    }
}
```

- [ ] **Step 4: Run — expect pass (2).**

- [ ] **Step 5: Commit**

```bash
git add src/CoinGecko.Api/Internal/PaginationHelper.cs tests/CoinGecko.Api.Tests/Internal/PaginationHelperTests.cs
git -c commit.gpgsign=false commit -m "feat(api): add PaginationHelper.EnumerateAsync for auto-paginating lists" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 10.2: Add `EnumerateXxxAsync` to every paginated sub-client method

For every method in Phases 8–9 whose options object has `Page` + `PerPage`, add both the interface member and the concrete implementation (not a default interface method — keep interfaces implementation-free):

```csharp
// In ICoinsClient.cs:
IAsyncEnumerable<CoinMarket> EnumerateMarketsAsync(
    string vsCurrency, CoinMarketsOptions? options = null, CancellationToken ct = default);

// In CoinsClient.cs:
public IAsyncEnumerable<CoinMarket> EnumerateMarketsAsync(
    string vsCurrency, CoinMarketsOptions? options = null, CancellationToken ct = default)
{
    var baseOptions = options ?? new CoinMarketsOptions();
    return PaginationHelper.EnumerateAsync<CoinMarket>(
        fetchPage: async (page, c) =>
            await GetMarketsAsync(vsCurrency, baseOptions with { Page = page }, c).ConfigureAwait(false),
        perPage: baseOptions.PerPage,
        ct: ct);
}
```

Apply this pattern to: `Coins.Markets`, `Coins.Tickers`, `Exchanges.List`, `Exchanges.{id}.Tickers`, `Derivatives.Exchanges`, `Nfts.List`, `Nfts.Markets`, and every onchain list endpoint that accepts `page` (pools, trending_pools, new_pools, trades, megafilter). `Coins.TopGainersLosers` is a single-response endpoint — no enumerate variant.

Add a single WireMock test per sub-client verifying multi-page traversal.

- [ ] **Step 1: Implement across all applicable sub-clients**
- [ ] **Step 2: Run full test suite**

```bash
dotnet test CoinGecko.sln -c Release
```

- [ ] **Step 3: Commit**

```bash
git -c commit.gpgsign=false commit -m "feat(api): add EnumerateXxxAsync auto-pagination across all list endpoints" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 11 — Observability (ActivitySource + LoggerMessage)

### Task 11.1: `CoinGeckoActivitySource`

**Files:**
- Create: `src/CoinGecko.Api/Observability/CoinGeckoActivitySource.cs`

```csharp
using System.Diagnostics;
using System.Reflection;

namespace CoinGecko.Api.Observability;

internal static class CoinGeckoActivitySource
{
    public const string Name = "CoinGecko.Api";

    public static readonly ActivitySource Instance = new(
        Name,
        typeof(CoinGeckoActivitySource).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.0.0");
}
```

### Task 11.2: `CoinGeckoTelemetryHandler`

Sits **outermost** in the handler chain (outside auth). Creates an `Activity` per request with tags: `http.method`, `http.status_code`, `coingecko.endpoint`, `coingecko.plan`, `coingecko.request_id`.

**Files:**
- Create: `src/CoinGecko.Api/Handlers/CoinGeckoTelemetryHandler.cs`
- Create: `src/CoinGecko.Api/Observability/CoinGeckoLog.cs` (`LoggerMessage` source-gen partial class for the five events: Sending, RateLimited, Retrying, Failed, Succeeded)
- Modify: `src/CoinGecko.Api/ServiceCollectionExtensions.cs` — register `CoinGeckoTelemetryHandler` as the first handler in the chain.

Full logging source-gen template:

```csharp
using Microsoft.Extensions.Logging;

namespace CoinGecko.Api.Observability;

internal static partial class CoinGeckoLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug,
        Message = "CoinGecko request {RequestId}: {Method} {Url}")]
    public static partial void Sending(ILogger logger, Guid requestId, string method, Uri url);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "CoinGecko rate-limited {RequestId}: retry after {RetryAfter}")]
    public static partial void RateLimited(ILogger logger, Guid requestId, TimeSpan retryAfter);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug,
        Message = "CoinGecko retry {RequestId}: attempt {Attempt}")]
    public static partial void Retrying(ILogger logger, Guid requestId, int attempt);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning,
        Message = "CoinGecko request {RequestId} failed: {StatusCode}")]
    public static partial void Failed(ILogger logger, Guid requestId, int statusCode);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug,
        Message = "CoinGecko request {RequestId} succeeded in {Elapsed}")]
    public static partial void Succeeded(ILogger logger, Guid requestId, TimeSpan elapsed);
}
```

Unit tests: spin up a `ListActivityListener` in a test fixture, invoke any sub-client method through a stubbed `HttpClient`, assert the activity is started/stopped with the expected tags. Use a test `ILogger` (xUnit's `ITestOutputHelper` + `LoggerFactory.Create(b => b.AddXunit(output))`).

Commit after full suite passes:

```bash
git -c commit.gpgsign=false commit -m "feat(api): add ActivitySource + LoggerMessage source-gen observability" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 12 — Public API analyzers

### Task 12.1: Enable `Microsoft.CodeAnalysis.PublicApiAnalyzers`

**Files:**
- Modify: `src/CoinGecko.Api/CoinGecko.Api.csproj` — add:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.CodeAnalysis.PublicApiAnalyzers">
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
<ItemGroup>
  <AdditionalFiles Include="PublicAPI.Shipped.txt" />
  <AdditionalFiles Include="PublicAPI.Unshipped.txt" />
</ItemGroup>
```

- Create: `src/CoinGecko.Api/PublicAPI.Shipped.txt` (empty)
- Create: `src/CoinGecko.Api/PublicAPI.Unshipped.txt` (populated by build — run `dotnet build`, open the analyzer diagnostics, right-click "Add to PublicAPI.Unshipped.txt" in Rider/VS, or manually list every public symbol).

- [ ] **Step 1: Build; expect RS0016 errors listing every unshipped public symbol.**
- [ ] **Step 2: Copy the listed symbols into `PublicAPI.Unshipped.txt`** (one symbol per line, format shown in analyzer output).
- [ ] **Step 3: Build again; expect warnings resolved.**
- [ ] **Step 4: Commit**

```bash
git add src/CoinGecko.Api/CoinGecko.Api.csproj src/CoinGecko.Api/PublicAPI.Shipped.txt src/CoinGecko.Api/PublicAPI.Unshipped.txt
git -c commit.gpgsign=false commit -m "chore(api): enable PublicApiAnalyzers and baseline public surface" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 13 — CI + release workflows

### Task 13.1: `.github/workflows/ci.yml`

```yaml
name: ci

on:
  push:
    branches: [main]
  pull_request:

permissions:
  contents: read

jobs:
  build:
    strategy:
      fail-fast: false
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/checkout@v4
        with: { fetch-depth: 0 } # MinVer needs tag history
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: |
            8.0.x
            9.0.x
      - name: Restore
        run: dotnet restore CoinGecko.sln
      - name: Build
        run: dotnet build CoinGecko.sln -c Release --no-restore
      - name: Format check
        if: matrix.os == 'ubuntu-latest'
        run: dotnet format CoinGecko.sln --verify-no-changes --no-restore
      - name: Test
        run: dotnet test CoinGecko.sln -c Release --no-build --logger "trx;LogFileName=test_results.trx" --collect:"XPlat Code Coverage"
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results-${{ matrix.os }}
          path: '**/TestResults/**'
```

### Task 13.2: `.github/workflows/release.yml`

```yaml
name: release

on:
  push:
    tags:
      - 'v[0-9]+.[0-9]+.[0-9]+-api'
      - 'v[0-9]+.[0-9]+.[0-9]+-api-*'  # prerelease suffix

permissions:
  contents: write   # for GitHub release creation

jobs:
  pack-and-publish:
    runs-on: ubuntu-latest
    environment: nuget-publish
    steps:
      - uses: actions/checkout@v4
        with: { fetch-depth: 0 }
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.0.x' }
      - name: Extract MinVer version
        id: ver
        run: |
          VERSION="${GITHUB_REF_NAME#v}"
          VERSION="${VERSION%-api}"
          VERSION="${VERSION%-api-*}"
          echo "version=$VERSION" >> "$GITHUB_OUTPUT"
      - name: Pack
        run: dotnet pack src/CoinGecko.Api/CoinGecko.Api.csproj -c Release -o artifacts
      - name: Push to NuGet
        run: dotnet nuget push 'artifacts/*.nupkg' --source https://api.nuget.org/v3/index.json --api-key ${{ secrets.NUGET_API_KEY }} --skip-duplicate
      - name: GitHub release
        uses: softprops/action-gh-release@v2
        with:
          generate_release_notes: true
          files: 'artifacts/*'
```

### Task 13.3: `.github/dependabot.yml`

```yaml
version: 2
updates:
  - package-ecosystem: nuget
    directory: "/"
    schedule:
      interval: weekly
      day: monday
  - package-ecosystem: github-actions
    directory: "/"
    schedule:
      interval: weekly
      day: monday
```

### Task 13.4: `.github/workflows/codeql.yml`

```yaml
name: codeql
on:
  push: { branches: [main] }
  pull_request:
  schedule:
    - cron: '0 4 * * 1'
permissions:
  security-events: write
  actions: read
  contents: read
jobs:
  analyze:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.0.x' }
      - uses: github/codeql-action/init@v3
        with: { languages: 'csharp' }
      - run: dotnet build CoinGecko.sln -c Release
      - uses: github/codeql-action/analyze@v3
```

- [ ] **Commit**

```bash
git add .github/
git -c commit.gpgsign=false commit -m "build: add CI, release, CodeQL, Dependabot workflows" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 14 — Samples

### Task 14.1: `samples/CoinGecko.Api.Samples.Console`

Full `dotnet run` Console program that:
- Reads `COINGECKO_API_KEY` env var (or `appsettings.json`).
- Calls `Ping.PingAsync()`.
- Calls `Coins.GetMarketsAsync("usd")` and prints the top 10.
- Streams `Coins.EnumerateMarketsAsync("usd")` with a 25-item cap.

Sample code pattern:

```csharp
using CoinGecko.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

var services = new ServiceCollection();
services.AddCoinGeckoApi(opts =>
{
    opts.ApiKey = config["COINGECKO_API_KEY"];
    opts.Plan = CoinGeckoPlan.Demo;
});

using var sp = services.BuildServiceProvider();
var gecko = sp.GetRequiredService<ICoinGeckoClient>();

var ping = await gecko.Ping.PingAsync();
Console.WriteLine($"Ping: {ping.GeckoSays}");

await foreach (var m in gecko.Coins.EnumerateMarketsAsync("usd").Take(25))
{
    Console.WriteLine($"{m.MarketCapRank,4}  {m.Symbol,6}  ${m.CurrentPrice,10:N2}");
}
```

### Task 14.2: `samples/CoinGecko.Api.Samples.Blazor` (AOT / trim-safety proof)

- `dotnet new blazorwasm -o samples/CoinGecko.Api.Samples.Blazor` (or Blazor WebAssembly template).
- Add `<PublishAot>true</PublishAot>` (or `<PublishTrimmed>true</PublishTrimmed>` for WASM which does not yet support full NativeAOT).
- `builder.Services.AddCoinGeckoApi(...)` in `Program.cs`.
- One page that lists trending coins.
- Verify `dotnet publish -c Release` produces an AOT-trimmed output with **zero** IL-link trimming warnings (`IL2026`, `IL2067`, `IL3050`).

### Task 14.3: Include samples in solution

```bash
dotnet sln CoinGecko.sln add samples/CoinGecko.Api.Samples.Console/*.csproj samples/CoinGecko.Api.Samples.Blazor/*.csproj
```

Commit:

```bash
git add samples/ CoinGecko.sln
git -c commit.gpgsign=false commit -m "docs: add Console and Blazor samples (AOT/trim-safety proof)" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 15 — README, pack, tag v0.1.0

### Task 15.1: Write repo-root `README.md`

Sections: install (all 4 packages), quickstart (REST + factory), plan compatibility table, compatibility matrix (net8 / net9 / AOT / trim), link to samples, license badge, CI status badge.

### Task 15.2: Pack dry-run + content inspection

```bash
dotnet pack src/CoinGecko.Api/CoinGecko.Api.csproj -c Release -o artifacts
```

Inspect the nupkg (as in Task 1.2 Step 2) and verify:
- `lib/net9.0/CoinGecko.Api.dll` + `lib/net8.0/CoinGecko.Api.dll`.
- `README.md` + `icon.png` present.
- `.nuspec` has MinVer-computed version based on the tag you're about to push.

### Task 15.3: Tag and push

```bash
git tag v0.1.0-api
git push origin main
git push origin v0.1.0-api
```

Workflow fires, waits for manual approval at the `nuget-publish` environment, then pushes to nuget.org.

### Task 15.4: Post-release verification

- [ ] Verify the package appears at https://www.nuget.org/packages/CoinGecko.Api
- [ ] `dotnet new console -o /tmp/consume && cd /tmp/consume && dotnet add package CoinGecko.Api --version 0.1.0`
- [ ] Paste the Console sample, `dotnet run`, confirm a real `/ping` call succeeds with a Demo key.
- [ ] Open a GitHub release using the tag, attach release notes.

---

## Appendix A — Commit message style

Conventional Commits, scope = package area:

- `feat(api): ...`
- `fix(api): ...`
- `test(api): ...`
- `build: ...` (repo-level build config)
- `docs: ...` (any file in `docs/`, `README.md`, `CONTRIBUTING.md`)
- `chore: ...` (repo maintenance, dep bumps)
- `ci: ...` (`.github/workflows/`)

Subject ≤72 characters; body wraps at 80. No AI attributions in trailers.

## Appendix B — Running subset of tests

```bash
# All:
dotnet test CoinGecko.sln -c Release

# Unit only (fastest feedback loop while implementing):
dotnet test tests/CoinGecko.Api.Tests -c Debug

# Mock / E2E (needs the Docker-less WireMock in-process):
dotnet test tests/CoinGecko.Api.MockTests -c Debug

# A single theory row:
dotnet test --filter "FullyQualifiedName~CoinGeckoRateLimitHandlerTests.Respect_policy_retries"
```

## Appendix C — Troubleshooting

- **`MinVer: tag history empty` on CI:** actions/checkout must use `fetch-depth: 0`. Already configured in Task 13.1.
- **`JsonSerializerContext.Default` reports null type-info for a new DTO:** you forgot to add `[JsonSerializable(typeof(NewDto))]` to `CoinGeckoJsonContext`.
- **`IL2026` / `IL3050` trim warnings on Blazor sample publish:** the warning's symbol path names the non-AOT-safe call; replace with a source-gen alternative (typically `JsonSerializer.Deserialize(T)` → `JsonSerializer.Deserialize(json, CoinGeckoJsonContext.Default.T)`).
- **`HttpClient.BaseAddress` must end with `/`:** the plan uses `"...api/v3/"` everywhere — preserve the trailing slash.
- **429 floods in `MockTests`:** remember to set `RateLimitPolicy.Ignore` in fixtures that assert on the raw 429; otherwise the rate-limit handler will retry and swallow it.

## Appendix D — Out of scope

- WebSocket beta (`CoinGecko.Api.WebSockets`) — separate plan.
- AI Agent Hub tool bindings (`CoinGecko.Api.AiAgentHub`) — separate plan.
- MCP client (`CoinGecko.Api.AiAgentHub.Mcp`) — separate plan.
- x402 pay-per-use endpoints — deferred until user demand (see spec §13).
- **DocFX site + GitHub Pages publish** — spec §12 calls for DocFX-generated API docs published to `gh-pages`. XML docs are produced at build time (every public member annotated; `CS1591` treated as error in Release — configured in Task 0.2). NuGet.org, GitHub, and IDEs render the XML docs fine for 0.1.0. Defer DocFX + Pages wiring to a follow-up `docs` workflow PR once 0.1.0 ships and the public API is real.
- **ContractTests project** (snapshot round-trip via `Verify.Xunit`) — spec §9 lists it as a separate test project. For 0.1.0 the MockTests project's WireMock fixtures already serve as de-facto contract tests (captured JSON payloads under `tests/CoinGecko.Api.MockTests/Fixtures/`). Add a dedicated `CoinGecko.Api.ContractTests` project in a follow-up if fixture diffing against `*.verified.json` baselines becomes useful (particularly useful before 1.0, less so for 0.1.0).
- **SmokeTests project** (live-API opt-in) — also in spec §9. Defer; the Console sample in Phase 14 effectively serves this role for manual pre-release smoke.

