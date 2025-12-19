using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer les états de compte.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EtatCompteController (EtatCompteManager _manager, IMapper _couleurMapper) : ControllerBase
    {
        /// <summary>
        /// Récupère un état à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'état recherchée.</param>
        /// <returns>
        /// <item><description><see cref="EtatCompteDTO"/> si l'état existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucun état ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EtatCompteDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EtatCompteDTO>> GetById(int id)
        {
            var result = await _manager.GetByIdAsync(id);

            if (result is null)
                return NotFound();

            return new ActionResult<EtatCompteDTO>(_couleurMapper.Map<EtatCompteDTO>(result));
        }
        /// <summary>
        /// Récupère la liste de toute les états.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="EtatCompteDTO"/> (200 OK).
        /// </returns>
        [HttpGet]
        [ActionName("GetAll")]
        [ProducesResponseType(typeof(IEnumerable<EtatCompteDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EtatCompteDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<EtatCompteDTO>>(_couleurMapper.Map<IEnumerable<EtatCompteDTO>>(list));
        }
    }
}
