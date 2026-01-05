using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class CategorieWebService : BaseWebService<CategorieDTO>
    {
        public CategorieWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Categorie";
    }
}
