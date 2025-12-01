using Api_c_sharp.Models.Repository.Interfaces;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les categories.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class CategorieController(CategorieManager _manager, IMapper _categorieMapper) : ControllerBase
{
    /// <summary>
    /// Récupère une categorie à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="CategorieDTO"/> si la categorie existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune categorie ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<CategorieDTO>> GetById(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<CategorieDTO>(_categorieMapper.Map<CategorieDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de toutes les categories.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="CategorieDTO"/> (200 OK).
    /// </returns>
    [HttpGet]
    [ActionName("GetAll")]
    public async Task<ActionResult<IEnumerable<CategorieDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<CategorieDTO>>(_categorieMapper.Map<IEnumerable<CategorieDTO>>(list));
    }
}