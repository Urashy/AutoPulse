using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les annonces.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class AnnonceController(AnnonceManager _manager, IMapper _annonceMapper) : ControllerBase
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
    public async Task<ActionResult<AnnonceDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _annonceMapper.Map<AnnonceDTO>(result);
    }

    /// <summary>
    /// Récupère une annonce à partir de son nom exact (insensible à la casse).
    /// </summary>
    /// <param name="str">Nom de l'annonce recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AnnonceDTO"/> si l'annonce existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByString")]
    [HttpGet("{str}")]
    public async Task<ActionResult<AnnonceDTO>> GetByString(string str)
    {
        var result = await _manager.GetByNameAsync(str);

        if (result is null)
        {
            return NotFound();
        }

        return _annonceMapper.Map<AnnonceDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les annonces.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="AnnonceDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
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
    public async Task<ActionResult<AnnonceDTO>> Post([FromBody] AnnonceCreateUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var entity = _annonceMapper.Map<Annonce>(dto);
        entity.DatePublication = DateTime.SpecifyKind(dto.DatePublication, DateTimeKind.Utc);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdAnnonce }, entity);
    }

    /// <summary>
    /// Met à jour une annonce existante.
    /// </summary>
    /// <param name="id">Identifiant unique de l'annonce à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="AnnonceDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune annonce ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] AnnonceDTO dto)
    {
        if (id != dto.IdAnnonce)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _annonceMapper.Map<Annonce>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

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
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _manager.GetByIdAsync(id);

        if (entity == null)
            return NotFound();

        await _manager.DeleteAsync(entity);
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
    public async Task<ActionResult<List<AnnonceDTO>>> GetByIdMiseEnAvant(int idmiseenavant)
    {
        var result = await _manager.GetAnnoncesByMiseEnAvant(idmiseenavant);

        if (result == null || !result.Any())
            return NotFound();

        var dtos = _annonceMapper.Map<List<AnnonceDTO>>(result);
        return Ok(dtos);
    }

    /// <summary>
    /// Récupère une liste d'annonces filtrées selon plusieurs critères, avec pagination.
    /// </summary>
    /// <param name="id">Identifiant de l'annonce (optionnel).</param>
    /// <param name="idcarburant">Identifiant du type de carburant (optionnel).</param>
    /// <param name="idmarque">Identifiant de la marque (optionnel).</param>
    /// <param name="idmodele">Identifiant du modèle (optionnel).</param>
    /// <param name="prixmin">Prix minimum (optionnel).</param>
    /// <param name="prixmax">Prix maximum (optionnel).</param>
    /// <param name="idtypevoiture">Identifiant du type de voiture (optionnel).</param>
    /// <param name="idtypevendeur">Identifiant du type de vendeur (optionnel).</param>
    /// <param name="nom">Nom ou mot-clé de recherche (optionnel).</param>
    /// <param name="kmmin">Kilométrage minimum (optionnel).</param>
    /// <param name="kmmax">Kilométrage maximum (optionnel).</param>
    /// <param name="departement">Code postal du département (optionnel).</param>
    /// <param name="pageNumber">Numéro de la page (commence à 1, par défaut: 1).</param>
    /// <param name="pageSize">Nombre d'annonces par page (par défaut: 21).</param>
    /// <returns>
    /// Une liste de <see cref="AnnonceDTO"/> correspondant aux critères de recherche (200 OK).
    /// </returns>
    [ActionName("GetFiltered")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AnnonceDTO>>> GetFiltered(
        [FromQuery] int? id = null,
        [FromQuery] int? idcarburant = null,
        [FromQuery] int? idmarque = null,
        [FromQuery] int? idmodele = null,
        [FromQuery] int? prixmin = null,
        [FromQuery] int? prixmax = null,
        [FromQuery] int? idtypevoiture = null,
        [FromQuery] int? idtypevendeur = null,
        [FromQuery] string? nom = null,
        [FromQuery] int? kmmin = null,
        [FromQuery] int? kmmax = null,
        [FromQuery] string? departement = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 21)
    {
        var result = await _manager.GetFilteredAnnonces(
            id ?? 0,
            idcarburant ?? 0,
            idmarque ?? 0,
            idmodele ?? 0,
            prixmin ?? 0,
            prixmax ?? 0,
            idtypevoiture ?? 0,
            idtypevendeur ?? 0,
            nom ?? string.Empty,
            kmmin ?? 0,
            kmmax ?? 0,
            departement ?? string.Empty,
            pageNumber,
            pageSize
        );
        return Ok(_annonceMapper.Map<IEnumerable<AnnonceDTO>>(result));
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
    [HttpGet("{idmiseenavant}")]
    public async Task<ActionResult<AnnonceDTO>> GetByCompteFavoris(int compteid)
    {
        var result = await _manager.GetAnnoncesByCompteFavoris(compteid);

        if (result is null)
            return NotFound();

        return _annonceMapper.Map<AnnonceDTO>(result);
    }
}