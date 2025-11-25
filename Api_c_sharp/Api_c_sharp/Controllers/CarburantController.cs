using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les carburants.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class CarburantController(CarburantManager _manager, IMapper _carburantMapper) : ControllerBase
{
    /// <summary>
    /// Récupère une carburant à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="CarburantDTO"/> si la carburant existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune carburant ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<CarburantDTO>> GetById(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<CarburantDTO>(_carburantMapper.Map<CarburantDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de toutes les carburants.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="carburantDTO"/> (200 OK).
    /// </returns>
    [HttpGet]
    [ActionName("GetAll")]
    public async Task<ActionResult<IEnumerable<CarburantDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<CarburantDTO>>(_carburantMapper.Map<IEnumerable<CarburantDTO>>(list));
    }
}