using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class CategorieWebService : BaseWebService<Categorie>
    {
        protected override string ApiEndpoint => "Categorie";
    }
}
