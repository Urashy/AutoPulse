using Api_c_sharp.Models.Repository.Interfaces;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les moyens de paiement.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class MoyenPaiementController(MoyenPaiementManager _manager, IMapper _mapper) : ControllerBase
{
    /// <summary>
    /// Récupère un moyen de paiement à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du modele recherchée.</param>
    /// <returns>
    /// <item><description><see cref="MoyenPaiementDTO"/> si le moyen de paiement existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune moyen de paiement ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MoyenPaiementDTO>> GetById(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return new ActionResult<MoyenPaiementDTO>(_mapper.Map<MoyenPaiementDTO>(result));
    }

    /// <summary>
    /// Récupère la liste de toutes les moyens de paiement.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="MoyenPaiementDTO"/> (200 OK).
    /// </returns>
    [HttpGet]
    [ActionName("GetAll")]
    public async Task<ActionResult<IEnumerable<MoyenPaiementDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<MoyenPaiementDTO>>(_mapper.Map<IEnumerable<MoyenPaiementDTO>>(list));
    }
}