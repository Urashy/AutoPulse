using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class TypeCompteWebService : BaseWebService<TypeCompte>
    {
        protected override string ApiEndpoint => "TypeCompte";
    }
}
