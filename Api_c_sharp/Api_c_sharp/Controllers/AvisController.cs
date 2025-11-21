using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les avis.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class AvisController(AvisManager _manager, IMapper _mapper) : ControllerBase
{
    /// <summary>
    /// Récupère un avis à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la annonce recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AvisDetailDTO"/> si la annonce existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<AvisDetailDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _mapper.Map<AvisDetailDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les avis.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="avis"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvisListDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<AvisListDTO>>(_mapper.Map<IEnumerable<AvisListDTO>>(list));
    }

    /// <summary>
    /// Crée une nouveau avis.
    /// </summary>
    /// <param name="dto">Objet <see cref="avis"/> contenant les informations du avis à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec le avis créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<AvisCreateDTO>> Post([FromBody] AvisCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Avis>(dto);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdAvis }, entity);
    }

    /// <summary>
    /// Met à jour un avis existant.
    /// </summary>
    /// <param name="id">Identifiant unique de la annonce à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="avis"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun avis ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] AvisCreateDTO dto)
    {
        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _mapper.Map<Avis>(dto);
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

    /// <summary>
    /// Récupère des avis à partir de son type.
    /// </summary>
    /// <param name="idtype">Identifiant unique du type recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AvisListDTO"/> si l'avis existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun avis ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAllByType")]
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<AvisListDTO>>> GetAllByType(int idcompte)
    {
        var result = await _manager.GetAvisByCompteId(idcompte);

        if (result is null)
            return NotFound();

        return new ActionResult<IEnumerable<AvisListDTO>>(_mapper.Map<IEnumerable<AvisListDTO>>(result));

    }


}