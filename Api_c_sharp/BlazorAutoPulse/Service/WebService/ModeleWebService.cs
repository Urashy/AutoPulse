using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;
using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.WebService
{
    public class ModeleWebService : BaseWebService<ModeleDTO>, IModeleService
    {
        public ModeleWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Modele";

        public async Task<IEnumerable<ModeleDTO>> FiltreModeleParMarque(int idMarque)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetAllByMarque/{idMarque}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ModeleDTO>>() 
                   ?? Enumerable.Empty<ModeleDTO>();
        }
    }
}
