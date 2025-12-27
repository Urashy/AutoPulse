using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Authentification;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using BlazorAutoPulse.Services;
using BlazorAutoPulse.ViewModel;
using BlazorAutoPulse.ViewModel.Administration;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReinitialisationMdp = BlazorAutoPulse.Model.ReinitialisationMdp;
using VoitureDetailDTO = AutoPulse.Shared.DTO.VoitureDetailDTO;

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
            builder.Services.AddScoped<IService<CompteDetailDTO>, CompteWebService>();
            builder.Services.AddScoped<IService<Carburant>, CarburantWebService>();
            builder.Services.AddScoped<IService<Categorie>, CategorieWebService>();
            builder.Services.AddScoped<IService<BoiteDeVitesse>, BoiteVitesseWebService>();
            builder.Services.AddScoped<IService<Motricite>, MotriciteWebService>();
            builder.Services.AddScoped<IService<APourCouleur>, APourCouleurWebService>();
            builder.Services.AddScoped<IService<AvisListDTO>, AvisWebService>();
            builder.Services.AddScoped<IService<CommandeDTO>, CommandeWebService>();
            builder.Services.AddScoped<IService<VueDTO>, VueWebService>();
            builder.Services.AddScoped<IService<TypeJournalDTO>, TypeJournalWebService>();

            //----------------------- Service avec interface spécifique
            builder.Services.AddScoped<IAnnonceService, AnnonceWebService>();
            builder.Services.AddScoped<IModeleService, ModeleWebService>();
            builder.Services.AddScoped<IServiceConnexion, ConnexionWebService>();
            builder.Services.AddScoped<IPostImageService, PostImageWebService>();
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
            builder.Services.AddScoped<ISignalementService, SignalementWebService>();
            builder.Services.AddScoped<ITypeSignalementService, TypeSignalementWebService>();
            builder.Services.AddScoped<IBloqueService, BloquerWebService>();
            builder.Services.AddScoped<IPieceJointeService, PieceJointeWebService>();
            builder.Services.AddScoped<IAdresseService, AdresseWebService>();
            builder.Services.AddScoped<IPlainteService, PlainteWebService>();
            builder.Services.AddScoped<INotificationService, NotificationWebService>();
            builder.Services.AddScoped<IVoitureService, VoitureWebService>();
            builder.Services.AddScoped<IIAService, IAWebService>();
            builder.Services.AddScoped<IJournalService, JournalWebService>();
            builder.Services.AddScoped<IPlainteService, PlainteWebService>();
            builder.Services.AddScoped<IOffreService, OffreWebService>();
            builder.Services.AddScoped<ConversationStateService>();
            builder.Services.AddScoped<IAutoCompleteService,AdresseAutoCompleteService>();

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
            builder.Services.AddScoped<ToastViewModel>();
            builder.Services.AddScoped<SignalementViewModel>();
            builder.Services.AddScoped<ComptePublicViewModel>();
            builder.Services.AddScoped<AdminSignalementsViewModel>();
            builder.Services.AddTransient<AnnonceComposantViewModel>();
            builder.Services.AddTransient<FileUploadViewModel>();
            builder.Services.AddScoped<AdresseComposantViewModel>();
            builder.Services.AddScoped<AdminLayoutViewModel>();
            builder.Services.AddScoped<CommandeComposantViewModel>();
            builder.Services.AddScoped<AvisComposantViewModel>();
            builder.Services.AddScoped<NotificationViewModel>();
            builder.Services.AddScoped<AdminJournauxViewModel>();
            builder.Services.AddScoped<ModifierAnnonceViewModel>();
            builder.Services.AddScoped<AdminPlainteViewModel>();

            //----------------------- Singleton
            builder.Services.AddSingleton<ISignalRService>(sp => new SignalRWebService());
            builder.Services.AddSingleton<NotificationService>();
            builder.Services.AddSingleton<FavoriStateService>();

            builder.Services.AddHttpClient<IAutoCompleteService, AdresseAutoCompleteService>(client =>
            {
                client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
                client.DefaultRequestHeaders.Add("User-Agent", "BlazorAutoPulse/1.0");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            // ✅ HttpClient en Scoped (standard pour Blazor WebAssembly)
            builder.Services.AddScoped(sp =>
            {
                return new HttpClient
                {
                    BaseAddress = new Uri("http://localhost:5086/api/")
                };
            });
            
            //----------------------- State service
            builder.Services.AddScoped<ConversationStateService>();

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