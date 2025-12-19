using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Api_c_sharp.Hubs;
using Api_c_sharp.Models.Entity;
using Microsoft.AspNetCore.SignalR;

namespace Api_c_sharp.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les conversations.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class ConversationController(
    ConversationManager _manager, 
    IConversationEnrichmentService _enrichmentService,
    IMapper _mapper,
    IHubContext<MessageHub> _hubContext = null) : ControllerBase
{
    /// <summary>
    /// Récupère une conversation à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la conversation.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="ConversationDetailDTO"/> si la conversation existe (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune conversation ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConversationDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDetailDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _mapper.Map<ConversationDetailDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les conversations.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de <see cref="ConversationListDTO"/> (200).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConversationListDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConversationListDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<ConversationListDTO>>(_mapper.Map<IEnumerable<ConversationListDTO>>(list));
    }

    /// <summary>
    /// Crée une nouvelle conversation.
    /// </summary>
    /// <param name="dto">Objet <see cref="ConversationCreateDTO"/> contenant les informations de la conversation.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la conversation créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    [ProducesResponseType(typeof(ConversationDetailDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConversationDetailDTO>> Post([FromBody] ConversationCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Conversation>(dto);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdConversation }, entity);
    }

    /// <summary>
    /// Met à jour une conversation existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la conversation à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="ConversationUpdateDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si le modèle est invalide (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune conversation ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put(int id, [FromBody] ConversationUpdateDTO dto)
    {
        if(!ModelState.IsValid)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _mapper.Map<Conversation>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime une conversation existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la conversation à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune conversation ne correspond (404).</description></item>
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
    /// Récupère les conversations associées à un compte.
    /// Les données sont enrichies (participants, messages non lus).
    /// </summary>
    /// <param name="idcompte">Identifiant unique du compte.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de <see cref="ConversationListDTO"/> si des conversations existent (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune conversation n’est associée au compte (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetConversationsByCompteID")]
    [HttpGet("{idcompte}")]
    [ProducesResponseType(typeof(IEnumerable<ConversationListDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ConversationListDTO>>> GetConversationsByCompteID(int idcompte)
    {
        var conversations = await _manager.GetConversationsByCompteID(idcompte);

        if (conversations is null || !conversations.Any())
            return NotFound();

        var result = await _enrichmentService.EnrichConversationsAsync(conversations, idcompte);

        return new ActionResult<IEnumerable<ConversationListDTO>>(result);
    }

    /// <summary>
    /// Crée une nouvelle conversation entre deux comptes et envoie le premier message.
    /// Notifie les participants via SignalR.
    /// </summary>
    /// <param name="idcompteenvoi">Identifiant du compte émetteur.</param>
    /// <param name="idcompterecoi">Identifiant du compte destinataire.</param>
    /// <param name="dto">Objet <see cref="ConversationCreateDTO"/> contenant la conversation et le premier message.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la conversation créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost("{idcompteenvoi}/{idcompterecoi}")]
    [ProducesResponseType(typeof(ConversationListDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConversationListDTO>> PostComplet(
        [FromRoute] int idcompteenvoi,
        [FromRoute] int idcompterecoi,
        [FromBody] ConversationCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Conversation>(dto);
        await _manager.PostComplet(entity, dto.message,idcompteenvoi,idcompterecoi);
        
        if (_hubContext != null)
        {
            await MessageHub.NotifyNewConversation(
                _hubContext,
                entity.IdConversation,
                idcompteenvoi,
                idcompterecoi,
                dto.message
            );
            
            await _hubContext.Clients.Group($"conversation_{entity.IdConversation}")
                .SendAsync("ReceiveMessage",
                    entity.IdConversation,
                    idcompterecoi,
                    dto.message,
                    entity.DateDernierMessage);
        }

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdConversation }, entity);
    }
}