using Api_c_sharp.DTO;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

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

            // Tu peux adapter le type MIME selon le fichier
            return File(imageEntity.Fichier, "image/jpeg"); 
        }

        // GET ALL
        [ActionName("GetAll")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<ImageDTO>>(list));
        }

        // POST
        [ActionName("Post")]
        [HttpPost]
        public async Task<ActionResult<ImageDTO>> Post([FromForm] ImageUploadDTO dto)
        {
            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);
            byte[] bytes = ms.ToArray();

            var imageDto = new ImageDTO
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
        public async Task<ActionResult> Put(int id, [FromBody] Image dto)
        {
            if (id != dto.IdImage)
                return BadRequest();

            var existing = await _manager.GetByIdAsync(id);

            if (existing == null)
                return NotFound();

            var updatedEntity = _mapper.Map<Image>(dto);
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
    }
}
