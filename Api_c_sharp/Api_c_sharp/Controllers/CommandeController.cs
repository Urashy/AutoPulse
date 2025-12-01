using AutoPulse.Shared.DTO;
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
/// Contrôleur REST permettant de gérer les commandes.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class CommandeController(CommandeManager _manager, IMapper _mapper) : ControllerBase
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
    public async Task<ActionResult<IEnumerable<CommandeDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<CommandeDTO>>(_mapper.Map<IEnumerable<CommandeDTO>>(list));
    }

    /// <summary>
    /// Crée une nouvelle commande.
    /// </summary>
    /// <param name="dto">Objet <see cref="CommandeDTO"/> contenant les informations de la commande à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la commande créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<Commande>> Post([FromBody] CommandeDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Commande>(dto);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdCommande }, entity);
    }

    /// <summary>
    /// Met à jour une commande existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la commande à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="CommandeDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune commande ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] CommandeDTO dto)
    {
        if (id != dto.IdCommande)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _mapper.Map<Commande>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

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
    /// <param name="idCompte">Identifiant unique du type recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="Commande"/> si la commande existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune commande ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAllByType")]
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<CommandeDTO>>> GetAllByType(int idCompte)
    {
        var result = await _manager.GetCommandeByCompteId(idCompte);

        if (result is null)
            return NotFound();

        return new ActionResult<IEnumerable<CommandeDTO>>(_mapper.Map<IEnumerable<CommandeDTO>>(result));

    }


}