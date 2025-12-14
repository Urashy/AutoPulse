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
    public class MiseEnAvantControllerTests
    {
        private Mock<MiseEnAvantManager> _mockManager;
        private MiseEnAvantController _controller;
        private IMapper _mapper;
        private MiseEnAvant _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            // (le mock n'utilisera pas le context réel)
            _mockManager = new Mock<MiseEnAvantManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new MiseEnAvant
            {
                IdMiseEnAvant = 1,
                LibelleMiseEnAvant = "Standard"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new MiseEnAvantController(_mockManager.Object, _mapper);
        }
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdMiseEnAvant))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdMiseEnAvant);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(MiseEnAvantDTO));
            Assert.AreEqual(_objetcommun.LibelleMiseEnAvant, result.Value.LibelleMiseEnAvant);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((MiseEnAvant)null);

            // Act
            var result = await _controller.GetById(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange
            var MiseEnAvantList = new List<MiseEnAvant>
            {
                _objetcommun,
                new MiseEnAvant
                {
                    IdMiseEnAvant = 2,
                    LibelleMiseEnAvant = "Premium"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(MiseEnAvantList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<MiseEnAvantDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleMiseEnAvant == _objetcommun.LibelleMiseEnAvant));
            Assert.AreEqual(2, result.Value.Count());
        }
    }
}
