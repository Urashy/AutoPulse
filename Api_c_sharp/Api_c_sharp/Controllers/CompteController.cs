using System.Text;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using System.Security.Cryptography;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Api_c_sharp.Models.Repository.Managers;

namespace App.Controllers;

/// <summary>
/// Contrôleur REST permettant de gérer les comptes.
/// Les méthodes exposent ou consomment des DTO afin
/// d’assurer la séparation entre le modèle de domaine
/// et la couche API.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class CompteController(CompteManager _manager, IMapper _compteMapper) : ControllerBase
{
    /// <summary>
    /// Récupère une annoncs à partir de son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de la compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteDTO"/> si la compte existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetById")]
    [HttpGet("{id}")]
    public async Task<ActionResult<CompteDetailDTO>> GetByID(int id)
    {
        var result = await _manager.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return _compteMapper.Map<CompteDetailDTO>(result);
    }

    /// <summary>
    /// Récupère une compte à partir de son nom exact (insensible à la casse).
    /// </summary>
    /// <param name="str">Nom de la compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteDTO"/> si la compte existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByString")]
    [HttpGet("{str}")]
    public async Task<ActionResult<CompteDetailDTO>> GetByString(string str)
    {
        var result = await _manager.GetByNameAsync(str);

        if (result is null)
        {
            return NotFound();
        }

        return _compteMapper.Map<CompteDetailDTO>(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les comptes.
    /// </summary>
    /// <returns>
    /// Une liste de <see cref="CompteDTO"/> (200 OK).
    /// </returns>
    [ActionName("GetAll")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompteListDTO>>> GetAll()
    {
        var list = await _manager.GetAllAsync();
        return new ActionResult<IEnumerable<CompteListDTO>>(_compteMapper.Map<IEnumerable<CompteListDTO>>(list));
    }

    /// <summary>
    /// Crée une nouvelle compte.
    /// </summary>
    /// <param name="dto">Objet <see cref="CompteDTO"/> contenant les informations de la compte à créer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CreatedAtActionResult"/> avec la compte créée (201).</description></item>
    /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Post")]
    [HttpPost]
    public async Task<ActionResult<Compte>> Post([FromBody] CompteCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _compteMapper.Map<Compte>(dto);
        entity.MotDePasse = ComputeSha256Hash(entity.MotDePasse);
        await _manager.AddAsync(entity);

        return CreatedAtAction(nameof(GetByID), new { id = entity.IdCompte }, entity);
    }

    /// <summary>
    /// Met à jour une compte existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la compte à mettre à jour.</param>
    /// <param name="dto">Objet <see cref="CompteDTO"/> contenant les nouvelles valeurs.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
    /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Put")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] CompteUpdateDTO dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var toUpdate = await _manager.GetByIdAsync(id);

        if (toUpdate == null)
            return NotFound();

        var updatedEntity = _compteMapper.Map<Compte>(dto);
        await _manager.UpdateAsync(toUpdate, updatedEntity);

        return NoContent();
    }

    /// <summary>
    /// Supprime une compte existante.
    /// </summary>
    /// <param name="id">Identifiant unique de la compte à supprimer.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("Delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _manager.GetByIdAsync(id);

        if (entity == null)
            return NotFound();

        await _manager.DeleteAsync(entity);
        return NoContent();
    }


    /// <summary>
    /// Récupère un compte à partir d'un id de type de compte.
    /// </summary>
    /// <param name="type">Identifiant unique de la compte recherchée.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="CompteListDTO"/> si les compte existe (200 OK).</description></item>
    /// <item><description><see cref="NotFoundResult"/> si aucune compte ne correspond (404).</description></item>
    /// </list>
    /// </returns>
    [ActionName("GetByIdMiseEnAvant")]
    [HttpGet("{idmiseenavant}")]
    public async Task<ActionResult<IEnumerable<CompteListDTO>>> GetByTypeCompte(int type)
    {
        var result = await _manager.GetComptesByTypes(type);

        if (result is null)
            return NotFound();

        return new ActionResult<IEnumerable<CompteListDTO>>(_compteMapper.Map<IEnumerable<CompteListDTO>>(result));
    }

    /// <summary>
    /// Calcule le hachage SHA-256 d'une chaîne de caractères.
    /// </summary>
    /// <param name="rawData">Les données brutes à hacher.</param>
    /// <returns>Le hachage SHA-256 de la chaîne d'entrée.</returns>
    static public string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}