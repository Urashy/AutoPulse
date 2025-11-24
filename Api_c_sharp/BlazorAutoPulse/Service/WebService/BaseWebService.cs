using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BlazorAutoPulse.Service;

public abstract class BaseWebService<T> : IService<T> where T : class
{
    protected readonly HttpClient _httpClient;
    protected abstract string ApiEndpoint { get; }

    protected BaseWebService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://localhost:5086/api/{ApiEndpoint}/")
        };
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<T>>("GetAll") ?? Enumerable.Empty<T>();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<T>($"{id}");
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        // Convertit ton objet en JSON
        var json = JsonSerializer.Serialize(entity, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Console.WriteLine("JSON envoyé :");
        Console.WriteLine(json);

        // Envoie du JSON
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("Post", content);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public virtual async Task UpdateAsync(int id, T entity)
    {
        var response = await _httpClient.PutAsJsonAsync($"Put/{id}", entity);
        response.EnsureSuccessStatusCode();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"Delete/{id}");
        response.EnsureSuccessStatusCode();
    }
}