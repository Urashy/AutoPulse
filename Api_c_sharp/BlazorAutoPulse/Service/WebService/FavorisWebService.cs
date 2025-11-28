using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class FavorisWebService : BaseWebService<Favori>, IFavorisService
    {
        public FavorisWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Compte";

        public async Task<IEnumerable<Favori>> GetMesFavoris(int IdCompte)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GetMesFavoris"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<Favori>>();
        }
    }
}