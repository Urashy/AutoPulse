using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
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
    [TestClass()]
    [TestCategory("unit")]
    public class VueControllerTests
    {
        private Mock<VueManager> _mockManager;
        private VueController _controller;
        private IMapper _mapper;
        private Vue _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock
            _mockManager = new Mock<VueManager>(null);

            // Création de la vue de référence
            _objetcommun = new Vue
            {
                IdCompte = 1,
                IdAnnonce = 1
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new VueController(_mockManager.Object, _mapper);
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetVueByIdsAsync(_objetcommun.IdCompte, _objetcommun.IdAnnonce))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetByIDs(_objetcommun.IdCompte, _objetcommun.IdAnnonce);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(VueDTO));
            Assert.AreEqual(_objetcommun.IdAnnonce, result.Value.IdAnnonce);
            Assert.AreEqual(_objetcommun.IdCompte, result.Value.IdCompte);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetVueByIdsAsync(9999, _objetcommun.IdCompte))
                       .ReturnsAsync((Vue)null);

            // Act
            var result = await _controller.GetByIDs(9999, _objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange
            var vuesList = new List<Vue>
            {
                _objetcommun,
                new Vue
                {
                    IdCompte = 2,
                    IdAnnonce = 2
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(vuesList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            var list = result.Value.ToList();
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<VueDTO>));
            Assert.IsTrue(list.Any());
            Assert.IsTrue(list.Any(x => x.IdAnnonce == _objetcommun.IdAnnonce));
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public async Task PostTest()
        {
            // Arrange
            VueDTO dto = new VueDTO
            {
                IdCompte = 1,
                IdAnnonce = 2
            };

            var vueEntity = _mapper.Map<Vue>(dto);

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Vue>()))
                       .ReturnsAsync(vueEntity)
                       .Verifiable();

            // Act
            var actionResult = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdEntity = (VueDTO)created.Value;
            Assert.AreEqual(dto.IdAnnonce, createdEntity.IdAnnonce);
            Assert.AreEqual(dto.IdCompte, createdEntity.IdCompte);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Vue>()), Times.Once);
        }

        [TestMethod]
        public async Task BadRequestPostTest()
        {
            // Arrange
            VueDTO dto = new VueDTO();
            _controller.ModelState.AddModelError("IdAnnonce", "Required");

            // Act
            var actionResult = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task PutTest()
        {
            // Arrange
            var existingVue = new Vue
            {
                IdCompte = _objetcommun.IdCompte,
                IdAnnonce = _objetcommun.IdAnnonce
            };

            VueDTO dto = new VueDTO
            {
                IdCompte = _objetcommun.IdCompte,
                IdAnnonce = _objetcommun.IdAnnonce
            };

            var updatedVue = _mapper.Map<Vue>(dto);

            _mockManager.Setup(m => m.GetVueByIdsAsync(_objetcommun.IdCompte, _objetcommun.IdAnnonce))
                       .ReturnsAsync(existingVue);
            _mockManager.Setup(m => m.UpdateAsync(existingVue, updatedVue))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_objetcommun.IdCompte, _objetcommun.IdAnnonce, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Vue>(), It.IsAny<Vue>()), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundPutTest()
        {
            // Arrange
            VueDTO dto = new VueDTO
            {
                IdCompte = _objetcommun.IdCompte,
                IdAnnonce = _objetcommun.IdAnnonce
            };

            _mockManager.Setup(m => m.GetVueByIdsAsync(9999, 1))
                       .ReturnsAsync((Vue)null);

            // Act
            var result = await _controller.Put(9999, 1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutTest()
        {
            // Arrange
            VueDTO dto = new VueDTO
            {
                IdCompte = _objetcommun.IdCompte,
            };
            _controller.ModelState.AddModelError("IdAnnonce", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdCompte, _objetcommun.IdAnnonce, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task DeleteTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetVueByIdsAsync(_objetcommun.IdCompte, _objetcommun.IdAnnonce))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.DeleteAsync(_objetcommun))
                       .Returns(Task.FromResult(true))
                       .Verifiable();

            // Act
            var result = await _controller.Delete(_objetcommun.IdCompte, _objetcommun.IdAnnonce);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(_objetcommun), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundDeleteTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetVueByIdsAsync(9999, _objetcommun.IdCompte))
                       .ReturnsAsync((Vue)null);

            // Act
            var result = await _controller.Delete(9999, _objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}