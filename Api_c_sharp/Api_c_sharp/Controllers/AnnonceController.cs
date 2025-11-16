using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les marques.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class AnnonceController(IDataRepository<Annonce, string> _manager, IMapper _marqueMapper) : ControllerBase
{
    /// <summary>
    /// Récupère une marque à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la marque recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AnnonceDTO"/> si la marque existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune marque ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<AnnonceDTO>> Get(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _marqueMapper.Map<AnnonceDTO>(result);
    }

    /// <summary>
    /// Récupère une marque à partir de son nom exact (insensible à la casse).
    /// </summary>
    /// <param name="str">Nom de la marque recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AnnonceDTO"/> si la marque existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune marque ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [HttpGet("{str}")]
    public async Task<ActionResult<AnnonceDTO>> GetByString(string str)
    {
        var result = await _manager.GetByNameAsync(str);

        if (result is null)
        {
            return NotFound();
        }

        return _marqueMapper.Map<AnnonceDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les marques.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="AnnonceDTO"/> (200 OK).
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AnnonceDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<AnnonceDTO>>(_marqueMapper.Map<IEnumerable<AnnonceDTO>>(list));
    }

    /// <summary>
    /// Crée une nouvelle marque.
    /// </summary>
    /// <param name="dto">Objet <see cref="AnnonceDTO"/> contenant les informations de la marque à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la marque créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [HttpPost]
    public async Task<ActionResult<AnnonceDTO>> Post([FromBody] AnnonceDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _marqueMapper.Map<Annonce>(dto);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(Get), new { id = entity.IdAnnonce }, entity);
    }

    /// <summary>
    /// Met à jour une marque existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la marque à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="AnnonceDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune marque ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] AnnonceDTO dto)
    {
        if (id != dto.IdAnnonce)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _marqueMapper.Map<Annonce>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime une marque existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la marque à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune marque ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _manager.GetByIdAsync(id);

        if (entity == null)
            return NotFound();

        await _manager.DeleteAsync(entity);
        return NoContent();
    }
}