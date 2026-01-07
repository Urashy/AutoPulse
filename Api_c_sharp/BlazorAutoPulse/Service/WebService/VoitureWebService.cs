using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class VoitureWebService: BaseWebService<VoitureDetailDTO>, IVoitureService
{
    private IVoitureService _voitureServiceImplementation;

    public VoitureWebService(IHttpClientFactory factory) : base(factory)
    {
    }

    protected override string ApiEndpoint => "Voiture";
    public async Task UpdateVoitureAsync(int id, VoitureUpdateDTO entity)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"Put/{id}"))
        {
            Content = JsonContent.Create(entity)
        };
        
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
    }
}