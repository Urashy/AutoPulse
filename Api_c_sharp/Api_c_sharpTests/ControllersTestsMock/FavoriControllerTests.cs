using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass]
    public class FavoriControllerTests
    {
        private Mock<FavoriManager> _mockManager;
        private IMapper _mapper;
        private Mock<IJournalService> _mockJournal;
        private FavoriController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<FavoriManager>(null);
            _mockJournal = new Mock<IJournalService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Favori, FavoriDTO>().ReverseMap();
            });

            _mapper = config.CreateMapper();
            _controller = new FavoriController(_mockManager.Object, _mapper, _mockJournal.Object);
        }

        #region GET
        [TestMethod]
        public async Task GetByIDS_ReturnsOk_WhenExists()
        {
            // Arrange
            var entity = new Favori { IdCompte = 1, IdAnnonce = 2 };

            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 2))
                        .ReturnsAsync(entity);

            // Act
            var result = await _controller.GetByIDS(1, 2);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(FavoriDTO));
        }

        [TestMethod]
        public async Task GetByIDS_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 2))
                        .ReturnsAsync((Favori)null);

            // Act
            var result = await _controller.GetByIDS(1, 2);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        
        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            // Arrange
            var data = new List<Favori>
            {
                new Favori { IdCompte = 1 , IdAnnonce = 1 },
                new Favori { IdCompte = 2 , IdAnnonce = 2 }
            };

            _mockManager.Setup(m => m.GetAllAsync()).ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task GetByCompteId_ReturnsList()
        {
            // Arrange
            var data = new List<Favori>
            {
                new Favori { IdCompte = 1, IdAnnonce = 1 },
                new Favori { IdCompte = 1, IdAnnonce = 2 }
            };

            _mockManager.Setup(m => m.GetByCompteIdAsync(1))
                        .ReturnsAsync(data);

            // Act
            var result = await _controller.GetByCompteId(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
            Assert.IsTrue(result.Value.All(f => f.IdCompte == 1));
        }

        #endregion

        #region POST
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            // Arrange
            var dto = new FavoriDTO { IdCompte = 1, IdAnnonce = 2 };
            var entity = new Favori { IdCompte = 1, IdAnnonce = 2 };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Favori>()))
                        .ReturnsAsync(entity);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("x", "invalid");
            var dto = new FavoriDTO();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Favori>()), Times.Never);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var existing = new Favori { IdCompte = 1, IdAnnonce = 1 };
            var dto = new FavoriDTO { IdCompte = 1, IdAnnonce = 1 };

            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 1))
                        .ReturnsAsync(existing);

            _mockManager.Setup(m => m.UpdateAsync(existing, It.IsAny<Favori>()))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(1, 1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("x", "invalid");
            var dto = new FavoriDTO { IdAnnonce = 1 };

            // Act
            var result = await _controller.Put(1, 1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Favori>(), It.IsAny<Favori>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 1))
                        .ReturnsAsync((Favori)null);

            var dto = new FavoriDTO { IdCompte = 1, IdAnnonce = 1 };

            // Act
            var result = await _controller.Put(1, 1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var existing = new Favori { IdCompte = 1, IdAnnonce = 1 };

            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 1))
                        .ReturnsAsync(existing);

            _mockManager.Setup(m => m.DeleteAsync(existing))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1, 1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 1))
                        .ReturnsAsync((Favori)null);

            // Act
            var result = await _controller.Delete(1, 1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region Favoris
        [TestMethod]
        public async Task IsFavorite_ReturnsTrue_WhenExists()
        {
            // Arrange
            var entity = new Favori { IdCompte = 1, IdAnnonce = 2 };

            _mockManager.Setup(m => m.ExistsAsync(1, 2))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.IsFavorite(1, 2);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Value);
        }

        [TestMethod]
        public async Task IsFavorite_ReturnsFalse_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.ExistsAsync(1, 2))
                        .ReturnsAsync(false);

            // Act
            var result = await _controller.IsFavorite(1, 2);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Value);
        }
        #endregion

    }
}
