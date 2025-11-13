using System.Net.Http.Json;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service;

public class AnnonceWebService: IService<Annonce>
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5117/api/Annonce/")
    };

    public async Task<IEnumerable<Annonce>> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<Annonce>>("GetAll");
    }

    public async Task<Annonce> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Annonce>($"{id}");
    }

    public async Task<Annonce> CreateAsync(Annonce annonce)
    {
        var response = await _httpClient.PostAsJsonAsync("Post", annonce);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Annonce>();
    }

    public async Task UpdateAsync(int id, Annonce annonce)
    {
        var response = await _httpClient.PutAsJsonAsync($"Put/{id}", annonce);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"Delete/{id}");
        response.EnsureSuccessStatusCode();
    }
}