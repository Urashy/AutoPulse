using Api_c_sharp.Models.Repository.Interfaces;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les modeles blender.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class ModeleBlenderController(ModeleBlenderManager _manager, IMapper _modeldeblendermapper) : ControllerBase
{
    /// <summary>
    /// Récupère un modele blender à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="ModeleBlenderDTO"/> si la motricité existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune motricité ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ModeleBlenderDTO>> GetById(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<ModeleBlenderDTO>(_modeldeblendermapper.Map<ModeleBlenderDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de toutes les modeles blender.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="ModeleBlenderDTO"/> (200 OK).
    /// </returns>
    [HttpGet]
    [ActionName("GetAll")]
    public async Task<ActionResult<IEnumerable<ModeleBlenderDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<ModeleBlenderDTO>>(_modeldeblendermapper.Map<IEnumerable<ModeleBlenderDTO>>(list));
    }
}