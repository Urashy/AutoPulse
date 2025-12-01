using Api_c_sharp.DTO;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer les favoris.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d'assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FavoriController : ControllerBase
    {
        private readonly FavoriManager _manager;
        private readonly IMapper _mapper;

        public FavoriController(FavoriManager manager, IMapper mapper)
        {
            _manager = manager;
            _mapper = mapper;
        }

        /// <summary>
        /// Récupère tous les favoris.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="FavoriDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<FavoriDTO>>(_mapper.Map<IEnumerable<FavoriDTO>>(list));
        }

        /// <summary>
        /// Récupère un favori spécifique par ID de compte et ID d'annonce.
        /// </summary>
        /// <param name="idCompte">Identifiant unique du compte.</param>
        /// <param name="idAnnonce">Identifiant unique de l'annonce.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="FavoriDTO"/> si le favori existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucun favori ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet]
        public async Task<ActionResult<FavoriDTO>> GetById([FromQuery] int idCompte, [FromQuery] int idAnnonce)
        {
            var result = await _manager.GetByIdAsync(idAnnonce);

            if (result is null)
                return NotFound();

            return _mapper.Map<FavoriDTO>(result);
        }

        /// <summary>
        /// Ajoute une annonce aux favoris.
        /// </summary>
        /// <param name="dto">Objet <see cref="FavoriDTO"/> contenant l'ID du compte et de l'annonce.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec le favori créé (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        public async Task<ActionResult<FavoriDTO>> Post([FromBody] FavoriDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<Favori>(dto);
            await _manager.AddAsync(entity);

            return CreatedAtAction(
                nameof(GetById),
                new { idCompte = entity.IdCompte, idAnnonce = entity.IdAnnonce },
                _mapper.Map<FavoriDTO>(entity)
            );
        }

        /// <summary>
        /// Supprime un favori.
        /// </summary>
        /// <param name="idCompte">Identifiant unique du compte.</param>
        /// <param name="idAnnonce">Identifiant unique de l'annonce.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucun favori ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Delete")]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int idCompte, [FromQuery] int idAnnonce)
        {
            var entity = await _manager.GetByIdAsync(idCompte, idAnnonce);

            if (entity == null)
                return NotFound();

            await _manager.DeleteAsync(entity);
            return NoContent();
        }

        /// <summary>
        /// Vérifie si une annonce est en favori pour un compte.
        /// </summary>
        /// <param name="idCompte">Identifiant unique du compte.</param>
        /// <param name="idAnnonce">Identifiant unique de l'annonce.</param>
        /// <returns>
        /// <see cref="bool"/> indiquant si l'annonce est en favori (200 OK).
        /// </returns>
        [ActionName("IsFavorite")]
        [HttpGet]
        public async Task<ActionResult<bool>> IsFavorite([FromQuery] int idCompte, [FromQuery] int idAnnonce)
        {
            var result = await _manager.GetByIdAsync(idCompte, idAnnonce);
            return Ok(result != null);
        }


        /// <summary>
        /// Récupère tous les favoris d'un compte.
        /// </summary>
        [ActionName("GetByCompteId")]
        [HttpGet("{idCompte}")]
        public async Task<ActionResult<IEnumerable<FavoriDTO>>> GetByCompteId(int idCompte)
        {
            var favoris = await _manager.GetByCompteIdAsync(idCompte);
            return Ok(_mapper.Map<IEnumerable<FavoriDTO>>(favoris));
        }
    }
}