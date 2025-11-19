using Api_c_sharp.DTO;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer les types de compte.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TypeCompteController (TypeCompteManager _manager, IMapper _typeCompteMapper) : ControllerBase
    {
        /// <summary>
        /// Récupère untype de compte à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique du modele recherchée.</param>
        /// <returns>
        /// <item><description><see cref="TypeCompteDTO"/> si le type de compte existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucun type de compte ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeCompteDTO>> GetById(int id)
        {
            var result = await _manager.GetByIdAsync(id);

            if (result is null)
                return NotFound();

            return new ActionResult<TypeCompteDTO>(_typeCompteMapper.Map<TypeCompteDTO>(result));
        }
        /// <summary>
        /// Récupère la liste de toutes les types de compte.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="TypeCompteDTO"/> (200 OK).
        /// </returns>
        [HttpGet]
        [ActionName("GetAll")]
        public async Task<ActionResult<IEnumerable<TypeCompteDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<TypeCompteDTO>>(_typeCompteMapper.Map<IEnumerable<TypeCompteDTO>>(list));
        }
    }
}
