using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class EtatCompteControllerTests
    {
        private Mock<EtatCompteManager> _mockManager;
        private EtatCompteController _controller;
        private IMapper _mapper;
        private EtatCompte _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            _mockManager = new Mock<EtatCompteManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new EtatCompte
            {
                IdEtatCompte = 1,
                Libelle = "Etat 1"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new EtatCompteController(_mockManager.Object, _mapper);
        }
        #region GET
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdEtatCompte))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdEtatCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(EtatCompteDTO));
            Assert.AreEqual(_objetcommun.Libelle, result.Value.Libelle);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((EtatCompte)null);

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
            var EtatCompteList = new List<EtatCompte>
            {
                _objetcommun,
                new EtatCompte
                {
                    IdEtatCompte = 2,
                    Libelle ="Etat 2"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(EtatCompteList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<EtatCompteDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
            Assert.AreEqual(2, result.Value.Count());
        }
        #endregion
    }
}
