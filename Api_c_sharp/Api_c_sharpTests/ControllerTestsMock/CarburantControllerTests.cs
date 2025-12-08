using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using App.Controllers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class CarburantControllerTestsMock
    {
        private Mock<CarburantManager> _mockManager;
        private CarburantController _controller;
        private IMapper _mapper;
        private Carburant _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            // (le mock n'utilisera pas le context réel)
            _mockManager = new Mock<CarburantManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new Carburant
            {
                IdCarburant = 1,
                LibelleCarburant = "Essence"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new CarburantController(_mockManager.Object, _mapper);
        }
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCarburant))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdCarburant);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CarburantDTO));
            Assert.AreEqual(_objetcommun.LibelleCarburant, result.Value.LibelleCarburant);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Carburant)null);

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
            var CarburantList = new List<Carburant>
            {
                _objetcommun,
                new Carburant
                {
                    IdCarburant = 2,
                    LibelleCarburant = "Diesel"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(CarburantList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CarburantDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleCarburant == _objetcommun.LibelleCarburant));
            Assert.AreEqual(2, result.Value.Count());
        }
    }
}
