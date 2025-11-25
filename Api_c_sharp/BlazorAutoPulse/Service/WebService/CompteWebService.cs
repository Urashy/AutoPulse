using System.Net.Http.Json;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service;

public class CompteWebService : BaseWebService<Compte>, ICompteService
{
    protected override string ApiEndpoint => "Compte";

    public async Task<Compte> GetMe()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "GetMe");
        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Compte>();
    }
}