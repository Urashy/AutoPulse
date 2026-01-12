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

        public async Task<bool> TelechargerFacturePdf(int idCommande)
        {
            try
            {
                Console.WriteLine($"[Facture] Téléchargement de la facture pour commande {idCommande}");

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    BuildUrl($"GetFacturePdf/{idCommande}")
                );

                var response = await SendWithCredentialsAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Facture] Erreur: {error}");
                    return false;
                }

                // Récupère le contenu en bytes
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"facture-commande-{idCommande}.pdf";

                Console.WriteLine($"[Facture] Fichier reçu: {fileBytes.Length} bytes");

                // Déclenche le téléchargement via JavaScript
                await _js.InvokeVoidAsync("downloadFile", fileName, "application/pdf", fileBytes);

                Console.WriteLine($"[Facture] Téléchargement réussi");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Facture] Exception: {ex.Message}");
                return false;
            }
        }
    }
}
