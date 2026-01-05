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
    public class TypeJournalControllerTests
    {
        private Mock<TypeJournalManager> _mockManager;
        private TypeJournalController _controller;
        private IMapper _mapper;
        private TypeJournal _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            _mockManager = new Mock<TypeJournalManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new TypeJournal
            {
                IdTypeJournaux = 1,
                LibelleTypeJournaux = "Connexion"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new TypeJournalController(_mockManager.Object, _mapper);
        }
        #region GET
            #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdTypeJournaux))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(_objetcommun.IdTypeJournaux);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(TypeJournalDTO));
            Assert.AreEqual(_objetcommun.LibelleTypeJournaux, result.Value.LibelleTypeJournaux);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((TypeJournal)null);

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
            var TypeJournalList = new List<TypeJournal>
            {
                _objetcommun,
                new TypeJournal
                {
                    IdTypeJournaux = 2,
                    LibelleTypeJournaux = "Déconnexion"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(TypeJournalList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<TypeJournalDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleTypeJournaux == _objetcommun.LibelleTypeJournaux));
            Assert.AreEqual(2, result.Value.Count());
        }
        #endregion
        #endregion
    }
}
