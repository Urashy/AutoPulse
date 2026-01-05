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
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Api_c_sharp.Models.Repository.Interfaces;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class OffreControllerTests
    {
        private Mock<OffreManager> _mockManager;
        private OffreController _controller;
        private IMapper _mapper;
        private Offre _objetcommun;
        private Mock<INotificationService> _notificationService;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            _mockManager = new Mock<OffreManager>(null);

            // Création de l'adresse de référence
            _objetcommun = new Offre
            {
                IdOffre = 1,
                DateOffre = System.DateTime.Now,
                Valeur = 99,
                IdMessage = 1,
                IdAnnonce = 1
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _notificationService = new Mock<INotificationService>();
            // Injection dans le controller
            _controller = new OffreController(_mockManager.Object, _mapper,_notificationService.Object);
        }
        #region GET
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdOffre))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetByID(_objetcommun.IdOffre);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(OffreDTO));
            Assert.AreEqual(_objetcommun.Valeur, result.Value.Valeur);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Offre)null);

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
            var adressesList = new List<Offre>
            {
                _objetcommun,
                new Offre
                {
                    IdOffre = 2,
                    DateOffre = System.DateTime.Now,
                    Valeur = 150,
                    IdMessage = 2,
                    IdAnnonce = 1
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(adressesList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<OffreDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Valeur == _objetcommun.Valeur));
            Assert.AreEqual(2, result.Value.Count());
        }
        [TestMethod]
        public async Task GetByMessage()
        {
            // Arrange
            var annoncesList = new List<Offre> { _objetcommun };

            _mockManager.Setup(m => m.GetOffresByMessageIdAsync(1))
                       .ReturnsAsync(annoncesList);

            // Act
            var result = await _controller.GetByMessage(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<OffreDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Valeur == _objetcommun.Valeur));
        }

        [TestMethod]
        public async Task NotFoundGetByMessage()
        {
            // Arrange
            _mockManager.Setup(m => m.GetOffresByMessageIdAsync(0))
                       .ReturnsAsync((IEnumerable<Offre>)null);

            // Act
            var result = await _controller.GetByMessage(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task PostAdresseTest()
        {
            // Arrange
            OffreCreateDTO offreDTO = new OffreCreateDTO
            {
                Valeur = 200,
                IdMessage = 1,
                IdAnnonce = 1
            };

            var adresseEntity = _mapper.Map<Offre>(offreDTO);
            adresseEntity.IdOffre = 2;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Offre>()))
                       .ReturnsAsync(adresseEntity)
                       .Verifiable();

            // Act
            var actionResult = await _controller.Post(offreDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdAdresse = (Offre)created.Value;
            Assert.AreEqual(offreDTO.Valeur, createdAdresse.Valeur);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Offre>()), Times.Once);
        }

        [TestMethod]
        public async Task BadRequestPostAdresseTest()
        {
            // Arrange
            OffreCreateDTO offreDTO = new OffreCreateDTO
            {
                Valeur = 0,
                IdMessage = 1,
                IdAnnonce = 1
            };

            // Simuler une erreur de validation du modèle
            _controller.ModelState.AddModelError("Valeur", "La Valeur doit être supérieur à 0");

            // Act
            var actionResult = await _controller.Post(offreDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Offre>()), Times.Never);

        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteAdresseTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdOffre))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.DeleteAsync(_objetcommun))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Delete(_objetcommun.IdOffre);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(_objetcommun), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundDeleteAdresseTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Offre)null);

            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutAdresseTest()
        {
            // Arrange
            Offre existingoffre = new Offre()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 150,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1
            };

            OffreUpdateDTO updatedAdresseDTO = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 180,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1
            };

            var updatedoffre = _mapper.Map<Offre>(updatedAdresseDTO);

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdOffre))
                       .ReturnsAsync(existingoffre);
            _mockManager.Setup(m => m.UpdateAsync(existingoffre, updatedoffre))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, updatedAdresseDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Offre>(), It.IsAny<Offre>()), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundPutAdresseTest()
        {
            // Arrange
            OffreUpdateDTO updatedAdresseDTO = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 180,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1
            };

            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Offre)null);

            // Act
            var result = await _controller.Put(0, updatedAdresseDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutAdresseTest()
        {
            // Arrange
            OffreUpdateDTO updatedAdresseDTO = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 180,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1
            };

            _controller.ModelState.AddModelError("Valeur", "La Valeur doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, updatedAdresseDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Offre>(), It.IsAny<Offre>()), Times.Never);
        }
        #endregion
    }
}