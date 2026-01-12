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
                Console.WriteLine($"[FactureWebService] Tentative de récupération facture {commandeId}");

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    BuildUrl($"GetFacturePdf/{commandeId}?download=false")
                );

                var response = await SendWithCredentialsAsync(request);

                Console.WriteLine($"[FactureWebService] Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[FactureWebService] Erreur HTTP {response.StatusCode}: {errorContent}");
                    return null;
                }

                // ✅ CORRECTION 3 : Vérifier le Content-Type
                var contentType = response.Content.Headers.ContentType?.MediaType;
                Console.WriteLine($"[FactureWebService] Content-Type: {contentType}");

                if (contentType != "application/pdf")
                {
                    Console.WriteLine($"[FactureWebService] ⚠️ Type de contenu inattendu: {contentType}");
                }

                var stream = await response.Content.ReadAsStreamAsync();

                Console.WriteLine($"[FactureWebService] ✅ Stream récupéré, longueur: {stream.Length} bytes");

                return stream;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"[FactureWebService] ❌ Erreur HTTP: {httpEx.StatusCode} - {httpEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FactureWebService] ❌ Erreur inattendue: {ex.GetType().Name}");
                Console.WriteLine($"[FactureWebService] Message: {ex.Message}");
                Console.WriteLine($"[FactureWebService] StackTrace: {ex.StackTrace}");
                return null;
            }
        }
    }
}