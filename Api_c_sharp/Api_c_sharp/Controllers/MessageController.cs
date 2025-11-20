using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les messages.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class MessageController(MessageManager _manager, IMapper _messagemapper) : ControllerBase
{
    /// <summary>
    /// Récupère un message à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la message recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="MessageDTO"/> si la message existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune message ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MessageDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _messagemapper.Map<MessageDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les messages.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="MessageDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<MessageDTO>>(_messagemapper.Map<IEnumerable<MessageDTO>>(list));
    }

    /// <summary>
    /// Crée une nouveau message.
    /// </summary>
    /// <param name="dto">Objet <see cref="messageDTO"/> contenant les informations de la message à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la message créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<MessageDTO>> Post([FromBody] MessageDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _messagemapper.Map<Message>(dto);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdMessage }, entity);
    }

    /// <summary>
    /// Met à jour un message existant.
    /// </summary>
    /// <param name="id">Identifiant unique de la message à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="messageDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune message ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] MessageDTO dto)
    {
        if (id != dto.IdMessage)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _messagemapper.Map<Message>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime un message existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la message à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune message ne correspond (404).</description></item>
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
    /// Récupère une liste de message à partir d'une conversation.
    /// </summary>
    /// <param name="id">Identifiant unique de la message recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="MessageDTO"/> si la message existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune message ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAllByConversation")]
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetByConversation(int idconversation)
    {
        var result = await _manager.GetMessagesByConversation(idconversation);

        if (result is null)
            return NotFound();

        return new ActionResult<IEnumerable<MessageDTO>>(_messagemapper.Map<IEnumerable<MessageDTO>>(result));

    }


}