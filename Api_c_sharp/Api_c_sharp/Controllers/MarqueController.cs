using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.DTO;
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
public class MarqueController(MarqueManager _manager, IMapper _marqueMapper) : ControllerBase
{
    /// <summary>
    /// Récupère une marque à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="MarqueDTO"/> si la marque existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune marque ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<MarqueDTO>> Get(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<MarqueDTO>(_marqueMapper.Map<MarqueDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de toutes les marques.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="MarqueDTO"/> (200 OK).
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarqueDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<MarqueDTO>>(_marqueMapper.Map<IEnumerable<MarqueDTO>>(list));
    }
}