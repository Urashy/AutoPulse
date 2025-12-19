using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class PlainteWebService : BaseWebService<PlainteCreateDTO>, IPlainteService
    {
        public PlainteWebService(HttpClient httpClient) : base(httpClient) { }

        // Définit la base de l'URL pour ce contrôleur. 
        // Avec BuildUrl, cela donnera "Plainte/Action"
        protected override string ApiEndpoint => "Plainte";

        public async Task<PlainteDTO> CreatePlainteAsync(PlainteCreateDTO plainteDTO)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("Post"))
            {
                Content = JsonContent.Create(plainteDTO)
            };

            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PlainteDTO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erreur Post : {error}");
                // En cas d'erreur, on retourne null ou une nouvelle instance vide selon ta préférence
                return null;
            }
        }

        public async Task UpdatePlainteAsync(int id, PlainteUpdateDTO entity)
        {
            // Cible l'action [HttpPut("{id}")] du contrôleur
            // URL générée : Plainte/Put/{id}
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"Put/{id}"))
            {
                Content = JsonContent.Create(entity)
            };

            // Utilise SendWithCredentialsAsync pour inclure les cookies d'auth (admin)
            var response = await SendWithCredentialsAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erreur lors de la mise à jour de la plainte {id} : {error}");
            }
        }

        public async Task<IEnumerable<PlainteDTO>> GetAllPlaintesAsync()
        {
            // Cible l'action [HttpGet] GetAll du contrôleur
            // URL générée : Plainte/GetAll
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