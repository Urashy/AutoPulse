using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using App.Controllers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace App.ControllersUnitaires.Tests
{
    [TestClass]
    public class CommandeControllerTests
    {
        private CommandeController _controller;
        private AutoPulseBdContext _context;
        private CommandeManager _manager;
        private IMapper _mapper;
        private Commande _commandeCommun;
        private IJournalService _journalService;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            // AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _journalService = new JournalManager(_context, NullLogger<JournalManager>.Instance);
            _manager = new CommandeManager(_context);
            _controller = new CommandeController(_manager, _mapper, _journalService);

            // Reset DB
            _context.Commandes.RemoveRange(_context.Commandes);
            await _context.SaveChangesAsync();




            // ----- ENTITÉS -----

            var tyepeCompte = new TypeCompte()
            {
                IdTypeCompte = 1,
                Libelle = "Standard"
            };
            await _context.TypesCompte.AddAsync(tyepeCompte);


            var annonce = new Annonce()
            {
                IdAnnonce = 1,
                Libelle = "Super produit",
                IdCompte = 1
            };

            var vendeur = new Compte
            {
                IdCompte = 1,
                Pseudo = "john",
                MotDePasse = "hashedpassword",
                Nom = "Doe",
                Prenom = "John",
                Email = "john@doe.com",
                DateCreation = DateTime.UtcNow,
                DateDerniereConnexion = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = 1
            };
            await _context.Comptes.AddAsync(vendeur);

            var acheteur = new Compte
            {
                IdCompte = 2,
                Pseudo = "johny",
                MotDePasse = "hashedpassword",
                Nom = "Doe",
                Prenom = "John",
                Email = "johny@doe.com",
                DateCreation = DateTime.UtcNow,
                DateDerniereConnexion = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = 1
            };
            await _context.Comptes.AddAsync(acheteur);
            var moyenPaiement = new MoyenPaiement()
            {
                IdMoyenPaiement = 1,
                TypePaiement = "Carte"
            };

            await _context.Annonces.AddAsync(annonce);
            await _context.MoyensPaiements.AddAsync(moyenPaiement);

            _commandeCommun = new Commande()
            {
                IdCommande = 1,
                IdAnnonce = 1,
                IdAcheteur = 50,
                IdVendeur = 11,
                IdMoyenPaiement = 1,
                CommandeAnnonceNav = annonce,
                CommandeMoyenPaiementNav = moyenPaiement,
                Date = DateTime.UtcNow
            };

            await _context.Commandes.AddAsync(_commandeCommun);
            await _context.SaveChangesAsync();
        }

        // -------------------------------------------------------------
        // GET BY ID
        // -------------------------------------------------------------
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Given : Une commande existante
            var id = _commandeCommun.IdCommande;

            // When : On appelle GetById
            var result = await _controller.GetByID(id);

            // Then : Le résultat doit être un DTO valide
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CommandeDetailDTO));
            Assert.AreEqual(id, result.Value.IdCommande);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Given : Un ID inexistant
            var idInexistant = 0;

            // When : On appelle GetById
            var result = await _controller.GetByID(idInexistant);

            // Then : On doit obtenir 404
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // -------------------------------------------------------------
        // GET ALL
        // -------------------------------------------------------------
        [TestMethod]
        public async Task GetAllTest()
        {
            // Given : Une base contenant au moins une commande

            // When : On appelle GetAll
            var result = await _controller.GetAll();

            // Then : La liste doit être non vide
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        // -------------------------------------------------------------
        // POST
        // -------------------------------------------------------------
        [TestMethod]
        public async Task PostCommandeTest()
        {
            // Given : Un DTO valide
            CommandeCreateDTO commandeCreateDTO = new CommandeCreateDTO
            {
                IdCommande = 2,
                IdVendeur = _commandeCommun.IdVendeur,
                IdAcheteur = _commandeCommun.IdAcheteur,
                IdAnnonce = _commandeCommun.IdAnnonce,
                IdMoyenPaiement = _commandeCommun.IdMoyenPaiement,
                Date = DateTime.UtcNow
            };

            // When : On appelle Post
            var result = await _controller.Post(commandeCreateDTO);

            // Then : La commande doit être créée (201)
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task BadRequestPostCommandeTest()
        {
            CommandeCreateDTO dto = new CommandeCreateDTO();

            _controller.ModelState.AddModelError("Erreur", "Required");

            // When : On appelle Post
            var result = await _controller.Post(dto);

            // Then : On récupère un 400
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        // -------------------------------------------------------------
        // PUT
        // -------------------------------------------------------------
        [TestMethod]
        public async Task PutCommandeTest()
        {
            // Given : Un DTO valide avec un ID correspondant
            CommandeCreateDTO dto = new CommandeCreateDTO
            {
                IdCommande = _commandeCommun.IdCommande,
                IdVendeur = _commandeCommun.IdVendeur,
                IdAcheteur = _commandeCommun.IdAcheteur,
                IdAnnonce = _commandeCommun.IdAnnonce,
                IdMoyenPaiement = _commandeCommun.IdMoyenPaiement,
                Date = DateTime.UtcNow
            };

            // When : On appelle Put
            var result = await _controller.Put(_commandeCommun.IdCommande, dto);

            // Then : La mise à jour doit renvoyer 204
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task PutBadRequestTest()
        {
            // Given : un DTO dont la validation doit échouer
            var dto = new CommandeCreateDTO { IdCommande = 999 };

            // On force une erreur de validation pour déclencher BadRequest()
            _controller.ModelState.AddModelError("Test", "Invalid Model");

            // When
            var result = await _controller.Put(1, dto);

            // Then
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task PutNotFoundTest()
        {
            // Given : Une commande inexistante
            var dto = new CommandeCreateDTO() { IdCommande = 10 };

            // When : On appelle Put
            var result = await _controller.Put(10, dto);

            // Then : 404 NotFound
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -------------------------------------------------------------
        // DELETE
        // -------------------------------------------------------------
        [TestMethod]
        public async Task DeleteCommandeTest()
        {
            // Given : Une commande existante
            var id = _commandeCommun.IdCommande;

            // When : On appelle Delete
            var result = await _controller.Delete(id);

            // Then : La commande doit être supprimée
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            Assert.IsNull(await _manager.GetByIdAsync(id));
        }

        [TestMethod]
        public async Task NotFoundDeleteCommandeTest()
        {
            // Given : Un ID inexistant
            var id = 0;

            // When : On appelle Delete
            var result = await _controller.Delete(id);

            // Then : 404
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -------------------------------------------------------------
        // GET ALL BY TYPE
        // -------------------------------------------------------------
        [TestMethod]
        public async Task GetCommandeByCompteIDTest()
        {

            var idAcheteur = _commandeCommun.IdAcheteur;

            var result = await _controller.GetCommandeByCompteID(idAcheteur);

            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetCommandeByCompteIDNotFoundTest()
        {
            // Acts
            var result = await _controller.GetCommandeByCompteID(0);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult)); 
        }
    }
}
