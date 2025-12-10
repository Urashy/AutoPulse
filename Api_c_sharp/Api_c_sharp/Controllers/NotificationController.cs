using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer les adresses.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NotificationController(NotificationManager _manager, IMapper _mapper,CompteManager _comptemanager) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle adresse.
        /// </summary>
        /// <param name="dto">Objet <see cref="NotificationCreateDTO"/> contenant les informations de l'adresse à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec l'adresse créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        public async Task<ActionResult<NotificationCreateDTO>> Post([FromBody] NotificationCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<Notification>(dto);
            await _manager.AddAsync(entity);

            return CreatedAtAction(nameof(GetByID), new { id = entity.IdNotification }, entity);
        }

        /// <summary>
        /// Met à jour une Adresse existante.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse à mettre à jour.</param>
        /// <param name="dto">Objet <see cref="NotificationCreateDTO"/> contenant les nouvelles valeurs.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] NotificationCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var toUpdate = await _manager.GetByIdAsync(id);

            if (toUpdate == null)
                return NotFound();

            var updatedEntity = _mapper.Map<Notification>(dto);
            await _manager.UpdateAsync(toUpdate, updatedEntity);

            return NoContent();
        }
        /// <summary>
        /// Supprime une adresse existante.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse à supprimer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
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
        /// Récupère la liste de toutes les adresses.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="NotificationDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<NotificationDTO>>(_mapper.Map<IEnumerable<NotificationDTO>>(list));
        }

        /// <summary>
        /// Récupère une adresse à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NotificationDTO"/> si l'adresse existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationDTO>> GetByID(int id)
        {
            var result = await _manager.GetByIdAsync(id);

            if (result is null)
                return NotFound();

            return _mapper.Map<NotificationDTO>(result);
        }

        /// <summary>
        /// Récupère la liste de toutes les adresses.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="NotificationDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetNotificationByCompteID")]
        [HttpGet("{idcompte}")]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetNotificationByCompteID(int idcompte)
        {
            IEnumerable<Notification> list = await _manager.GetNotificationsByCompteAsync(idcompte);

            if(list is null || !list.Any())
                return NotFound();

            return new ActionResult<IEnumerable<NotificationDTO>>(_mapper.Map<IEnumerable<NotificationDTO>>(list));
        }

        /// <summary>
        /// Récupère la liste de toutes les adresses.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="NotificationDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetUnreadNotificationByCompte")]
        [HttpGet("{idcompte}")]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetUnreadNotificationByCompte(int idcompte)
        {
            IEnumerable<Notification> list = await _manager.GetUnreadNotificationsByCompteAsync(idcompte);

            if (list is null || !list.Any())
                return NotFound();

            return new ActionResult<IEnumerable<NotificationDTO>>(_mapper.Map<IEnumerable<NotificationDTO>>(list));
        }

        /// <summary>
        /// Récupère la liste de toutes les adresses.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="NotificationDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetUnreadCountByCompte")]
        [HttpGet("{idcompte}")]
        public async Task<ActionResult<int>> GetUnreadCountByCompte(int idcompte)
        {
            return await _manager.GetUnreadCountAsync(idcompte);
        }

        /// <summary>
        /// Met à jour une Adresse existante.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse à mettre à jour.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("MarkAsRead")]
        [HttpPut("{idNotification}")]
        public async Task<ActionResult> MarkAsRead(int idNotification)
        {
            var toUpdate = await _manager.GetByIdAsync(idNotification);

            if (toUpdate == null)
                return NotFound();

            await _manager.MarkAsReadAsync(idNotification);

            return NoContent();
        }


        /// <summary>
        /// Met à jour une Adresse existante.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse à mettre à jour.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("MarkAllAsRead")]
        [HttpPut("{idCompte}")]
        public async Task<ActionResult> MarkAllAsRead(int idCompte)
        {
            Compte compte = await _comptemanager.GetByIdAsync(idCompte);

            if (compte == null) 
                return NotFound();
            
            await _manager.MarkAllAsReadAsync(idCompte);

            return NoContent();
        }

        /// <summary>
        /// Supprime une adresse existante.
        /// </summary>
        /// <param name="daysold">Identifiant unique de l'adresse à supprimer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Delete")]
        [HttpDelete("{daysold}")]
        public async Task<IActionResult> DeleteOldNotification(int daysold)
        {
            await _manager.DeleteOldNotificationsAsync(daysold);
            return NoContent();
        }
    }
}
