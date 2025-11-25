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
            return await _httpClient.GetFromJsonAsync<IEnumerable<Modele>>($"GetAllByMarque/{idMarque}")
                   ?? Enumerable.Empty<Modele>();
        }
    }
}
