using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les avis.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class AvisController(AvisManager _manager, IMapper _mapper, IJournalService _journalService) : ControllerBase
{
    /// <summary>
    /// Récupère un avis à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la annonce recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AvisDetailDTO"/> si l'avi existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun avis ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AvisDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvisDetailDTO>> GetById(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _mapper.Map<AvisDetailDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les avis.
    /// </summary>
    /// <returns>
    /// Une liste d'avis/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AvisListDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AvisListDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<AvisListDTO>>(_mapper.Map<IEnumerable<AvisListDTO>>(list));
    }

    /// <summary>
    /// Crée une nouveau avis.
    /// </summary>
    /// <param name="dto">Objet avis/> contenant les informations du avis à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec le avis créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    [ProducesResponseType(typeof(AvisDetailDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AvisDetailDTO>> Post([FromBody] AvisCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Avis>(dto);
        await _manager.AddAsync(entity);
        await _journalService.LogDepotAvisAsync(dto.IdJugeur, dto.IdJugee, entity.IdAvis, dto.NoteAvis, dto.ContenuAvis);

        return CreatedAtAction(nameof(GetById), new { id = entity.IdAvis }, entity);
    }

    /// <summary>
    /// Met à jour un avis existant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'avis à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="AvisUpdateDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun avis ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put(int id, [FromBody] AvisUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _mapper.Map<Avis>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime un avis existant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'avis à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun avis ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Delete")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _manager.GetByIdAsync(id);

        if (entity == null)
            return NotFound();

        await _manager.DeleteAsync(entity);
        return NoContent();
    }

    /// <summary>
    /// Récupère des avis à partir d'un compte.
    /// </summary>
    /// <param name="idcompte">Identifiant unique du compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="AvisListDTO"/> si l'avis existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun avis ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAvisByCompteID")]
    [HttpGet("{idcompte}")]
    [ProducesResponseType(typeof(IEnumerable<AvisListDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AvisListDTO>>> GetAvisByCompteId(int idcompte)
    {
        var result = await _manager.GetAvisByCompteId(idcompte);

        if (!result.Any())
            return NotFound();

        return new ActionResult<IEnumerable<AvisListDTO>>(_mapper.Map<IEnumerable<AvisListDTO>>(result));

    }


}