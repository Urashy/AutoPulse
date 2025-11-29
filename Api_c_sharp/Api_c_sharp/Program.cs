using System.Text;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Authentification;
using Api_c_sharp.Models.Repository;
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AutoPulseBdContext>(options =>
    options.UseNpgsql(connectionString));

//------------------------------Mapper------------------------------
builder.Services.AddAutoMapper(typeof(MapperProfile));

//------------------------------Managers (DI)------------------------------
builder.Services.AddScoped<AnnonceManager>();
builder.Services.AddScoped<AdresseManager>();
builder.Services.AddScoped<MarqueManager>();
builder.Services.AddScoped<ModeleManager>();
builder.Services.AddScoped<BoiteDeVitesseManager>();
builder.Services.AddScoped<CarburantManager>();
builder.Services.AddScoped<CategorieManager>();
builder.Services.AddScoped<MiseEnAvantManager>();
builder.Services.AddScoped<MotriciteManager>();
builder.Services.AddScoped<PaysManager>();
builder.Services.AddScoped<TypeJournalManager>();
builder.Services.AddScoped<CompteManager>();
builder.Services.AddScoped<ImageManager>();
builder.Services.AddScoped<TypeCompteManager>();
builder.Services.AddScoped<CouleurManager>();
builder.Services.AddScoped<VoitureManager>();
builder.Services.AddScoped<APourCouleurManager>();
builder.Services.AddScoped<FavoriManager>();
builder.Services.AddScoped<ReinitialisationMotDePasseManager>();

// Enregistrer aussi les interfaces pour ModeleManager (car il a une m�thode sp�ciale)
builder.Services.AddScoped<IModeleRepository>(sp => sp.GetRequiredService<ModeleManager>());

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
            .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazor");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();