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
    public class TypeSignalementControllerTests
    {
        private Mock<TypeSignalementManager> _mockManager;
        private TypeSignalementController _controller;
        private IMapper _mapper;
        private TypeSignalement _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            _mockManager = new Mock<TypeSignalementManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new TypeSignalement
            {
                IdTypeSignalement = 1,
                LibelleTypeSignalement = "Annoce frauduleuse"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new TypeSignalementController(_mockManager.Object, _mapper);
        }
        #region GET
            #region GetById 
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdTypeSignalement))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdTypeSignalement);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(TypeSignalementDTO));
            Assert.AreEqual(_objetcommun.LibelleTypeSignalement, result.Value.LibelleTypeSignalement);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((TypeSignalement)null);

            // Act
            var result = await _controller.GetById(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

            #region GetAll
        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange
            var TypeSignalementList = new List<TypeSignalement>
            {
                _objetcommun,
                new TypeSignalement
                {
                    IdTypeSignalement = 2,
                    LibelleTypeSignalement = "Photo trompeuse"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(TypeSignalementList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<TypeSignalementDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleTypeSignalement == _objetcommun.LibelleTypeSignalement));
            Assert.AreEqual(2, result.Value.Count());
        }
        #endregion
        #endregion
    }
}
