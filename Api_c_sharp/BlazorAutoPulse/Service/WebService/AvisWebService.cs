using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class AvisWebService: BaseWebService<AvisListDTO>, IAvisService
    {
        public AvisWebService(IHttpClientFactory factory) : base(factory)
        {

        }
        protected override string ApiEndpoint => "Avis";

        public async Task<IEnumerable<AvisListDTO>> GetAvisByCompte(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetAvisByCompteID/{id}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<AvisListDTO>>();
        }

        public async Task<ServiceResult<AvisListDTO>> CreateAvis(AvisCreateDTO avis)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("Post"))
                {
                    Content = JsonContent.Create(avis)
                };

                var response = await SendWithCredentialsAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AvisListDTO>();
                    return ServiceResult<AvisListDTO>.SuccessResult(result);
                }

                if(response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var errorMessage = await response.Content.ReadFromJsonAsync<string>();
                    return ServiceResult<AvisListDTO>.ErrorResult(errorMessage ?? "Une erreur est survenue");
                }

                return ServiceResult<AvisListDTO>.ErrorResult($"Erreur lors de l'envoi de l'avis ({response.StatusCode})");
            }
            catch (Exception ex)
            {
                return ServiceResult<AvisListDTO>.ErrorResult(ex.Message);
            }
        }

    }
}
