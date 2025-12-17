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
/// Contrôleur REST permettant de gérer les pays.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class PaysController(PaysManager _manager, IMapper _paysMapper) : ControllerBase
{
    /// <summary>
    /// Récupère un pays à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du pays recherchée.</param>
    /// <returns>
    /// <item><description><see cref="PaysDTO"/> si la pays existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun pays ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<PaysDTO>> GetById(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<PaysDTO>(_paysMapper.Map<PaysDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de toutes les pays.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="PaysDTO"/> (200 OK).
    /// </returns>
    [HttpGet]
    [ActionName("GetAll")]
    public async Task<ActionResult<IEnumerable<PaysDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<PaysDTO>>(_paysMapper.Map<IEnumerable<PaysDTO>>(list));
    }
}