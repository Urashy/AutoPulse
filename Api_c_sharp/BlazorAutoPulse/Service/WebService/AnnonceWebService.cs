using System.Net.Http.Json;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service;

public class AnnonceWebService : BaseWebService<Annonce> , IAnnonceService
{
    protected override string ApiEndpoint => "Annonce";
    public async Task<IEnumerable<Annonce>> GetFilteredAnnoncesAsync(ParametreRecherche searchParams)
    {
        var queryString = searchParams.ToQueryString();
        var url = string.IsNullOrEmpty(queryString) ? "GetFiltered" : $"GetFiltered?{queryString}";

        return await _httpClient.GetFromJsonAsync<IEnumerable<Annonce>>(url)
               ?? Enumerable.Empty<Annonce>();
    }
}