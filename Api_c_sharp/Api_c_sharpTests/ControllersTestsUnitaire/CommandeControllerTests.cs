using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Api_c_sharp.Controllers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api_c_sharp.ControllersUnitaires.Tests
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

        #region GET
            #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            var id = _commandeCommun.IdCommande;

            // Act
            var result = await _controller.GetByID(id);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CommandeDetailDTO));
            Assert.AreEqual(id, result.Value.IdCommande);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            var idInexistant = 0;

            // Act
            var result = await _controller.GetByID(idInexistant);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

            #region GetAll
        [TestMethod]
        public async Task GetAllTest()
        {
            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }
        #endregion

            #region GetCommandeByCompteID
        [TestMethod]
        public async Task GetCommandeByCompteIDTest()
        {
            // Arrange
            var idAcheteur = _commandeCommun.IdAcheteur;

            // Act
            var result = await _controller.GetCommandeByCompteID(idAcheteur);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetCommandeByCompteIDNotFoundTest()
        {
            // Act
            var result = await _controller.GetCommandeByCompteID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion s
        #endregion

        #region POST
        [TestMethod]
        public async Task PostCommandeTest()
        {
            // Arrange
            CommandeCreateDTO commandeCreateDTO = new CommandeCreateDTO
            {
                IdVendeur = _commandeCommun.IdVendeur,
                IdAcheteur = _commandeCommun.IdAcheteur,
                IdAnnonce = _commandeCommun.IdAnnonce,
                IdMoyenPaiement = _commandeCommun.IdMoyenPaiement,
                Date = DateTime.UtcNow
            };

            // Act
            var result = await _controller.Post(commandeCreateDTO);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task BadRequestPostCommandeTest()
        {
            // Arrange
            CommandeCreateDTO dto = new CommandeCreateDTO();

            _controller.ModelState.AddModelError("Erreur", "Required");

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutCommandeTest()
        {
            // Arrange
            CommandeUpdateDTO dto = new CommandeUpdateDTO
            {
                IdCommande = _commandeCommun.IdCommande,
                IdVendeur = _commandeCommun.IdVendeur,
                IdAcheteur = _commandeCommun.IdAcheteur,
                IdAnnonce = _commandeCommun.IdAnnonce,
                IdMoyenPaiement = _commandeCommun.IdMoyenPaiement,
                Date = DateTime.UtcNow
            };

            // Act
            var result = await _controller.Put(_commandeCommun.IdCommande, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task PutBadRequestTest()
        {
            // Arrange
            var dto = new CommandeUpdateDTO { IdCommande = 999 };

            _controller.ModelState.AddModelError("Test", "Invalid Model");

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task PutNotFoundTest()
        {
            // Arrange
            var dto = new CommandeUpdateDTO() { IdCommande = 10 };

            // Act
            var result = await _controller.Put(10, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteCommandeTest()
        {
            // Arrange
            var id = _commandeCommun.IdCommande;

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            Assert.IsNull(await _manager.GetByIdAsync(id));
        }

        [TestMethod]
        public async Task NotFoundDeleteCommandeTest()
        {
            // Arrange
            var id = 0;

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion
        
    }
}
