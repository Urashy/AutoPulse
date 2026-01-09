using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Api_c_sharp.Models.Repository.Interfaces;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass]
    public class AvisControllerTests
    {
        private Mock<AvisManager> _mockManager;
        private Mock<IJournalService> _mockJournal;
        private IMapper _mapper;
        private AvisController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<AvisManager>(null);
            _mockJournal = new Mock<IJournalService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Avis, AvisDetailDTO>().ReverseMap();
                cfg.CreateMap<Avis, AvisListDTO>().ReverseMap();
                cfg.CreateMap<Avis, AvisCreateDTO>().ReverseMap();
                cfg.CreateMap<Avis, AvisUpdateDTO>().ReverseMap();
            });

            _mapper = config.CreateMapper();

            _controller = new AvisController(_mockManager.Object, _mapper, _mockJournal.Object);
        }

        #region GET
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            var avis = new Avis
            {
                IdAvis = 1,
                IdJugee = 2,
                IdJugeur = 3,
                IdCommande = 4,
                ContenuAvis = "test",
                NoteAvis = 4
            };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(avis);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(AvisDetailDTO));
            Assert.AreEqual(1, result.Value.IdAvis);
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync((Avis)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }


        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            // Arrange
            var data = new List<Avis>
            {
                new Avis { IdAvis = 1, ContenuAvis = "A", NoteAvis = 4 },
                new Avis { IdAvis = 2, ContenuAvis = "B", NoteAvis = 5 }
            };


            _mockManager.Setup(m => m.GetAllAsync())
                        .ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task Post_ReturnsCreated_WhenAvisDoesNotExist()
        {
            // Arrange
            var dto = new AvisCreateDTO
            {
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "test",
                NoteAvis = 4
            };

            var entity = new Avis
            {
                IdAvis = 10,
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "test",
                NoteAvis = 4
            };

            _mockManager.Setup(m => m.ExisteDejaAsync(dto.IdCommande, dto.IdJugeur))
                        .ReturnsAsync(false);

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Avis>()))
                        .ReturnsAsync(entity);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            _mockManager.Verify(m => m.ExisteDejaAsync(dto.IdCommande, dto.IdJugeur), Times.Once);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Avis>()), Times.Once);
            _mockJournal.Verify(j => j.LogDepotAvisAsync(
                dto.IdJugeur,
                dto.IdJugee,
                It.IsAny<int>(),
                dto.NoteAvis,
                dto.ContenuAvis), Times.Once);
        }

        [TestMethod]
        public async Task Post_ReturnsConflict_WhenAvisAlreadyExists()
        {
            // Arrange
            var dto = new AvisCreateDTO
            {
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "test",
                NoteAvis = 4
            };

            _mockManager.Setup(m => m.ExisteDejaAsync(dto.IdCommande, dto.IdJugeur))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
            var conflictResult = (ConflictObjectResult)result.Result;
            Assert.AreEqual("Vous avez déjà déposé un avis pour cette commande.", conflictResult.Value);

            _mockManager.Verify(m => m.ExisteDejaAsync(dto.IdCommande, dto.IdJugeur), Times.Once);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Avis>()), Times.Never);
            _mockJournal.Verify(j => j.LogDepotAvisAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new AvisCreateDTO();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.ExisteDejaAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Avis>()), Times.Never);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var existing = new Avis
            {
                IdAvis = 1,
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "old",
                NoteAvis = 2
            };

            var dto = new AvisUpdateDTO
            {
                IdAvis = 1,
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "updated",
                NoteAvis = 5
            };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(existing);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new AvisUpdateDTO();

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Avis>(), It.IsAny<Avis>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync((Avis)null);

            var dto = new AvisUpdateDTO
            {
                IdAvis = 1,
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "updated",
                NoteAvis = 5
            };

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var avis = new Avis { IdAvis = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(avis);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync((Avis)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region GetBy
        [TestMethod]
        public async Task GetAvisByCompteID_ReturnsList_WhenExists()
        {
            // Arrange
            var data = new List<Avis>
            {
                new Avis { IdAvis = 1, ContenuAvis = "A", NoteAvis = 4 },
                new Avis { IdAvis = 2, ContenuAvis = "B", NoteAvis = 5 }
            };

            _mockManager.Setup(m => m.GetAvisByCompteId(5))
                        .ReturnsAsync(data);

            // Act
            var result = await _controller.GetAvisByCompteId(5);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task GetAvisByCompteID_ReturnsNotFound_WhenEmpty()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAvisByCompteId(5))
                        .ReturnsAsync(new List<Avis>());

            // Act
            var result = await _controller.GetAvisByCompteId(5);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion
    }
}