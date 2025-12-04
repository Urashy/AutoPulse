using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class AvisWebService: BaseWebService<AvisListDTO>, IAvisService
    {
        public AvisWebService(HttpClient httpClient) : base(httpClient)
        {

        }
        protected override string ApiEndpoint => "Avis";

        public async Task<IEnumerable<AvisListDTO>> GetAvisByCompte(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"/GetAvisByCompteID/{id}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<AvisListDTO>>();
        }
    }
}
