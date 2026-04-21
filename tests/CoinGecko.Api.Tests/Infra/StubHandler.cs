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
