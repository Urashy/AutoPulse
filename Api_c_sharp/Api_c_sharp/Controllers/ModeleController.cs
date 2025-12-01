using Api_c_sharp.Models.Repository.Interfaces;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les marques.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class ModeleController(ModeleManager _manager, IMapper _marqueMapper) : ControllerBase
{
    /// <summary>
    /// Récupère un modele à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="ModeleDTO"/> si le modele existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun modele ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ModeleDTO>> Get(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<ModeleDTO>(_marqueMapper.Map<ModeleDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de tous les modèles.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="ModeleDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModeleDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<ModeleDTO>>(_marqueMapper.Map<IEnumerable<ModeleDTO>>(list));
    }

    /// <summary>
    /// Récupère la liste de tous les modèles en fonction d'une marque.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="ModeleDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAllByMarque")]
    [HttpGet("{marqueId}")]
    public async Task<ActionResult<IEnumerable<ModeleDTO>>> GetAllByMarque(int marqueId)
    {
        var list = await _manager.GetModelesByMarqueIdAsync(marqueId);
        if (list is null)
        {
            return NotFound();
        }

        return new ActionResult<IEnumerable<ModeleDTO>>(_marqueMapper.Map<IEnumerable<ModeleDTO>>(list));
    }
}