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
using Api_c_sharp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//------------------------------Connection DB (CORRIGÉ)------------------------------
string connectionString;

if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("LocaleConnection");
    Console.WriteLine("Environnement: Development");
}
else
{
    // Sur Azure, essayer plusieurs sources dans l'ordre
    // 1. Variable d'environnement standard Azure
    connectionString = Environment.GetEnvironmentVariable("AZURE_POSTGRESQL_CONNECTIONSTRING");

    // 2. Si vide, essayer avec le format App Settings
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_AZURE_POSTGRESQL_CONNECTIONSTRING");
    }

    // 3. Si toujours vide, fallback sur appsettings
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = builder.Configuration.GetConnectionString("AzureConnection");
    }

    Console.WriteLine($"Environnement: {builder.Environment.EnvironmentName}");
}

// Log sécurisé (sans afficher le mot de passe)
if (!string.IsNullOrEmpty(connectionString))
{
    var safeLog = connectionString.Split(';')[0]; // Affiche juste Host=...
    Console.WriteLine($"Connexion configurée: {safeLog}...");
}
else
{
    Console.WriteLine("ERREUR: Aucune connection string trouvée!");
    throw new InvalidOperationException("Connection string manquante!");
}

// IMPORTANT: Décommenter et enregistrer le DbContext
builder.Services.AddDbContext<AutoPulseBdContext>(options =>
{
    options.UseNpgsql(connectionString);

    // Optionnel: ajouter des logs pour le debug
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

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
builder.Services.AddScoped<CartebancaireManager>();
builder.Services.AddScoped<CategorieManager>();
builder.Services.AddScoped<CommandeManager>();
builder.Services.AddScoped<CompteManager>();
builder.Services.AddScoped<ConversationManager>();
builder.Services.AddScoped<CouleurManager>();
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
builder.Services.AddScoped<PaiementManager>();
builder.Services.AddScoped<PaysManager>();
builder.Services.AddScoped<PieceJointeManager>();
builder.Services.AddScoped<PlainteManager>();
builder.Services.AddScoped<RefreshTokenManager>();
builder.Services.AddScoped<SignalementManager>();
builder.Services.AddScoped<TokenEmailManager>();
builder.Services.AddScoped<TypeCompteManager>();
builder.Services.AddScoped<TypeJournalManager>();
builder.Services.AddScoped<TypeSignalementManager>();
builder.Services.AddScoped<VoitureManager>();
builder.Services.AddScoped<VueManager>();

builder.Services.AddScoped<IAdresseRepository>(sp => sp.GetRequiredService<AdresseManager>());
builder.Services.AddScoped<IAnnonceRepository>(sp => sp.GetRequiredService<AnnonceManager>());
builder.Services.AddScoped<ICarteBancaireRepository>(sp => sp.GetRequiredService<CartebancaireManager>());
builder.Services.AddScoped<IAvisRepository>(sp => sp.GetRequiredService<AvisManager>());
builder.Services.AddScoped<ICompteRepository>(sp => sp.GetRequiredService<CompteManager>());
builder.Services.AddScoped<ICommandeRepository>(sp => sp.GetRequiredService<CommandeManager>());
builder.Services.AddScoped<IConversationRepository>(sp => sp.GetRequiredService<ConversationManager>());
builder.Services.AddScoped<IConversationEnrichmentService, ConversationEnrichmentService>();
builder.Services.AddScoped<IJournalService>(sp => sp.GetRequiredService<JournalManager>());
builder.Services.AddScoped<IModeleRepository>(sp => sp.GetRequiredService<ModeleManager>());
builder.Services.AddScoped<INotificationService>(sp => sp.GetRequiredService<NotificationManager>());
builder.Services.AddScoped<IOffreRepository>(sp => sp.GetRequiredService<OffreManager>());
builder.Services.AddScoped<IRefreshTokenRepository>(sp => sp.GetRequiredService<RefreshTokenManager>());

// Tâche de nettoyage automatique des tokens expirés
builder.Services.AddHostedService<RefreshTokenCleanupService>();
builder.Services.AddHostedService<CheckPaiementService>();

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

//------------------------------CORS - CONFIGURATION------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5296",
            "https://localhost:5296",
            "https://azure-blazor-autopulse-a9e3eqdbhmg9a3d9.francecentral-01.azurewebsites.net"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();

    });
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddHostedService<EfWarmupService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseForwardedHeaders();
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowBlazor");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<MessageHub>("/messagehub");
app.MapControllers();

app.MapGet("/health", async (AutoPulseBdContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        if (canConnect)
        {
            // Test une vraie requête
            var count = await db.Set<Compte>().CountAsync();
            return Results.Ok(new
            {
                status = "healthy",
                database = "connected",
                compteCount = count,
                timestamp = DateTime.UtcNow
            });
        }
        return Results.Json(new { status = "unhealthy", error = "Cannot connect" }, statusCode: 503);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Health check error: {ex}");
        return Results.Json(new
        {
            status = "unhealthy",
            error = ex.Message,
            stackTrace = ex.StackTrace
        }, statusCode: 503);
    }
});

app.MapGet("/ping", () => Results.Ok(new
{
    status = "alive",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// Test configuration
app.MapGet("/test-config", (IConfiguration config) =>
{
    try
    {
        var jwtIssuer = config["Jwt:Issuer"];
        var pythonApi = config["PythonAPI:BaseUrl"];

        // NE PAS logger le mot de passe complet !
        var connStr = Environment.GetEnvironmentVariable("AZURE_POSTGRESQL_CONNECTIONSTRING");
        var connStrFromConfig = config.GetConnectionString("AzureConnection");

        return Results.Ok(new
        {
            jwtConfigured = !string.IsNullOrEmpty(jwtIssuer),
            pythonApiConfigured = !string.IsNullOrEmpty(pythonApi),
            envVarExists = !string.IsNullOrEmpty(connStr),
            configExists = !string.IsNullOrEmpty(connStrFromConfig),
            connStrSource = !string.IsNullOrEmpty(connStr) ? "Environment Variable" :
                           !string.IsNullOrEmpty(connStrFromConfig) ? "AppSettings" : "None",
            // Afficher juste le début sans le mot de passe
            connStrPreview = (connStr ?? connStrFromConfig ?? "NULL")
                .Split(';')[0] + "..."
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message, stackTrace = ex.StackTrace }, statusCode: 500);
    }
});

// Test DB simple sans manager
app.MapGet("/test-db", async (AutoPulseBdContext db) =>
{
    try
    {
        // Test 1 : Connexion
        var canConnect = await db.Database.CanConnectAsync();
        if (!canConnect)
        {
            return Results.Json(new
            {
                error = "Cannot connect to database",
                canConnect = false
            }, statusCode: 503);
        }

        // Test 2 : Requête simple
        var compteCount = await db.Set<Compte>().CountAsync();

        return Results.Ok(new
        {
            status = "db_ok",
            canConnect = true,
            compteCount = compteCount,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Npgsql.NpgsqlException npgEx)
    {
        // Erreur PostgreSQL spécifique
        return Results.Json(new
        {
            error = "PostgreSQL Error",
            message = npgEx.Message,
            code = npgEx.ErrorCode,
            detail = npgEx.Detail,
            hint = npgEx.Hint
        }, statusCode: 500);
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            error = ex.GetType().Name,
            message = ex.Message,
            stackTrace = ex.StackTrace
        }, statusCode: 500);
    }
});

// Health check amélioré
app.MapGet("/health", async (AutoPulseBdContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        if (canConnect)
        {
            var count = await db.Set<Compte>().CountAsync();
            return Results.Ok(new
            {
                status = "healthy",
                database = "connected",
                compteCount = count,
                timestamp = DateTime.UtcNow
            });
        }
        return Results.Json(new { status = "unhealthy", error = "Cannot connect" }, statusCode: 503);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Health check error: {ex}");
        return Results.Json(new
        {
            status = "unhealthy",
            error = ex.Message,
            type = ex.GetType().Name,
            innerError = ex.InnerException?.Message
        }, statusCode: 503);
    }
});

app.Run();