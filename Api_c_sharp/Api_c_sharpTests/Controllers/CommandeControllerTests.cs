using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using App.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Controllers.Tests
{
    [TestClass]
    public class CommandeControllerTests
    {
        private CommandeController _controller;
        private AutoPulseBdContext _context;
        private CommandeManager _manager;
        private IMapper _mapper;
        private Commande _commandeCommun;

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

            _manager = new CommandeManager(_context);
            _controller = new CommandeController(_manager, _mapper);

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
            var dto = new CommandeDTO()
            {
                IdCommande = 2,
                Date = DateTime.UtcNow,
                PseudoAcheteur = "john",
                PseudoVendeur = "johny",
                LibelleAnnonce = "Super produit",
                MoyenPaiement = "Carte"
            };

            // When : On appelle Post
            var result = await _controller.Post(dto);

            // Then : La commande doit être créée (201)
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task BadRequestPostCommandeTest()
        {
            // Given : Un DTO invalide
            var dto = new CommandeDTO();
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
            var dto = new CommandeDTO()
            {
                IdCommande = _commandeCommun.IdCommande,
                Date = DateTime.UtcNow,
                PseudoAcheteur = "TestA",
                PseudoVendeur = "TestV",
                LibelleAnnonce = "Modifié",
                MoyenPaiement = "Carte"
            };

            // When : On appelle Put
            var result = await _controller.Put(_commandeCommun.IdCommande, dto);

            // Then : La mise à jour doit renvoyer 204
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task PutBadRequestTest()
        {
            // Given : un mismatch ID
            var dto = new CommandeDTO { IdCommande = 999 };

            // When : On appelle Put avec un autre ID
            var result = await _controller.Put(1, dto);

            // Then : 400 BadRequest
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task PutNotFoundTest()
        {
            // Given : Une commande inexistante
            var dto = new CommandeDTO() { IdCommande = 10 };

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
        public async Task GetAllByTypeTest()
        {
            // Given : Un acheteur ayant une commande
            var idAcheteur = _commandeCommun.IdAcheteur;

            // When : On appelle GetAllByType
            var result = await _controller.GetAllByType(idAcheteur);

            // Then : La liste doit contenir des commandes
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }
    }
}
