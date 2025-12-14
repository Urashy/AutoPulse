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
    public class ModeleControllerTestsMoq
    {
        private Mock<ModeleManager> _mockManager;
        private ModeleController _controller;
        private IMapper _mapper;
        private Modele _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock
            _mockManager = new Mock<ModeleManager>(null);

            // Création du modèle de référence
            _objetcommun = new Modele
            {
                IdModele = 1,
                LibelleModele = "ModTest",
                IdMarque = 1
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new ModeleController(_mockManager.Object, _mapper);
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdMarque))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.Get(_objetcommun.IdMarque);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(ModeleDTO));
            Assert.AreEqual(_objetcommun.LibelleModele, result.Value.LibelleModele);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Modele)null);

            // Act
            var result = await _controller.Get(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange
            var modelesList = new List<Modele>
            {
                _objetcommun,
                new Modele
                {
                    IdModele = 2,
                    LibelleModele = "ModTest2",
                    IdMarque = 1
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(modelesList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<ModeleDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleModele == _objetcommun.LibelleModele));
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task GetModelesByMarqueIdTest()
        {
            // Arrange
            var modelesList = new List<Modele> { _objetcommun };

            _mockManager.Setup(m => m.GetModelesByMarqueIdAsync(_objetcommun.IdMarque))
                       .ReturnsAsync(modelesList);

            // Act
            var result = await _controller.GetAllByMarque(_objetcommun.IdMarque);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<ModeleDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleModele == _objetcommun.LibelleModele));
        }

        [TestMethod]
        public async Task NotFoundGetModelesByMarqueIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetModelesByMarqueIdAsync(0))
                       .ReturnsAsync((IEnumerable<Modele>)null);

            // Act
            var result = await _controller.GetAllByMarque(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}