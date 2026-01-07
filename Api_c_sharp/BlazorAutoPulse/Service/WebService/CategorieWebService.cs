using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class CategorieWebService : BaseWebService<CategorieDTO>
    {
        public CategorieWebService(IHttpClientFactory factory) : base(factory)
        {
        }

        protected override string ApiEndpoint => "Categorie";
    }
}
