using Api_c_sharp.Models.Repository.Interfaces;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using MailKit.Net.Smtp;
using MimeKit;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Controllers;

[Route("api/[controller]/[action]")]
public class ReinitialisationMotDePasseController(ReinitialisationMotDePasseManager _manager, IConfiguration _config) : ControllerBase
{
    /// <summary>
    /// Récupère une demande de réinitialisation de mot de passe à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la demande de réinitialisation.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>La demande de réinitialisation correspondante (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune demande ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReinitialisationMotDePasse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReinitialisationMotDePasse>> Get(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<ReinitialisationMotDePasse>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les demandes de réinitialisation de mot de passe.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de demandes de réinitialisation (200).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReinitialisationMotDePasse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReinitialisationMotDePasse>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<ReinitialisationMotDePasse>>(list);
    }

    /// <summary>
    /// Récupère une demande de réinitialisation de mot de passe à partir de son token.
    /// </summary>
    /// <param name="str">Token de réinitialisation.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>La demande de réinitialisation correspondante (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun token ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByString")]
    [HttpGet("{str}")]
    [ProducesResponseType(typeof(ReinitialisationMotDePasse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReinitialisationMotDePasse>> GetByString(string str)
    {
        var result = await _manager.GetByNameAsync(str);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Crée une demande de réinitialisation de mot de passe et envoie un code par email.
    /// </summary>
    /// <param name="dto">Objet <see cref="ReinitialiseMdpDTO"/> contenant l'email et l'identifiant du compte.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Message de confirmation indiquant qu'un code a été envoyé (200).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    [ProducesResponseType(typeof(ReinitialisationMotDePasse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReinitialisationMotDePasse>> Post([FromBody] ReinitialiseMdpDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Random rand = new Random();
        var resetToken = rand.Next(0, 9999999).ToString();
        var expiration = DateTime.UtcNow.AddMinutes(15);

        ReinitialisationMotDePasse reinitMdp = new ReinitialisationMotDePasse()
        {
            IdReinitialisationMdp = 0,
            IdCompte = dto.IdCompte,
            Email = dto.Email,
            Token = resetToken,
            Expiration = expiration,
            Utilise = false
        };

        await _manager.AddAsync(reinitMdp);
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("AutoPulse", "no-reply@autopulse.com"));
        message.To.Add(new MailboxAddress("", reinitMdp.Email));
        message.Subject = "Réinitialisation de mot de passe";
        message.Body = new TextPart("plain")
        {
            Text = $"Bonjour {reinitMdp.Email},\n\n" +
                   $"Voici votre code de réinitialisation : {resetToken}\n\n" +
                   $"Ce code expirera dans 15 minutes."
        };
        
        string user = _config["Email:GmailUser"];
        string password = _config["Email:GmailPass"];

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(user, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        return Ok(new { Message = "Si cet email existe, un code de réinitialisation a été envoyé." });

        return CreatedAtAction(nameof(Get), new { id = reinitMdp.IdReinitialisationMdp }, reinitMdp);
    }

    /// <summary>
    /// Met à jour une demande de réinitialisation de mot de passe existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la demande de réinitialisation.</param>
    /// <param name="dto">Objet <see cref="ReinitialisationMotDePasse"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si les données sont invalides (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune demande ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put(int id, [FromBody] ReinitialisationMotDePasse dto)
    {
        if(!ModelState.IsValid)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        await _manager.UpdateAsync(toUpdate, dto);

        return NoContent();
    }

    /// <summary>
    /// Supprime une demande de réinitialisation de mot de passe à partir de son token.
    /// </summary>
    /// <param name="token">Token de réinitialisation.</param>
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
    /// Vérifie la validité d'un code de réinitialisation de mot de passe.
    /// </summary>
    /// <param name="dto">Objet <see cref="ReinitialiseMdpDTO"/> contenant l'email et le code.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Confirmation si le code est valide (200).</description></item>
    /// <item><description><see cref="NoContentResult"/> si le code est vide (204).</description></item>
    /// <item><description><see cref="NotFoundObjectResult"/> si le code est invalide ou expiré (404).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("VerifCode")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> VerifCode([FromBody] ReinitialiseMdpDTO dto)
    {
        if(!ModelState.IsValid)
            return BadRequest();

        if (string.IsNullOrEmpty(dto.Code))
            return NoContent();
        
        ReinitialisationMotDePasse entity = await _manager.VerificationCode(dto.Email, dto.Code);
        
        if (entity == null || entity.Expiration < DateTime.UtcNow)
            return NotFound(new { Message = "Code invalide ou expiré" });
        
        return Ok(new { Message = "Code validé avec succès" });
    }
}