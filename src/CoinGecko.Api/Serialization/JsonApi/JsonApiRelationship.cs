using System.Text.Json.Serialization;

namespace CoinGecko.Api.Serialization.JsonApi;

/// <summary>Represents a JSON:API relationship object containing a resource linkage reference.</summary>
public sealed class JsonApiRelationship
{
    /// <summary>Gets or sets the resource linkage reference for this relationship.</summary>
    [JsonPropertyName("data")] public JsonApiResourceRef? Data { get; set; }
}

/// <summary>Identifies a related resource by its <c>id</c> and <c>type</c>.</summary>
public sealed class JsonApiResourceRef
{
    /// <summary>Gets or sets the unique identifier of the referenced resource.</summary>
    [JsonPropertyName("id")] public string? Id { get; set; }

    /// <summary>Gets or sets the type of the referenced resource.</summary>
    [JsonPropertyName("type")] public string? Type { get; set; }
}
