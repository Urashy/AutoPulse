using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service
{
    public class AnnonceWebService : BaseWebService<Annonce>, IAnnonceService
    {
        public AnnonceWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Annonce";

        public async Task<IEnumerable<AnnonceDTO>> GetByCompteID(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"/GetAnnoncesByCompteId/{id}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<AnnonceDTO>>()
                   ?? Enumerable.Empty<AnnonceDTO>();
        }

        public async Task<IEnumerable<AnnonceDTO>> GetByIdMiseEnAvant(int id, int pageNumber = 1, int pageSize = 21)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                BuildUrl($"GetByIdMiseEnAvant/{id}?pageNumber={pageNumber}&pageSize={pageSize}")
            );
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<AnnonceDTO>>()
                   ?? Enumerable.Empty<AnnonceDTO>();
        }

        public async Task<IEnumerable<AnnonceDTO>> GetFilteredAnnoncesAsync(ParametreRecherche searchParams)
        {
            var queryString = searchParams.ToQueryString();
            var url = string.IsNullOrEmpty(queryString) ? "GetFiltered" : $"GetFiltered?{queryString}";

            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(url));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<AnnonceDTO>>()
                   ?? Enumerable.Empty<AnnonceDTO>();
        }

        public async Task<IEnumerable<AnnonceDTO>> GetAnnoncesFavoritesByCompteId(int compteId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                BuildUrl($"GetByCompteFavoris/{compteId}")
            );
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<AnnonceDTO>>()
                   ?? Enumerable.Empty<AnnonceDTO>();
        }
    }
}