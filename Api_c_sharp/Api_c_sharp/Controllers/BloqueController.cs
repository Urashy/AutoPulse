using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer la table des bloqué.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BloqueController(BloqueManager _manager, IMapper _bloqueMapper) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle laiason de bloqué (bloquant et bloqué).
        /// </summary>
        /// <param name="dto">Objet <see cref="BloqueDTO"/> contenant les informations de la liaison à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec la liaison créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        [ProducesResponseType(typeof(BloqueDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BloqueDTO>> Post([FromBody] BloqueDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var entity = _bloqueMapper.Map<Bloque>(dto);
            entity.DateBloque = DateTime.UtcNow;
            await _manager.AddAsync(entity);

            // Retourne bien les deux clés
            return CreatedAtAction(nameof(GetByID), new { idBloquant = entity.IdBloquant, idBloque = entity.IdBloque }, entity);
        }

        /// <summary>
        /// Met à jour une liaison existante.
        /// </summary>
        /// <param name="idBloque"></param>
        /// <param name="dto">Objet <see cref="BloqueDTO"/> contenant les nouvelles valeurs.</param>
        /// <param name="idBloquant"></param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune liaison ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{idBloquant}/{idBloque}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(int idBloquant, int idBloque, [FromBody] BloqueDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (idBloquant != dto.IdBloquant || idBloque != dto.IdBloque)
                return BadRequest();

            var toUpdate = await _manager.GetBloqueByIdsAsync(idBloque, idBloquant);
            if (toUpdate == null)
                return NotFound();

            var updated = _bloqueMapper.Map<Bloque>(dto);

            await _manager.UpdateAsync(toUpdate, updated);

            return NoContent();
        }
        /// <summary>
        /// Supprime une liaison existante.
        /// </summary>
        /// <param name="id">Identifiant unique de la liaison à supprimer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune liaison ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Delete")]
        [HttpDelete("{idBloquant}/{idBloque}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int idBloquant, int idBloque)
        {
            var entity = await _manager.GetBloqueByIdsAsync(idBloque, idBloquant);

            if (entity == null)
                return NotFound();

            await _manager.DeleteAsync(entity);
            return NoContent();
        }
        /// <summary>
        /// Récupère la liste de toutes les liaisons.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="BloqueDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BloqueDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BloqueDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<BloqueDTO>>(_bloqueMapper.Map<IEnumerable<BloqueDTO>>(list));
        }
        /// <summary>
        /// Récupère une liaison à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de la liaison recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="BloqueDTO"/> si la liaison existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune liaison ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{idBloquant}/{idBloque}")]
        [ProducesResponseType(typeof(BloqueDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BloqueDTO>> GetByID(int idBloquant, int idBloque)
        {
            var result = await _manager.GetBloqueByIdsAsync(idBloque, idBloquant);

            if (result == null)
                return NotFound();

            return _bloqueMapper.Map<BloqueDTO>(result);
        }

        /// <summary>
        /// Vérifie si un compte est en bloque pour un compte.
        /// </summary>
        /// <param name="idBloque">Identifiant unique du compte bloque .</param>
        /// <param name="idBloquant">Identifiant unique du compte bloquant.</param>
        /// <returns>
        /// <see cref="bool"/> indiquant si l'annonce est en favori (200 OK).
        /// </returns>
        [ActionName("HasBloque")]
        [HttpGet]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> HasBloque([FromQuery] int idBloque, [FromQuery] int idBloquant, bool premierestbloquant)
        {
            bool result;
            if(premierestbloquant)
                 result = await _manager.ExistsAsync(idBloque, idBloquant);
            else 
                 result = await _manager.ExistsAsync(idBloquant, idBloque);
            return result;
        }
    }
}
