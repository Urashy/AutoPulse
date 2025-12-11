using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Api_c_sharp.Controllers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersUnitaires.Tests
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

            EtatSignalement etat = new EtatSignalement()
            {
                IdEtatSignalement = 1,
                LibelleEtatSignalement = "Ouvert"
            };

            EtatSignalement etat2 = new EtatSignalement()
            {
                IdEtatSignalement = 2,
                LibelleEtatSignalement = "Traité"
            };

            EtatSignalement etat3 = new EtatSignalement()
            {
                IdEtatSignalement = 3,
                LibelleEtatSignalement = "Rejeté"
            };

            TypeSignalement type = new TypeSignalement()
            {
                IdTypeSignalement = 1,
                LibelleTypeSignalement = "Spam"
            };

            Compte compteSignalant = new Compte()
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

            Compte compteSignale = new Compte()
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
            await _context.EtatSignalements.AddAsync(etat2);
            await _context.EtatSignalements.AddAsync(etat3);
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
            SignalementCreateDTO signalementCreateDTO = new SignalementCreateDTO
            {
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement
            };
            ClaimCookie(1);

            var result = await _controller.Post(signalementCreateDTO);

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
            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
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
            SignalementUpdateDTO dto = new SignalementUpdateDTO 
            { 
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = null,
            };

            _controller.ModelState.AddModelError("DescriptionSignalement", "Required");

            // When
            var result = await _controller.Put(1, dto);

            // Then
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task PutNotFoundTest()
        {
            // Given : Une signalement inexistante
            var dto = new SignalementUpdateDTO() { IdSignalement = 10 };

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

        [TestMethod]
        public async Task GetFilteredCompte()
        {
            // Given: Un signalement existant avec "suspect" dans la description

            // When: On filtre par état, type et recherche "suspect"
            var result = await _controller.GetFilteredSignalement(
                _signalementCommun.IdEtatSignalement,
                2,
                "suspect");

            // Then: On doit obtenir une liste avec des résultats
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any(), "Devrait trouver au moins un signalement");
        }

        [TestMethod]
        public async Task GetFilteredAnnonce()
        {
            _signalementCommun.IdCompteSignale = null;
            _signalementCommun.IdAnnonceSignale = 1;
            await _context.SaveChangesAsync();
            var result = await _controller.GetFilteredSignalement(
                _signalementCommun.IdEtatSignalement,
                1,
                "suspect");

            // Then: On doit obtenir une liste avec des résultats
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any(), "Devrait trouver au moins un signalement");
        }

        [TestMethod]
        public async Task EmptyResultGetFilteredSignalementTest()
        {
            // Given: Des paramètres qui ne matchent aucun signalement

            // When: On filtre avec des critères qui ne donnent aucun résultat
            var result = await _controller.GetFilteredSignalement(0, 0, "existe pas");

            // Then: On doit obtenir une liste vide (pas une erreur)
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsFalse(result.Value.Any(), "Ne devrait trouver aucun signalement");
        }

        [TestMethod]
        public async Task GetFilteredSignalement_FilterByEtatOnlyTest()
        {
            // When: Filtre uniquement par état (typeId=0 = pas de filtre type)
            var result = await _controller.GetFilteredSignalement(
                _signalementCommun.IdEtatSignalement,
                0,
                null);

            // Then
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.All(s => s.IdEtatSignalement == _signalementCommun.IdEtatSignalement));
        }

        [TestMethod]
        public async Task GetFilteredSignalement_CaseInsensitiveSearchTest()
        {
            // When: Recherche avec casse mixte
            var result = await _controller.GetFilteredSignalement(0, 0, "CoMpOrTeMeNt");

            // Then: Devrait trouver "Comportement suspect" malgré la casse différente
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetFilteredSignalement_NoFiltersTest()
        {
            // When: Aucun filtre (etatId=0, typeId=0, recherche=null)
            var result = await _controller.GetFilteredSignalement(0, 0, null);

            // Then: Devrait retourner tous les signalements
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetFilteredSignalement_SearchInPseudoTest()
        {
            // When: Recherche par pseudo du compte signalant
            var result = await _controller.GetFilteredSignalement(0, 0, "alice");

            // Then: Devrait trouver le signalement créé par alice
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(s => s.PseudoSignalant == "alice"));
        }

        [TestMethod]
        public async Task UpdateEtatTest()
        {

            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement,
                IdEtatSignalement = 2,

            };

            var result = await _controller.UpdateEtat(_signalementCommun.IdSignalement, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var signalementMisAJour = await _manager.GetByIdAsync(_signalementCommun.IdSignalement);
            Assert.AreEqual(dto.IdEtatSignalement, signalementMisAJour.IdEtatSignalement);
        }

        [TestMethod]
        public async Task UpdateEtatNotFoundTest()
        {

            var idInexistant = 999;
            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement,
                IdEtatSignalement = 2
            };


            var result = await _controller.UpdateEtat(idInexistant, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task UpdateEtatBadRequestInvalidState5Test()
        {

            var id = _signalementCommun.IdSignalement;
            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement,
                IdEtatSignalement = 5
            };

            var result = await _controller.UpdateEtat(id, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task BadRequestUpdateEtatTest()
        {
            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = null,
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement,
                IdEtatSignalement = 2,

            };

            _controller.ModelState.AddModelError("DescriptionSignalement", "Required");

            // When
            var result = await _controller.UpdateEtat(1, dto);

            // Then
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task PostBadRequestAnnonceEtCompteTest()
        {

            SignalementCreateDTO dto = new SignalementCreateDTO
            {
                DescriptionSignalement = "Description test",
                IdAnnonceSignale = 1,
                IdCompteSignale = 2,
                IdTypeSignalement = 1
            };
            ClaimCookie(1);


            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.AreEqual("Un signalement ne peut pas cibler à la fois une annonce et un compte",
                            badRequestResult.Value);
        }

        [TestMethod]
        public async Task PostBadRequestAucuneCibleTest()
        {

            SignalementCreateDTO dto = new SignalementCreateDTO
            {
                DescriptionSignalement = "Description test",
                IdAnnonceSignale = null,
                IdCompteSignale = null,
                IdTypeSignalement = 1
            };
            ClaimCookie(1);

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.AreEqual("Un signalement doit cibler soit une annonce soit un compte",
                            badRequestResult.Value);
        }

        private void ClaimCookie(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim("idUser", userId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}
