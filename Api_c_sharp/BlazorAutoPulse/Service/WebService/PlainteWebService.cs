using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class PlainteWebService : BaseWebService<PlainteCreateDTO>, IPlainteService
    {
        public PlainteWebService(HttpClient httpClient) : base(httpClient) { }

        protected override string ApiEndpoint => "Plainte";

        // SUPPRIMÉ: CreatePlainteAsync car on utilise maintenant PostWithErrorHandlingAsync héritée
        // La méthode PostWithErrorHandlingAsync de BaseWebService gère déjà les erreurs

        public async Task UpdatePlainteAsync(int id, PlainteUpdateDTO entity)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"Put/{id}"))
            {
                Content = JsonContent.Create(entity)
            };

            var response = await SendWithCredentialsAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erreur lors de la mise à jour de la plainte {id} : {error}");
            }
        }

        public async Task<IEnumerable<PlainteDTO>> GetAllPlaintesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GetAll"));

            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<PlainteDTO>>()
                       ?? new List<PlainteDTO>();
            }

            Console.WriteLine($"Erreur lors de la récupération des plaintes : {response.StatusCode}");
            return new List<PlainteDTO>();
        }
    }
}