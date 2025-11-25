using System.Net.Http.Json;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service;

public class AnnonceWebService : BaseWebService<Annonce> , IAnnonceService
{
    public AnnonceWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Annonce";
    public async Task<IEnumerable<Annonce>> GetByIdMiseEnAvant(int id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetByIdMiseEnAvant/{id}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Annonce>>();
    }

    public async Task<IEnumerable<Annonce>> GetFilteredAnnoncesAsync(ParametreRecherche searchParams)
    {
        var queryString = searchParams.ToQueryString();
        var url = string.IsNullOrEmpty(queryString) ? "GetFiltered" : $"GetFiltered?{queryString}";
    
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(url));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Annonce>>() 
               ?? Enumerable.Empty<Annonce>();
    }
}