using System.Data.Entity.Infrastructure;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LoginRequest = AutoPulse.Shared.DTO.Authentification.LoginRequest;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using MailKit.Net.Smtp;
using MimeKit;
using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api_c_sharp.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les comptes.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class CompteController(CompteManager _manager, IMapper _compteMapper, IConfiguration config, IJournalService _journalService, RefreshTokenManager _refreshTokenManager) : ControllerBase
{
#region CRUD Classique
    /// <summary>
    /// Récupère un compte à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteDetailDTO"/> si le compte existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CompteDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompteDetailDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _compteMapper.Map<CompteDetailDTO>(result);
    }

    /// <summary>
    /// Récupère un compte à partir de son nom exact (insensible à la casse).
    /// </summary>
    /// <param name="str">Nom du compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteDetailDTO"/> si le compte existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByString")]
    [HttpGet("{str}")]
    [ProducesResponseType(typeof(CompteDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// Récupère la liste de touts les comptes.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="CompteGetDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CompteGetDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CompteGetDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<CompteGetDTO>>(_compteMapper.Map<IEnumerable<CompteGetDTO>>(list));
    }

    /// <summary>
    /// Crée un nouveau compte.
    /// </summary>
    /// <param name="dto">Objet <see cref="CompteCreateDTO"/> contenant les informations du compte à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec le compte créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    [ProducesResponseType(typeof(Compte), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Compte>> Post([FromBody] CompteCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _compteMapper.Map<Compte>(dto);
        entity.MotDePasse = ComputeSha256Hash(entity.MotDePasse);
        entity.DateNaissance = DateTime.SpecifyKind(entity.DateNaissance, DateTimeKind.Utc);
        entity.DateCreation = DateTime.UtcNow;
        entity.DateDerniereConnexion = DateTime.UtcNow;
        entity.IdEtatCompte = 1;

        try
        {
            await _manager.AddAsync(entity);
            await _journalService.LogCreationCompteAsync(entity.IdCompte, entity.Pseudo);

            return CreatedAtAction(nameof(GetByID), new { id = entity.IdCompte }, entity);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                pgEx.ConstraintName == "IX_t_e_compte_com_com_email")
            {
                ModelState.AddModelError(
                    nameof(dto.Email),
                    "Cet email est déjà utilisé."
                );

                return ValidationProblem(ModelState);
            }

            throw;
        }
    }

    /// <summary>
    /// Met à jour un compte existant.
    /// </summary>
    /// <param name="id">Identifiant unique du compte à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="CompteUpdateDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put(int id, [FromBody] CompteUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        Compte updatedEntity = _compteMapper.Map<Compte>(dto);
        updatedEntity.MotDePasse = toUpdate.MotDePasse;
        updatedEntity.DateDerniereConnexion = toUpdate.DateDerniereConnexion;
        updatedEntity.DateCreation = toUpdate.DateCreation;
        await _journalService.LogModificationProfilAsync(id);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Anonymise un compte existant.
    /// </summary>
    /// <param name="id">Identifiant unique du compte à anonymiser.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si l’anonymisation réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("PutAnonymise")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> PutAnonymise(int id)
    {
        Compte compte = await _manager.GetByIdAsync(id);

        if (compte == null)
            return NotFound();

        await _manager.UpdateAnonymise(id);
        
        Response.Cookies.Delete("access_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        });

        return NoContent();
    }

    /// <summary>
    /// Met à jour le type de compte d’un compte existant.
    /// </summary>
    /// <param name="id">Identifiant unique du compte à modifier.</param>
    /// <param name="dto">Objet <see cref="CompteModifTypeCompteDTO"/> contenant le nouveau type de compte.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte ne correspond à l’identifiant fourni (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("PutTypeCompte")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> PutTypeCompte(int id, [FromBody] CompteModifTypeCompteDTO dto)
    {
        Compte compte = await _manager.GetByIdAsync(id);

        if (compte == null)
            return NotFound();

        bool estpro = false;

        if (compte.IdTypeCompte == 2)
            estpro = true;

        await _manager.UpdateTypeCompte(compte,dto,estpro);

        return NoContent();
    }

    /// <summary>
    /// Supprime un compte existant.
    /// </summary>
    /// <param name="id">Identifiant unique du compte à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Delete")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// <summary>
    /// Récupère les informations du compte actuellement authentifié.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteDetailDTO"/> du compte connecté (200).</description></item>
    /// <item><description><see cref="UnauthorizedResult"/> si l’utilisateur n’est pas authentifié (401).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si le compte n’existe plus (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetMe")]
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(CompteDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompteDetailDTO>> GetMe()
    {
        var claim = User.FindFirst("idUser")?.Value;
        if (string.IsNullOrEmpty(claim))
            return Unauthorized();

        int userId = int.Parse(claim);
        Compte user = await _manager.GetByIdAsync(userId);

        if (user == null)
            return NotFound();
        
        return _compteMapper.Map<CompteDetailDTO>(user);
    }

    /// <summary>
    /// Récupère la liste des comptes en fonction de leur type de compte.
    /// </summary>
    /// <param name="type">Identifiant du type de compte.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de <see cref="CompteGetDTO"/> si des comptes existent (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte ne correspond au type fourni (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByTypeCompte")]
    [HttpGet("{type}")]
    [ProducesResponseType(typeof(IEnumerable<CompteGetDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CompteGetDTO>>> GetByTypeCompte(int type)
    {
        var result = await _manager.GetComptesByTypes(type);

        if (result is null || !result.Any())
            return NotFound();

        return new ActionResult<IEnumerable<CompteGetDTO>>(_compteMapper.Map<IEnumerable<CompteGetDTO>>(result));
    }

    /// <summary>
    /// Récupère les comptes ayant ajouté une annonce en favoris.
    /// </summary>
    /// <param name="idannonce">Identifiant unique de l’annonce.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de <see cref="CompteGetDTO"/> si des comptes existent (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte n’a mis l’annonce en favoris (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetComptesByFavoris")]
    [HttpGet("{idannonce}")]
    [ProducesResponseType(typeof(IEnumerable<CompteGetDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CompteGetDTO>>> GetCompteByAnnonceFavori(int idannonce)
    {
        var result = await _manager.GetCompteByIdAnnonceFavori(idannonce);

        if (result is null || !result.Any())
            return NotFound();

        return new ActionResult<IEnumerable<CompteGetDTO>>(_compteMapper.Map<IEnumerable<CompteGetDTO>>(result));
    }


    /// <summary>
    /// Récupère le profil public d’un compte à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du compte.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteProfilPublicDTO"/> si le compte existe (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte ne correspond à l’identifiant fourni (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetProfilPublic")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CompteProfilPublicDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompteProfilPublicDTO>> GetProfilPublic(int id)
    {
        var result = await _manager.GetProfilPublic(id);

        if (result is null)
            return NotFound();

        return _compteMapper.Map<CompteProfilPublicDTO>(result);
    }

    /// <summary>
    /// Active ou désactive l’état d’un compte.
    /// </summary>
    /// <param name="idcompte">Identifiant unique du compte.</param>
    /// <param name="estretirer">Indique si le compte doit être retiré (true) ou réactivé (false).</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte ne correspond à l’identifiant fourni (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("ToggleEtatCompte")]
    [HttpPut("{idcompte}/{estretirer}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ToggleEtatCompte(int idcompte, bool estretirer = false)
    {
        Compte compte = await _manager.GetByIdAsync(idcompte);

        if (compte is null)
            return NotFound();

        await _manager.ToggleEtatCompte(idcompte, estretirer);

        return NoContent();
    }
#endregion

#region Authentification Classique
    /// <summary>
    /// Authentifie un utilisateur. Si l'A2F est activé, renvoie un statut spécial.
    /// </summary>
    /// <param name="login">Informations de connexion.</param>
    /// <param name="rememberMe">Si true, le refresh token dure 30 jours. Sinon, jusqu'à fermeture du navigateur.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Succès avec token si pas d'A2F (200).</description></item>
    /// <item><description>Demande de code A2F si A2F activé (202).</description></item>
    /// <item><description><see cref="UnauthorizedResult"/> si authentification échoue (401).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si données invalides (400).</description></item>
    /// </list>
    /// </returns>
    [HttpPost]
    [AllowAnonymous]
    [ActionName("Login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest login,
        [FromQuery] bool rememberMe = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.MotDePasse))
            {
                return BadRequest(new { message = "Email et mot de passe requis" });
            }

            var compte = await AuthenticateCompte(login);

            if (compte == null)
            {
                return Unauthorized(new { message = "Email ou mot de passe incorrect" });
            }

            // ✅ Vérification A2F
            var (a2fActif, derniereActivation) = await _manager.GetStatutA2f(compte.IdCompte);
            bool doitReactiverA2f = await _manager.DoitReactiverA2f(compte.IdCompte);
            if (a2fActif || doitReactiverA2f)
            {
                // Envoyer code A2F par email
                var rand = new Random();
                var codeA2f = rand.Next(0, 9999999).ToString("D7");
                var expiration = DateTime.UtcNow.AddMinutes(15);

                var tokenA2f = new TokenEmail
                {
                    IdCompte = compte.IdCompte,
                    Email = compte.Email,
                    Token = codeA2f,
                    Expiration = expiration,
                    Utilise = false,
                    TypeToken = doitReactiverA2f ? "A2F_ACTIVATION" : "A2F_CONNEXION"
                };

                await _manager.EnregistrerA2f(tokenA2f);

                // Envoi email (code existant)
                await EnvoyerEmailA2f(compte.Email, codeA2f, doitReactiverA2f);

                return Accepted(new
                {
                    message = "Code A2F envoyé par email",
                    requiresA2f = true,
                    mustReactivate = doitReactiverA2f,
                    userId = compte.IdCompte,
                    email = compte.Email
                });
            }

            // ✅ Connexion sans A2F : Générer les tokens
            var accessToken = GenerateJwtToken(login);
            var refreshToken = GenerateRefreshToken();

            // ✅ Stocker le refresh token en base (hashé)
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            await _refreshTokenManager.StoreRefreshTokenAsync(
                compte.IdCompte,
                refreshToken,
                rememberMe,
                ipAddress,
                userAgent
            );

            // ✅ Définir les cookies
            SetAuthCookies(accessToken, refreshToken, rememberMe);

            await _journalService.LogConnexionAsync(compte.IdCompte);

            return Ok(new
            {
                message = "Login OK",
                userId = compte.IdCompte,
                pseudo = compte.Pseudo,
                role = compte.IdTypeCompte,
                rememberMe = rememberMe
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
    [ActionName("Logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst("idUser")?.Value;
        
            if (!string.IsNullOrEmpty(userId))
            {
                await _journalService.LogDeconnexionAsync(int.Parse(userId));
            }

            // ✅ Révoquer le refresh token s'il existe
            if (Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
            {
                await _refreshTokenManager.RevokeRefreshTokenAsync(refreshToken);
            }

            // ✅ Supprimer les cookies
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            Response.Cookies.Delete("refresh_token", new CookieOptions
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
    
    /// <summary>
    /// Valide le code A2F et finalise la connexion.
    /// </summary>
    /// <param name="dto">Objet contenant l'email, le code et le type de validation.</param>
    /// <param name="rememberMe">Si true, le refresh token dure 30 jours. Sinon, jusqu'à fermeture du navigateur.</param>
    [HttpPost]
    [AllowAnonymous]
    [ActionName("ValidateA2fLogin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateA2fLogin(
        [FromBody] TokenEmailVerifDTO dto,
        [FromQuery] bool rememberMe = false)
    {
        try
        {
            var compte = await _manager.GetByNameAsync(dto.Email);

            if (compte == null)
                return BadRequest(new { message = "Compte introuvable" });

            // Valider le code A2F (méthode existante à implémenter)
            // ...

            if (dto.TypeToken == "A2F_ACTIVATION")
            {
                await _manager.ActiverA2f(compte.IdCompte);
            }

            // ✅ Générer les tokens
            var loginRequest = new LoginRequest { Email = dto.Email, MotDePasse = compte.MotDePasse };
            var accessToken = GenerateJwtToken(loginRequest);
            var refreshToken = GenerateRefreshToken();

            // ✅ Stocker le refresh token
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            await _refreshTokenManager.StoreRefreshTokenAsync(
                compte.IdCompte,
                refreshToken,
                rememberMe,
                ipAddress,
                userAgent
            );

            // ✅ Définir les cookies
            SetAuthCookies(accessToken, refreshToken, rememberMe);

            await _journalService.LogConnexionAsync(compte.IdCompte);

            return Ok(new
            {
                message = "Login OK",
                userId = compte.IdCompte,
                pseudo = compte.Pseudo,
                role = compte.IdTypeCompte,
                rememberMe = rememberMe
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur ValidateA2fLogin: {ex.Message}");
            return StatusCode(500, new { message = "Erreur serveur" });
        }
    }
    
    [HttpPost]
    [AllowAnonymous]
    [ActionName("Refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            // ✅ Lire le refresh token du cookie
            if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
            {
                Console.WriteLine("❌ Refresh token manquant dans le cookie");
                return Unauthorized(new { message = "Token manquant" });
            }

            Console.WriteLine("🔄 Tentative de refresh du token...");

            // ✅ Valider le refresh token et récupérer le compte
            var compte = await _refreshTokenManager.ValidateRefreshTokenAsync(refreshToken);

            if (compte == null)
            {
                Console.WriteLine("❌ Refresh token invalide ou expiré");
                
                // ✅ Supprimer les cookies invalides
                Response.Cookies.Delete("access_token");
                Response.Cookies.Delete("refresh_token");
                
                return Unauthorized(new { message = "Token invalide ou expiré" });
            }

            Console.WriteLine($"✅ Refresh token valide pour compte {compte.IdCompte}");

            // ✅ Générer un nouvel access token
            var loginRequest = new LoginRequest
            {
                Email = compte.Email,
                MotDePasse = compte.MotDePasse
            };
            var newAccessToken = GenerateJwtToken(loginRequest);

            // ✅ Mettre à jour le cookie access_token
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15),
                Path = "/"
            };

            Response.Cookies.Append("access_token", newAccessToken, accessCookieOptions);

            // ✅ OPTIONNEL : Si rotation activée, mettre à jour aussi le refresh token
            // Response.Cookies.Append("refresh_token", newRefreshToken, refreshCookieOptions);

            Console.WriteLine("✅ Access token rafraîchi avec succès");

            return Ok(new { message = "Token rafraîchi" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur Refresh: {ex.Message}");
            return StatusCode(500, new { message = "Erreur serveur" });
        }
    }
#endregion

#region A2F

    /// <summary>
    /// Récupère le statut A2F d'un compte.
    /// </summary>
    /// <param name="idCompte">Identifiant du compte.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Le statut A2F avec la date de dernière activation (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si le compte n'existe pas (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetStatutA2f")]
    [HttpGet("{idCompte}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetStatutA2f(int idCompte)
    {
        var compte = await _manager.GetByIdAsync(idCompte);

        if (compte == null)
            return NotFound();

        var (a2fActif, derniereActivation) = await _manager.GetStatutA2f(idCompte);

        return Ok(new
        {
            A2fActif = a2fActif,
            DateDerniereActivation = derniereActivation,
            DoitReactiver = await _manager.DoitReactiverA2f(idCompte)
        });
    }

    /// <summary>
    /// Vérifie si l'A2F doit être réactivé pour un compte (plus de 30 jours).
    /// </summary>
    /// <param name="idCompte">Identifiant du compte.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>True si l'A2F doit être réactivé, False sinon (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si le compte n'existe pas (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("VerifActivA2f")]
    [HttpGet("{idCompte}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> VerifActivA2f(int idCompte)
    {
        var compte = await _manager.GetByIdAsync(idCompte);

        if (compte == null)
            return NotFound();

        bool doitReactiver = await _manager.DoitReactiverA2f(idCompte);

        return Ok(doitReactiver);
    }

    /// <summary>
    /// Active l'A2F pour un compte après validation d'un code.
    /// </summary>
    /// <param name="dto">Objet contenant l'identifiant du compte et le code de validation.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si l'activation réussit (204).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le code est invalide (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si le compte n'existe pas (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("ActiverA2f")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActiverA2f([FromBody] A2fActivationDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var compte = await _manager.GetByIdAsync(dto.IdCompte);

        if (compte == null)
            return NotFound();

        // Note : La vérification du code doit être faite via TokenEmailController/VerifCode
        // avant d'appeler cette méthode. Le DTO devrait contenir une confirmation que le code
        // a été validé, ou cette méthode devrait être appelée après validation réussie.

        await _manager.ActiverA2f(dto.IdCompte);
        await _journalService.LogActionAsync(dto.IdCompte, 15, "L'utilisateur à activer l'A2F");

        return NoContent();
    }

    /// <summary>
    /// Désactive l'A2F pour un compte.
    /// </summary>
    /// <param name="idCompte">Identifiant du compte.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la désactivation réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si le compte n'existe pas (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("DesactiverA2f")]
    [HttpPut("{idCompte}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DesactiverA2f(int idCompte)
    {
        var compte = await _manager.GetByIdAsync(idCompte);

        if (compte == null)
            return NotFound();

        await _manager.DesactiverA2f(idCompte);
        await _journalService.LogActionAsync(idCompte, 16, "L'utilisateur à activer l'A2F");

        return NoContent();
    }

    /// <summary>
    /// Démarre le processus d'activation A2F en envoyant un code par email.
    /// </summary>
    /// <param name="idCompte">Identifiant du compte.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Message de confirmation (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si le compte n'existe pas (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("DemanderActivationA2f")]
    [HttpPost("{idCompte}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DemanderActivationA2f(int idCompte)
    {
        var compte = await _manager.GetByIdAsync(idCompte);

        if (compte == null)
            return NotFound();

        // Génération et envoi du code via l'API TokenEmail
        // Cette méthode devrait appeler le service TokenEmail
        // Pour l'instant, on retourne juste un message

        return Ok(new { Message = "Un code d'activation a été envoyé à votre adresse email." });
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

            IdTypeCompte = 1,
            IdEtatCompte = 1
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
            IdEtatCompte = entityToUpdate.IdEtatCompte,
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
    public async Task<bool> VerifUser([FromBody] ChangementMdpDTO dto)
    {
        string hash = ComputeSha256Hash(dto.MotDePasse);
        var result = await _manager.VerifMotDePasse(dto.Email, hash) is not null;

        if (result)
        {
            return true;
        }
        return false;
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
            new Claim("role", compte.IdTypeCompte == 3? "Admin" : "Authorized"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("idUser", compte.IdCompte.ToString()),
        };
        
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(15),
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

#region Méthodes Helper
    /// <summary>
    /// Définit les cookies d'authentification
    /// </summary>
    private void SetAuthCookies(string accessToken, string refreshToken, bool rememberMe)
    {
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            Expires = rememberMe ? DateTimeOffset.UtcNow.AddMinutes(15) : null,
            Path = "/"
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            // ✅ Si rememberMe = true : 30 jours, sinon : session cookie
            Expires = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null,
            Path = "/"
        };

        Response.Cookies.Append("access_token", accessToken, accessCookieOptions);
        Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
    }

    /// <summary>
    /// Génère un refresh token aléatoire sécurisé
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Envoie un email avec le code A2F
    /// </summary>
    private async Task EnvoyerEmailA2f(string email, string code, bool isReactivation)
    {
        string sujet = isReactivation
            ? "Réactivation A2F requise"
            : "Code de connexion A2F";

        string message = isReactivation
            ? $"Votre authentification à deux facteurs doit être réactivée.\nCode : {code}"
            : $"Votre code de connexion A2F : {code}";

        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("AutoPulse", "no-reply@autopulse.com"));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = sujet;
        emailMessage.Body = new TextPart("plain") { Text = message };

        string user = config["Email:GmailUser"];
        string password = config["Email:GmailPass"];

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(user, password);
        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }
#endregion
}