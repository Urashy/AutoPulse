using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers;
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
    public class EtatSignalementControllerTests
    {
        private Mock<EtatSignalementManager> _mockManager;
        private EtatSignalementController _controller;
        private IMapper _mapper;
        private EtatSignalementPlainte _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            // (le mock n'utilisera pas le context réel)
            _mockManager = new Mock<EtatSignalementManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new EtatSignalementPlainte
            {
                IdEtatSignalement = 1,
                LibelleEtatSignalement = "Regardé"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new EtatSignalementController(_mockManager.Object, _mapper);
        }
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdEtatSignalement))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdEtatSignalement);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(EtatSignalementDTO));
            Assert.AreEqual(_objetcommun.LibelleEtatSignalement, result.Value.LibelleEtatSignalement);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((EtatSignalementPlainte)null);

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
            var EtatSignalementList = new List<EtatSignalementPlainte>
            {
                _objetcommun,
                new EtatSignalementPlainte
                {
                    IdEtatSignalement = 2,
                    LibelleEtatSignalement ="En attente"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(EtatSignalementList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<EtatSignalementDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleEtatSignalement == _objetcommun.LibelleEtatSignalement));
            Assert.AreEqual(2, result.Value.Count());
        }
    }
}
