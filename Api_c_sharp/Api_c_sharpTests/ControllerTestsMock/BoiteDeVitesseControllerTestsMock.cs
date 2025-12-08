using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers;
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
    public class BoiteDeVitesseControllerTestsMock
    {
        private Mock<BoiteDeVitesseManager> _mockManager;
        private BoiteDeVitesseController _controller;
        private IMapper _mapper;
        private BoiteDeVitesse _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            // (le mock n'utilisera pas le context réel)
            _mockManager = new Mock<BoiteDeVitesseManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new BoiteDeVitesse
            {
                IdBoiteDeVitesse = 1,
                LibelleBoite ="Auto"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new BoiteDeVitesseController(_mockManager.Object, _mapper);
        }
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdBoiteDeVitesse))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdBoiteDeVitesse);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(BoiteDeVitesseDTO));
            Assert.AreEqual(_objetcommun.LibelleBoite, result.Value.LibelleBoite);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((BoiteDeVitesse)null);

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
            var BoiteDeVitesseList = new List<BoiteDeVitesse>
            {
                _objetcommun,
                new BoiteDeVitesse
                {
                    IdBoiteDeVitesse = 2,
                    LibelleBoite ="Manuel"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(BoiteDeVitesseList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<BoiteDeVitesseDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleBoite == _objetcommun.LibelleBoite));
            Assert.AreEqual(2, result.Value.Count());
        }
    }
}
