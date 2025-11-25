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

// Enregistrer aussi les interfaces pour ModeleManager (car il a une m�thode sp�ciale)
builder.Services.AddScoped<IModeleRepository>(sp => sp.GetRequiredService<ModeleManager>());

//------------------------------Authentification------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(config =>
{
    config.AddPolicy(Policies.Authorized, Policies.Logged());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(policy =>
    policy
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin()
);

app.UseHttpsRedirection();

app.UseAuthentication(); // IMPORTANT: � placer AVANT UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();