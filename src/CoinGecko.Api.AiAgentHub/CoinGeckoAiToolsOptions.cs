namespace CoinGecko.Api.AiAgentHub;

/// <summary>Configuration for <see cref="CoinGeckoAiTools.Create"/>.</summary>
public sealed class CoinGeckoAiToolsOptions
{
    /// <summary>Which tool groups to include.</summary>
    public CoinGeckoToolSet Tools { get; set; } = CoinGeckoToolSet.All;

    /// <summary>Cap on the number of rows list-returning tools emit (prevents LLM context overflow).</summary>
    public int MaxResults { get; set; } = 25;

    /// <summary>Include <see cref="CoinGeckoToolSet.Onchain"/> tools even when they'd be redundant with other tools. Default <c>true</c>.</summary>
    public bool IncludeOnchainTools { get; set; } = true;

    /// <summary>Optional predicate that must return true for a tool name to be included. Runs after <see cref="Tools"/> filtering.</summary>
    public Func<string, bool>? ToolFilter { get; set; }
}
