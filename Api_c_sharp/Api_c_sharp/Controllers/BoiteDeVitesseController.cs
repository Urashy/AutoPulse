using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les boite de vitesses.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class BoiteDeVitesseController(BoiteDeVitesseManager _manager, IMapper _mapper) : ControllerBase
{
    /// <summary>
    /// Récupère une boite de vitesse à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="boite de vitesseDTO"/> si la boite de vitesse existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune boite de vitesse ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<BoiteDeVitesseDTO>> GetById(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<BoiteDeVitesseDTO>(_mapper.Map<BoiteDeVitesseDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de toutes les boite de vitesses.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="boite de vitesseDTO"/> (200 OK).
    /// </returns>
    [HttpGet]
    [ActionName("GetAll")]
    public async Task<ActionResult<IEnumerable<BoiteDeVitesseDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<BoiteDeVitesseDTO>>(_mapper.Map<IEnumerable<BoiteDeVitesseDTO>>(list));
    }
}