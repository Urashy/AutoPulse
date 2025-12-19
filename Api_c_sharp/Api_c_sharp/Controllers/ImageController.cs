using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ImageController(ImageManager _manager, IMapper _mapper) : ControllerBase
    {
        /// <summary>
        /// Récupère une image à partir de son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'image.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>Le fichier image si elle existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune image ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ImageDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ImageDTO>> GetById(int id)
        {
            var imageEntity = await _manager.GetByIdAsync(id);
            if (imageEntity == null || imageEntity.Fichier == null)
                return NotFound();

            return File(imageEntity.Fichier, "image/jpeg");
        }

        /// <summary>
        /// Récupère tous les images.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="ImageDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ImageDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ImageDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<ImageDTO>>(_mapper.Map<IEnumerable<ImageDTO>>(list));
        }

        /// <summary>
        /// Ajoute une image.
        /// </summary>
        /// <param name="dto">Objet <see cref="ImageDTO"/> contient les inforamtion de l'image.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec l'image créé (201).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        [ProducesResponseType(typeof(ImageDTO), StatusCodes.Status201Created)]
        public async Task<ActionResult<ImageDTO>> Post([FromForm] ImageUploadDTO dto)
        {
            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);
            byte[] bytes = ms.ToArray();

            ImageDTO imageDto = new ImageDTO
            {
                IdImage = dto.IdImage,
                IdVoiture = dto.IdVoiture,
                IdCompte = dto.IdCompte,
                Fichier = bytes
            };

            await _manager.AddAsync(_mapper.Map<Image>(imageDto));

            return CreatedAtAction(nameof(GetById), new { id = imageDto.IdImage }, imageDto);
        }

        /// <summary>
        /// Met à jour une image existant.
        /// </summary>
        /// <param name="id">Identifiant d'une image.</param>
        /// <param name="dto">Objet <see cref="ImageUploadDTO"/> contenant les nouvelles valeurs.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l'ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune image ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(int id, [FromForm] ImageUploadDTO dto)
        {
            if (id != dto.IdImage)
                return BadRequest();

            var existing = await _manager.GetByIdAsync(id);

            if (existing == null)
                return NotFound();
            
            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);
            byte[] bytes = ms.ToArray();

            ImageDTO imageDto = new ImageDTO
            {
                IdImage = dto.IdImage,
                IdVoiture = dto.IdVoiture,
                IdCompte = dto.IdCompte,
                Fichier = bytes
            };
            
            var updatedEntity = _mapper.Map<Image>(imageDto);
            await _manager.UpdateAsync(existing, updatedEntity);

            return NoContent();
        }

        /// <summary>
        /// Supprime une image.
        /// </summary>
        /// <param name="id">Identifiant unique d'une image.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune image ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Delete")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            var entity = await _manager.GetByIdAsync(id);

            if (entity == null)
                return NotFound();

            await _manager.DeleteAsync(entity);

            return NoContent();
        }

        /// <summary>
        /// Récupère la première image associée à une voiture.
        /// </summary>
        /// <param name="voitureId">Identifiant unique de la voiture.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>Un fichier image JPEG si une image est trouvée (200).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune image ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetFirstImage")]
        [HttpGet("{voitureId}")]
        [ProducesResponseType(typeof(ImageDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ImageDTO>> GetImagesByVoitureId(int voitureId)
        {
            var imageEntity = await _manager.GetFirstImageByVoitureID(voitureId);

            if (imageEntity == null || imageEntity.Fichier == null)
                return NotFound();

            return File(imageEntity.Fichier, "image/jpeg"); 
        }


        /// <summary>
        /// Récupère les identifiants de toutes les images associées à une voiture.
        /// </summary>
        /// <param name="voitureId">Identifiant unique de la voiture.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>Une liste d'identifiants d'images si des données existent (200).</description></item>
        /// <item><description><see cref="NoContentResult"/> si aucune image n'est trouvée (204).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetAllImagesByVoitureId")]
        [HttpGet("{voitureId}")]
        [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<IEnumerable<int>>> GetAllImagesByVoitureId(int voitureId)
        {
            var listId = await _manager.GetAllImagesByVoitureId(voitureId);
            if (listId == null || !listId.Any())
                return NoContent();
            return Ok(listId);
        }


        /// <summary>
        /// Récupère l'image associée à un compte.
        /// </summary>
        /// <param name="compteId">Identifiant unique du compte.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>L'image du compte si elle existe (200).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune image ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetImageByCompte")]
        [HttpGet("{compteId}")]
        [ProducesResponseType(typeof(ImageDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ImageDTO>> GetImageByCompteID(int compteId)
        {
            var imageEntity = await _manager.GetImageByCompteID(compteId);

            if (imageEntity == null || imageEntity.Fichier == null)
                return NotFound();
            
            ImageDTO imageDto = _mapper.Map<ImageDTO>(imageEntity);
            return Ok(imageDto);
        }
    }
}
