using Api_c_sharp.Hubs;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Api_c_sharp.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les commandes.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class CommandeController(CommandeManager _manager, IMapper _mapper, IJournalService _journalService,AnnonceManager _managerannonce, IHubContext<MessageHub> _hubContext = null) : ControllerBase
{
    /// <summary>
    /// Récupère une commande à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la commande recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CommandeDetailDTO"/> si la commande existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune commande ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CommandeDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandeDetailDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _mapper.Map<CommandeDetailDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les commandes.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="CommandeDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CommandeDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommandeDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<CommandeDTO>>(_mapper.Map<IEnumerable<CommandeDTO>>(list));
    }

    /// <summary>
    /// Crée une nouvelle commande.
    /// </summary>
    /// <param name="dto">Objet <see cref="CommandeCreateDTO"/> contenant les informations de la commande à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la commande créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    [ProducesResponseType(typeof(CommandeDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommandeDTO>> Post([FromBody] CommandeCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Commande>(dto);
        await _manager.AddAsync(entity);
        await _journalService.LogAchatAsync(dto.IdAcheteur,dto.IdVendeur,entity.IdCommande,dto.IdAnnonce,dto.IdMoyenPaiement);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdCommande }, entity);
    }

    /// <summary>
    /// Met à jour une commande existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la commande à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="CommandeUpdateDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune commande ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put(int id, [FromBody] CommandeUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        // ✅ Mémoriser l'ancien état AVANT la mise à jour
        var oldStateId = toUpdate.IdEtatCommande;

        // Logique métier pour l'annonce
        if (dto.IdEtatCommande == 5)
        {
            Annonce e = await _managerannonce.GetByIdAsync(dto.IdAnnonce);
            Annonce updated = e;
            updated.IdEtatAnnonce = 2;
            await _managerannonce.UpdateAsync(e, updated);
        }

        var updatedEntity = _mapper.Map<Commande>(dto);

        await _manager.UpdateAsync(toUpdate, updatedEntity);

        if (_hubContext != null && dto.IdEtatCommande != oldStateId)
        {
            var commandeUpdated = await _manager.GetByIdAsync(id);
            string newStateName = commandeUpdated?.EtatCommandeCommandeNav?.Libelle ?? "État inconnu";

            await MessageHub.NotifyCommandeStateChanged(
                _hubContext,
                dto.IdCommande,
                dto.IdEtatCommande,
                newStateName,  
                dto.IdAcheteur,
                dto.IdVendeur
            );

            Console.WriteLine($"🔔 [Controller] Notification envoyée: Commande {dto.IdCommande} -> État {dto.IdEtatCommande} ({newStateName})");
        }

        return NoContent();
    }

    /// <summary>
    /// Supprime une commande existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la commande à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune commande ne correspond (404).</description></item>
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
    /// Récupère des commandes à partir du compte.
    /// </summary>
    /// <param name="idCompte">Identifiant unique du compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CommandeDTO"/> si le compte a des commande (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetCommandeByCompteID")]
    [HttpGet("{idCompte}")]
    [ProducesResponseType(typeof(IEnumerable<CommandeDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CommandeDTO>>> GetCommandeByCompteID(int idCompte)
    {
        var result = await _manager.GetCommandesByCompteId(idCompte);

        if (result is null || !result.Any())
            return NotFound();

        return new ActionResult<IEnumerable<CommandeDTO>>(_mapper.Map<IEnumerable<CommandeDTO>>(result));
    }

    [ActionName("GetCommandeByConversationId")]
    [HttpGet("{idconv}")]
    [ProducesResponseType(typeof(CommandeDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandeDTO>> GetCommandeByConversationID(int idconv)
    {
        var result = await _manager.GetCommandeByConversation(idconv);

        if (result is null)
            return NotFound();

        return new ActionResult<CommandeDTO>(_mapper.Map<CommandeDTO>(result));
    }


    

}