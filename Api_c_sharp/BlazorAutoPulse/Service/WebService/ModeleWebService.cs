using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class ModeleWebService : BaseWebService<Modele>, IModeleService
    {
        public ModeleWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Modele";

        public async Task<IEnumerable<Modele>> FiltreModeleParMarque(int idMarque)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetAllByMarque/{idMarque}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<Modele>>() 
                   ?? Enumerable.Empty<Modele>();
        }
    }
}
