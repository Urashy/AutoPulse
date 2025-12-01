using System.Text;
using Api_c_sharp.Models.Repository.Interfaces;
using AutoPulse.Shared.DTO;
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
using System.Text.Json;
using AutoPulse.Shared.DTO;
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
#region CRUD Classique
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
    public async Task<ActionResult<CompteGetDTO>> GetByString(string str)
    {
        var result = await _manager.GetByNameAsync(str);

        if (result is null)
        {
            return NotFound();
        }

        return _compteMapper.Map<CompteGetDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les comptes.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="CompteDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompteGetDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<CompteGetDTO>>(_compteMapper.Map<IEnumerable<CompteGetDTO>>(list));
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
        if (id != dto.IdCompte)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        Compte updatedEntity = _compteMapper.Map<Compte>(dto);
        updatedEntity.MotDePasse = toUpdate.MotDePasse;
        updatedEntity.DateDerniereConnexion = toUpdate.DateDerniereConnexion;
        updatedEntity.DateCreation = toUpdate.DateCreation;
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Met à jour une compte existante.
    /// </summary>
    /// <param name="id">Identifiant unique du compte à mettre à jour.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("PutAnonymise")]
    [HttpPut("{id}")]
    public async Task<ActionResult> PutAnonymise(int id)
    {
        Compte compte = await _manager.GetByIdAsync(id);

        if (compte == null)
            return NotFound();

        await _manager.UpdateAnonymise(id);

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
#endregion 

#region Autre methode
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
    /// Récupère un compte à partir d'un id de type de compte.
    /// </summary>
    /// <param name="type">Identifiant unique de la compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteGetDTO"/> si les compte existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByTypeCompte")]
    [HttpGet("{type}")]
    public async Task<ActionResult<IEnumerable<CompteGetDTO>>> GetByTypeCompte(int type)
    {
        var result = await _manager.GetComptesByTypes(type);

        if (result is null)
            return NotFound();

        return new ActionResult<IEnumerable<CompteGetDTO>>(_compteMapper.Map<IEnumerable<CompteGetDTO>>(result));
    }

    /// <summary>
    /// Récupère des comptes qui ont mis en favoris à partir d'une annonce.
    /// </summary>
    /// <param name="type">Identifiant unique de la compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteGetDTO"/> si les comptes existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetComptesByFavoris")]
    [HttpGet("{idannonce}")]
    public async Task<ActionResult<IEnumerable<CompteGetDTO>>> GetCompteByAnnonceFavori(int idannonce)
    {
        var result = await _manager.GetCompteByIdAnnonceFavori(idannonce);

        if (result is null)
            return NotFound();

        return new ActionResult<IEnumerable<CompteGetDTO>>(_compteMapper.Map<IEnumerable<CompteGetDTO>>(result));
    }
#endregion

#region Authentification Classique
    //----------------------------------------------
    // LOGIN
    //----------------------------------------------
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        try
        {
            // Validation des données d'entrée
            if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.MotDePasse))
            {
                return BadRequest(new { message = "Email et mot de passe requis" });
            }

            // Authentification
            Compte compte = await AuthenticateCompte(login);
            
            if (compte == null)
            {
                return Unauthorized(new { message = "Email ou mot de passe incorrect" });
            }

            // Génération du token JWT
            var tokenString = GenerateJwtToken(login);
            
            // Configuration du cookie
            CookieOptions cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(1),
                Domain = null,
                Path = "/"
            };
            
            Response.Cookies.Append("access_token", tokenString, cookieOptions);
            
            return Ok(new { 
                message = "Login OK",
                userId = compte.IdCompte,
                pseudo = compte.Pseudo
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur Login: {ex.Message}");
            return StatusCode(500, new { message = "Erreur serveur lors de la connexion" });
        }
    }
    
    [HttpPost]
    [Authorize]
    public IActionResult Logout()
    {
        try
        {
            // Efface le cookie JWT HTTP-only
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return Ok(new { message = "Logout OK" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur Logout: {ex.Message}");
            return StatusCode(500, new { message = "Erreur lors de la déconnexion" });
        }
    }
#endregion

#region Authentification Google
    [ActionName("GoogleLogin")]
    [HttpGet]
    public IActionResult GoogleLogin()
    {
        var clientId = config["Authentication:Google:ClientId"];
        Console.WriteLine(clientId);
        var redirectUri = config["Authentication:Google:RedirectUri"];
        var scope = "openid profile email";
            
        var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                            $"client_id={clientId}&" +
                            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                            $"response_type=code&" +
                            $"scope={Uri.EscapeDataString(scope)}";
            
        return Ok(new { url = googleAuthUrl });
    }
    
    [ActionName("GoogleCallback")]
    [HttpGet]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Code manquant");

        try
        {
            // 1. Échanger le code contre un access token
            GoogleTokenResponse tokenResponse = await ExchangeCodeForToken(code);
            
            // 2. Récupérer les infos utilisateur depuis Google
            GoogleUserInfo userInfo = await GetGoogleUserInfo(tokenResponse.AccessToken);
            
            // 3. Créer ou récupérer le compte
            (bool, Compte) compteCreate = await GetOrCreateCompte(userInfo);
            bool existing = compteCreate.Item1;
            Compte compte = compteCreate.Item2;
            
            // 4. Générer ton JWT
            LoginRequest loginRequest = new LoginRequest()
            {
                Email = compte.Email,
                MotDePasse = compte.MotDePasse,
            };
            string jwtToken = GenerateJwtToken(loginRequest);
            
            // 5. Définir le cookie
            Response.Cookies.Append("access_token", jwtToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(1),
                Path = "/",
                Domain = null
            });
            
            // 6. Rediriger vers le front
            if (existing)
            {
                return Redirect("http://localhost:5296/compte");
            }
            return Redirect("http://localhost:5296/complete-profile");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur : {ex.Message}");
        }
    }
#endregion

#region Outils Authentification Google
    private async Task<GoogleTokenResponse> ExchangeCodeForToken(string code)
    {
        var clientId = config["Authentication:Google:ClientId"];
        var clientSecret = config["Authentication:Google:ClientSecret"];
        var redirectUri = config["Authentication:Google:RedirectUri"];

        using var httpClient = new HttpClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        });

        var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GoogleTokenResponse>(json);
    }

    private async Task<GoogleUserInfo> GetGoogleUserInfo(string accessToken)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        
        var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GoogleUserInfo>(json);
    }

    private async Task<(bool, Compte)> GetOrCreateCompte(GoogleUserInfo userInfo)
    {
        // Cherche si un compte existe déjà avec cet email
        var existingCompte = await _manager.GetByNameAsync(userInfo.Email);
        
        if (existingCompte != null)
        {
            // Mise à jour du Google ID si nécessaire
            if (string.IsNullOrEmpty(existingCompte.GoogleId))
            {
                existingCompte.GoogleId = userInfo.Id;
                await _manager.UpdateAsync(existingCompte, existingCompte);
            }
            return (true, existingCompte);
        }

        // Créer un nouveau compte
        var newCompte = new Compte
        {
            Email = userInfo.Email,
            Pseudo = userInfo.Name ?? userInfo.Email.Split('@')[0],
            Nom = userInfo.FamilyName ?? "Nom",
            Prenom = userInfo.GivenName ?? "Prénom",
            GoogleId = userInfo.Id,
            AuthProvider = "Google",
            MotDePasse = Guid.NewGuid().ToString(),

            DateCreation = DateTime.UtcNow,
            DateDerniereConnexion = DateTime.UtcNow,

            DateNaissance = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),

            IdTypeCompte = 1
        };
        await _manager.AddAsync(newCompte);
        return (false, newCompte);
    }
#endregion

#region Modif Mdp
    [ActionName("ModifMdp")]
    [HttpPost]
    public async Task<IActionResult> ModifMdp([FromBody] ChangementMdpDTO dto)
    {
        var entityToUpdate = await _manager.GetByIdAsync(dto.IdCompte.Value);

        if (entityToUpdate == null)
            return NotFound("Compte introuvable.");

        // Création d’une copie contenant les nouvelles valeurs
        Compte updatedEntity = new Compte
        {
            IdCompte = entityToUpdate.IdCompte,
            Pseudo = entityToUpdate.Pseudo,
            MotDePasse = ComputeSha256Hash(dto.MotDePasse),
            Nom = entityToUpdate.Nom,
            Prenom = entityToUpdate.Prenom,
            Email = entityToUpdate.Email,
            DateCreation = entityToUpdate.DateCreation,
            DateDerniereConnexion = entityToUpdate.DateDerniereConnexion,
            DateNaissance = entityToUpdate.DateNaissance,
            Biographie = entityToUpdate.Biographie,
            IdTypeCompte = entityToUpdate.IdTypeCompte,
            RaisonSociale = entityToUpdate.RaisonSociale,
        };

        await _manager.UpdateAsync(entityToUpdate, updatedEntity);

        return Ok("Mot de passe modifié avec succès.");
    }

    /// <summary>
    /// Vérifie si un utilisateur existe dans la base de données en fonction du mot de passe.
    /// </summary>
    /// <param name="mdp">Mot de passe à vérifier.</param>
    /// <returns>Vrai si l'utilisateur avec ce mot de passe existe, sinon faux.</returns>
    [ActionName("VerifUser")]
    [HttpPost]
    public bool VerifUser([FromBody] ChangementMdpDTO dto)
    {
        string hash = ComputeSha256Hash(dto.MotDePasse);
        return _manager.VerifMotDePasse(dto.Email, hash) != null;
    }
#endregion
    
#region Outils Authentification Classique
    /// <summary>
    /// Authentifie un compte utilisateur avec les informations de connexion.
    /// </summary>
    /// <param name="login">Les informations de connexion de l'utilisateur.</param>
    /// <returns>Le compte utilisateur si l'authentification réussit, sinon null.</returns>
    private async Task<Compte> AuthenticateCompte(LoginRequest login)
    {
        try
        {
            return await _manager.AuthenticateCompte(login.Email, ComputeSha256Hash(login.MotDePasse));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur AuthenticateCompte: {ex.Message}");
            return null;
        }
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
#endregion    
}