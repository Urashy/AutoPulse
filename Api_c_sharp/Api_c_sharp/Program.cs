 using System.Text;
using Api_c_sharp.Hubs;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Authentification;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.AI;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//------------------------------Connection DB------------------------------
var connectionString = builder.Configuration.GetConnectionString("LocaleConnection");

builder.Services.AddDbContext<AutoPulseBdContext>(options =>
    options.UseNpgsql(connectionString));

//------------------------------Mapper------------------------------
builder.Services.AddAutoMapper(typeof(MapperProfile));

//------------------------------Managers (DI)------------------------------
builder.Services.AddScoped<AdresseManager>();
builder.Services.AddScoped<AnnonceManager>();
builder.Services.AddScoped<APourConversationManager>();
builder.Services.AddScoped<APourCouleurManager>();
builder.Services.AddScoped<AvisManager>();
builder.Services.AddScoped<BloqueManager>();
builder.Services.AddScoped<BoiteDeVitesseManager>();
builder.Services.AddScoped<CarburantManager>();
builder.Services.AddScoped<CategorieManager>();
builder.Services.AddScoped<CommandeManager>();
builder.Services.AddScoped<CompteManager>();
builder.Services.AddScoped<ConversationManager>();
builder.Services.AddScoped<CouleurManager>();
builder.Services.AddScoped<EtatAnnonceManager>();
builder.Services.AddScoped<EtatCompteManager>();
builder.Services.AddScoped<EtatSignalementManager>();
builder.Services.AddScoped<FactureManager>();
builder.Services.AddScoped<FavoriManager>();
builder.Services.AddScoped<ImageManager>();
builder.Services.AddScoped<JournalManager>();
builder.Services.AddScoped<MarqueManager>();
builder.Services.AddScoped<MessageManager>();
builder.Services.AddScoped<MiseEnAvantManager>();
builder.Services.AddScoped<ModeleManager>();
builder.Services.AddScoped<ModeleBlenderManager>();
builder.Services.AddScoped<MotriciteManager>();
builder.Services.AddScoped<MoyenPaiementManager>();
builder.Services.AddScoped<NotificationManager>();
builder.Services.AddScoped<OffreManager>();
builder.Services.AddScoped<PaysManager>();
builder.Services.AddScoped<PieceJointeManager>();
builder.Services.AddScoped<PlainteManager>();
builder.Services.AddScoped<SignalementManager>();
builder.Services.AddScoped<TokenEmailManager>();
builder.Services.AddScoped<TypeCompteManager>();
builder.Services.AddScoped<TypeJournalManager>();
builder.Services.AddScoped<TypeSignalementManager>();
builder.Services.AddScoped<VoitureManager>();
builder.Services.AddScoped<VueManager>();

builder.Services.AddScoped<IAdresseRepository>(sp => sp.GetRequiredService<AdresseManager>());
builder.Services.AddScoped<IAnnonceRepository>(sp => sp.GetRequiredService<AnnonceManager>());
builder.Services.AddScoped<IAvisRepository>(sp => sp.GetRequiredService<AvisManager>());
builder.Services.AddScoped<ICompteRepository>(sp => sp.GetRequiredService<CompteManager>());
builder.Services.AddScoped<ICommandeRepository>(sp => sp.GetRequiredService<CommandeManager>());
builder.Services.AddScoped<IConversationRepository>(sp => sp.GetRequiredService<ConversationManager>());
builder.Services.AddScoped<IConversationEnrichmentService, ConversationEnrichmentService>();
builder.Services.AddScoped<IJournalService>(sp => sp.GetRequiredService<JournalManager>());
builder.Services.AddScoped<IModeleRepository>(sp => sp.GetRequiredService<ModeleManager>());
builder.Services.AddScoped<INotificationService>(sp => sp.GetRequiredService<NotificationManager>());
builder.Services.AddScoped<IOffreRepository>(sp => sp.GetRequiredService<OffreManager>());

// Enregistrement du service IA avec HttpClient
builder.Services.AddHttpClient<IIAService, IAManager>(client =>
{
    var pythonApiUrl = builder.Configuration["PythonAPI:BaseUrl"] ?? "http://localhost:8000";
    var timeout = builder.Configuration.GetValue<int>("PythonAPI:Timeout", 120);
    
    client.BaseAddress = new Uri(pythonApiUrl);
    client.Timeout = TimeSpan.FromSeconds(timeout);
});

//------------------------------Authentification------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Lire le token depuis le cookie
            var token = context.HttpContext.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(token))
                context.Token = token;

            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])
        )
    };
});

builder.Services.AddAuthorization(config =>
{
    config.AddPolicy(Policies.Authorized, Policies.Logged());
    config.AddPolicy(Policies.Admin, Policies.AdminLogged());
});

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor",
        policy => policy
            .WithOrigins("http://localhost:5296")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true));
});

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHub<MessageHub>("/messagehub");

app.UseRouting();
app.UseCors("AllowBlazor");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();