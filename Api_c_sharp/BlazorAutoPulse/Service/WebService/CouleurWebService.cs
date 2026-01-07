using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class CouleurWebService: BaseWebService<CouleurDTO>, ICouleurService
{
    public CouleurWebService(IHttpClientFactory factory) : base(factory)
    {
    }

    protected override string ApiEndpoint => "Couleur";
    
    public async Task<List<CouleurDTO>> GetCouleursByVoitureId(int voitureId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetCouleursByVoitureID/{voitureId}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CouleurDTO>>();
    }
}