using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Api_c_sharp.Controllers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class VoitureControllerTests
    {
        private Mock<VoitureManager> _mockManager;
        private VoitureController _controller;
        private IMapper _mapper;
        private Voiture _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            _mockManager = new Mock<VoitureManager>(null);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _controller = new VoitureController(_mockManager.Object, _mapper);

            _objetcommun = new Voiture
            {
                IdVoiture = 1,
                IdMarque = 10,
                IdModele = 20,
                Kilometrage = 50000,
                Annee = 2020,
                MiseEnCirculation = DateTime.UtcNow,
            };
        }
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetByID(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(VoitureDetailDTO));
            Assert.AreEqual(1, result.Value.IdVoiture);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Voiture)null);

            // Act
            var result = await _controller.GetByID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange
            var VoitureList = new List<Voiture>
            {
                _objetcommun,
                new Voiture
                {
                    IdVoiture = 2,
                    IdMarque = 30,
                    IdModele = 40,
                    Kilometrage = 100000,
                    Annee = 2018
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(VoitureList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<VoitureDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.IdVoiture == _objetcommun.IdVoiture));
            Assert.AreEqual(2, result.Value.Count());
        }
        [TestMethod]
        public async Task Post_ReturnsCreatedAtAction()
        {
            // Arrange
            var voitureCreateDto = new VoitureCreateDTO
            {
                IdMarque = 10,
                IdModele = 20,
                Kilometrage = 50000,
                Annee = 2020,
                MiseEnCirculation = DateTime.UtcNow,
            };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Voiture>()))
                        .Callback<Voiture>(v => v.IdVoiture = 1)
                        .ReturnsAsync((Voiture v) => v);

            // Act
            var result = await _controller.Post(voitureCreateDto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var created = result.Result as CreatedAtActionResult;

            Assert.IsNotNull(created);

            var returnedDto = created.Value as Voiture;
            Assert.IsNotNull(returnedDto);
            Assert.AreEqual(1, returnedDto.IdVoiture);
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            var voitureCreateDto = new VoitureCreateDTO
            {
                IdMarque = 10,
                IdModele = 20,
                Kilometrage = -50000, // Invalid value
                Annee = 2020,
                MiseEnCirculation = DateTime.UtcNow,
            };
            _controller.ModelState.AddModelError("Kilometrage", "Le kilométrage ne peut pas être négatif.");
            // Act
            var result = await _controller.Post(voitureCreateDto);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Voiture>()), Times.Never);
        }
        [TestMethod]
        public async Task Put_ReturnsNoContent()
        {
            // Arrange
            var dto = new VoitureUpdateDTO
            {
                IdMarque = 10,
                IdModele = 20,
                Kilometrage = 50000,
                Annee = 2020,
                MiseEnCirculation = DateTime.UtcNow,
            };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(_objetcommun);

            _mockManager.Setup(m => m.UpdateAsync(_objetcommun, It.IsAny<Voiture>()))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            var dto = new VoitureUpdateDTO();

            _controller.ModelState.AddModelError("Error", "Invalid");
            // Act
            var result = await _controller.Put(1, dto);
            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Voiture>(), It.IsAny<Voiture>()), Times.Never);
        }
        [TestMethod]
        public async Task Put_ReturnsNotFound()
        {
            // Arrange
            var dto = new VoitureUpdateDTO
            {
                IdMarque = 10,
                IdModele = 20,
                Kilometrage = 50000,
                Annee = 2020,
                MiseEnCirculation = DateTime.UtcNow
            };

            _mockManager.Setup(m => m.GetByIdAsync(999))
                        .ReturnsAsync((Voiture)null);

            // Act
            var result = await _controller.Put(999, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNoContent()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.DeleteAsync(_objetcommun))
                        .Returns(Task.CompletedTask);
            // Act
            var result = await _controller.Delete(1);
            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }
        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenVoitureDoesNotExist()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                        .ReturnsAsync((Voiture)null);
            // Act
            var result = await _controller.Delete(0);
            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
