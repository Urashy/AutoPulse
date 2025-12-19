using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using Api_c_sharp.Hubs;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Microsoft.AspNetCore.SignalR;

namespace Api_c_sharp.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les annonces.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class AnnonceController(AnnonceManager _manager, IMapper _annonceMapper, IJournalService _journalService, INotificationService _notifService, IHubContext<MessageHub> _hubContext = null) : ControllerBase
{
    /// <summary>
    /// Récupère une annoncs à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'annonce recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AnnonceDTO"/> si l'nnonce existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnnonceDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnonceDetailDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _annonceMapper.Map<AnnonceDetailDTO>(result);
    }

    /// <summary>
    /// Récupère une annonce à partir de son nom exact (insensible à la casse).
    /// </summary>
    /// <param name="str">Nom de l'annonce recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AnnonceDetailDTO"/> si l'annonce existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByString")]
    [HttpGet("{str}")]
    [ProducesResponseType(typeof(AnnonceDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnonceDetailDTO>> GetByString(string str)
    {
        var result = await _manager.GetByNameAsync(str);

        if (result is null)
        {
            return NotFound();
        }

        return _annonceMapper.Map<AnnonceDetailDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les annonces.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="AnnonceDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AnnonceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AnnonceDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<AnnonceDTO>>(_annonceMapper.Map<IEnumerable<AnnonceDTO>>(list));
    }

    /// <summary>
    /// Crée une nouvelle annonce.
    /// </summary>
    /// <param name="dto">Objet <see cref="AnnonceDTO"/> contenant les informations de l'annonce à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec l'annonce créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    [ProducesResponseType(typeof(AnnonceDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnnonceDTO>> Post([FromBody] AnnonceCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var entity = _annonceMapper.Map<Annonce>(dto);
        entity.DatePublication = DateTime.SpecifyKind(dto.DatePublication, DateTimeKind.Utc);
        await _journalService.LogPublicationAnnonceAsync(
           entity.IdCompte,
           entity.IdAnnonce,
           entity.Libelle
        );
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdAnnonce }, entity);
    }

    /// <summary>
    /// Met à jour une annonce existante.
    /// </summary>
    /// <param name="id">Identifiant unique de l'annonce à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="AnnonceUpdateDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put(int id, [FromBody] AnnonceUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();
        
        double oldPrice = toUpdate.Prix;
        double newPrice = dto.Prix;

        var updatedEntity = _annonceMapper.Map<Annonce>(dto);
        
        await _journalService.LogModificationAnnonceAsync(
            toUpdate.IdCompte,
            id,
            toUpdate.Libelle
        );
        
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        if (_hubContext != null)
        {
            if (newPrice < oldPrice)
            {
                await _notifService.NotifAnnonce(updatedEntity.IdAnnonce, oldPrice, newPrice);
                
                await MessageHub.NotifyPriceDrop(
                    _hubContext,
                    id,
                    oldPrice,
                    newPrice,
                    toUpdate.Libelle
                );
        
            }
        }
        
        return NoContent();
    }

    /// <summary>
    /// Supprime une annonce existante.
    /// </summary>
    /// <param name="id">Identifiant unique de l'annonce à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Delete")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _manager.GetByIdAsync(id);

        if (entity == null)
            return NotFound();
        
        await _notifService.NotifSuppressionAnnonce(entity.IdAnnonce);
        bool result = await _manager.DeleteAsync(entity);

        if (!result)
            return BadRequest("La suppression est impossible en raison d'une commande effectué sur cette annonce");
        
        await _journalService.LogSuppressionAnnonceAsync(
            entity.IdCompte,
            id,
            entity.Libelle
        );

        return NoContent();
    }


    /// <summary>
    /// Récupère une annoncs à partir d'un id de mise en avant.
    /// </summary>
    /// <param name="idmiseenavant">Identifiant unique de l'annonce recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AnnonceDTO"/> si les annonce existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByIdMiseEnAvant")]
    [HttpGet("{idmiseenavant}")]
    [ProducesResponseType(typeof(IEnumerable<AnnonceDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AnnonceDTO>>> GetByIdMiseEnAvant(
        int idmiseenavant,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 21)
    {
        var result = await _manager.GetAnnoncesByMiseEnAvant(idmiseenavant,pageNumber,pageSize);

        if (result == null || !result.Any())
            return NotFound();

        return new ActionResult<IEnumerable<AnnonceDTO>>(_annonceMapper.Map<IEnumerable<AnnonceDTO>>(result));
    }

    /// <summary>
    /// Récupère une liste d'annonces filtrées selon plusieurs critères, avec pagination.
    /// </summary>
    /// <param name="param">Paremetre contenant tout les infos de la recherche.</param>
    /// <returns>
    /// Une liste de <see cref="AnnonceDTO"/> correspondant aux critères de recherche (200 OK).
    /// </returns>
    [ActionName("GetFiltered")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AnnonceDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AnnonceDTO>>> GetFiltered(
        [FromQuery] ParametreRecherche param = null)
    {
        IEnumerable<Annonce> result = await _manager.GetFilteredAnnonces(
            param
        );

        if (result == null || !result.Any())
            return NotFound();

        return new ActionResult<IEnumerable<AnnonceDTO>>(_annonceMapper.Map<IEnumerable<AnnonceDTO>>(result));
    }

    /// <summary>
    /// Récupère une annoncs à partir d'un id de mise en avant.
    /// </summary>
    /// <param name="compteid">Identifiant unique de l'annonce recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AnnonceDTO"/> si les annonce existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByCompteFavoris")]
    [HttpGet("{compteid}")]
    [ProducesResponseType(typeof(IEnumerable<AnnonceDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AnnonceDTO>>> GetByCompteFavoris(int compteid)
    {
        var result = await _manager.GetAnnoncesByCompteFavoris(compteid);

        if (result == null || !result.Any())
            return NotFound();

        return new ActionResult<IEnumerable<AnnonceDTO>>(_annonceMapper.Map<IEnumerable<AnnonceDTO>>(result));

    }

    /// <summary>
    /// Récupère la liste de toutes les adresses pour un compte donnée.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="AnnonceDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAnnoncesByCompteId")]
    [HttpGet("{idcompte}")]
    [ProducesResponseType(typeof(IEnumerable<AnnonceDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AnnonceDTO>>> GetAnnoncesByCompteID(int idcompte)
    {
        var list = await _manager.GetAnnoncesByCompteID(idcompte);

        if (list is null || !list.Any())
            return NotFound();

        return new ActionResult<IEnumerable<AnnonceDTO>>(_annonceMapper.Map<IEnumerable<AnnonceDTO>>(list));
    }

    /// <summary>
    /// Vérifie si une annonce est masqué.
    /// </summary>
    /// <param name="idannonce">Identifiant de l'annonce a vérifier</param>
    /// <returns>
    /// <see cref="bool"/> indiquant si l'annonce est en favori (200 OK).
    /// </returns>
    [ActionName("EstMasquer")]
    [HttpGet("{idannonce}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> EstMasque(int idannonce)
    {
        bool result = await _manager.EstMasque(idannonce);
        return result;
    }

    /// <summary>
    /// Récupère la liste de toutes les annonces.
    /// </summary>
    /// <param name="idannonce">Identifiant de l'annonce pour recup les annonces similaires</param>
    /// <returns>
    /// Une liste de <see cref="AnnonceDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetSimilaires")]
    [HttpGet("{idannonce}")]
    [ProducesResponseType(typeof(IEnumerable<AnnonceDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AnnonceDTO>>> GetSimilaires(int idannonce)
    {
        Annonce entity = await _manager.GetByIdAsync(idannonce);

        if (entity is null)
            return NotFound();

        IEnumerable<Annonce> list  = await _manager.GetAnnoncesSimilaires(entity);

        return new ActionResult<IEnumerable<AnnonceDTO>>(_annonceMapper.Map<IEnumerable<AnnonceDTO>>(list));
    }
}