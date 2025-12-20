using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer les plaintes.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PlainteController(PlainteManager _manager, IMapper _adresseMapper) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle plainte.
        /// </summary>
        /// <param name="dto">Objet <see cref="PlainteCreateDTO"/> contenant les informations de la plainte à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec la plainte créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        [ProducesResponseType(typeof(PlainteDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PlainteDTO>> Post([FromBody] PlainteCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var plainte = await _manager.GetPlainteByCompteID(dto.IdCompte);

            var plaintesExistantes = await _manager.GetPlainteByCompteID(dto.IdCompte);
            if (plaintesExistantes.Any(p => p.IdEtat == 1))
            {
                return BadRequest("Vous avez déjà une plainte en attente de traitement.");
            }

            var entity = _adresseMapper.Map<Plainte>(dto);
            await _manager.AddAsync(entity);

            return CreatedAtAction(nameof(GetByID), new { id = entity.IdPlainte }, entity);
        }

        /// <summary>
        /// Met à jour une plainte existante.
        /// </summary>
        /// <param name="id">Identifiant unique de la plainte à mettre à jour.</param>
        /// <param name="dto">Objet <see cref="PlainteUpdateDTO"/> contenant les nouvelles valeurs.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune plainte ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(int id, [FromBody] PlainteUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var toUpdate = await _manager.GetByIdAsync(id);

            if (toUpdate == null)
                return NotFound();

            var updatedEntity = _adresseMapper.Map<Plainte>(dto);
            await _manager.UpdateAsync(toUpdate, updatedEntity);

            return NoContent();
        }
        /// <summary>
        /// Supprime une plainte existante.
        /// </summary>
        /// <param name="id">Identifiant unique de la plainte à supprimer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune plainte ne correspond (404).</description></item>
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
        /// Récupère la liste de toutes les plaintes.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="PlainteDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PlainteDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PlainteDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<PlainteDTO>>(_adresseMapper.Map<IEnumerable<PlainteDTO>>(list));
        }
        /// <summary>
        /// Récupère une plainte à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de la plainte recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="PlainteDTO"/> si la plainte existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune plainte ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PlainteDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlainteDTO>> GetByID(int id)
        {
            var result = await _manager.GetByIdAsync(id);

            if (result is null)
                return NotFound();

            return _adresseMapper.Map<PlainteDTO>(result);
        }


        /// <summary>
        /// Récupère une plainte à partir de son identifiant de compte.
        /// </summary>
        /// <param name="id">Identifiant unique de la plainte recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="PlainteDTO"/> si la plainte existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune plainte ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetByIdCompte")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PlainteDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlainteDTO>> GetByIDCompte(int id)
        {
            var result = await _manager.GetPlainteByCompteID(id);

            if (result is null)
                return NotFound();

            return _adresseMapper.Map<PlainteDTO>(result);
        }

    }
}
