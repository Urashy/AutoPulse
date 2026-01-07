using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class CarteBancaireWebService: BaseWebService<CarteBancaireDTO>, ICarteBancaireService
    {
        public CarteBancaireWebService(IHttpClientFactory factory) : base(factory)
        {

        }
        protected override string ApiEndpoint => "CarteBancaire";

        public async Task<IEnumerable<CarteBancaireDTO>> GetCarteBancaireByCompte(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetCarteBancaireByCompteID/{id}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<CarteBancaireDTO>>();
        }
    }
}
