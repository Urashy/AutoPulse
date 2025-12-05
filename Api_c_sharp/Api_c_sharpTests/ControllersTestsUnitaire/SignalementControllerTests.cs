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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ControllersUnitaires.Tests
{
    [TestClass]
    public class SignalementControllerTests
    {
        private SignalementController _controller;
        private AutoPulseBdContext _context;
        private SignalementManager _manager;
        private IMapper _mapper;
        private Signalement _signalementCommun;
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
            _manager = new SignalementManager(_context);
            _controller = new SignalementController(_manager, _mapper, _journalService);

            // Reset DB
            _context.Signalements.RemoveRange(_context.Signalements);
            await _context.SaveChangesAsync();




            // ----- ENTITÉS -----

            var etat = new EtatSignalement()
            {
                IdEtatSignalement = 1,
                LibelleEtatSignalement = "Ouvert"
            };

            var type = new TypeSignalement()
            {
                IdTypeSignalement = 1,
                LibelleTypeSignalement = "Spam"
            };

            var compteSignalant = new Compte()
            {
                IdCompte = 1,
                Pseudo = "alice",
                MotDePasse = "pw",
                Nom = "Doe",
                Prenom = "Alice",
                Email = "alice@test.com",
                DateCreation = DateTime.UtcNow,
                DateDerniereConnexion = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = 1
            };

            var compteSignale = new Compte()
            {
                IdCompte = 2,
                Pseudo = "bob",
                MotDePasse = "pw",
                Nom = "Smith",
                Prenom = "Bob",
                Email = "bob@test.com",
                DateCreation = DateTime.UtcNow,
                DateDerniereConnexion = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = 1
            };

            await _context.EtatSignalements.AddAsync(etat);
            await _context.TypesSignalement.AddAsync(type);
            await _context.Comptes.AddAsync(compteSignalant);
            await _context.Comptes.AddAsync(compteSignale);

            // --------------------------
            // Signalement commun
            // --------------------------

            _signalementCommun = new Signalement()
            {
                IdSignalement = 1,
                DescriptionSignalement = "Comportement suspect",
                IdCompteSignalant = 1,
                IdCompteSignale = 2,
                IdEtatSignalement = 1,
                IdTypeSignalement = 1,
                DateCreationSignalement = DateTime.UtcNow
            };

            await _context.Signalements.AddAsync(_signalementCommun);
            await _context.SaveChangesAsync();
        }

        // -------------------------------------------------------------
        // GET BY ID
        // -------------------------------------------------------------
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Given : Une Signalement existante
            var id = _signalementCommun.IdSignalement;

            // When : On appelle GetById
            var result = await _controller.GetByID(id);

            // Then : Le résultat doit être un DTO valide
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(SignalementDTO));
            Assert.AreEqual(id, result.Value.IdSignalement);
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
            // Given : Une base contenant au moins une signalement

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
        public async Task PostSignalementTest()
        {
            // Given : Un DTO valide
            SignalementCreateDTO signalementCreateDTO = new SignalementCreateDTO
            {
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = _signalementCommun.IdCompteSignale,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement
            };

            // When : On appelle Post
            var result = await _controller.Post(signalementCreateDTO);

            // Then : La signalement doit être créée (201)
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task BadRequestPostSignalementTest()
        {
            SignalementCreateDTO dto = new SignalementCreateDTO();

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
        public async Task PutSignalementTest()
        {
            // Given : Un DTO valide avec un ID correspondant
            SignalementCreateDTO dto = new SignalementCreateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = _signalementCommun.IdCompteSignale,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement
            };

            // When : On appelle Put
            var result = await _controller.Put(_signalementCommun.IdSignalement, dto);

            // Then : La mise à jour doit renvoyer 204
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task PutBadRequestTest()
        {
            // Given : un DTO dont la validation doit échouer
            var dto = new SignalementCreateDTO { IdSignalement = 999 };

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
            // Given : Une signalement inexistante
            var dto = new SignalementCreateDTO() { IdSignalement = 10 };

            // When : On appelle Put
            var result = await _controller.Put(10, dto);

            // Then : 404 NotFound
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -------------------------------------------------------------
        // DELETE
        // -------------------------------------------------------------
        [TestMethod]
        public async Task DeleteSignalementTest()
        {
            // Given : Une signalement existante
            var id = _signalementCommun.IdSignalement;

            // When : On appelle Delete
            var result = await _controller.Delete(id);

            // Then : La signalement doit être supprimée
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            Assert.IsNull(await _manager.GetByIdAsync(id));
        }

        [TestMethod]
        public async Task NotFoundDeleteSignalementTest()
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
            
            var result = await _controller.GetAllByEtatSignalement(_signalementCommun.IdEtatSignalement);

            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task NotFoundGetAllbyEtatSignalementTest()
        {

            var result = await _controller.GetAllByEtatSignalement(0);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}
