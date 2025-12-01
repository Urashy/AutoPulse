using System.Net.Http.Json;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class CouleurWebService: BaseWebService<Couleur>, ICouleurService
{
    public CouleurWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Couleur";
    
    public async Task<List<Couleur>> GetCouleursByVoitureId(int voitureId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetCouleursByVoitureID/{voitureId}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Couleur>>();
    }
}