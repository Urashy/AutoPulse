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
/// Contrôleur REST permettant de gérer les etats d'annonces.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class EtatAnnonceController(EtatAnnonceManager _manager, IMapper _mapper) : ControllerBase
{
    /// <summary>
    /// Récupère un etat d'nnonce à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'état recherchée.</param>
    /// <returns>
    /// <item><description><see cref="EtatAnnonceDTO"/> si l'état annonce existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun état ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EtatAnnonceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EtatAnnonceDTO>> GetById(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<EtatAnnonceDTO>(_mapper.Map<EtatAnnonceDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de tous les états d'annonce.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Une collection de <see cref="EtatAnnonceDTO"/> (200 OK).
    /// </description>
    /// </item>
    /// </list>
    /// </returns>
    [HttpGet]
    [ActionName("GetAll")]
    [ProducesResponseType(typeof(IEnumerable<EtatAnnonceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EtatAnnonceDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<EtatAnnonceDTO>>(_mapper.Map<IEnumerable<EtatAnnonceDTO>>(list));
    }
}