using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    

    public class OffreWebService : BaseWebService<OffreDTO>, IOffreService
    {
        public OffreWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Offre";

        public async Task<IEnumerable<OffreDTO>> GetOffresByMessageAsync(int idMessage)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    BuildUrl($"GetByMessage/{idMessage}")
                );
                var response = await SendWithCredentialsAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<IEnumerable<OffreDTO>>()
                       ?? Enumerable.Empty<OffreDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetOffresByMessageAsync: {ex.Message}");
                return Enumerable.Empty<OffreDTO>();
            }
        }

        public async Task<bool> AccepterOffreAsync(int idOffre)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    BuildUrl($"AccepterOffre/{idOffre}")
                );
                var response = await SendWithCredentialsAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur AccepterOffreAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RefuserOffreAsync(int idOffre)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    BuildUrl($"RefuserOffre/{idOffre}")
                );
                var response = await SendWithCredentialsAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur RefuserOffreAsync: {ex.Message}");
                return false;
            }
        }
    
        
        public async Task CreateAsync(OffreCreateDTO offreDto)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    BuildUrl($"/")
                )
                {
                    Content = JsonContent.Create(offreDto)
                };
                var response = await SendWithCredentialsAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur CreateAsync: {ex.Message}");
            }
        }
    }
}