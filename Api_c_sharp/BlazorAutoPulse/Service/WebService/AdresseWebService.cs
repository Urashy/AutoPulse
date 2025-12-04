using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService;

public class AdresseWebService: BaseWebService<Adresse>, IAdresseService
{
    public AdresseWebService(HttpClient httpClient) : base(httpClient)
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
}