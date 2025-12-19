using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer les notifications.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NotificationController(NotificationManager _manager, IMapper _mapper,CompteManager _comptemanager) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle notification.
        /// </summary>
        /// <param name="dto">Objet <see cref="NotificationCreateDTO"/> contenant les informations de la notification à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec la notification créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        [ProducesResponseType(typeof(NotificationCreateDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NotificationCreateDTO>> Post([FromBody] NotificationCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<Notification>(dto);
            await _manager.AddAsync(entity);

            return CreatedAtAction(nameof(GetByID), new { id = entity.IdNotification }, entity);
        }

        /// <summary>
        /// Met à jour une notification existante.
        /// </summary>
        /// <param name="id">Identifiant unique de la notification à mettre à jour.</param>
        /// <param name="dto">Objet <see cref="NotificationUpdateDTO"/> contenant les nouvelles valeurs.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune notification ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(int id, [FromBody] NotificationUpdateDTO dto)
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
        /// Supprime une notification existante.
        /// </summary>
        /// <param name="id">Identifiant unique de la notification à supprimer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune notification ne correspond (404).</description></item>
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
        /// Récupère la liste de toutes les notifications.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="NotificationDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<NotificationDTO>>(_mapper.Map<IEnumerable<NotificationDTO>>(list));
        }

        /// <summary>
        /// Récupère une notification à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de la notification recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NotificationDTO"/> si la notification existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune notification ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NotificationDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NotificationDTO>> GetByID(int id)
        {
            var result = await _manager.GetByIdAsync(id);

            if (result is null)
                return NotFound();

            return _mapper.Map<NotificationDTO>(result);
        }

        /// <summary>
        /// Récupère toutes les notifications associées à un compte.
        /// </summary>
        /// <param name="idcompte">Identifiant unique du compte.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>Une collection de notifications du compte (200).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune notification n'est trouvée (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetNotificationByCompteID")]
        [HttpGet("{idcompte}")]
        [ProducesResponseType(typeof(IEnumerable<NotificationDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetNotificationByCompteID(int idcompte)
        {
            IEnumerable<Notification> list = await _manager.GetNotificationsByCompteAsync(idcompte);

            if(list is null || !list.Any())
                return NotFound();

            return new ActionResult<IEnumerable<NotificationDTO>>(_mapper.Map<IEnumerable<NotificationDTO>>(list));
        }

        /// <summary>
        /// Récupère toutes les notifications non lues associées à un compte.
        /// </summary>
        /// <param name="idcompte">Identifiant unique du compte.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>Une collection de notifications non lues (200).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune notification non lue n'est trouvée (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetUnreadNotificationByCompte")]
        [HttpGet("{idcompte}")]
        [ProducesResponseType(typeof(IEnumerable<NotificationDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetUnreadNotificationByCompte(int idcompte)
        {
            IEnumerable<Notification> list = await _manager.GetUnreadNotificationsByCompteAsync(idcompte);

            if (list is null || !list.Any())
                return NotFound();

            return new ActionResult<IEnumerable<NotificationDTO>>(_mapper.Map<IEnumerable<NotificationDTO>>(list));
        }

        /// <summary>
        /// Récupère le nombre de notifications non lues pour un compte.
        /// </summary>
        /// <param name="idcompte">Identifiant unique du compte.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>Le nombre de notifications non lues (200).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetUnreadCountByCompte")]
        [HttpGet("{idcompte}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetUnreadCountByCompte(int idcompte)
        {
            return await _manager.GetUnreadCountAsync(idcompte);
        }

        /// <summary>
        /// Marque une notification comme lue.
        /// </summary>
        /// <param name="idNotification">Identifiant unique de la notification.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la notification est marquée comme lue (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si la notification n'existe pas (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("MarkAsRead")]
        [HttpPut("{idNotification}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> MarkAsRead(int idNotification)
        {
            var toUpdate = await _manager.GetByIdAsync(idNotification);

            if (toUpdate == null)
                return NotFound();

            await _manager.MarkAsReadAsync(idNotification);

            return NoContent();
        }


        /// <summary>
        /// Marque toutes les notifications d'un compte comme lues.
        /// </summary>
        /// <param name="idCompte">Identifiant unique du compte.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si les notifications sont mises à jour (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si le compte n'existe pas (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("MarkAllAsRead")]
        [HttpPut("{idCompte}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> MarkAllAsRead(int idCompte)
        {
            Compte compte = await _comptemanager.GetByIdAsync(idCompte);

            if (compte == null) 
                return NotFound();
            
            await _manager.MarkAllAsReadAsync(idCompte);

            return NoContent();
        }

        /// <summary>
        /// Supprime les notifications plus anciennes qu'un nombre de jours donné.
        /// </summary>
        /// <param name="daysold">Nombre de jours d'ancienneté des notifications à supprimer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression est effectuée (204).</description></item>
        /// </list>
        /// </returns>
        [ActionName("DeleteOldNotification")]
        [HttpDelete("{daysold}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteOldNotification(int daysold)
        {
            await _manager.DeleteOldNotificationsAsync(daysold);
            return NoContent();
        }
    }
}
