using System.Text;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using System.Security.Cryptography;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using Api_c_sharp.Models.Authentification;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using LoginRequest = Api_c_sharp.Models.Authentification.LoginRequest;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les comptes.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class CompteController(CompteManager _manager, IMapper _compteMapper, IConfiguration config) : ControllerBase
{
    /// <summary>
    /// Récupère une annoncs à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteDTO"/> si la compte existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<CompteDetailDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _compteMapper.Map<CompteDetailDTO>(result);
    }
    
    [ActionName("GetMe")]
    [Authorize]
    [HttpGet]
    public IActionResult GetMe()
    {
        var claim = User.FindFirst("idUser")?.Value;
        if (string.IsNullOrEmpty(claim))
            return Unauthorized();

        int userId = int.Parse(claim);
        Compte user = _manager.GetByIdAsync(userId).Result;

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Récupère une compte à partir de son nom exact (insensible à la casse).
    /// </summary>
    /// <param name="str">Nom de la compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteDTO"/> si la compte existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByString")]
    [HttpGet("{str}")]
    public async Task<ActionResult<CompteDetailDTO>> GetByString(string str)
    {
        var result = await _manager.GetByNameAsync(str);

        if (result is null)
        {
            return NotFound();
        }

        return _compteMapper.Map<CompteDetailDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les comptes.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="CompteDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompteListDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<CompteListDTO>>(_compteMapper.Map<IEnumerable<CompteListDTO>>(list));
    }

    /// <summary>
    /// Crée une nouvelle compte.
    /// </summary>
    /// <param name="dto">Objet <see cref="CompteDTO"/> contenant les informations de la compte à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la compte créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<Compte>> Post([FromBody] CompteCreateDTO dto)
    {

        var entity = _compteMapper.Map<Compte>(dto);
        entity.MotDePasse = ComputeSha256Hash(entity.MotDePasse);
        entity.DateNaissance = DateTime.SpecifyKind(entity.DateNaissance, DateTimeKind.Utc);
        entity.DateCreation = DateTime.UtcNow;
        entity.DateDerniereConnexion = DateTime.UtcNow;
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdCompte }, entity);
    }

    /// <summary>
    /// Met à jour une compte existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la compte à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="CompteDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] CompteUpdateDTO dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _compteMapper.Map<Compte>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime une compte existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la compte à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _manager.GetByIdAsync(id);

        if (entity == null)
            return NotFound();

        await _manager.DeleteAsync(entity);
        return NoContent();
    }


    /// <summary>
    /// Récupère un compte à partir d'un id de type de compte.
    /// </summary>
    /// <param name="type">Identifiant unique de la compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteListDTO"/> si les compte existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByTypeCompte")]
    [HttpGet("{type}")]
    public async Task<ActionResult<IEnumerable<CompteListDTO>>> GetByTypeCompte(int type)
    {
        var result = await _manager.GetComptesByTypes(type);

        if (result is null)
            return NotFound();

        return new ActionResult<IEnumerable<CompteListDTO>>(_compteMapper.Map<IEnumerable<CompteListDTO>>(result));
    }

    /// <summary>
    /// Récupère des comptes qui ont mis en favoris à partir d'une annonce.
    /// </summary>
    /// <param name="type">Identifiant unique de la compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteListDTO"/> si les comptes existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetComptesByFavoris")]
    [HttpGet("{idannonce}")]
    public async Task<ActionResult<IEnumerable<CompteListDTO>>> GetCompteByAnnonceFavori(int idannonce)
    {
        var result = await _manager.GetCompteByIdAnnonceFavori(idannonce);

        if (result is null)
            return NotFound();

        return new ActionResult<IEnumerable<CompteListDTO>>(_compteMapper.Map<IEnumerable<CompteListDTO>>(result));
    }
    
    //----------------------------------------------
    // LOGIN
    //----------------------------------------------
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest login)
    {
        IActionResult response = Unauthorized();
        Compte compte = AuthenticateCompte(login);
        if (compte != null)
        {
            var tokenString = GenerateJwtToken(login);
            CookieOptions cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(1),
            };
            Response.Cookies.Append("access_token", tokenString, cookieOptions);
        }
        
        return Ok(new {message = "Login OK"});
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
        return _manager.VerifMotDePasse(hash) != null;
    }

    /// <summary>
    /// Authentifie un compte utilisateur avec les informations de connexion.
    /// </summary>
    /// <param name="login">Les informations de connexion de l'utilisateur.</param>
    /// <returns>Le compte utilisateur si l'authentification réussit, sinon null.</returns>
    private Compte AuthenticateCompte(LoginRequest login)
    {
        return _manager.AuthenticateCompte(login.Email, ComputeSha256Hash(login.MotDePasse)).Result;
    }

    /// <summary>
    /// Génère un jeton JWT pour un utilisateur authentifié.
    /// </summary>
    /// <param name="compteInfo">Les informations de l'utilisateur pour générer le jeton.</param>
    /// <returns>Le jeton JWT généré.</returns>
    private string GenerateJwtToken(LoginRequest compteInfo)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        Compte compte = _manager.GetByNameAsync(compteInfo.Email).Result;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, compteInfo.Email),
            new Claim("role", "Authorized"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("idUser", compte.IdCompte.ToString()),
        };
        
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
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