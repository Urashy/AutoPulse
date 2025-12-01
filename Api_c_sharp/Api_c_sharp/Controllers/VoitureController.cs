using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using AutoMapper;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les voitures.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class VoitureController(VoitureManager _manager, IMapper _mapper) : ControllerBase
{
    /// <summary>
    /// Récupère une voiture à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la voiture recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="VoitureDetailDTO"/> si la voiture existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune voiture ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<VoitureDetailDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _mapper.Map<VoitureDetailDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les voitures.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="VoitureDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VoitureDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<VoitureDTO>>(_mapper.Map<IEnumerable<VoitureDTO>>(list));
    }

    /// <summary>
    /// Crée une nouvelle voiture.
    /// </summary>
    /// <param name="dto">Objet <see cref="avis"/> contenant les informations des voitures à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la voiture créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<VoitureDTO>> Post([FromBody] VoitureCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Voiture>(dto);
        entity.MiseEnCirculation = DateTime.SpecifyKind(dto.MiseEnCirculation, DateTimeKind.Utc);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdVoiture }, entity);
    }

    /// <summary>
    /// Met à jour une voiture existant.
    /// </summary>
    /// <param name="id">Identifiant unique de la voiture à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="VoitureCreateDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun avis ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] VoitureCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _mapper.Map<Voiture>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime un avis existant.
    /// </summary>
    /// <param name="id">Identifiant unique de la annonce à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun avis ne correspond (404).</description></item>
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

}