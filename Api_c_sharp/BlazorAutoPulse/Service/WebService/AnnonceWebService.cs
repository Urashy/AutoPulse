using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using AnnonceDetailDTO = AutoPulse.Shared.DTO.AnnonceDetailDTO;

namespace BlazorAutoPulse.Service
{
    public class AnnonceWebService : BaseWebService<AnnonceDTO>, IAnnonceService
    {
        public AnnonceWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Annonce";

        public async Task<IEnumerable<AnnonceDTO>> GetByCompteID(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetAnnoncesByCompteId/{id}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<AnnonceDTO>>()
                   ?? Enumerable.Empty<AnnonceDTO>();
        }
        
        public async Task<AnnonceDTO> CreateAnnonceAsync(AnnonceCreateDTO entity)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("Post"))
            {
                Content = JsonContent.Create(entity)
            };
        
            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AnnonceDTO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erreur Post : {error}");
                return await response.Content.ReadFromJsonAsync<AnnonceDTO>();
            }
        }

        public async Task UpdateAnnonceAsync(int id, AnnonceUpdateDTO entity)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"Put/{id}"))
            {
                Content = JsonContent.Create(entity)
            };
        
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
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

        public async Task<bool> EstMasquerAsync(int id)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get, BuildUrl($"EstMasquer/{id}")
            );

            var response = await SendWithCredentialsAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erreur Get : {error}");
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();

            bool resultat = bool.Parse(json);

            return resultat;
        }

        public async Task<AnnonceDetailDTO> GetAnnonceDetailById(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetById/{id.ToString()}"));
            var response = await SendWithCredentialsAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<AnnonceDetailDTO>();
        }
    }
}