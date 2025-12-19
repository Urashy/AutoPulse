using Api_c_sharp.Models.Repository.Interfaces;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les motricités.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class MotriciteController(MotriciteManager _manager, IMapper _motriciteMapper) : ControllerBase
{
    /// <summary>
    /// Récupère une motricité à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="MotriciteDTO"/> si la motricité existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune motricité ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MotriciteDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MotriciteDTO>> GetById(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<MotriciteDTO>(_motriciteMapper.Map<MotriciteDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de toutes les motricités.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="MotriciteDTO"/> (200 OK).
    /// </returns>
    [HttpGet]
    [ActionName("GetAll")]
    [ProducesResponseType(typeof(IEnumerable<MotriciteDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MotriciteDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<MotriciteDTO>>(_motriciteMapper.Map<IEnumerable<MotriciteDTO>>(list));
    }
}