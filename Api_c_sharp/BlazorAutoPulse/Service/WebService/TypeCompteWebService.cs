using BlazorAutoPulse.Model;
using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class TypeCompteWebService : BaseWebService<TypeCompteDTO>, ITypeCompteService
    {
        public TypeCompteWebService(IHttpClientFactory factory) : base(factory)
        {
        }

        protected override string ApiEndpoint => "TypeCompte";

        public async Task<IEnumerable<TypeCompteDTO>> GetTypeComptesPourChercher()
        {
            var response = await _httpClient.GetAsync($"{ApiEndpoint}/GetTypeComptesPourChercher");
            response.EnsureSuccessStatusCode();
            var typeComptes = await response.Content.ReadFromJsonAsync<IEnumerable<TypeCompteDTO>>();
            return typeComptes ?? Enumerable.Empty<TypeCompteDTO>();
        }
    }
}