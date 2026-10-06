using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Achai.Client;

public static class AchaiClient
{
    public const string DefaultBaseUrl = "https://achai-api.onrender.com";

    public static AchaiApiClient Create(string baseUrl = DefaultBaseUrl, HttpClient? httpClient = null)
    {
        var adapter = httpClient is null
            ? new HttpClientRequestAdapter(new AnonymousAuthenticationProvider())
            : new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
        adapter.BaseUrl = baseUrl.TrimEnd('/');

        return new AchaiApiClient(adapter);
    }
}
