using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer la table de jointure entre couleur et voiture.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BloqueController(BloqueManager _manager, IMapper _bloqueMapper) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle adresse.
        /// </summary>
        /// <param name="dto">Objet <see cref="BloqueDTO"/> contenant les informations de l'adresse à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec l'adresse créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        public async Task<ActionResult<BloqueDTO>> Post([FromBody] BloqueDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _bloqueMapper.Map<Bloque>(dto);
            await _manager.AddAsync(entity);

            // Retourne bien les deux clés
            return CreatedAtAction(nameof(GetByID), new { idBloquant = entity.IdBloquant, idBloque = entity.IdBloque }, entity);
        }

        /// <summary>
        /// Met à jour une Adresse existante.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse à mettre à jour.</param>
        /// <param name="dto">Objet <see cref="BloqueDTO"/> contenant les nouvelles valeurs.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{idBloquant}/{idBloque}")]
        public async Task<ActionResult> Put(int idBloquant, int idBloque, [FromBody] BloqueDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (idBloquant != dto.IdBloquant || idBloque != dto.IdBloque)
                return BadRequest();

            var toUpdate = await _manager.GetByIdsAsync(idBloque, idBloquant);
            if (toUpdate == null)
                return NotFound();

            var updated = _bloqueMapper.Map<Bloque>(dto);

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
        [HttpDelete("{idBloquant}/{idBloque}")]
        public async Task<IActionResult> Delete(int idBloquant, int idBloque)
        {
            var entity = await _manager.GetByIdsAsync(idBloque, idBloquant);

            if (entity == null)
                return NotFound();

            await _manager.DeleteAsync(entity);
            return NoContent();
        }
        /// <summary>
        /// Récupère la liste de toutes les adresses.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="BloqueDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BloqueDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<BloqueDTO>>(_bloqueMapper.Map<IEnumerable<BloqueDTO>>(list));
        }
        /// <summary>
        /// Récupère une adresse à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="BloqueDTO"/> si l'adresse existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{idBloquant}/{idBloque}")]
        public async Task<ActionResult<BloqueDTO>> GetByID(int idBloquant, int idBloque)
        {
            var result = await _manager.GetByIdsAsync(idBloque, idBloquant);

            if (result == null)
                return NotFound();

            return _bloqueMapper.Map<BloqueDTO>(result);
        }
    }
}
