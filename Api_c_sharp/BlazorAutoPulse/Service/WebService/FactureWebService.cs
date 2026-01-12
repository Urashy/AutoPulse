using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class FactureWebService : BaseWebService<CommandeDTO>, IFactureService
    {
        private readonly IJSRuntime _js;

        public FactureWebService(IHttpClientFactory factory, IJSRuntime js) : base(factory)
        {
            _js = js;
        }
        protected override string ApiEndpoint => "Facture";

        
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
