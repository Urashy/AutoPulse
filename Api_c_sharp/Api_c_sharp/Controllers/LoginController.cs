using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Api_c_sharp.DTO;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Authentification;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using LoginRequest = Api_c_sharp.Models.Authentification.LoginRequest;

namespace Api_c_sharp.Controllers;

[Route("api/[controller]/[action]")]
public class LoginController: ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AutoPulseBdContext _context;
    
    public LoginController(AutoPulseBdContext dbContext, IConfiguration config)
    {
        _config = config;
        _context = dbContext;
    }
    
    /// <summary>
    /// Authentifie un utilisateur avec son nom d'utilisateur et mot de passe.
    /// </summary>
    /// <param name="login">Objet contenant les informations de connexion de l'utilisateur.</param>
    /// <returns>Un jeton JWT si l'authentification est réussie, ou un statut 401 si l'authentification échoue.</returns>
    /// <response code="200">Retourne le jeton JWT et les détails de l'utilisateur.</response>
    /// <response code="401">Retourne une erreur si l'utilisateur n'est pas authentifié.</response>
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest login)
    {
        IActionResult response = Unauthorized();
        Compte compte = AuthenticateCompte(login);
        if (compte != null)
        {
            var tokenString = GenerateJwtToken(login);
            response = Ok(new
            {
                token = tokenString,
                userDetails = compte
            });
        }
        
        return response;
    }

    /// <summary>
    /// Vérifie si un utilisateur existe dans la base de données en fonction du mot de passe.
    /// </summary>
    /// <param name="mdp">Mot de passe à vérifier.</param>
    /// <returns>Vrai si l'utilisateur avec ce mot de passe existe, sinon faux.</returns>
    [HttpGet("{mdp}")]
    public bool VerifUser(string mdp)
    {
        string hash = ComputeSha256Hash(mdp);
        return _context.Comptes.SingleOrDefault(x => x.MotDePasse == hash) != null;
    }

    /// <summary>
    /// Authentifie un compte utilisateur avec les informations de connexion.
    /// </summary>
    /// <param name="login">Les informations de connexion de l'utilisateur.</param>
    /// <returns>Le compte utilisateur si l'authentification réussit, sinon null.</returns>
    private Compte AuthenticateCompte(LoginRequest login)
    {
        return _context.Comptes.SingleOrDefault(x => x.Email.ToUpper() == login.Email.ToUpper() && 
                                                x.MotDePasse == ComputeSha256Hash(login.MotDePasse));
    }

    /// <summary>
    /// Génère un jeton JWT pour un utilisateur authentifié.
    /// </summary>
    /// <param name="compteInfo">Les informations de l'utilisateur pour générer le jeton.</param>
    /// <returns>Le jeton JWT généré.</returns>
    private string GenerateJwtToken(LoginRequest compteInfo)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, compteInfo.Email),
            new Claim("role", "Authorized"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Calcule le hachage SHA-256 d'une chaîne de caractères.
    /// </summary>
    /// <param name="rawData">Les données brutes à hacher.</param>
    /// <returns>Le hachage SHA-256 de la chaîne d'entrée.</returns>
    static public string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}