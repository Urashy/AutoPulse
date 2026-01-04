using Api_c_sharp.Controllers;
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
    public class CouleurControllerTests
    {
        private Mock<CouleurManager> _mockManager;
        private CouleurController _controller;
        private IMapper _mapper;
        private Couleur _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            _mockManager = new Mock<CouleurManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new Couleur
            {
                IdCouleur = 1,
                LibelleCouleur = "Essence"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new CouleurController(_mockManager.Object, _mapper);
        }

        #region GET
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCouleur))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdCouleur);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CouleurDTO));
            Assert.AreEqual(_objetcommun.LibelleCouleur, result.Value.LibelleCouleur);
        }

        
        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Couleur)null);

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
            var CouleurList = new List<Couleur>
            {
                _objetcommun,
                new Couleur
                {
                    IdCouleur = 2,
                    LibelleCouleur = "Diesel"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(CouleurList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CouleurDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleCouleur == _objetcommun.LibelleCouleur));
            Assert.AreEqual(2, result.Value.Count());
        }
        [TestMethod]
        public async Task GetCouleursByVoitureID_ReturnsList()
        {
            // Arrange
            int voitureId = 5;

            var couleurs = new List<Couleur>
            {
                new Couleur { IdCouleur = 1, LibelleCouleur = "Rouge" },
                new Couleur { IdCouleur = 2, LibelleCouleur = "Bleu" }
            };

            _mockManager.Setup(m => m.GetCouleursByVoitureId(voitureId))
                        .ReturnsAsync(couleurs);

            // Act
            var result = await _controller.GetCouleursByVoitureID(voitureId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CouleurDTO>));
            Assert.AreEqual(2, result.Value.Count());
            Assert.IsTrue(result.Value.Any(c => c.LibelleCouleur == "Rouge"));
        }
        #endregion

    }
}
