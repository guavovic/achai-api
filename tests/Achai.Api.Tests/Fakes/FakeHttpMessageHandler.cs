using System.Net;
using System.Text;

namespace Achai.Api.Tests.Fakes;

public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private Func<HttpRequestMessage, HttpResponseMessage> _responder =
        _ => throw new InvalidOperationException("Nenhuma resposta configurada para o handler falso.");

    public List<HttpRequestMessage> Requests { get; } = [];

    public void RespondWith(HttpStatusCode statusCode, string content, string mediaType = "application/json") =>
        _responder = _ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, mediaType)
        };

    public void RespondWith(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        _responder = responder;

    public void ThrowOnRequest(Exception exception) =>
        _responder = _ => throw exception;

    public HttpClient CreateClient(string baseAddress) =>
        new(this, disposeHandler: false) { BaseAddress = new Uri(baseAddress) };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        lock (Requests)
            Requests.Add(request);

        return Task.FromResult(_responder(request));
    }
}
