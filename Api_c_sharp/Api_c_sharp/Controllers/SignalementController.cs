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
/// Contrôleur REST permettant de gérer les signalements.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class SignalementController(SignalementManager _manager, IMapper _mapper, IJournalService _journalService) : ControllerBase
{
    /// <summary>
    /// Récupère un signalement à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la signalement recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="SignalementDTO"/> si la signalement existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune signalement ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<SignalementDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _mapper.Map<SignalementDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les signalements.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="SignalementDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SignalementDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<SignalementDTO>>(_mapper.Map<IEnumerable<SignalementDTO>>(list));
    }

    /// <summary>
    /// Crée une nouveau signalements.
    /// </summary>
    /// <param name="dto">Objet <see cref="SignalementCreateDTO"/> contenant les informations de la signalement à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la signalement créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<SignalementCreateDTO>> Post([FromBody] SignalementCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Signalement>(dto);
        await _journalService.LogSignalementAsync(dto.IdCompteSignalant,dto.IdCompteSignale,dto.IdSignalement,dto.IdTypeSignalement,dto.DescriptionSignalement);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdSignalement }, entity);
    }

    /// <summary>
    /// Met à jour un signalements existant.
    /// </summary>
    /// <param name="id">Identifiant unique de la signalement à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="SignalementCreateDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune signalement ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] SignalementCreateDTO dto)
    {
        if (id != dto.IdSignalement)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _mapper.Map<Signalement>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime un signalement existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la signalement à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune signalement ne correspond (404).</description></item>
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
    /// Récupère des signalement à partir de son état.
    /// </summary>
    /// <param name="id">Identifiant unique de la signalement recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="SignalementDTO"/> si la signalement existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune signalement ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAllByEtatSignalement")]
    [HttpGet("{idetatsignalement}")]
    public async Task<ActionResult<IEnumerable<SignalementDTO>>> GetAllByEtatSignalement(int idetatsignalement)
    {
        var result = await _manager.GetSignalementsByEtat(idetatsignalement);

        if (result is null || !result.Any())
            return NotFound();

        return new ActionResult<IEnumerable<SignalementDTO>>(_mapper.Map<IEnumerable<SignalementDTO>>(result));

    }


}