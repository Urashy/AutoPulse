using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using MailKit.Net.Smtp;
using MimeKit;

namespace Api_c_sharp.Controllers;

[Route("api/[controller]/[action]")]
public class ReinitialisationMotDePasseController(ReinitialisationMotDePasseManager _manager, IConfiguration _config) : ControllerBase
{
    /// <summary>
    /// Récupère un modele à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="ModeleDTO"/> si le modele existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun modele ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ReinitialisationMotDePasse>> Get(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<ReinitialisationMotDePasse>(result);
    }

    /// <summary>
    /// Récupère la liste de tous les modèles.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="ModeleDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReinitialisationMotDePasse>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<ReinitialisationMotDePasse>>(list);
    }
    
    /// <summary>
    /// Récupère une annonce à partir de son nom exact (insensible à la casse).
    /// </summary>
    /// <param name="str">Nom de l'annonce recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AnnonceDTO"/> si l'annonce existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByString")]
    [HttpGet("{str}")]
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
    /// Crée une nouveau conversation.
    /// </summary>
    /// <param name="dto">Objet <see cref="ConversationCreateDTO"/> contenant les informations du conversation à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec le conversation créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<ReinitialisationMotDePasse>> Post([FromBody] ReinitialiseMdpDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var resetToken = Guid.NewGuid().ToString();
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
    /// Met à jour un Conversation existant.
    /// </summary>
    /// <param name="id">Identifiant unique de la annonce à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="Conversation"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune Conversation ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
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
    [HttpDelete("{token}")]
    public async Task<IActionResult> Delete(string token)
    {
        var entity = await _manager.GetByNameAsync(token);

        if (entity == null)
            return NotFound();

        await _manager.DeleteAsync(entity);
        return NoContent();
    }

    [ActionName("VerifCode")]
    [HttpPost]
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