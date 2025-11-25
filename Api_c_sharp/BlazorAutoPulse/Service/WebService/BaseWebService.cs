using System.Net.Http.Headers;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BlazorAutoPulse.Service;

public abstract class BaseWebService<T> : IService<T> where T : class
{
    protected readonly HttpClient _httpClient;
    protected abstract string ApiEndpoint { get; }

    protected BaseWebService()
    {
        _httpClient = new HttpClient
        {
            //BaseAddress = new Uri($"https://localhost:5086/api/{ApiEndpoint}/")
            BaseAddress = new Uri($"https://localhost:7295/api/{ApiEndpoint}/")
        };
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "GetAll");
        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<T>>();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{id}");
        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        var json = JsonSerializer.Serialize(entity);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "Post");
        request.Content = content;

        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public virtual async Task UpdateAsync(int id, T entity)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"Put/{id}")
        {
            Content = JsonContent.Create(entity)
        };

        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"Delete/{id}");
        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();
    }
    
    protected async Task<HttpResponseMessage> SendWithCredentialsAsync(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return await _httpClient.SendAsync(request);
    }
}