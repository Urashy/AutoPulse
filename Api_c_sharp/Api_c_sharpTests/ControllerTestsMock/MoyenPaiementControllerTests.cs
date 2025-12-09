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
    public class MoyenPaiementControllerTestsMock
    {
        private Mock<MoyenPaiementManager> _mockManager;
        private MoyenPaiementController _controller;
        private IMapper _mapper;
        private MoyenPaiement _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            // (le mock n'utilisera pas le context réel)
            _mockManager = new Mock<MoyenPaiementManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new MoyenPaiement
            {
                IdMoyenPaiement = 1,
                TypePaiement = "Carte bancaire"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new MoyenPaiementController(_mockManager.Object, _mapper);
        }
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdMoyenPaiement))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdMoyenPaiement);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(MoyenPaiementDTO));
            Assert.AreEqual(_objetcommun.TypePaiement, result.Value.TypePaiement);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((MoyenPaiement)null);

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
            var MoyenPaiementList = new List<MoyenPaiement>
            {
                _objetcommun,
                new MoyenPaiement
                {
                    IdMoyenPaiement = 2,
                    TypePaiement = "PayPal"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(MoyenPaiementList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<MoyenPaiementDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.TypePaiement == _objetcommun.TypePaiement));
            Assert.AreEqual(2, result.Value.Count());
        }
    }
}
