using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Api_c_sharp.Models.Entity;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les avis.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class ConversationController(
    ConversationManager _manager, 
    IConversationEnrichmentService _enrichmentService,
    IMapper _mapper) : ControllerBase
{
    /// <summary>
    /// Récupère une conversation à partir de son identifiant.
    /// </summary>
    [ActionName("GetById")]
    [HttpGet("{id}")]
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
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConversationListDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<ConversationListDTO>>(_mapper.Map<IEnumerable<ConversationListDTO>>(list));
    }

    /// <summary>
    /// Crée une nouvelle conversation.
    /// </summary>
    [ActionName("Post")]
    [HttpPost]
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
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] ConversationCreateDTO dto)
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
    /// Récupère la liste de toutes les conversations en fonction d'un compte.
    /// Utilise un service dédié pour enrichir les données (participants, messages non lus).
    /// </summary>
    [ActionName("GetConversationsByCompteID")]
    [HttpGet("{idcompte}")]
    public async Task<ActionResult<IEnumerable<ConversationListDTO>>> GetConversationsByCompteID(int idcompte)
    {
        var conversations = await _manager.GetConversationsByCompteID(idcompte);

        if (conversations is null || !conversations.Any())
            return NotFound();

        // Utiliser le service d'enrichissement pour ajouter les informations manquantes
        var result = await _enrichmentService.EnrichConversationsAsync(conversations, idcompte);

        return new ActionResult<IEnumerable<ConversationListDTO>>(result);
    }
}