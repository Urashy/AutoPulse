using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Api_c_sharp.Hubs;
using Microsoft.AspNetCore.SignalR;
using Api_c_sharp.Models.Entity;

namespace App.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class MessageController(MessageManager _manager, IMapper _messagemapper,IJournalService _journalService, IHubContext<MessageHub> _hubContext = null ) : ControllerBase
{
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MessageDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _messagemapper.Map<MessageDTO>(result);
    }

    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<MessageDTO>>(_messagemapper.Map<IEnumerable<MessageDTO>>(list));
    }

    /// <summary>
    /// Crée un nouveau message et notifie TOUS les participants via SignalR
    /// </summary>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<MessageCreateDTO>> Post([FromBody] MessageCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _messagemapper.Map<Message>(dto);
        entity.DateEnvoiMessage = DateTime.UtcNow;

        await _journalService.LogEnvoiMessageAsync(dto.IdCompte, dto.IdConversation, dto.ContenuMessage);
        await _manager.AddAsync(entity);

        if (_hubContext != null)
        {
            await _hubContext.Clients.Group($"conversation_{entity.IdConversation}")
                .SendAsync("ReceiveMessage",
                    entity.IdConversation,
                    entity.IdCompte,
                    entity.ContenuMessage,
                    entity.DateEnvoiMessage);
        }

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdMessage }, entity);
    }

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

    [ActionName("GetAllByConversation")]
    [HttpGet("{idconversation}")]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetByConversation(int idconversation)
    {
        var result = await _manager.GetMessagesByConversation(idconversation);

        if (result is null)
            return NotFound();

        return new ActionResult<IEnumerable<MessageDTO>>(_messagemapper.Map<IEnumerable<MessageDTO>>(result));
    }
}