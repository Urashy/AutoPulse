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
    public class TypeCompteControllerTestsMock
    {
        private Mock<TypeCompteManager> _mockManager;
        private TypeCompteController _controller;
        private IMapper _mapper;
        private TypeCompte _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            // (le mock n'utilisera pas le context réel)
            _mockManager = new Mock<TypeCompteManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new TypeCompte
            {
                IdTypeCompte = 1,
                Libelle = "Particulier"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new TypeCompteController(_mockManager.Object, _mapper);
        }
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdTypeCompte))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdTypeCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(TypeCompteDTO));
            Assert.AreEqual(_objetcommun.Libelle, result.Value.Libelle);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((TypeCompte)null);

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
            var TypeCompteList = new List<TypeCompte>
            {
                _objetcommun,
                new TypeCompte
                {
                    IdTypeCompte = 2,
                    Libelle = "Professionel"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(TypeCompteList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<TypeCompteDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
            Assert.AreEqual(2, result.Value.Count());
        }
        // -----------------------------
        // GetTypeComptesPourChercher
        // -----------------------------
        [TestMethod]
        public async Task GetTypeComptesPourChercherTest()
        {
            var list = new List<TypeCompte>
            {
                _objetcommun,
                new TypeCompte { IdTypeCompte = 2, Libelle = "Professionel" }
            };

            _mockManager.Setup(m => m.GetTypeComptesPourChercher())
                       .ReturnsAsync(list);

            var result = await _controller.GetTypeComptesPourChercher();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }

        // -----------------------------
        // GetTypeCompteByCompteId
        // -----------------------------
        [TestMethod]
        public async Task GetTypeCompteByCompteIdTest()
        {
            _mockManager.Setup(m => m.GetTypeCompteByCompteId(10))
                       .ReturnsAsync(_objetcommun);

            var result = await _controller.GetTypeCompteByCompteId(10);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(_objetcommun.IdTypeCompte, result.Value.IdTypeCompte);
        }

        [TestMethod]
        public async Task NotFoundGetTypeCompteByCompteIdTest()
        {
            _mockManager.Setup(m => m.GetTypeCompteByCompteId(999))
                       .ReturnsAsync((TypeCompte)null);

            var result = await _controller.GetTypeCompteByCompteId(999);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}
