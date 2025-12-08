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
    public class CategorieControllerTestsMock
    {
        private Mock<CategorieManager> _mockManager;
        private CategorieController _controller;
        private IMapper _mapper;
        private Categorie _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            // (le mock n'utilisera pas le context réel)
            _mockManager = new Mock<CategorieManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new Categorie
            {
                IdCategorie = 1,
                LibelleCategorie = "Essence"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new CategorieController(_mockManager.Object, _mapper);
        }
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCategorie))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdCategorie);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CategorieDTO));
            Assert.AreEqual(_objetcommun.LibelleCategorie, result.Value.LibelleCategorie);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Categorie)null);

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
            var CategorieList = new List<Categorie>
            {
                _objetcommun,
                new Categorie
                {
                    IdCategorie = 2,
                    LibelleCategorie = "Diesel"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(CategorieList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CategorieDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleCategorie == _objetcommun.LibelleCategorie));
            Assert.AreEqual(2, result.Value.Count());
        }
    }
}
