using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer la table de jointure entre compte et annonce pour les vues.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VueController(VueManager _manager, IMapper _vuemapper) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle vue d'une annonce.
        /// </summary>
        /// <param name="dto">Objet <see cref="VueDTO"/> contenant les informations de l'adresse à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec l'adresse créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        public async Task<ActionResult<VueDTO>> Post([FromBody] VueDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _vuemapper.Map<Vue>(dto);
            await _manager.AddAsync(entity);

            // Retourne bien les deux clés
            return CreatedAtAction(nameof(GetByIDs), new { idAnnonce = entity.IdAnnonce, idCompte = entity.IdCompte }, dto);
        }

        /// <summary>
        /// Met à jour une vue existante.
        /// </summary>
        /// <param name="dto">Objet <see cref="VueDTO"/> contenant les nouvelles valeurs.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{idannonce}/{idCompte}")]
        public async Task<ActionResult> Put(int idannonce, int idCompte, [FromBody] VueDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var toUpdate = await _manager.GetVueByIdsAsync(idCompte, idannonce);
            if (toUpdate == null)
                return NotFound();

            var updated = _vuemapper.Map<Vue>(dto);

            await _manager.UpdateAsync(toUpdate, updated);

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
        [HttpDelete("{idConversation}/{idCompte}")]
        public async Task<IActionResult> Delete(int idConversation, int idCompte)
        {
            var entity = await _manager.GetVueByIdsAsync(idCompte, idConversation);

            if (entity == null)
                return NotFound();

            await _manager.DeleteAsync(entity);
            return NoContent();
        }
        /// <summary>
        /// Récupère la liste de toutes les adresses.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="APourConversationDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VueDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<VueDTO>>(_vuemapper.Map<IEnumerable<VueDTO>>(list));
        }
        /// <summary>
        /// Récupère une vue à partir de ses identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="VueDTO"/> si l'adresse existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{idannonce}/{idCompte}")]
        public async Task<ActionResult<VueDTO>> GetByIDs(int idannonce, int idCompte)
        {
            var result = await _manager.GetVueByIdsAsync(idCompte, idannonce);

            if (result == null)
                return NotFound();

            return _vuemapper.Map<VueDTO>(result);
        }
    }
}
