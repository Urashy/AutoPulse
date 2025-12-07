using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class SignalementWebService : BaseWebService<SignalementCreateDTO>, ISignalementService
{
    public SignalementWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Signalement";

    public async Task<IEnumerable<SignalementDTO>> GetAllAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GetAll"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<SignalementDTO>>()
               ?? Enumerable.Empty<SignalementDTO>();
    }
}