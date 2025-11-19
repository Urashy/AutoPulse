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
            builder.Services.AddScoped<IService<Annonce>, AnnonceWebService>();
            builder.Services.AddScoped<IService<Marque>, MarqueWebService>();

            //----------------------- Service avec interface sp�cifique
            builder.Services.AddScoped<IModeleService, ModeleWebService>();
            builder.Services.AddScoped<IServiceConnexion, ConnexionWebService>();

            //----------------------- View Model
            builder.Services.AddScoped<HomeViewModel>();
            builder.Services.AddScoped<ConnexionViewModel>();
            builder.Services.AddScoped<CreationCompteViewModel>();

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }
    }
}
