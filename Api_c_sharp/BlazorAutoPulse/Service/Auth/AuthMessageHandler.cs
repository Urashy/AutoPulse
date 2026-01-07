using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BlazorAutoPulse.Services.Auth;

public class AuthMessageHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private static Task<bool>? _refreshTask;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NavigationManager _navigation;

    public AuthMessageHandler(
        IHttpClientFactory httpClientFactory,
        NavigationManager navigation)
    {
        _httpClientFactory = httpClientFactory;
        _navigation = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        Console.WriteLine("🔒 401 détecté");

        // Lancer ou attendre le refresh
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_refreshTask == null)
            {
                _refreshTask = RefreshTokenAsync(cancellationToken);
            }
        }
        finally
        {
            RefreshLock.Release();
        }

        var refreshSuccess = await _refreshTask;

        if (!refreshSuccess)
        {
            Console.WriteLine("❌ Refresh échoué → logout");
            return response;
        }

        Console.WriteLine("✅ Refresh OK → retry");

        var retryRequest = CloneRequest(request);
        retryRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var refreshClient = _httpClientFactory.CreateClient("RefreshClient");

            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "Compte/Refresh");
            refreshRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await refreshClient.SendAsync(refreshRequest, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
        finally
        {
            _refreshTask = null;
        }
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (request.Content != null)
        {
            var ms = new MemoryStream();
            request.Content.CopyToAsync(ms).Wait();
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}