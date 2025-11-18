using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;

namespace App.Controllers;

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
    /// Récupère une marque à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="MotriciteDTO"/> si la motricité existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune motricité ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
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
    public async Task<ActionResult<IEnumerable<MotriciteDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<MotriciteDTO>>(_motriciteMapper.Map<IEnumerable<MotriciteDTO>>(list));
    }
}