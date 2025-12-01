using Api_c_sharp.Models.Repository.Interfaces;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les mise en avants.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class MiseEnAvantController(MiseEnAvantManager _manager, IMapper _mapper) : ControllerBase
{
    /// <summary>
    /// Récupère une mise en avant à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="MiseEnAvantDTO"/> si la mise en avant existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune mise en avant ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MiseEnAvantDTO>> GetById(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<MiseEnAvantDTO>(_mapper.Map<MiseEnAvantDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de toutes les mise en avants.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="MiseEnAvantDTO"/> (200 OK).
    /// </returns>
    [HttpGet]
    [ActionName("GetAll")]
    public async Task<ActionResult<IEnumerable<MiseEnAvantDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<MiseEnAvantDTO>>(_mapper.Map<IEnumerable<MiseEnAvantDTO>>(list));
    }
}