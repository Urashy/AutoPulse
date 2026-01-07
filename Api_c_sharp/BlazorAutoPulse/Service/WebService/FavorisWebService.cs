using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class FavoriWebService : BaseWebService<FavoriDTO>, IFavorisService
    {
        public FavoriWebService(IHttpClientFactory factory) : base(factory)
        {
        }

        protected override string ApiEndpoint => "Favori";

        public async Task<IEnumerable<FavoriDTO>> GetMesFavoris(int IdCompte)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetByCompteId/{IdCompte}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<FavoriDTO>>()
                   ?? Enumerable.Empty<FavoriDTO>();
        }

        public async Task<bool> IsFavorite(int idCompte, int idAnnonce)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"IsFavorite?idCompte={idCompte}&idAnnonce={idAnnonce}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<bool> ToggleFavorite(int idCompte, int idAnnonce)
        {
            var isFav = await IsFavorite(idCompte, idAnnonce);

            if (isFav)
            {
                // Supprimer des favoris
                var request = new HttpRequestMessage(HttpMethod.Delete, BuildUrl($"Delete?idCompte={idCompte}&idAnnonce={idAnnonce}"));
                var response = await SendWithCredentialsAsync(request);
                response.EnsureSuccessStatusCode();
                return false;
            }
            else
            {
                // Ajouter aux favoris
                var favori = new FavoriDTO { IdCompte = idCompte, IdAnnonce = idAnnonce };
                await CreateAsync(favori);
                return true;
            }
        }
    }
}