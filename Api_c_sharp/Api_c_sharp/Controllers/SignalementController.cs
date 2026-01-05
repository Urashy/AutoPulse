using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Npgsql.Internal;
using System.Collections.Generic;

namespace Api_c_sharp.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les signalements (annonces et comptes).
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class SignalementController(
    SignalementManager _manager,
    IMapper _mapper,
    IJournalService _journalService) : ControllerBase
{
    /// <summary>
    /// Récupère un signalement à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique du signalement.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Le signalement correspondant (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun signalement ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SignalementDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SignalementDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _mapper.Map<SignalementDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de tous les signalements.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de <see cref="SignalementDTO"/> (200).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SignalementDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SignalementDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<SignalementDTO>>(
            _mapper.Map<IEnumerable<SignalementDTO>>(list));
    }

    /// <summary>
    /// Crée un nouveau signalement concernant une annonce ou un compte.
    /// </summary>
    /// <param name="dto">Objet <see cref="SignalementCreateDTO"/> contenant les informations du signalement.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec le signalement créé (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si les données sont invalides (400).</description></item>
    /// <item><description><see cref="UnauthorizedResult"/> si l'utilisateur n'est pas authentifié (401).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    [ProducesResponseType(typeof(SignalementDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SignalementDTO>> Post([FromBody] SignalementCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validation : doit signaler SOIT une annonce SOIT un compte
        if (dto.IdAnnonceSignale.HasValue && dto.IdCompteSignale.HasValue)
            return BadRequest("Un signalement ne peut pas cibler à la fois une annonce et un compte");

        if (!dto.IdAnnonceSignale.HasValue && !dto.IdCompteSignale.HasValue)
            return BadRequest("Un signalement doit cibler soit une annonce soit un compte");

        // Récupération de l'ID du compte signalant depuis le token JWT
        string? compteId = User.FindFirst("idUser")?.Value;
        if (string.IsNullOrEmpty(compteId))
            return Unauthorized();

        dto.IdCompteSignalant = int.Parse(compteId);

        var entity = _mapper.Map<Signalement>(dto);
        entity.DateCreationSignalement = DateTime.UtcNow;
        entity.IdEtatSignalement = 1; // En attente

        await _manager.AddAsync(entity);

        // Log dans le journal
        if (dto.IdAnnonceSignale.HasValue)
        {
            await _journalService.LogSignalementAnnonceAsync(
                dto.IdCompteSignalant,
                dto.IdAnnonceSignale.Value,
                entity.IdSignalement,
                dto.IdTypeSignalement,
                dto.DescriptionSignalement);
        }
        else if (dto.IdCompteSignale.HasValue)
        {
            await _journalService.LogSignalementCompteAsync(
                dto.IdCompteSignalant,
                dto.IdCompteSignale.Value,
                entity.IdSignalement,
                dto.IdTypeSignalement,
                dto.DescriptionSignalement);
        }

        var signalementComplet = await _manager.GetByIdAsync(entity.IdSignalement);
        return CreatedAtAction(
            nameof(GetByID),
            new { id = entity.IdSignalement },
            _mapper.Map<SignalementDTO>(signalementComplet));
    }

    /// <summary>
    /// Met à jour un signalement existant.
    /// </summary>
    /// <param name="id">Identifiant unique du signalement.</param>
    /// <param name="dto">Objet <see cref="SignalementUpdateDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si les données sont invalides (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun signalement ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put(int id, [FromBody] SignalementUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _mapper.Map<Signalement>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime un signalement existant.
    /// </summary>
    /// <param name="id">Identifiant unique du signalement.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucun signalement ne correspond (404).</description></item>
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
    /// Récupère les signalements filtrés par état, type et critère de recherche.
    /// </summary>
    /// <param name="etatId">Identifiant de l'état du signalement.</param>
    /// <param name="typeId">Identifiant du type de signalement.</param>
    /// <param name="recherche">Chaîne de recherche optionnelle.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de signalements filtrés (200).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetFilteredSignalement")]
    [HttpGet("{etatId}/{typeId}")]
    [ProducesResponseType(typeof(IEnumerable<SignalementDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SignalementDTO>>> GetFilteredSignalement(int etatId, int typeId, [FromQuery] string? recherche)
    {
        var result = await _manager.GetSignalementsByEtatAndType(etatId, typeId, recherche ?? "");

        return new ActionResult<IEnumerable<SignalementDTO>>(
            _mapper.Map<IEnumerable<SignalementDTO>>(result));
    }

    /// <summary>
    /// Met à jour l'état d'un signalement.
    /// </summary>
    /// <param name="idSignalement">Identifiant unique du signalement.</param>
    /// <param name="dto">Objet <see cref="SignalementUpdateDTO"/> contenant le nouvel état.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si l'état est invalide (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si le signalement n'existe pas (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("UpdateEtat")]
    [HttpPut("{idSignalement}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateEtat(int idSignalement, [FromBody] SignalementUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var signalement = await _manager.GetByIdAsync(idSignalement);

        if (signalement == null)
            return NotFound();

        if (dto.IdEtatSignalement < 1 || dto.IdEtatSignalement > 3)
            return BadRequest("État invalide. Doit être 1 (En attente), 2 (Traité) ou 3 (Rejeté)");

        // Mapper le DTO vers l'entité existante
        var updatedEntity = _mapper.Map<Signalement>(dto);
        await _manager.UpdateAsync(signalement, updatedEntity);

        return NoContent();
    }
}