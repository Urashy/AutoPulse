using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass]
    public class BloqueControllerTests
    {
        private Mock<BloqueManager> _mockManager;
        private IMapper _mapper;
        private BloqueController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<BloqueManager>(null);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Bloque, BloqueDTO>().ReverseMap();
            });

            _mapper = config.CreateMapper();

            _controller = new BloqueController(_mockManager.Object, _mapper);
        }

        #region GET
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            var entity = new Bloque { IdBloquant = 1, IdBloque = 2 };

            _mockManager.Setup(m => m.GetBloqueByIdsAsync(2, 1))
                        .ReturnsAsync(entity);

            // Act
            var result = await _controller.GetByID(1, 2);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(BloqueDTO));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetBloqueByIdsAsync(2, 1))
                        .ReturnsAsync((Bloque)null);

            // Act
            var result = await _controller.GetByID(1, 2);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            // Arrange
            var data = new List<Bloque>
            {
                new Bloque { IdBloquant = 1, IdBloque = 2 },
                new Bloque { IdBloquant = 3, IdBloque = 4 }
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
        public async Task Post_ReturnsCreated()
        {
            // Arrange
            var dto = new BloqueDTO { IdBloquant = 1, IdBloque = 2 };
            var entity = new Bloque { IdBloquant = 1, IdBloque = 2 };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Bloque>()))
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

            var dto = new BloqueDTO();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Bloque>()), Times.Never);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var existing = new Bloque { IdBloquant = 1, IdBloque = 2 };
            var dto = new BloqueDTO { IdBloquant = 1, IdBloque = 2 };

            _mockManager.Setup(m => m.GetBloqueByIdsAsync(2, 1))
                        .ReturnsAsync(existing);

            // Act
            var result = await _controller.Put(1, 2, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new BloqueDTO();

            // Act
            var result = await _controller.Put(1, 2, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Bloque>(), It.IsAny<Bloque>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenIdsMismatch()
        {
            // Arrange
            var dto = new BloqueDTO { IdBloquant = 10, IdBloque = 20 };

            // Act
            var result = await _controller.Put(1, 2, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetBloqueByIdsAsync(2, 1))
                        .ReturnsAsync((Bloque)null);

            var dto = new BloqueDTO { IdBloquant = 1, IdBloque = 2 };

            // Act
            var result = await _controller.Put(1, 2, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var existing = new Bloque { IdBloquant = 1, IdBloque = 2 };

            _mockManager.Setup(m => m.GetBloqueByIdsAsync(2, 1))
                        .ReturnsAsync(existing);

            // Act
            var result = await _controller.Delete(1, 2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetBloqueByIdsAsync(2, 1))
                        .ReturnsAsync((Bloque)null);

            // Act
            var result = await _controller.Delete(1, 2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region BLOQUE
        [TestMethod]
        public async Task HasBloque_ReturnsTrue_WhenExists_FirstIsBloquant()
        {
            // Arrange
            _mockManager.Setup(m => m.ExistsAsync(2, 1))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.HasBloque(2, 1, true);

            // Assert
            Assert.AreEqual(true, result.Value);
        }

        [TestMethod]
        public async Task HasBloque_ReturnsTrue_WhenExists_SecondIsBloquant()
        {
            // Arrange
            _mockManager.Setup(m => m.ExistsAsync(1, 2))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.HasBloque(1, 2, true);

            // Assert
            Assert.AreEqual(true, result.Value);
        }

        [TestMethod]
        public async Task HasBloque_ReturnsFalse_WhenNotExist()
        {
            // Arrange
            _mockManager.Setup(m => m.ExistsAsync(It.IsAny<int>(), It.IsAny<int>()))
                        .ReturnsAsync(false);

            // Act
            var result = await _controller.HasBloque(1, 2, true);

            // Assert
            Assert.AreEqual(false, result.Value);
        }
        #endregion
    }
}
