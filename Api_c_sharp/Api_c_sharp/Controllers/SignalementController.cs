using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;

namespace App.Controllers;

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
    [ActionName("GetById")]
    [HttpGet("{id}")]
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
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SignalementDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<SignalementDTO>>(
            _mapper.Map<IEnumerable<SignalementDTO>>(list));
    }

    /// <summary>
    /// Crée un nouveau signalement (annonce ou compte).
    /// </summary>
    [ActionName("Post")]
    [HttpPost]
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
        string compteId = User.FindFirst("idUser")?.Value;
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
            await _journalService.LogSignalementAsync(
                dto.IdCompteSignalant,
                dto.IdAnnonceSignale.Value,
                entity.IdSignalement,
                dto.IdTypeSignalement,
                dto.DescriptionSignalement);
        }
        else if (dto.IdCompteSignale.HasValue)
        {
            await _journalService.LogSignalementAsync(
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
    [ActionName("Put")]
    [HttpPut("{id}")]
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
    /// Récupère les signalements par état.
    /// </summary>
    [ActionName("GetAllByEtatSignalement")]
    [HttpGet("{idetatsignalement}")]
    public async Task<ActionResult<IEnumerable<SignalementDTO>>> GetAllByEtatSignalement(int idetatsignalement)
    {
        var result = await _manager.GetSignalementsByEtat(idetatsignalement);

        if (result is null || !result.Any())
            return NotFound();

        return new ActionResult<IEnumerable<SignalementDTO>>(
            _mapper.Map<IEnumerable<SignalementDTO>>(result));
    }

    /// <summary>
    /// Met à jour l'état d'un signalement.
    /// </summary>
    /// <param name="idSignalement">ID du signalement</param>
    /// <param name="nouvelEtat">Nouvel état (1=En attente, 2=Traité, 3=Rejeté)</param>
    [ActionName("UpdateEtat")]
    [HttpPut("{idSignalement}/{nouvelEtat}")]
    public async Task<ActionResult> UpdateEtat(int idSignalement, int nouvelEtat)
    {
        var signalement = await _manager.GetByIdAsync(idSignalement);

        if (signalement == null)
            return NotFound();

        // Valider que le nouvel état est valide
        if (nouvelEtat < 1 || nouvelEtat > 3)
            return BadRequest("État invalide. Doit être 1 (En attente), 2 (Traité) ou 3 (Rejeté)");

        signalement.IdEtatSignalement = nouvelEtat;

        await _manager.UpdateAsync(signalement, signalement);

        return NoContent();
    }
}