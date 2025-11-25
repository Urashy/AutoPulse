using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class AnnonceDetailWebService : BaseWebService<AnnonceDetailDTO>, IAnnonceDetailService
    {
        protected override string ApiEndpoint => "Annonce";

        public override async Task<AnnonceDetailDTO> GetByIdAsync(int id)
        {
            // Appel à l'endpoint qui retourne les détails complets
            return await _httpClient.GetFromJsonAsync<AnnonceDetailDTO>($"GetById/{id}");
        }
    }
}