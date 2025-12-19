using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api_c_sharp.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class PieceJointeController(PieceJointeManager _manager, IMapper _mapper) : ControllerBase
{
    // Types de fichiers autorisés
    private static readonly Dictionary<string, string[]> TypesAutorises = new()
    {
        { "image", new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" } },
        { "document", new[] { ".pdf", ".doc", ".docx", ".txt" } }
    };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Upload un ou plusieurs fichiers encodés en Base64.
    /// </summary>
    /// <param name="uploadDtos">Liste des pièces jointes à uploader.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>La liste des pièces jointes uploadées (200).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si aucun fichier n'est fourni (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Upload")]
    [HttpPost]
    [ProducesResponseType(typeof(List<PieceJointeDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<PieceJointeDTO>>> Upload([FromBody] List<PieceJointeUploadDTO> uploadDtos)
    {
        if (uploadDtos == null || !uploadDtos.Any())
            return BadRequest("Aucun fichier fourni");

        var uploadedFiles = new List<PieceJointeDTO>();

        foreach (var dto in uploadDtos)
        {
            try
            {
                // Validation de la taille
                if (dto.TailleFichier > MaxFileSize)
                {
                    continue;
                }

                // Validation de l'extension
                var extension = dto.Extension.ToLowerInvariant();
                if (!EstExtensionAutorisee(extension))
                {
                    continue;
                }

                // Décoder le Base64
                byte[] contenuBinaire;
                try
                {
                    contenuBinaire = Convert.FromBase64String(dto.ContenuBase64);
                }
                catch (FormatException)
                {
                    continue;
                }

                // Créer l'entité
                var pieceJointe = new PieceJointe
                {
                    IdMessage = dto.IdMessage,
                    NomFichier = dto.NomFichier,
                    TypeMime = dto.TypeMime,
                    Extension = extension,
                    TailleFichier = dto.TailleFichier,
                    Contenu = contenuBinaire,
                    DateUpload = DateTime.UtcNow
                };

                await _manager.AddAsync(pieceJointe);
                
                // Mapper sans renvoyer le contenu (trop lourd)
                var resultDto = _mapper.Map<PieceJointeDTO>(pieceJointe);
                resultDto.ContenuBase64 = null; // Ne pas renvoyer le contenu dans la réponse
                
                uploadedFiles.Add(resultDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        return Ok(uploadedFiles);
    }

    /// <summary>
    /// Récupère une pièce jointe avec son contenu encodé en Base64.
    /// </summary>
    /// <param name="id">Identifiant unique de la pièce jointe.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>La pièce jointe avec son contenu (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune pièce jointe ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PieceJointeDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PieceJointeDTO>> GetById(int id)
    {
        var pieceJointe = await _manager.GetByIdWithContentAsync(id);

        if (pieceJointe == null)
            return NotFound();

        var dto = _mapper.Map<PieceJointeDTO>(pieceJointe);
        dto.ContenuBase64 = Convert.ToBase64String(pieceJointe.Contenu);

        return dto;
    }

    /// <summary>
    /// Télécharge une pièce jointe sous forme binaire.
    /// </summary>
    /// <param name="id">Identifiant unique de la pièce jointe.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Le fichier téléchargé (200).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune pièce jointe ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Download")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int id)
    {
        var pieceJointe = await _manager.GetByIdWithContentAsync(id);

        if (pieceJointe == null)
            return NotFound();

        return File(pieceJointe.Contenu, pieceJointe.TypeMime, pieceJointe.NomFichier);
    }

    /// <summary>
    /// Supprime une pièce jointe.
    /// </summary>
    /// <param name="id">Identifiant unique de la pièce jointe.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune pièce jointe ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Delete")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var pieceJointe = await _manager.GetByIdWithContentAsync(id);

        if (pieceJointe == null)
            return NotFound();

        await _manager.DeleteAsync(pieceJointe);
        return NoContent();
    }

    /// <summary>
    /// Récupère les pièces jointes associées à un message avec leur contenu encodé en Base64.
    /// </summary>
    /// <param name="idMessage">Identifiant unique du message.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de pièces jointes avec leur contenu (200).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByMessage")]
    [HttpGet("{idMessage}")]
    [ProducesResponseType(typeof(IEnumerable<PieceJointeDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PieceJointeDTO>>> GetByMessage(int idMessage)
    {
        var piecesJointes = await _manager.GetByMessageIdAsync(idMessage);
        
        var dtos = piecesJointes.Select(pj =>
        {
            var dto = _mapper.Map<PieceJointeDTO>(pj);
            dto.ContenuBase64 = Convert.ToBase64String(pj.Contenu);
            return dto;
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Récupère les métadonnées des pièces jointes associées à un message, sans le contenu.
    /// </summary>
    /// <param name="idMessage">Identifiant unique du message.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de métadonnées de pièces jointes (200).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetMetadataByMessage")]
    [HttpGet("{idMessage}")]
    [ProducesResponseType(typeof(IEnumerable<PieceJointeDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PieceJointeDTO>>> GetMetadataByMessage(int idMessage)
    {
        var piecesJointes = await _manager.GetByMessageIdAsync(idMessage);
        var dtos = _mapper.Map<IEnumerable<PieceJointeDTO>>(piecesJointes);
        
        // Ne pas inclure le contenu Base64
        foreach (var dto in dtos)
        {
            dto.ContenuBase64 = null;
        }

        return Ok(dtos);
    }

    private bool EstExtensionAutorisee(string extension)
    {
        return TypesAutorises.Values.Any(extensions => extensions.Contains(extension));
    }

    /// <summary>
    /// Crée une nouvelle pièce jointe.
    /// </summary>
    /// <param name="dto">Objet <see cref="PieceJointeCreateDTO"/> contenant les informations de la pièce jointe.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la pièce jointe créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    [ProducesResponseType(typeof(PieceJointeDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PieceJointeDTO>> Post([FromBody] PieceJointeCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<PieceJointe>(dto);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetById), new { id = entity.IdPieceJointe }, entity);
    }

    /// <summary>
    /// Récupère la liste de toutes les pièces jointes.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>Une collection de <see cref="PieceJointeDTO"/> (200).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PieceJointeDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PieceJointeDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<PieceJointeDTO>>(_mapper.Map<IEnumerable<PieceJointeDTO>>(list));
    }

    /// <summary>
    /// Met à jour une pièce jointe existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la pièce jointe.</param>
    /// <param name="dto">Objet contenant les nouvelles données de la pièce jointe.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si les données sont invalides (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune pièce jointe ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put(int id, [FromBody] PieceJointeUploadDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _mapper.Map<PieceJointe>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }
}