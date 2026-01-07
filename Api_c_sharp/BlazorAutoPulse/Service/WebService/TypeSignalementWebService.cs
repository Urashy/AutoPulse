using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class TypeSignalementWebService : BaseWebService<TypeSignalementDTO>, ITypeSignalementService
{
    public TypeSignalementWebService(IHttpClientFactory factory) : base(factory)
    {
    }

    protected override string ApiEndpoint => "TypeSignalement";
}