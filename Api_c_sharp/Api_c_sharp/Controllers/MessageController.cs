using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Api_c_sharp.Hubs;
using Microsoft.AspNetCore.SignalR;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class MessageController(
    MessageManager _manager, 
    IMapper _messagemapper,
    IJournalService _journalService, 
    IHubContext<MessageHub> _hubContext = null) : ControllerBase
{
    /// <summary>
    /// Récupère un message à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du message.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Le message correspondant s'il existe (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun message ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByID")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MessageDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _messagemapper.Map<MessageDTO>(result);
    }

    /// <summary>
    /// Récupère tous les messages.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une liste de <see cref="MessageDTO"/> (200 OK).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MessageDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<MessageDTO>>(_messagemapper.Map<IEnumerable<MessageDTO>>(list));
    }

    /// <summary>
    /// Crée un nouveau message et notifie tous les participants de la conversation via SignalR.
    /// </summary>
    /// <param name="dto">Données nécessaires à la création du message.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/>Le message créé avec succès (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si les données sont invalides (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    [ProducesResponseType(typeof(MessageCreateDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageCreateDTO>> Post([FromBody] MessageCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _messagemapper.Map<Message>(dto);
        entity.DateEnvoiMessage = DateTime.UtcNow;
        entity.EstLu = false;

        await _journalService.LogEnvoiMessageAsync(dto.IdCompte, dto.IdConversation, dto.ContenuMessage);
        await _manager.AddAsync(entity);

        if(_hubContext != null)
        {
            await _hubContext.Clients.Group($"conversation_{entity.IdConversation}")
            .SendAsync("ReceiveMessage",
                entity.IdConversation,
                entity.IdCompte,
                entity.ContenuMessage,
                entity.DateEnvoiMessage);
        }

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdMessage }, entity);
    }


    /// <summary>
    /// Met à jour un message existant.
    /// </summary>
    /// <param name="id">Identifiant unique du message.</param>
    /// <param name="dto">Nouvelles données du message.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si les données sont invalides (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun message ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put(int id, [FromBody] MessageUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _messagemapper.Map<Message>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }



    /// <summary>
    /// Supprime un message.
    /// </summary>
    /// <param name="id">Identifiant unique du message.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun message ne correspond (404).</description></item>
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

    /// <summary>
    /// Récupère tous les messages d'une conversation et les marque automatiquement comme lus.
    /// La mise à jour est effectuée via une fonction stockée en base de données et notifiée via SignalR.
    /// </summary>
    /// <param name="idconversation">Identifiant unique de la conversation.</param>
    /// <param name="iduser">Identifiant unique de l'utilisateur.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de messages marqués comme lus (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun message n'est trouvé (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAllByConversationAndMarkAsRead")]
    [HttpGet("{idconversation}/{iduser}")]
    [ProducesResponseType(typeof(IEnumerable<MessageDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetByConversationAndMarkAsRead(int idconversation, int iduser)
    {
        // Api_c_sharpel de la méthode qui marque les messages comme lus via la fonction BD
        var result = await _manager.GetMessagesByConversationAndMarkAsRead(idconversation, iduser);

        if (result is null || !result.Any())
            return NotFound();

        // Notifier via SignalR que les messages ont été lus
        if (_hubContext != null)
        {
            await _hubContext.Clients.Group($"conversation_{idconversation}")
            .SendAsync("MessagesRead", idconversation, iduser);
        }
        return new ActionResult<IEnumerable<MessageDTO>>(_messagemapper.Map<IEnumerable<MessageDTO>>(result));
    }

    /// <summary>
    /// Récupère le nombre de messages non lus d'une conversation pour un utilisateur.
    /// </summary>
    /// <param name="conversationId">Identifiant unique de la conversation.</param>
    /// <param name="userId">Identifiant unique de l'utilisateur.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Le nombre de messages non lus (200).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetUnreadCount")]
    [HttpGet("{conversationId}/{userId}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUnreadCount(int conversationId, int userId)
    {
        var count = await _manager.GetUnreadMessageCount(conversationId, userId);
        return Ok(count);
    }
}