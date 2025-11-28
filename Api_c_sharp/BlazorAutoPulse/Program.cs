using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Authentification;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using BlazorAutoPulse.ViewModel;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorAutoPulse
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            //----------------------- Service de base
            builder.Services.AddScoped<IService<Marque>, MarqueWebService>();
            builder.Services.AddScoped<IService<Compte>, CompteWebService>();
            builder.Services.AddScoped<IService<Image>, ImageWebService>();
            builder.Services.AddScoped<IService<Carburant>, CarburantWebService>();
            builder.Services.AddScoped<IService<Categorie>, CategorieWebService>();
            builder.Services.AddScoped<IService<TypeCompte>, TypeCompteWebService>();
            builder.Services.AddScoped<IService<BoiteDeVitesse>, BoiteVitesseWebService>();
            builder.Services.AddScoped<IService<Motricite>, MotriciteWebService>();
            builder.Services.AddScoped<IService<Couleur>, CouleurWebService>();
            builder.Services.AddScoped<IService<Voiture>, VoitureWebService>();
            builder.Services.AddScoped<IService<Adresse>, AdresseWebService>();
            builder.Services.AddScoped<IService<APourCouleur>, APourCouleurWebService>();

            //----------------------- Service avec interface spécifique
            builder.Services.AddScoped<IAnnonceService, AnnonceWebService>();
            builder.Services.AddScoped<IModeleService, ModeleWebService>();
            builder.Services.AddScoped<IServiceConnexion, ConnexionWebService>();
            builder.Services.AddScoped<IPostImageService, PostImageWebService>();
            builder.Services.AddScoped<IAnnonceDetailService, AnnonceDetailWebService>();
            builder.Services.AddScoped<ICompteService, CompteWebService>();
            builder.Services.AddScoped<IFavorisService, FavoriWebService>();

            //----------------------- View Model
            builder.Services.AddScoped<HomeViewModel>();
            builder.Services.AddScoped<ConnexionViewModel>();
            builder.Services.AddScoped<CreationCompteViewModel>();
            builder.Services.AddScoped<VenteViewModel>();
            builder.Services.AddScoped<RechercheViewModel>();
            builder.Services.AddScoped<AnnonceComposantViewModel>();
            builder.Services.AddScoped<GetAllViewModel>();
            builder.Services.AddScoped<AnnonceDetailViewModel>();
            builder.Services.AddScoped<CompteViewModel>();

            builder.Services.AddScoped(sp =>
            {
                return new HttpClient
                {
                    BaseAddress = new Uri("http://localhost:5086/api/")
                };
            });

            await builder.Build().RunAsync();
        }
    }
}