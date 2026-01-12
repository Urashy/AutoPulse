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

        
        
        public async Task<Stream?> GetFactureStreamAsync(int commandeId)
        {
            try
            {
                // On appelle l'API sans forcer le téléchargement côté serveur (download=false)
                // pour récupérer le flux brut.
                var response = await _httpClient.GetAsync($"GetFacturePdf/{commandeId}?download=false");


                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStreamAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetFactureStreamAsync : {ex.Message}");
                return null;
            }
        }

    }
}
