using System.Net;
using System.Text;

namespace BuscarEnderecos.API.Tests.Fakes
{
    /// <summary>
    /// Substitui a rede nos testes: devolve a resposta configurada e guarda as requisições recebidas.
    /// </summary>
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

        public void ThrowOnRequest(Exception exception) =>
            _responder = _ => throw exception;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_responder(request));
        }
    }
}
