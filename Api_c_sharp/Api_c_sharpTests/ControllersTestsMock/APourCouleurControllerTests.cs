using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass]
    public class APourCouleurControllerTests
    {
        private Mock<APourCouleurManager> _mockManager;
        private IMapper _mapper;
        private APourCouleurController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<APourCouleurManager>(null);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<APourCouleurDTO, APourCouleur>().ReverseMap();
            });

            _mapper = config.CreateMapper();

            _controller = new APourCouleurController(_mockManager.Object, _mapper);
        }

        #region GET
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            var entity = new APourCouleur {IdCouleur = 1, IdVoiture = 2 };
            _mockManager.Setup(m => m.GetAPourCouleursByIDS(2, 1))
                        .ReturnsAsync(entity);

            // Act
            var result = await _controller.GetByIDs(2, 1);

            // Assert
            Assert.IsInstanceOfType(result.Value, typeof(APourCouleurDTO));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAPourCouleursByIDS(1, 2))
                        .ReturnsAsync((APourCouleur)null);

            // Act
            var result = await _controller.GetByIDs(2, 1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }


        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            // Arrange
            var data = new List<APourCouleur>
            {
                new APourCouleur { IdCouleur = 1, IdVoiture = 10 },
                new APourCouleur { IdCouleur = 2, IdVoiture = 20 }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                        .ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            var list = result.Value;

            Assert.AreEqual(2, ((List<APourCouleurDTO>)list).Count);
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            // Arrange
            var dto = new APourCouleurDTO { IdCouleur = 1, IdVoiture = 2 };

            var entity = new APourCouleur { IdCouleur = 1, IdVoiture = 2 };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<APourCouleur>()))
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

            var dto = new APourCouleurDTO();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<APourCouleur>()), Times.Never);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var dto = new APourCouleurDTO { IdCouleur = 1, IdVoiture = 2 };

            var entity = new APourCouleur { IdCouleur = 1, IdVoiture = 2 };

            _mockManager.Setup(m => m.GetAPourCouleursByIDS(2, 1))
                        .ReturnsAsync(entity);

            // Act
            var result = await _controller.Put(2, 1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new APourCouleurDTO();

            // Act
            var result = await _controller.Put(2, 1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<APourCouleur>(), It.IsAny<APourCouleur>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAPourCouleursByIDS(1, 2))
                        .ReturnsAsync((APourCouleur)null);

            var dto = new APourCouleurDTO { IdCouleur = 1, IdVoiture = 2 };

            // Act
            var result = await _controller.Put(2, 1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var entity = new APourCouleur { IdVoiture = 1, IdCouleur = 2 };

            _mockManager.Setup(m => m.GetAPourCouleursByIDS(1, 2))
                        .ReturnsAsync(entity);

            _mockManager.Setup(m => m.DeleteAsync(entity))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1, 2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }


        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAPourCouleursByIDS(1, 2))
                        .ReturnsAsync((APourCouleur)null);

            // Act
            var result = await _controller.Delete(2, 1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion
    }
}
