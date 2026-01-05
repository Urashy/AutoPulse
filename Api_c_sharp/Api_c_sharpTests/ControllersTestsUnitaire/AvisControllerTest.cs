using Api_c_sharp.Mapper;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass]
    public class AvisControllerTests
    {
        private AvisController _controller;
        private AutoPulseBdContext _context;
        private AvisManager _manager;
        private IMapper _mapper;
        private Avis _objetCommun;
        private IJournalService _journalService;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _journalService = new JournalManager(_context, NullLogger<JournalManager>.Instance);
            _manager = new AvisManager(_context);
            _controller = new AvisController(_manager, _mapper,_journalService);

            // Nettoyage
            _context.Avis.RemoveRange(_context.Avis);
            await _context.SaveChangesAsync();

            // Création d’un avis commun
            var avis = new Avis()
            {
                IdAvis = 1,
                IdJugee = 1,
                IdCommande = 1,
                ContenuAvis = "Trés bonne avis",
                DateAvis = DateTime.Now,
                NoteAvis = 5
            };

            await _context.Avis.AddAsync(avis);
            await _context.SaveChangesAsync();

            _objetCommun = avis;
        }

        #region GET

            #region GetByID
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetCommun.IdAvis);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(AvisDetailDTO));
            Assert.AreEqual(_objetCommun.NoteAvis, result.Value.NoteAvis);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(0);

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
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AvisListDTO>));
            Assert.IsTrue(result.Value.Any());
        }
        #endregion

            #region GetAllByCompte
        [TestMethod]
        public async Task GetAllByCompte()
        {
            // Act
            var result = await _controller.GetAvisByCompteID(_objetCommun.IdJugee);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AvisListDTO>));
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task NotFoundGetAllByCompte()
        {
            // Act
            var result = await _controller.GetAvisByCompteID(999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #endregion

        #region POST
        [TestMethod]
        public async Task PostAvisTest()
        {
            // Arrange
            var dto = new AvisCreateDTO()
            {
                IdJugee = 2,
                IdCommande = 3,
                ContenuAvis = "Correct",
                NoteAvis = 3
            };

            // Act
            var actionResult = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdAvis = (Avis)created.Value;
            Assert.AreEqual(dto.ContenuAvis, createdAvis.ContenuAvis);
        }

        [TestMethod]
        public async Task BadRequestPostAvisTest()
        {
            // Arrange
            var dto = new AvisCreateDTO();
            _controller.ModelState.AddModelError("Commentaire", "Required");

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteAvisTest()
        {
            // Act
            var result = await _controller.Delete(_objetCommun.IdAvis);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var verify = await _manager.GetByIdAsync(_objetCommun.IdAvis);
            Assert.IsNull(verify);
        }

        [TestMethod]
        public async Task NotFoundDeleteAvisTest()
        {
            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutAvisTest()
        {
            // Arrange
            AvisUpdateDTO dto = new AvisUpdateDTO()
            {
                IdAvis = _objetCommun.IdAvis,
                IdJugee = 1,
                IdCommande = 1,
                ContenuAvis = "Modifié",
                NoteAvis = 4
            };

            // Act
            var result = await _controller.Put(_objetCommun.IdAvis, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updated = await _manager.GetByIdAsync(_objetCommun.IdAvis);
            Assert.AreEqual(dto.ContenuAvis, updated.ContenuAvis);
        }

        [TestMethod]
        public async Task NotFoundPutAvisTest()
        {
            // Arrange
            AvisUpdateDTO dto = new AvisUpdateDTO()
            {
                ContenuAvis = "Test"
            };

            // Act
            var result = await _controller.Put(0, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutAvisTest()
        {
            // Arrange
            AvisUpdateDTO dto = new AvisUpdateDTO()
            {
                IdAvis = _objetCommun.IdAvis,
                ContenuAvis = "",
            };

            _controller.ModelState.AddModelError("Commentaire", "Invalid");

            // Act
            var result = await _controller.Put(_objetCommun.IdAvis, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }
        #endregion
    }
}
