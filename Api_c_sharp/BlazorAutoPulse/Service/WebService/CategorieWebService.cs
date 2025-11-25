using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class CategorieWebService : BaseWebService<Categorie>
    {
        public CategorieWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Categorie";
    }
}
