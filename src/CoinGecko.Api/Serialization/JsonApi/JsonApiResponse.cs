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
    /// <summary>Gets or sets the primary data payload.</summary>
    [JsonPropertyName("data")] public T? Data { get; set; }

    /// <summary>Gets or sets included side-loaded resources for relationship resolution.</summary>
    [JsonPropertyName("included")] public IReadOnlyList<JsonApiResource>? Included { get; set; }

    /// <summary>Gets or sets the top-level meta object.</summary>
    [JsonPropertyName("meta")] public JsonObject? Meta { get; set; }

    /// <summary>Gets or sets the top-level links object (pagination URIs).</summary>
    [JsonPropertyName("links")] public JsonApiLinks? Links { get; set; }
}

/// <summary>Top-level <c>links</c> object (pagination URIs).</summary>
public sealed class JsonApiLinks
{
    /// <summary>Gets or sets the canonical URL for the current resource.</summary>
    [JsonPropertyName("self")] public string? Self { get; set; }

    /// <summary>Gets or sets the URL for the first page of results.</summary>
    [JsonPropertyName("first")] public string? First { get; set; }

    /// <summary>Gets or sets the URL for the last page of results.</summary>
    [JsonPropertyName("last")] public string? Last { get; set; }

    /// <summary>Gets or sets the URL for the previous page of results.</summary>
    [JsonPropertyName("prev")] public string? Prev { get; set; }

    /// <summary>Gets or sets the URL for the next page of results.</summary>
    [JsonPropertyName("next")] public string? Next { get; set; }
}

/// <summary>Generic untyped resource (used for <c>included[]</c> cross-references).</summary>
public sealed class JsonApiResource
{
    /// <summary>Gets or sets the unique identifier for this resource.</summary>
    [JsonPropertyName("id")] public string? Id { get; set; }

    /// <summary>Gets or sets the resource type string.</summary>
    [JsonPropertyName("type")] public string? Type { get; set; }

    /// <summary>Gets or sets the resource attributes bag.</summary>
    [JsonPropertyName("attributes")] public JsonObject? Attributes { get; set; }

    /// <summary>Gets or sets the named relationships for this resource.</summary>
    [JsonPropertyName("relationships")] public Dictionary<string, JsonApiRelationship>? Relationships { get; set; }
}
