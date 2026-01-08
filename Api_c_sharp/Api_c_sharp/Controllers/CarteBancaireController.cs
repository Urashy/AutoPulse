using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer les cartes bancaires.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CarteBancaireController(CartebancaireManager _manager, IMapper _mapper) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle carte bancaire.
        /// </summary>
        /// <param name="dto">Objet <see cref="CarteBancaireCreateDTO"/> contenant les informations de la carte bancaire à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec la carte bancaire créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        [ProducesResponseType(typeof(CarteBancaireDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CarteBancaireDTO>> Post([FromBody] CarteBancaireCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<CarteBancaire>(dto);
            entity.TypeCarte = GetTypeCarte(entity.NumeroCarte);
            await _manager.AddAsync(entity);

            return CreatedAtAction(nameof(GetByID), new { id = entity.IdCompte }, entity);
        }

        /// <summary>
        /// Met à jour une Carte Bancaire existante.
        /// </summary>
        /// <param name="id">Identifiant unique de la carte bancaire à mettre à jour.</param>
        /// <param name="dto">Objet <see cref="CarteBancaireUpdateDTO"/> contenant les nouvelles valeurs.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune carte bancaire ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(int id, [FromBody] CarteBancaireUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var toUpdate = await _manager.GetByIdAsync(id);

            if (toUpdate == null)
                return NotFound();

            var updatedEntity = _mapper.Map<CarteBancaire>(dto);
            updatedEntity.TypeCarte = GetTypeCarte(updatedEntity.NumeroCarte);

            await _manager.UpdateAsync(toUpdate, updatedEntity);

            return NoContent();
        }
        /// <summary>
        /// Supprime une carte bancaire existante.
        /// </summary>
        /// <param name="id">Identifiant unique de la carte bancaire à supprimer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune carte bancaire ne correspond (404).</description></item>
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
        /// Récupère la liste de toutes les cartes bancaires.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="CarteBancaireDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CarteBancaireDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CarteBancaireDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<CarteBancaireDTO>>(_mapper.Map<IEnumerable<CarteBancaireDTO>>(list));
        }
        /// <summary>
        /// Récupère une carte bancaire à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de la carte bancaire recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CarteBancaireDTO"/> si la carte bancaire existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune carte bancaire ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CarteBancaireDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CarteBancaireDTO>> GetByID(int id)
        {
            var result = await _manager.GetByIdAsync(id);

            if (result is null)
                return NotFound();

            return _mapper.Map<CarteBancaireDTO>(result);
        }


        /// <summary>
        /// Récupère la liste des cartes bancaires associées à un compte.
        /// </summary>
        /// <param name="idcompte">Identifiant unique du compte.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>Une collection de <see cref="CarteBancaireDTO"/> si des cartes bancaires existent (200).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune carte bancaire n'est associée au compte (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetCarteBancaireByCompteID")]
        [HttpGet("{idcompte}")]
        [ProducesResponseType(typeof(IEnumerable<CarteBancaireDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CarteBancaireDTO>>> GetCarteBancaireByCompteID(int idcompte)
        {
            var list = await _manager.GetCarteBancaireByCompteId(idcompte);

            if (!list.Any())
                return NotFound();

            return new ActionResult<IEnumerable<CarteBancaireDTO>>(_mapper.Map<IEnumerable<CarteBancaireDTO>>(list));
        }

        private static string GetTypeCarte(string numeroCarte)
        {
            if (string.IsNullOrWhiteSpace(numeroCarte))
                return "Classique";

            if (numeroCarte.StartsWith("4"))
                return "Visa";

            if (numeroCarte.Length == 16 &&
               (numeroCarte.StartsWith("51") ||
                numeroCarte.StartsWith("52") ||
                numeroCarte.StartsWith("53") ||
                numeroCarte.StartsWith("54") ||
                numeroCarte.StartsWith("55")))
                return "MasterCard";

            if (numeroCarte.StartsWith("34") || numeroCarte.StartsWith("37"))
                return "American Express";

            if (numeroCarte.StartsWith("6011") || numeroCarte.StartsWith("65"))
                return "Discover";

            return "Classique";
        }
    }
}
