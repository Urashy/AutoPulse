using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class CommandeWebService : BaseWebService<CommandeDTO>, ICommandeService
    {
        public CommandeWebService(HttpClient httpClient) : base(httpClient)
        {
        }
        protected override string ApiEndpoint => "Commande";

        public async Task<IEnumerable<CommandeDTO>> GetCommandeByCompte(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"/GetCommandeByCompteID/{id}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<CommandeDTO>>();
        }
    }
}
