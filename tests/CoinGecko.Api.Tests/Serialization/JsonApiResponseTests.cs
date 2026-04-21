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
