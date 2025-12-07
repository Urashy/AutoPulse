using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class TypeSignalementWebService : BaseWebService<TypeSignalementDTO>, ITypeSignalementService
{
    public TypeSignalementWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "TypeSignalement";
}