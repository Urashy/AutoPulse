using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class TypeCompteWebService : BaseWebService<TypeCompte>
    {
        public TypeCompteWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "TypeCompte";
    }
}
