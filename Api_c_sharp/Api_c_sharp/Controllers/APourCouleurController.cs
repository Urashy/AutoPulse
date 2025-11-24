using Api_c_sharp.DTO;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

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
    public class APourCouleurController(APourCouleurManager _manager, IMapper _adresseMapper) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle adresse.
        /// </summary>
        /// <param name="dto">Objet <see cref="APourCouleurDTO"/> contenant les informations de l'adresse à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec l'adresse créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        public async Task<ActionResult<APourCouleurDTO>> Post([FromBody] APourCouleurDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _adresseMapper.Map<APourCouleur>(dto);
            await _manager.AddAsync(entity);

            return CreatedAtAction(nameof(GetByID), new { id = entity.IdCouleur }, entity);
        }

        /// <summary>
        /// Met à jour une Adresse existante.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse à mettre à jour.</param>
        /// <param name="dto">Objet <see cref="APourCouleurDTO"/> contenant les nouvelles valeurs.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] APourCouleurDTO dto)
        {
            if (id != dto.IdCouleur)
                return BadRequest();

            var toUpdate = await _manager.GetByIdAsync(id);

            if (toUpdate == null)
                return NotFound();

            var updatedEntity = _adresseMapper.Map<APourCouleur>(dto);
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
        /// Une liste de <see cref="APourCouleurDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<APourCouleurDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<APourCouleurDTO>>(_adresseMapper.Map<IEnumerable<APourCouleurDTO>>(list));
        }
        /// <summary>
        /// Récupère une adresse à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="APourCouleur"/> si l'adresse existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{id}")]
        public async Task<ActionResult<APourCouleurDTO>> GetByID(int id)
        {
            var result = await _manager.GetByIdAsync(id);

            if (result is null)
                return NotFound();

            return _adresseMapper.Map<APourCouleurDTO>(result);
        }
    }
}
