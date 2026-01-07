using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BlazorAutoPulse.Service.WebService;

public class AdresseWebService: BaseWebService<AdresseDTO>, IAdresseService
{
    public AdresseWebService(IHttpClientFactory factory) : base(factory)
    {
    }
    protected override string ApiEndpoint => "Adresse";

    public async Task<IEnumerable<AdresseDTO>> GetAdresseByCompte(int id)
    {

        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetAdressesByCompteID/{id}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<AdresseDTO>>();
    }

    public async Task<AdresseDTO> CreateAdresseAsync(AdresseCreateDTO entity)
    {
        var json = JsonSerializer.Serialize(entity);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("Post"));
        request.Content = content;

        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AdresseDTO>();
    }

    public async Task UpdateAdresseAsync(int id, AdresseUpdateDTO entity)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"Put/{id}"))
        {
            Content = JsonContent.Create(entity)
        };
        
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
    }
}