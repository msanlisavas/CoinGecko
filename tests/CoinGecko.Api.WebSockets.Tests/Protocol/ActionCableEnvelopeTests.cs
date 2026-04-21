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
        // Semantic check: the serialized value is a JSON string whose content,
        // when parsed, equals the inner JSON. We avoid asserting exact unicode escaping
        // since STJ may produce " or \" depending on the runtime version.
        var unwrapped = JsonSerializer.Deserialize<string>(s, Opts);
        unwrapped.ShouldBe("{\"channel\":\"CGSimplePrice\"}");
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
