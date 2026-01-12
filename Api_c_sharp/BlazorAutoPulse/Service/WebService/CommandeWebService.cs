using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class CommandeWebService : BaseWebService<CommandeDTO>, ICommandeService
    {
        private readonly IJSRuntime _js;

        public CommandeWebService(IHttpClientFactory factory, IJSRuntime js) : base(factory)
        {
            _js = js;
        }
        protected override string ApiEndpoint => "Commande";

        public async Task<IEnumerable<CommandeDTO>> GetCommandeByCompte(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetCommandeByCompteID/{id}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<CommandeDTO>>();
        }

        public async Task<CommandeDetailDTO> GetCommandeDetailById(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetById/{id}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CommandeDetailDTO>();
        }
        public async Task UpdateCommandeAsync(int id, CommandeUpdateDTO entity)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"Put/{id}"))
            {
                Content = JsonContent.Create(entity)
            };

            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
        }
        public async Task<CommandeDTO> GetCommandeByIdConv(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetCommandeByConversationId/{id}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CommandeDTO>();
        }

    }
}
