using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class JournalWebService : BaseWebService<JournalDTO>, IJournalService
    {
        public JournalWebService(IHttpClientFactory factory) : base(factory)
        {
        }

        protected override string ApiEndpoint => "Journal";

        public async Task<IEnumerable<JournalDTO>> GetFilteredAsync(RechercheJournalDTO rechercheDto)
        {
            var queryParams = new List<string>();

            if (rechercheDto.IdType > 0)
            {
                queryParams.Add($"IdType={rechercheDto.IdType}");
            }

            // Gestion des dates
            if (rechercheDto.DebutIntervalle.HasValue)
            {
                queryParams.Add($"DebutIntervalle={rechercheDto.DebutIntervalle.Value:yyyy-MM-dd}");
            }

            if (rechercheDto.FinIntervalle.HasValue)
            {
                queryParams.Add($"FinIntervalle={rechercheDto.FinIntervalle.Value:yyyy-MM-dd}");
            }

            // Gestion de l'ordre
            queryParams.Add($"Order={rechercheDto.Order}");

            var queryString = string.Join("&", queryParams);

            // L'action dans le contrôleur s'appelle "GetFilteredJournal"
            var url = string.IsNullOrEmpty(queryString)
                ? "GetFilteredJournal"
                : $"GetFilteredJournal?{queryString}";

            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(url));
            var response = await SendWithCredentialsAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<JournalDTO>>()
                   ?? Enumerable.Empty<JournalDTO>();
        }
    }
}