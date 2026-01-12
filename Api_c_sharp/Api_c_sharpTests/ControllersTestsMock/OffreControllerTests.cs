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
using Microsoft.AspNetCore.SignalR;
using Api_c_sharp.Hubs;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class OffreControllerTests
    {
        private Mock<OffreManager> _mockManager;
        private Mock<CommandeManager> _mockCommandeManager;
        private Mock<FactureManager> _mockFactureManager;
        private Mock<MessageManager> _mockMessageManager;
        private Mock<AnnonceManager> _mockAnnonceManager;
        private Mock<INotificationService> _notificationService;
        private Mock<IHubContext<MessageHub>> _mockHubContext;
        private OffreController _controller;
        private IMapper _mapper;
        private Offre _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock du manager avec un paramètre null pour le context
            _mockManager = new Mock<OffreManager>(null);
            _mockCommandeManager = new Mock<CommandeManager>(null);
            _mockFactureManager = new Mock<FactureManager>(null);
            _mockAnnonceManager = new Mock<AnnonceManager>(null);
            _mockMessageManager = new Mock<MessageManager>(null);
            _notificationService = new Mock<INotificationService>();
            _mockHubContext = new Mock<IHubContext<MessageHub>>();

            // Création de l'offre de référence
            _objetcommun = new Offre
            {
                IdOffre = 1,
                DateOffre = System.DateTime.Now,
                Valeur = 99,
                IdMessage = 1,
                IdAnnonce = 1,
                EstAccepte = null
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new OffreController(
                _mockManager.Object,
                _mapper,
                _mockMessageManager.Object,
                _mockCommandeManager.Object,
                _mockFactureManager.Object,
                _mockAnnonceManager.Object,
                _notificationService.Object,
                _mockHubContext.Object
            );
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
            var offresList = new List<Offre>
            {
                _objetcommun,
                new Offre
                {
                    IdOffre = 2,
                    DateOffre = System.DateTime.Now,
                    Valeur = 150,
                    IdMessage = 2,
                    IdAnnonce = 1,
                    EstAccepte = null
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(offresList);

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
        public async Task GetByMessageTest()
        {
            // Arrange
            var offresList = new List<Offre> { _objetcommun };

            _mockManager.Setup(m => m.GetOffresByMessageIdAsync(1))
                       .ReturnsAsync(offresList);

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
        public async Task NotFoundGetByMessageTest()
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
        public async Task PostOffreTest()
        {
            // Arrange
            OffreCreateDTO offreDTO = new OffreCreateDTO
            {
                Valeur = 200,
                IdMessage = 1,
                IdAnnonce = 1
            };

            var offreEntity = _mapper.Map<Offre>(offreDTO);
            offreEntity.IdOffre = 2;

            var message = new Message
            {
                IdMessage = 1,
                IdCompte = 1,
                IdConversation = 1,
                ContenuMessage = "Test",
                DateEnvoiMessage = DateTime.Now
            };

            var annonce = new Annonce
            {
                IdAnnonce = 1,
                IdCompte = 2,
                Libelle = "Test annonce",
                Prix = 10000
            };

            // Mock pour vérifier qu'il n'y a pas d'offre en attente dans la conversation
            _mockManager.Setup(m => m.PendingOfferExistsInConversation(It.IsAny<int>()))
                        .ReturnsAsync(false);

            _mockMessageManager.Setup(m => m.GetByIdAsync(offreDTO.IdMessage))
                               .ReturnsAsync(message);

            _mockAnnonceManager.Setup(m => m.GetByIdAsync(offreDTO.IdAnnonce))
                               .ReturnsAsync(annonce);

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Offre>()))
                       .ReturnsAsync(offreEntity)
                       .Verifiable();

            _notificationService.Setup(n => n.NotifOffreAnnonce(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()))
                               .Returns(Task.CompletedTask);

            // Mock SignalR
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            // Act
            var actionResult = await _controller.Post(offreDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdOffre = (Offre)created.Value;
            Assert.AreEqual(offreDTO.Valeur, createdOffre.Valeur);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Offre>()), Times.Once);
            _notificationService.Verify(n => n.NotifOffreAnnonce(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Once);
        }

        [TestMethod]
        public async Task BadRequestPostOffreTest()
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

        [TestMethod]
        public async Task ConflictPostOffreTest_WhenPendingOfferExists()
        {
            // Arrange
            OffreCreateDTO offreDTO = new OffreCreateDTO
            {
                Valeur = 200,
                IdMessage = 1,
                IdAnnonce = 1
            };

            var message = new Message
            {
                IdMessage = 1,
                IdCompte = 1,
                IdConversation = 1,
                ContenuMessage = "Test",
                DateEnvoiMessage = DateTime.Now
            };

            // Mock pour simuler qu'une offre en attente existe déjà
            _mockManager.Setup(m => m.PendingOfferExistsInConversation(It.IsAny<int>()))
                        .ReturnsAsync(true);

            _mockMessageManager.Setup(m => m.GetByIdAsync(offreDTO.IdMessage))
                               .ReturnsAsync(message);

            // Act
            var actionResult = await _controller.Post(offreDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(ConflictObjectResult));
            var conflict = (ConflictObjectResult)actionResult.Result;
            Assert.AreEqual("Une offre en attente existe déjà dans cette conversation.", conflict.Value);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Offre>()), Times.Never);
        }

        [TestMethod]
        public async Task PostOffreTest_WithSameCompteIdForMessageAndAnnonce()
        {
            // Arrange
            OffreCreateDTO offreDTO = new OffreCreateDTO
            {
                Valeur = 200,
                IdMessage = 1,
                IdAnnonce = 1
            };

            var offreEntity = _mapper.Map<Offre>(offreDTO);
            offreEntity.IdOffre = 2;

            // Même IdCompte pour le message et l'annonce
            var message = new Message
            {
                IdMessage = 1,
                IdCompte = 1,
                IdConversation = 1,
                ContenuMessage = "Test",
                DateEnvoiMessage = DateTime.Now
            };

            var annonce = new Annonce
            {
                IdAnnonce = 1,
                IdCompte = 1, // Même compte que le message
                Libelle = "Test annonce",
                Prix = 10000
            };

            _mockManager.Setup(m => m.PendingOfferExistsInConversation(It.IsAny<int>()))
                        .ReturnsAsync(false);

            _mockMessageManager.Setup(m => m.GetByIdAsync(offreDTO.IdMessage))
                               .ReturnsAsync(message);

            _mockAnnonceManager.Setup(m => m.GetByIdAsync(offreDTO.IdAnnonce))
                               .ReturnsAsync(annonce);

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Offre>()))
                       .ReturnsAsync(offreEntity);

            _notificationService.Setup(n => n.NotifOffreAnnonce(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()))
                               .Returns(Task.CompletedTask);

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            // Act
            var actionResult = await _controller.Post(offreDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            // Vérifier que la notification est envoyée avec le bon IdCompte (celui de l'annonce)
            _notificationService.Verify(n => n.NotifOffreAnnonce(1, 1, 200), Times.Once);
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteOffreTest()
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
        public async Task NotFoundDeleteOffreTest()
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
        public async Task PutOffreTest()
        {
            // Arrange
            Offre existingOffre = new Offre()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 150,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1,
                EstAccepte = null
            };

            OffreUpdateDTO updatedOffreDTO = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 180,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1,
                EstAccepte = null  // Reste null, donc pas de notification
            };

            var message = new Message
            {
                IdMessage = 1,
                IdCompte = 1,
                IdConversation = 1,
                ContenuMessage = "Test",
                DateEnvoiMessage = DateTime.Now
            };

            var annonce = new Annonce
            {
                IdAnnonce = 1,
                IdCompte = 2,
                Libelle = "Test annonce",
                Prix = 10000
            };

            var updatedOffre = _mapper.Map<Offre>(updatedOffreDTO);

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdOffre))
                       .ReturnsAsync(existingOffre);

            _mockMessageManager.Setup(m => m.GetByIdAsync(1))
                               .ReturnsAsync(message);

            _mockAnnonceManager.Setup(m => m.GetByIdAsync(1))
                               .ReturnsAsync(annonce);

            _mockManager.Setup(m => m.UpdateAsync(existingOffre, updatedOffre))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Setup pour la notification (ne sera pas appelée car EstAccepte = null)
            _notificationService.Setup(n => n.NotifOfrreAccepterOuRejeter(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, updatedOffreDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Offre>(), It.IsAny<Offre>()), Times.Once);

            // Vérifier que la commande n'est pas créée quand EstAccepte = null
            _mockCommandeManager.Verify(m => m.AddAsync(It.IsAny<Commande>()), Times.Never);

            // Vérifier que la notification n'est pas envoyée quand EstAccepte = null
            _notificationService.Verify(n => n.NotifOfrreAccepterOuRejeter(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public async Task PutOffreTest_WhenAccepted_CreatesCommande()
        {
            // Arrange
            Offre existingOffre = new Offre()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 150,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1,
                EstAccepte = null
            };

            OffreUpdateDTO updatedOffreDTO = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 180,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1,
                EstAccepte = true // Offre acceptée
            };

            var message = new Message
            {
                IdMessage = 1,
                IdCompte = 2, // Acheteur
                IdConversation = 1,
                ContenuMessage = "Test",
                DateEnvoiMessage = DateTime.Now
            };

            var annonce = new Annonce
            {
                IdAnnonce = 1,
                IdCompte = 1, // Vendeur
                Libelle = "Test annonce",
                Prix = 10000
            };

            var updatedOffre = _mapper.Map<Offre>(updatedOffreDTO);

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdOffre))
                       .ReturnsAsync(existingOffre);

            _mockMessageManager.Setup(m => m.GetByIdAsync(1))
                               .ReturnsAsync(message);

            _mockAnnonceManager.Setup(m => m.GetByIdAsync(1))
                               .ReturnsAsync(annonce);

            _mockManager.Setup(m => m.UpdateAsync(existingOffre, updatedOffre))
                       .Returns(Task.CompletedTask);

            _mockCommandeManager.Setup(m => m.AddAsync(It.IsAny<Commande>()))
                                .ReturnsAsync((Commande c) => c)
                                .Verifiable();

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, updatedOffreDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockCommandeManager.Verify(m => m.AddAsync(It.IsAny<Commande>()), Times.Once);
        }

        [TestMethod]
        public async Task PutOffreTest_WhenAccepted_WithSameCompteId()
        {
            // Arrange
            Offre existingOffre = new Offre()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 150,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1,
                EstAccepte = null
            };

            OffreUpdateDTO updatedOffreDTO = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 180,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1,
                EstAccepte = true
            };

            // Même IdCompte pour le message et l'annonce
            var message = new Message
            {
                IdMessage = 1,
                IdCompte = 1,
                IdConversation = 1,
                ContenuMessage = "Test",
                DateEnvoiMessage = DateTime.Now
            };

            var annonce = new Annonce
            {
                IdAnnonce = 1,
                IdCompte = 1, // Même compte
                Libelle = "Test annonce",
                Prix = 10000
            };

            var updatedOffre = _mapper.Map<Offre>(updatedOffreDTO);

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdOffre))
                       .ReturnsAsync(existingOffre);

            _mockMessageManager.Setup(m => m.GetByIdAsync(1))
                               .ReturnsAsync(message);

            _mockAnnonceManager.Setup(m => m.GetByIdAsync(1))
                               .ReturnsAsync(annonce);

            _mockManager.Setup(m => m.UpdateAsync(existingOffre, updatedOffre))
                       .Returns(Task.CompletedTask);

            _mockCommandeManager.Setup(m => m.AddAsync(It.IsAny<Commande>()))
                                .ReturnsAsync((Commande c) => c);

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, updatedOffreDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            // Vérifier que la commande est créée avec le bon IdAcheteur
            _mockCommandeManager.Verify(m => m.AddAsync(It.Is<Commande>(c =>
                c.IdAcheteur == 1 && c.IdVendeur == 1
            )), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundPutOffreTest()
        {
            // Arrange
            OffreUpdateDTO updatedOffreDTO = new OffreUpdateDTO()
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
            var result = await _controller.Put(0, updatedOffreDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutOffreTest()
        {
            // Arrange
            OffreUpdateDTO updatedOffreDTO = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 180,
                DateOffre = _objetcommun.DateOffre,
                IdMessage = 1,
                IdAnnonce = 1
            };

            _controller.ModelState.AddModelError("Valeur", "La Valeur doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, updatedOffreDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Offre>(), It.IsAny<Offre>()), Times.Never);
        }
        #endregion
    }
}