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

            EtatSignalementPlainte etat = new EtatSignalementPlainte()
            {
                IdEtatSignalement = 1,
                LibelleEtatSignalement = "Ouvert"
            };

            EtatSignalementPlainte etat2 = new EtatSignalementPlainte()
            {
                IdEtatSignalement = 2,
                LibelleEtatSignalement = "Traité"
            };

            EtatSignalementPlainte etat3 = new EtatSignalementPlainte()
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

            await _context.EtatSignalementsPlaintes.AddAsync(etat);
            await _context.EtatSignalementsPlaintes.AddAsync(etat2);
            await _context.EtatSignalementsPlaintes.AddAsync(etat3);
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

        #region GET

            #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            var id = _signalementCommun.IdSignalement;

            // Act
            var result = await _controller.GetByID(id);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(SignalementDTO));
            Assert.AreEqual(id, result.Value.IdSignalement);
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

        #endregion


        #region POST
        [TestMethod]
        public async Task PostSignalementCompteTest()
        {
            // Arrange
            SignalementCreateDTO signalementCreateDTO = new SignalementCreateDTO
            {
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement
            };
            ClaimCookie(1);

            // Act
            var result = await _controller.Post(signalementCreateDTO);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task PostSignalementAnnonceTest()
        {
            // Arrange
            SignalementCreateDTO signalementCreateDTO = new SignalementCreateDTO
            {
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdAnnonceSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement
            };
            ClaimCookie(1);

            // Act
            var result = await _controller.Post(signalementCreateDTO);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task BadRequestPostSignalementTest()
        {
            // Arrange
            SignalementCreateDTO dto = new SignalementCreateDTO();

            _controller.ModelState.AddModelError("Erreur", "Required");

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task PostBadRequestAnnonceEtCompteTest()
        {
            // Arrange
            SignalementCreateDTO dto = new SignalementCreateDTO
            {
                DescriptionSignalement = "Description test",
                IdAnnonceSignale = 1,
                IdCompteSignale = 2,
                IdTypeSignalement = 1
            };
            ClaimCookie(1);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.AreEqual("Un signalement ne peut pas cibler à la fois une annonce et un compte",
                            badRequestResult.Value);
        }

        [TestMethod]
        public async Task PostBadRequestAucuneCibleTest()
        {
            // Arrange
            SignalementCreateDTO dto = new SignalementCreateDTO
            {
                DescriptionSignalement = "Description test",
                IdAnnonceSignale = null,
                IdCompteSignale = null,
                IdTypeSignalement = 1
            };
            ClaimCookie(1);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.AreEqual("Un signalement doit cibler soit une annonce soit un compte",
                            badRequestResult.Value);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutSignalementTest()
        {
            // Arrange
            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement
            };

            // Act
            var result = await _controller.Put(_signalementCommun.IdSignalement, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task PutBadRequestTest()
        {
            // Arrange
            SignalementUpdateDTO dto = new SignalementUpdateDTO 
            { 
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = null,
            };

            _controller.ModelState.AddModelError("DescriptionSignalement", "Required");

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task PutNotFoundTest()
        {
            // Arrange
            var dto = new SignalementUpdateDTO() { IdSignalement = 10 };

            // Act
            var result = await _controller.Put(10, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteSignalementTest()
        {
            // Arrange
            var id = _signalementCommun.IdSignalement;

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            Assert.IsNull(await _manager.GetByIdAsync(id));
        }

        [TestMethod]
        public async Task NotFoundDeleteSignalementTest()
        {
            // Arrange
            var id = 0;

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region FILTRED
        [TestMethod]
        public async Task GetFilteredCompte()
        {
            // Act
            var result = await _controller.GetFilteredSignalement(
                _signalementCommun.IdEtatSignalement,
                2,
                "suspect");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any(), "Devrait trouver au moins un signalement");
        }

        [TestMethod]
        public async Task GetFilteredAnnonce()
        {
            // Arrange
            _signalementCommun.IdCompteSignale = null;
            _signalementCommun.IdAnnonceSignale = 1;
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetFilteredSignalement(
                _signalementCommun.IdEtatSignalement,
                1,
                "suspect");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any(), "Devrait trouver au moins un signalement");
        }

        [TestMethod]
        public async Task EmptyResultGetFilteredSignalementTest()
        {
            // Act
            var result = await _controller.GetFilteredSignalement(0, 0, "existe pas");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsFalse(result.Value.Any(), "Ne devrait trouver aucun signalement");
        }

        [TestMethod]
        public async Task GetFilteredSignalement_FilterByEtatOnlyTest()
        {
            // Act
            var result = await _controller.GetFilteredSignalement(
                _signalementCommun.IdEtatSignalement,
                0,
                null);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.All(s => s.IdEtatSignalement == _signalementCommun.IdEtatSignalement));
        }

        [TestMethod]
        public async Task GetFilteredSignalement_CaseInsensitiveSearchTest()
        {
            // Act
            var result = await _controller.GetFilteredSignalement(0, 0, "CoMpOrTeMeNt");

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetFilteredSignalement_NoFiltersTest()
        {
            // Act
            var result = await _controller.GetFilteredSignalement(0, 0, null);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetFilteredSignalement_SearchInPseudoTest()
        {
            // Act
            var result = await _controller.GetFilteredSignalement(0, 0, "alice");

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(s => s.PseudoSignalant == "alice"));
        }
        #endregion

        #region Update
        [TestMethod]
        public async Task UpdateEtatTest()
        {
            // Arrange
            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement,
                IdEtatSignalement = 2,

            };

            // Act
            var result = await _controller.UpdateEtat(_signalementCommun.IdSignalement, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var signalementMisAJour = await _manager.GetByIdAsync(_signalementCommun.IdSignalement);
            Assert.AreEqual(dto.IdEtatSignalement, signalementMisAJour.IdEtatSignalement);
        }

        [TestMethod]
        public async Task UpdateEtatNotFoundTest()
        {
            // Arrange
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

            // Act
            var result = await _controller.UpdateEtat(idInexistant, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task UpdateEtatBadRequestInvalidState5Test()
        {
            // Arrange
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

            // Act
            var result = await _controller.UpdateEtat(id, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task BadRequestUpdateEtatTest()
        {
            // Arrange
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

            // Act
            var result = await _controller.UpdateEtat(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

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
