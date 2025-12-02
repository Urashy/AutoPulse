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
/// Contrôleur REST permettant de gérer les factures.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class FactureController(FactureManager _manager, IMapper _mapper) : ControllerBase
{
    /// <summary>
    /// Récupère une facture à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la facture recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="FactureDTO"/> si la facture existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune facture ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<FactureDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _mapper.Map<FactureDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les factures.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="FactureDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FactureDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<FactureDTO>>(_mapper.Map<IEnumerable<FactureDTO>>(list));
    }

    /// <summary>
    /// Crée une nouvelle facture.
    /// </summary>
    /// <param name="dto">Objet <see cref="FactureDTO"/> contenant les informations de la facture à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la facture créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<Facture>> Post([FromBody] FactureDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Facture>(dto);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdFacture }, entity);
    }

    /// <summary>
    /// Met à jour une facture existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la facture à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="Facture"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune facture ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] FactureDTO dto)
    {
        if (id != dto.IdFacture)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _mapper.Map<Facture>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime une facture existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la facture à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune facture ne correspond (404).</description></item>
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