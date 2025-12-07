using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class BloquerWebService: BaseWebService<BloqueDTO>, IBloqueService
{
    public BloquerWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Bloque";
    
    public async Task<bool> ABloque(int idBloquant, int idBloque)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"isBloque?idBloquant={idBloquant}&idBloque={idBloque}"));
        
        var response = await SendWithCredentialsAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur Get : {error}");
            return false;
        }
    }

    public async Task DeleteBloque(int idBloquant, int idBloque)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, BuildUrl($"Delete/{idBloquant}/{idBloque}"));
        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();
    }
}