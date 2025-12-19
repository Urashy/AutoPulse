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
    public class AdresseController(AdresseManager _manager, IMapper _adresseMapper) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle adresse.
        /// </summary>
        /// <param name="dto">Objet <see cref="AdresseCreateDTO"/> contenant les informations de l'adresse à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec l'adresse créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        [ProducesResponseType(typeof(AdresseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AdresseDTO>> Post([FromBody] AdresseCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _adresseMapper.Map<Adresse>(dto);
            await _manager.AddAsync(entity);

            return CreatedAtAction(nameof(GetByID), new { id = entity.IdAdresse }, entity);
        }

        /// <summary>
        /// Met à jour une Adresse existante.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse à mettre à jour.</param>
        /// <param name="dto">Objet <see cref="AdresseUpdateDTO"/> contenant les nouvelles valeurs.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(int id, [FromBody] AdresseUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var toUpdate = await _manager.GetByIdAsync(id);

            if (toUpdate == null)
                return NotFound();

            var updatedEntity = _adresseMapper.Map<Adresse>(dto);
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
        /// Récupère la liste de toutes les adresses.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="AdresseDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AdresseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AdresseDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<AdresseDTO>>(_adresseMapper.Map<IEnumerable<AdresseDTO>>(list));
        }
        /// <summary>
        /// Récupère une adresse à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'adresse recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="AdresseDTO"/> si l'adresse existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AdresseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AdresseDTO>> GetByID(int id)
        {
            var result = await _manager.GetByIdAsync(id);

            if (result is null)
                return NotFound();

            return _adresseMapper.Map<AdresseDTO>(result);
        }


        /// <summary>
        /// Récupère la liste des adresses associées à un compte.
        /// </summary>
        /// <param name="idcompte">Identifiant unique du compte.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>Une collection de <see cref="AdresseDTO"/> si des adresses existent (200).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune adresse n'est associée au compte (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetAdressesByCompteID")]
        [HttpGet("{idcompte}")]
        [ProducesResponseType(typeof(IEnumerable<AdresseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AdresseDTO>>> GetAdressesByCompteID(int idcompte)
        {
            var list = await _manager.GetAdresseByCompteID(idcompte);

            if (list is null || !list.Any())
                return NotFound();

            return new ActionResult<IEnumerable<AdresseDTO>>(_adresseMapper.Map<IEnumerable<AdresseDTO>>(list));
        }
    }
}
