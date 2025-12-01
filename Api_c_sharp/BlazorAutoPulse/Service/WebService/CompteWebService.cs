using System.Net.Http.Json;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service;

public class CompteWebService : BaseWebService<Compte>, ICompteService
{
    public CompteWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Compte";

    public async Task<Compte> GetByNameAsync(string name)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetByString/{name}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Compte>();
    }

    public async Task<Compte> GetMe()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GetMe"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Compte>();
    }

    public async Task<bool> VerifUser(ChangementMdp changementMdp)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("VerifUser"))
        {
            Content = JsonContent.Create(changementMdp)
        };
        
        var response = await SendWithCredentialsAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur Post : {error}");
            return false;
        }
    }

    public async Task<bool> ChangementMdp(ChangementMdp changementMdp)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("ModifMdp"))
        {
            Content = JsonContent.Create(changementMdp)
        };
        
        var response = await SendWithCredentialsAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur Post : {error}");
            return false;
        }
    }
}