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
    /// Upload un ou plusieurs fichiers encodés en Base64
    /// </summary>
    [ActionName("Upload")]
    [HttpPost]
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
    /// Récupérer une pièce jointe avec son contenu encodé en Base64
    /// </summary>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<PieceJointeDTO>> GetById(int id)
    {
        var pieceJointe = await _manager.GetByIdWithContentAsync(id);

        if (pieceJointe == null)
            return NotFound();

        var dto = _mapper.Map<PieceJointeDTO>(pieceJointe);
        dto.ContenuBase64 = Convert.ToBase64String(pieceJointe.Contenu);

        return Ok(dto);
    }

    /// <summary>
    /// Télécharger un fichier (retourne le binaire brut)
    /// </summary>
    [ActionName("Download")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var pieceJointe = await _manager.GetByIdWithContentAsync(id);

        if (pieceJointe == null)
            return NotFound();

        return File(pieceJointe.Contenu, pieceJointe.TypeMime, pieceJointe.NomFichier);
    }

    /// <summary>
    /// Supprimer une pièce jointe
    /// </summary>
    [ActionName("Delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pieceJointe = await _manager.GetByIdWithContentAsync(id);

        if (pieceJointe == null)
            return NotFound();

        await _manager.DeleteAsync(pieceJointe);
        return NoContent();
    }

    /// <summary>
    /// Récupérer les pièces jointes d'un message AVEC leur contenu Base64
    /// </summary>
    [ActionName("GetByMessage")]
    [HttpGet("{idMessage}")]
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
    /// Récupérer les métadonnées des pièces jointes (sans le contenu)
    /// </summary>
    [ActionName("GetMetadataByMessage")]
    [HttpGet("{idMessage}")]
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
}