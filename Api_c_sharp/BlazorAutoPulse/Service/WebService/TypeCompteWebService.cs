using BlazorAutoPulse.Model;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class TypeCompteWebService : BaseWebService<TypeCompte>
    {
        public TypeCompteWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "TypeCompte";

        public async Task<IEnumerable<TypeCompte>> GetTypeComptesPourChercher()
        {
            var response = await _httpClient.GetAsync($"{ApiEndpoint}/GetTypeComptesPourChercher");
            response.EnsureSuccessStatusCode();
            var typeComptes = await response.Content.ReadFromJsonAsync<IEnumerable<TypeCompte>>();
            return typeComptes ?? Enumerable.Empty<TypeCompte>();
        }
    }
}