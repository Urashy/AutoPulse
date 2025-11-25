using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class AnnonceDetailWebService : BaseWebService<AnnonceDetailDTO>, IAnnonceDetailService
    {
        public AnnonceDetailWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Annonce";

        public override async Task<AnnonceDetailDTO> GetByIdAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetById/{id}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AnnonceDetailDTO>();
        }
    }
}