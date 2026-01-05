using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer la table de jointure entre compte et conversation.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class APourConversationController(APourConversationManager _manager, IMapper _aPourCoversationMapper) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle liaison entre compte et conversation.
        /// </summary>
        /// <param name="dto">Objet <see cref="APourConversationDTO"/> contenant les Ids de la liaison à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec la liaison créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        [ProducesResponseType(typeof(APourConversationDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APourConversationDTO>> Post([FromBody] APourConversationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _aPourCoversationMapper.Map<APourConversation>(dto);
            await _manager.AddAsync(entity);

            // Retourne bien les deux clés
            return CreatedAtAction(nameof(GetById), new { idConversation = entity.IdConversation, idCompte = entity.IdCompte }, entity);
        }

        /// <summary>
        /// Met à jour une liaison existante.
        /// </summary>
        /// <param name="idCompte"></param>
        /// <param name="dto">Objet <see cref="APourConversationDTO"/> contenant les nouvelles valeurs.</param>
        /// <param name="idConversation"></param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune laiason ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{idConversation}/{idCompte}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(int idConversation, int idCompte, [FromBody] APourConversationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var toUpdate = await _manager.GetAPourConversationByIDS(idCompte, idConversation);
            if (toUpdate == null)
                return NotFound();

            var updated = _aPourCoversationMapper.Map<APourConversation>(dto);

            await _manager.UpdateAsync(toUpdate, updated);

            return NoContent();
        }

        /// <summary>
        /// Supprime une liaison existante.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune liaison ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Delete")]
        [HttpDelete("{idConversation}/{idCompte}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int idConversation, int idCompte)
        {
            var entity = await _manager.GetAPourConversationByIDS(idCompte, idConversation);

            if (entity == null)
                return NotFound();

            await _manager.DeleteAsync(entity);
            return NoContent();
        }
        /// <summary>
        /// Récupère la liste de toutes les liaisons.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="APourConversationDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<APourConversationDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<APourConversationDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<APourConversationDTO>>(_aPourCoversationMapper.Map<IEnumerable<APourConversationDTO>>(list));
        }

        /// <summary>
        /// Récupère une laiason à partir de son identifiant.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="APourConversationDTO"/> si la liaison existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune liaison ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{idConversation}/{idCompte}")]
        [ProducesResponseType(typeof(APourConversationDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APourConversationDTO>> GetById(int idConversation, int idCompte)
        {
            var result = await _manager.GetAPourConversationByIDS(idCompte, idConversation);

            if (result == null)
                return NotFound();

            return _aPourCoversationMapper.Map<APourConversationDTO>(result);
        }
    }
}
