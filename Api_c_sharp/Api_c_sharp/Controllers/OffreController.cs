using Api_c_sharp.Hubs;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Api_c_sharp.Controllers
{
    /// <summary>
    /// Contrôleur REST permettant de gérer les offres.
    /// Les méthodes exposent ou consomment des DTO afin
    /// d’assurer la séparation entre le modèle de domaine
    /// et la couche API.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OffreController(OffreManager _manager, IMapper _offremapper, MessageManager _managermessage,CommandeManager _managercommande,FactureManager _managerfacture,AnnonceManager _managerannonce, INotificationService _notifService, IHubContext<MessageHub> _hubContext = null) : ControllerBase
    {
        /// <summary>
        /// Crée une nouvelle offre.
        /// </summary>
        /// <param name="dto">Objet <see cref="OffreCreateDTO"/> contenant les informations de l'offre à créer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="CreatedAtActionResult"/> avec l'offre créée (201).</description></item>
        /// <item><description><see cref="BadRequestObjectResult"/> si le modèle est invalide (400).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Post")]
        [HttpPost]
        [ProducesResponseType(typeof(OffreDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OffreDTO>> Post([FromBody] OffreCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var mess = await _managermessage.GetByIdAsync(dto.IdMessage);

            bool res = await _manager.PendingOfferExistsInConversation(mess.IdConversation);

            if (res)
            {
                return Conflict("Une offre en attente existe déjà dans cette conversation.");
            }

            var entity = _offremapper.Map<Offre>(dto);

            await _manager.AddAsync(entity);
            
            var annonce = await _managerannonce.GetByIdAsync(entity.IdAnnonce);
            var message = await _managermessage.GetByIdAsync(entity.IdMessage);
            
            int idAcheteur;
                
            if(message.IdCompte == annonce.IdCompte)
            {
                idAcheteur = annonce.IdCompte;
            }
            else
            {
                idAcheteur = message.IdCompte;
            }

            await _notifService.NotifOffreAnnonce(entity.IdAnnonce, idAcheteur, entity.Valeur);
            
            var messageAssocie = await _managermessage.GetByIdAsync(entity.IdMessage);

            if (messageAssocie != null && _hubContext != null)
            {
                await _hubContext.Clients.Group($"conversation_{messageAssocie.IdConversation}")
                    .SendAsync("ReceiveMessageWithOffre",
                        messageAssocie.IdConversation,
                        messageAssocie.IdCompte, // L'ID de l'expéditeur
                        messageAssocie.ContenuMessage,  // Le texte du message
                        messageAssocie.DateEnvoiMessage,
                        messageAssocie.IdMessage,
                        entity.IdOffre,
                        entity.Valeur,             // La valeur de l'offre (vérifie le nom de la prop : Prix, Montant ou Valeur)
                        entity.IdAnnonce);
            }

            // Retourne bien les deux clés
            return CreatedAtAction(nameof(GetByID), new { idoffre = entity.IdOffre }, entity);
        }

        /// <summary>
        /// Met à jour une offre existante.
        /// </summary>
        /// <param name="dto">Objet <see cref="OffreUpdateDTO"/> contenant les nouvelles valeurs.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la mise à jour réussit (204).</description></item>
        /// <item><description><see cref="BadRequestResult"/> si l’ID fourni ne correspond pas à celui du DTO (400).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune offre ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Put")]
        [HttpPut("{idoffre}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(int idoffre, [FromBody] OffreUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var toUpdate = await _manager.GetByIdAsync(idoffre);
            if (toUpdate == null)
                return NotFound();

            var updated = _offremapper.Map<Offre>(dto);

            await _manager.UpdateAsync(toUpdate, updated);

            var message = await _managermessage.GetByIdAsync(toUpdate.IdMessage);

            var annonce = await _managerannonce.GetByIdAsync(toUpdate.IdAnnonce);

            int idAcheteur;
            
            if(message.IdCompte == annonce.IdCompte)
            {
                idAcheteur = annonce.IdCompte;
            }
            else
            {
                idAcheteur = message.IdCompte;
            }

            Commande commandeEntity = new Commande();
            
            if (updated.EstAccepte == true)
            {

                CommandeCreateDTO com = new CommandeCreateDTO
                {
                    IdAcheteur = idAcheteur,
                    IdVendeur = annonce.IdCompte,
                    IdAnnonce = dto.IdAnnonce,
                    IdOffre = idoffre,
                    Date = DateTime.UtcNow,
                    IdMoyenPaiement = 1,
                    IdEtatCommande = 1
                };

                commandeEntity = _offremapper.Map<Commande>(com);

                await _managercommande.AddAsync(commandeEntity);

                Facture factureEntity = new Facture
                {
                    IdCommande = commandeEntity.IdCommande
                };
                await _managerfacture.AddAsync(factureEntity);
            }

            if (updated.EstAccepte != null)
            {
                await _notifService.NotifOfrreAccepterOuRejeter(annonce.IdAnnonce, idAcheteur, commandeEntity.IdCommande, updated.Valeur, (bool)updated.EstAccepte);
            }

            if (_hubContext != null)
            {
                await _hubContext.Clients.Group($"conversation_{message.IdConversation}")
                .SendAsync("OffreStatusChanged", dto.IdOffre, dto.EstAccepte);
            }

            return NoContent();
        }
        /// <summary>
        /// Supprime une offre existante.
        /// </summary>
        /// <param name="id">Identifiant unique de l'offre à supprimer.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="NoContentResult"/> si la suppression réussit (204).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune offre ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("Delete")]
        [HttpDelete("{idoffre}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int idoffre)
        {
            var entity = await _manager.GetByIdAsync(idoffre);

            if (entity == null)
                return NotFound();

            await _manager.DeleteAsync(entity);
            return NoContent();
        }
        /// <summary>
        /// Récupère la liste de toutes les offres.
        /// </summary>
        /// <returns>
        /// Une liste de <see cref="OffreDTO"/> (200 OK).
        /// </returns>
        [ActionName("GetAll")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OffreDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OffreDTO>>> GetAll()
        {
            var list = await _manager.GetAllAsync();
            return new ActionResult<IEnumerable<OffreDTO>>(_offremapper.Map<IEnumerable<OffreDTO>>(list));
        }
        /// <summary>
        /// Récupère une offre à partir de ses identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'offre recherchée.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="OffreDTO"/> si l'offre existe (200 OK).</description></item>
        /// <item><description><see cref="NotFoundResult"/> si aucune offre ne correspond (404).</description></item>
        /// </list>
        /// </returns>
        [ActionName("GetById")]
        [HttpGet("{idoffre}")]
        [ProducesResponseType(typeof(OffreDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OffreDTO>> GetByID(int idoffre)
        {
            var result = await _manager.GetByIdAsync(idoffre);

            if (result == null)
                return NotFound();

            return _offremapper.Map<OffreDTO>(result);
        }


        /// <summary>
        /// Récupère toutes les offres liées à un message.
        /// </summary>
        /// <param name="idMessage">Identifiant du message.</param>
        /// <returns>Liste des offres associées au message.</returns>
        [ActionName("GetByMessage")]
        [HttpGet("{idMessage}")]
        [ProducesResponseType(typeof(IEnumerable<OffreDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OffreDTO>>> GetByMessage(int idMessage)
        {
            var offres = await _manager.GetOffresByMessageIdAsync(idMessage);

            if (offres == null || !offres.Any())
                return NotFound();

            return new ActionResult<IEnumerable<OffreDTO>>(_offremapper.Map<IEnumerable<OffreDTO>>(offres));
        }

    }
}
