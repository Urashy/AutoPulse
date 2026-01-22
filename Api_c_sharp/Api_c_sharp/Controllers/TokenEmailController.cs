using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using MimeKit;

namespace Api_c_sharp.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les tokens email (réinitialisation MDP, A2F, etc.).
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class TokenEmailController(
    TokenEmailManager _manager,
    IConfiguration _config,
    IMapper _mapper ) : ControllerBase
{
    /// <summary>
    /// Récupère un token email à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du token.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Le token correspondant (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun token ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TokenEmail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TokenEmail>> Get(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Récupère tous les tokens email.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de tokens (200).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TokenEmail>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TokenEmail>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return Ok(list);
    }

    /// <summary>
    /// Récupère un token email à partir de sa valeur.
    /// </summary>
    /// <param name="str">Valeur du token.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Le token correspondant (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun token ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByString")]
    [HttpGet("{str}")]
    [ProducesResponseType(typeof(TokenEmail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TokenEmail>> GetByString(string str)
    {
        var result = await _manager.GetByNameAsync(str);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Crée un token email et envoie un code par email.
    /// </summary>
    /// <param name="dto">Objet contenant l'email, l'identifiant du compte et le type de token.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Message de confirmation indiquant qu'un code a été envoyé (200).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Post([FromBody] TokenEmailCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Random rand = new Random();
        var token = rand.Next(0, 9999999).ToString("D7");
        var expiration = DateTime.UtcNow.AddMinutes(15);

        TokenEmail tokenEmail = _mapper.Map<TokenEmail>(dto);
        tokenEmail.Token = token;
        tokenEmail.Expiration = expiration;
        tokenEmail.Utilise = false;

        await _manager.AddAsync(tokenEmail);

        // Personnalisation du message selon le type de token
        string sujet = dto.TypeToken switch
        {
            "REINIT_MDP" => "Réinitialisation de mot de passe",
            "A2F_ACTIVATION" => "Activation de l'authentification à deux facteurs",
            "A2F_CONNEXION" => "Code de connexion A2F",
            _ => "Code de vérification"
        };

        string corpsMessage = dto.TypeToken switch
        {
            "REINIT_MDP" => $"Bonjour,\n\nVoici votre code de réinitialisation : {token}\n\nCe code expirera dans 15 minutes.",
            "A2F_ACTIVATION" => $"Bonjour,\n\nVoici votre code d'activation A2F : {token}\n\nCe code expirera dans 15 minutes.",
            "A2F_CONNEXION" => $"Bonjour,\n\nVoici votre code de connexion : {token}\n\nCe code expirera dans 15 minutes.",
            _ => $"Bonjour,\n\nVoici votre code de vérification : {token}\n\nCe code expirera dans 15 minutes."
        };

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("AutoPulse", "no-reply@autopulse.com"));
        message.To.Add(new MailboxAddress("", dto.Email));
        message.Subject = sujet;
        message.Body = new TextPart("plain")
        {
            Text = corpsMessage
        };

        string? user = _config["GmailUser"] ?? _config["Email:GmailUser"];
        string? password = _config["GmailPass"] ?? _config["Email:GmailPass"];

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(user, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        return Ok(new { Message = "Un code de vérification a été envoyé par email." });
    }

    /// <summary>
    /// Met à jour un token email existant.
    /// </summary>
    /// <param name="id">Identifiant unique du token.</param>
    /// <param name="dto">Objet contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si les données sont invalides (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun token ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put(int id, [FromBody] TokenEmail dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        await _manager.UpdateAsync(toUpdate, dto);

        return NoContent();
    }

    /// <summary>
    /// Supprime un token email à partir de sa valeur.
    /// </summary>
    /// <param name="token">Valeur du token.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun token ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Delete")]
    [HttpDelete("{token}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string token)
    {
        var entity = await _manager.GetByNameAsync(token);

        if (entity == null)
            return NotFound();

        await _manager.DeleteAsync(entity);
        return NoContent();
    }

    /// <summary>
    /// Vérifie la validité d'un code de vérification.
    /// </summary>
    /// <param name="dto">Objet contenant l'email, le code et le type de token.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Confirmation si le code est valide (200).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si les données sont invalides (400).</description></item>
    /// <item><description><see cref="NotFoundObjectResult"/> si le code est invalide ou expiré (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("VerifCode")]
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> VerifCode([FromBody] TokenEmailVerifDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        if (string.IsNullOrEmpty(dto.Code))
            return BadRequest(new { Message = "Le code est requis" });

        TokenEmail? entity = await _manager.VerificationCode(dto.Email, dto.Code, dto.TypeToken);

        if (entity == null || entity.Expiration < DateTime.UtcNow)
            return NotFound(new { Message = "Code invalide ou expiré" });

        return Ok(new { Message = "Code validé avec succès", IdToken = entity.IdTokenEmail });
    }

    /// <summary>
    /// Marque un token comme utilisé.
    /// </summary>
    /// <param name="id">Identifiant du token.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun token ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("MarquerUtilise")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarquerUtilise(int id)
    {
        var token = await _manager.GetByIdAsync(id);

        if (token == null)
            return NotFound();

        token.Utilise = true;
        await _manager.UpdateAsync(token, token);

        return NoContent();
    }

    /// <summary>
    /// Invalide tous les tokens d'un type pour un compte donné.
    /// </summary>
    /// <param name="idCompte">Identifiant du compte.</param>
    /// <param name="typeToken">Type de token à invalider.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si l'invalidation réussit (204).</description></item>
    /// </list>
    /// </returns>
    [ActionName("InvaliderTokensParType")]
    [HttpPut("{idCompte}/{typeToken}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> InvaliderTokensParType(int idCompte, string typeToken)
    {
        await _manager.InvaliderTokensParType(idCompte, typeToken);
        return NoContent();
    }

    /// <summary>
    /// Nettoie tous les tokens expirés ou déjà utilisés.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si le nettoyage réussit (204).</description></item>
    /// </list>
    /// </returns>
    [ActionName("NettoyerTokensExpires")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> NettoyerTokensExpires()
    {
        await _manager.NettoyerTokensExpires();
        return NoContent();
    }
}