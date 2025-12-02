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
/// Contrôleur REST permettant de gérer les journaux.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class JournalController(JournalManager _manager, IMapper _mapper) : ControllerBase
{
    /// <summary>
    /// Récupère un journal à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la annonce recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="JournalDTO"/> si la annonce existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<JournalDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _mapper.Map<JournalDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les journaux.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="Journal"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JournalDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<JournalDTO>>(_mapper.Map<IEnumerable<JournalDTO>>(list));
    }

    /// <summary>
    /// Crée une nouveau journal.
    /// </summary>
    /// <param name="dto">Objet <see cref="Journal"/> contenant les informations du journal à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec le journal créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<JournalDTO>> Post([FromBody] JournalDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Journal>(dto);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdJournal }, entity);
    }

    /// <summary>
    /// Met à jour un journal existant.
    /// </summary>
    /// <param name="id">Identifiant unique de la annonce à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="JournalDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun journal ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] JournalDTO dto)
    {
        if (ModelState.IsValid == false)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _mapper.Map<Journal>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime un journal existant.
    /// </summary>
    /// <param name="id">Identifiant unique de la annonce à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun journal ne correspond (404).</description></item>
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
    /// Récupère des journaux à partir de son type.
    /// </summary>
    /// <param name="idtype">Identifiant unique du type recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="JournalDTO"/> si le journal existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun journal ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAllByType")]
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<JournalDTO>>> GetAllByType(int idtype)
    {
        var result = await _manager.GetJournalByType(idtype);

        if (result is null)
            return NotFound();

        return new ActionResult<IEnumerable<JournalDTO>>(_mapper.Map<IEnumerable<JournalDTO>>(result));

    }


}