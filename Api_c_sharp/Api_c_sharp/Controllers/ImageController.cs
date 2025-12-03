using AutoPulse.Shared.DTO;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ImageController(ImageManager _manager, IMapper _mapper) : ControllerBase
    {
        // GET BY ID
        [ActionName("GetById")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ImageDTO>> GetById(int id)
        {
            var imageEntity = await _manager.GetByIdAsync(id);
            if (imageEntity == null || imageEntity.Fichier == null)
                return NotFound();

            return File(imageEntity.Fichier, "image/jpeg");
        }

        // GET ALL
        [ActionName("GetAll")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<ImageDTO>>(_mapper.Map<IEnumerable<ImageDTO>>(list));
        }

        // POST
        [ActionName("Post")]
        [HttpPost]
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

        // PUT
        [ActionName("Put")]
        [HttpPut("{id}")]
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

        // DELETE
        [ActionName("Delete")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var entity = await _manager.GetByIdAsync(id);

            if (entity == null)
                return NotFound();

            await _manager.DeleteAsync(entity);

            return NoContent();
        }

        // GET BY ID
        [ActionName("GetFirstImage")]
        [HttpGet("{voitureId}")]
        public async Task<ActionResult<ImageDTO>> GetImagesByVoitureId(int voitureId)
        {
            var imageEntity = await _manager.GetFirstImageByVoitureID(voitureId);

            if (imageEntity == null || imageEntity.Fichier == null)
                return NotFound();

            return File(imageEntity.Fichier, "image/jpeg"); 
        }

        [ActionName("GetAllImagesByVoitureId")]
        [HttpGet("{voitureId}")]
        public async Task<ActionResult<IEnumerable<int>>> GetAllImagesByVoitureId(int voitureId)
        {
            var listId = await _manager.GetAllImagesByVoitureId(voitureId);
            if (listId == null || !listId.Any())
                return NoContent();
            return Ok(listId);
        }

        [ActionName("GetImageByCompte")]
        [HttpGet("{compteId}")]
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
