using System.Net.Http.Json;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class ReinitialisationMdpWebService : BaseWebService<ReinitialisationMdp>, IReinitialiseMdp
{
    public ReinitialisationMdpWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "ReinitialisationMotDePasse";

    public async Task<bool> VerifCode(ReinitialisationMdp data)
    {
        var url = BuildUrl("VerifCode");

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(data)
        };
        
        var response = await SendWithCredentialsAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur PUT : {error}");
            return false;
        }
    }

    public async Task DeleteByNameAsync(string name)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, BuildUrl($"Delete/{name}"));
        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();
    }
}