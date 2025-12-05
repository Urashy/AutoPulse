using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class AdresseControllerTestsMoq
    {
        private Mock<AdresseManager> _mockManager;
        private AdresseController _controller;
        private IMapper _mapper;
        private Adresse _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager
            _mockManager = new Mock<AdresseManager>();

            // Création de l'adresse de référence
            _objetcommun = new Adresse
            {
                IdAdresse = 1,
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = 1,
                IdCompte = 1
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new AdresseController(_mockManager.Object, _mapper);
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdAdresse))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetByID(_objetcommun.IdAdresse);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(AdresseDTO));
            Assert.AreEqual(_objetcommun.Rue, result.Value.Rue);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Adresse)null);

            // Act
            var result = await _controller.GetByID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange
            var adressesList = new List<Adresse>
            {
                _objetcommun,
                new Adresse
                {
                    IdAdresse = 2,
                    Nom = "Travail",
                    LibelleVille = "Paris",
                    CodePostal = "75000",
                    Rue = "Avenue des Champs",
                    Numero = 10,
                    IdPays = 1,
                    IdCompte = 1
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(adressesList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AdresseDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Rue == _objetcommun.Rue));
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task PostAdresseTest()
        {
            // Arrange
            var adresseDTO = new AdresseDTO()
            {
                Nom = "Travail",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 15,
                IdPays = 1,
                IdCompte = 1
            };

            var adresseEntity = _mapper.Map<Adresse>(adresseDTO);
            adresseEntity.IdAdresse = 2;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Adresse>()))
                                .ReturnsAsync(adresseEntity)
                                .Verifiable();

            // Act
            var actionResult = await _controller.Post(adresseDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdAdresse = (Adresse)created.Value;
            Assert.AreEqual(adresseDTO.Rue, createdAdresse.Rue);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Adresse>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAdresseTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdAdresse))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.DeleteAsync(_objetcommun))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Delete(_objetcommun.IdAdresse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(_objetcommun), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundDeleteAdresseTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Adresse)null);

            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutAdresseTest()
        {
            // Arrange
            var existingAdresse = new Adresse
            {
                IdAdresse = _objetcommun.IdAdresse,
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = 1,
                IdCompte = 1
            };

            var updatedAdresseDTO = new AdresseDTO()
            {
                IdAdresse = _objetcommun.IdAdresse,
                Nom = "Domicile",
                LibelleVille = "Chavanod",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = 1,
                IdCompte = 1,
            };

            var updatedAdresse = _mapper.Map<Adresse>(updatedAdresseDTO);

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdAdresse))
                       .ReturnsAsync(existingAdresse);
            _mockManager.Setup(m => m.UpdateAsync(existingAdresse, updatedAdresse))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_objetcommun.IdAdresse, updatedAdresseDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Adresse>(), It.IsAny<Adresse>()), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundPutAdresseTest()
        {
            // Arrange
            var adresseDTO = new AdresseDTO()
            {
                IdAdresse = 0,
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = 1,
                IdCompte = 1,
            };

            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Adresse)null);

            // Act
            var result = await _controller.Put(0, adresseDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutAdresseTest()
        {
            // Arrange
            var adresseDTO = new AdresseDTO()
            {
                IdAdresse = _objetcommun.IdAdresse,
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = -12,
                IdPays = 1,
                IdCompte = 1,
            };

            _controller.ModelState.AddModelError("Numero", "Le Numero doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdAdresse, adresseDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task BadRequestPostAdresseTest()
        {
            // Arrange
            var adresseDTO = new AdresseDTO
            {
                Nom = null,
            };

            _controller.ModelState.AddModelError("Nom", "Required");

            // Act
            var actionResult = await _controller.Post(adresseDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task GetAdresseByCompteIDTest()
        {
            // Arrange
            var adressesList = new List<Adresse>
            {
                _objetcommun,
                new Adresse
                {
                    IdAdresse = 2,
                    Nom = "Secondaire",
                    LibelleVille = "Lyon",
                    CodePostal = "69000",
                    Rue = "Rue de la République",
                    Numero = 5,
                    IdPays = 1,
                    IdCompte = 1
                }
            };

            _mockManager.Setup(m => m.GetAdresseByCompteID(_objetcommun.IdCompte))
                       .ReturnsAsync(adressesList);

            // Act
            var result = await _controller.GetAdressesByCompteID(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AdresseDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Rue == _objetcommun.Rue));
        }

        [TestMethod]
        public async Task NotFoundGetAdresseByCompteIDTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAdresseByCompteID(0))
                       .ReturnsAsync(new List<Adresse>());

            // Act
            var result = await _controller.GetAdressesByCompteID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AdresseDTO>));
            Assert.IsFalse(result.Value.Any());
        }
    }
}