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
