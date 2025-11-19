using Api_c_sharp.DTO;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer les couleurs.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CouleurController (CouleurManager _manager, IMapper _couleurMapper) : ControllerBase
    {
        /// <summary>
        /// Récupère une couleur à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique du modele recherchée.</param>
        /// <returns>
        /// <item><description><see cref="CouleurDTO"/> si la couleur existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune couleur ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{id}")]
        public async Task<ActionResult<CouleurDTO>> GetById(int id)
        {
            var result = await _manager.GetByIdAsync(id);

            if (result is null)
                return NotFound();

            return new ActionResult<CouleurDTO>(_couleurMapper.Map<CouleurDTO>(result));
        }
        /// <summary>
        /// Récupère la liste de toutes les couleurs.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="CouleurDTO"/> (200 OK).
        /// </returns>
        [HttpGet]
        [ActionName("GetAll")]
        public async Task<ActionResult<IEnumerable<CouleurDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<CouleurDTO>>(_couleurMapper.Map<IEnumerable<CouleurDTO>>(list));
        }
    }
}
