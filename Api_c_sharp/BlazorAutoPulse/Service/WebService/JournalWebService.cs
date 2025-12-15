using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class JournalWebService : BaseWebService<JournalDTO>, IJournalService
    {
        public JournalWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Journal";

        public async Task<IEnumerable<JournalDTO>> GetFilteredAsync(int? typeId, DateTime? dateDebut, DateTime? dateFin, int ordre)
        {
            var queryParams = new List<string>();

            if (typeId.HasValue)
                queryParams.Add($"typeId={typeId.Value}");

            if (dateDebut.HasValue)
                queryParams.Add($"dateDebut={dateDebut.Value:yyyy-MM-dd}");

            if (dateFin.HasValue)
                queryParams.Add($"dateFin={dateFin.Value:yyyy-MM-dd}");

            // Ajout du paramètre d'ordre (int)
            queryParams.Add($"ordre={ordre}");

            var queryString = string.Join("&", queryParams);
            var url = string.IsNullOrEmpty(queryString) ? "GetFiltered" : $"GetFiltered?{queryString}";

            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(url));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<JournalDTO>>() ?? Enumerable.Empty<JournalDTO>();


            return await Task.FromResult(Enumerable.Empty<JournalDTO>());
        }
    }
}