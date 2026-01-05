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
    public class EtatAnnonceControllerTests
    {
        private Mock<EtatAnnonceManager> _mockManager;
        private EtatAnnonceController _controller;
        private IMapper _mapper;
        private EtatAnnonce _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            _mockManager = new Mock<EtatAnnonceManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new EtatAnnonce
            {
                IdEtatAnnonce = 1,
                LibelleEtatAnnonce = "Afficher"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new EtatAnnonceController(_mockManager.Object, _mapper);
        }
        #region GET
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdEtatAnnonce))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdEtatAnnonce);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(EtatAnnonceDTO));
            Assert.AreEqual(_objetcommun.LibelleEtatAnnonce, result.Value.LibelleEtatAnnonce);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((EtatAnnonce)null);

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
            var EtatAnnonceList = new List<EtatAnnonce>
            {
                _objetcommun,
                new EtatAnnonce
                {
                    IdEtatAnnonce = 2,
                    LibelleEtatAnnonce ="vendu"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(EtatAnnonceList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<EtatAnnonceDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleEtatAnnonce == _objetcommun.LibelleEtatAnnonce));
            Assert.AreEqual(2, result.Value.Count());
        }
        #endregion
    }
}
