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
    public class MotriciteControllerTestsMock
    {
        private Mock<MotriciteManager> _mockManager;
        private MotriciteController _controller;
        private IMapper _mapper;
        private Motricite _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            // (le mock n'utilisera pas le context réel)
            _mockManager = new Mock<MotriciteManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new Motricite
            {
                IdMotricite = 1,
                LibelleMotricite = "4x4"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new MotriciteController(_mockManager.Object, _mapper);
        }
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdMotricite))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdMotricite);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(MotriciteDTO));
            Assert.AreEqual(_objetcommun.LibelleMotricite, result.Value.LibelleMotricite);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Motricite)null);

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
            var MotriciteList = new List<Motricite>
            {
                _objetcommun,
                new Motricite
                {
                    IdMotricite = 2,
                    LibelleMotricite = "Traction"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(MotriciteList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<MotriciteDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleMotricite == _objetcommun.LibelleMotricite));
            Assert.AreEqual(2, result.Value.Count());
        }
    }
}
