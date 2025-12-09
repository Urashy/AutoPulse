using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    // Hérédité de BaseWebService (présumé)
    public class PlainteWebService : BaseWebService<PlainteCreateDTO>, IPlainteService
    {
        public PlainteWebService(HttpClient httpClient) : base(httpClient) { }

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
                return await response.Content.ReadFromJsonAsync<PlainteDTO>();
            }
        }
    }
}