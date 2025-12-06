using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Authentification;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using BlazorAutoPulse.Services;
using BlazorAutoPulse.ViewModel;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReinitialisationMdp = BlazorAutoPulse.Model.ReinitialisationMdp;

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
            builder.Services.AddScoped<IService<Carburant>, CarburantWebService>();
            builder.Services.AddScoped<IService<Categorie>, CategorieWebService>();
            builder.Services.AddScoped<IService<BoiteDeVitesse>, BoiteVitesseWebService>();
            builder.Services.AddScoped<IService<Motricite>, MotriciteWebService>();
            builder.Services.AddScoped<IService<Voiture>, VoitureWebService>();
            builder.Services.AddScoped<IService<APourCouleur>, APourCouleurWebService>();
            builder.Services.AddScoped<IService<AvisListDTO>, AvisWebService>();
            builder.Services.AddScoped<IService<CommandeDTO>, CommandeWebService>();

            //----------------------- Service avec interface spécifique
            builder.Services.AddScoped<IAnnonceService, AnnonceWebService>();
            builder.Services.AddScoped<IModeleService, ModeleWebService>();
            builder.Services.AddScoped<IServiceConnexion, ConnexionWebService>();
            builder.Services.AddScoped<IPostImageService, PostImageWebService>();
            builder.Services.AddScoped<IAnnonceDetailService, AnnonceDetailWebService>();
            builder.Services.AddScoped<ICompteService, CompteWebService>();
            builder.Services.AddScoped<IFavorisService, FavoriWebService>();
            builder.Services.AddScoped<IImageService, ImageWebService>();
            builder.Services.AddScoped<IReinitialiseMdp, ReinitialisationMdpWebService>();
            builder.Services.AddScoped<ICouleurService, CouleurWebService>();
            builder.Services.AddScoped<ISignalRService, SignalRWebService>();
            builder.Services.AddScoped<IConversationService, ConversationWebService>();
            builder.Services.AddScoped<ITypeCompteService, TypeCompteWebService>();
            builder.Services.AddScoped<IAdresseService, AdresseWebService>();
            builder.Services.AddScoped<IMessageService, MessageWebService>();
            builder.Services.AddScoped<IAvisService, AvisWebService>();
            builder.Services.AddScoped<ICommandeService, CommandeWebService>();

            //----------------------- View Model
            builder.Services.AddScoped<HomeViewModel>();
            builder.Services.AddScoped<ConnexionViewModel>();
            builder.Services.AddScoped<CreationCompteViewModel>();
            builder.Services.AddScoped<VenteViewModel>();
            builder.Services.AddScoped<RechercheViewModel>();
            builder.Services.AddScoped<GetAllViewModel>();
            builder.Services.AddScoped<AnnonceDetailViewModel>();
            builder.Services.AddScoped<CompteViewModel>();
            builder.Services.AddScoped<FavorisViewModel>();
            builder.Services.AddScoped<OubliMdpViewModel>();
            builder.Services.AddScoped<CompleteProfileViewModel>();
            builder.Services.AddScoped<ConversationViewModel>();
            builder.Services.AddScoped<MainLayoutViewModel>();
            builder.Services.AddScoped<AdminDashboardViewModel>();
            builder.Services.AddScoped<AdminUtilisateursViewModel>();
            builder.Services.AddScoped<AdminAnnoncesViewModel>();

            builder.Services.AddTransient<AnnonceComposantViewModel>();
            
            //----------------------- Singleton
            builder.Services.AddSingleton<ISignalRService, SignalRWebService>();
            builder.Services.AddSingleton<NotificationService>();
            
            //----------------------- State service
            builder.Services.AddScoped<ConversationStateService>();

            builder.Services.AddScoped(sp =>
            {
                return new HttpClient
                {
                    BaseAddress = new Uri("http://localhost:5086/api/")
                };
            });
            
            builder.Services.AddSingleton<ISignalRService>(sp =>
            {
                return new SignalRWebService();
            });

            await builder.Build().RunAsync();
        }
    }
}