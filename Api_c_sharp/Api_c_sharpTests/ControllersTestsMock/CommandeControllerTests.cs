using Api_c_sharp.Controllers;
using Api_c_sharp.Hubs;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass]
    public class CommandeControllerTests
    {
        private Mock<CommandeManager> _mockManager;
        private IMapper _mapper;
        private Mock<IJournalService> _mockJournal;
        private Mock<AnnonceManager> _mockManagerannonce;
        private Mock<IHubContext<MessageHub>> _mockHubContext;
        private Mock<IHubClients> _mockClients;
        private Mock<IClientProxy> _mockClientProxy;
        private CommandeController _controller;

        [TestInitialize]
        public void Initialize()
        {
            _mockManager = new Mock<CommandeManager>(null);
            _mockManagerannonce = new Mock<AnnonceManager>(null);
            _mockJournal = new Mock<IJournalService>();
            _mockHubContext = new Mock<IHubContext<MessageHub>>();
            _mockClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();

            _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
            _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Commande, CommandeDTO>().ReverseMap();
                cfg.CreateMap<Commande, CommandeDetailDTO>().ReverseMap();
                cfg.CreateMap<CommandeCreateDTO, Commande>().ReverseMap();
                cfg.CreateMap<CommandeUpdateDTO, Commande>().ReverseMap();
            });

            _mapper = config.CreateMapper();
            _controller = new CommandeController(_mockManager.Object, _mapper, _mockJournal.Object, _mockManagerannonce.Object,_mockHubContext.Object);
        }


        #region GET
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            var entity = new Commande { IdCommande = 1, IdAcheteur = 2, IdVendeur = 3, IdAnnonce = 4, IdMoyenPaiement = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            // Act
            var result = await _controller.GetByID(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CommandeDetailDTO));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);

            // Act
            var result = await _controller.GetByID(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            // Arrange
            var data = new List<Commande>
            {
                new Commande { IdCommande = 1 },
                new Commande { IdCommande = 2 }
            };

            _mockManager.Setup(m => m.GetAllAsync()).ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }

        #region GetCommandeByConversationID Tests
        [TestMethod]
        public async Task GetCommandeByConversationID_ReturnsOk_WhenExists()
        {
            // Arrange
            var commande = new Commande
            {
                IdCommande = 5,
                IdAcheteur = 2,
                IdVendeur = 3,
                IdAnnonce = 4,
                IdMoyenPaiement = 1,
                IdEtatCommande = 1,
                Date = DateTime.Now
            };

            _mockManager.Setup(m => m.GetCommandeByConversation(1))
                       .ReturnsAsync(commande)
                       .Verifiable();

            // Act
            var result = await _controller.GetCommandeByConversationID(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CommandeDTO));
            Assert.AreEqual(commande.IdCommande, result.Value.IdCommande);
            Assert.AreEqual(commande.IdEtatCommande, result.Value.IdEtatCommande);
            _mockManager.Verify(m => m.GetCommandeByConversation(1), Times.Once);
        }

        [TestMethod]
        public async Task GetCommandeByConversationID_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetCommandeByConversation(999))
                       .ReturnsAsync((Commande)null)
                       .Verifiable();

            // Act
            var result = await _controller.GetCommandeByConversationID(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockManager.Verify(m => m.GetCommandeByConversation(999), Times.Once);
        }
        #endregion
        #endregion

        #region POST
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            // Arrange
            var dto = new CommandeCreateDTO { IdAcheteur = 1, IdVendeur = 2, IdAnnonce = 3, IdMoyenPaiement = 1 };
            var entity = new Commande { IdCommande = 10, IdAcheteur = 1, IdVendeur = 2 };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Commande>())).ReturnsAsync(entity);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("x", "invalid");
            var dto = new CommandeCreateDTO();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Commande>()), Times.Never);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var entity = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 1,
                IdAnnonce = 1
            };
            var dto = new CommandeUpdateDTO
            {
                IdCommande = 1,
                IdEtatCommande = 2,
                IdAnnonce = 1,
                IdAcheteur = 1,
                IdVendeur = 2
            };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()), Times.Once);
        }

        [TestMethod]
        public async Task Put_UpdatesAnnonce_WhenEtatCommandeIs5()
        {
            // Arrange
            var annonce = new Annonce { IdAnnonce = 1, IdEtatAnnonce = 1 };
            var entity = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 1,
                IdAnnonce = 1
            };
            var dto = new CommandeUpdateDTO
            {
                IdCommande = 1,
                IdEtatCommande = 5,
                IdAnnonce = 1,
                IdAcheteur = 1,
                IdVendeur = 2
            };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockManagerannonce.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(annonce);
            _mockManagerannonce.Setup(m => m.UpdateAsync(It.IsAny<Annonce>(), It.Is<Annonce>(a => a.IdEtatAnnonce == 2)))
                              .Returns(Task.CompletedTask)
                              .Verifiable();
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManagerannonce.Verify(m => m.GetByIdAsync(1), Times.Once);
            _mockManagerannonce.Verify(m => m.UpdateAsync(It.IsAny<Annonce>(), It.Is<Annonce>(a => a.IdEtatAnnonce == 2)), Times.Once);
        }

        [TestMethod]
        public async Task Put_DoesNotUpdateAnnonce_WhenEtatCommandeIsNot5()
        {
            // Arrange
            var entity = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 1,
                IdAnnonce = 1
            };
            var dto = new CommandeUpdateDTO
            {
                IdCommande = 1,
                IdEtatCommande = 3,
                IdAnnonce = 1,
                IdAcheteur = 1,
                IdVendeur = 2
            };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManagerannonce.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManagerannonce.Verify(m => m.UpdateAsync(It.IsAny<Annonce>(), It.IsAny<Annonce>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Invalid Model");
            var dto = new CommandeUpdateDTO { IdCommande = 1 };

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);
            var dto = new CommandeUpdateDTO { IdCommande = 1 };

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_DoesNotSendSignalRNotification_WhenStateDoesNotChange()
        {
            // Arrange
            var mockHubContext = new Mock<IHubContext<MessageHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            var controllerWithHub = new CommandeController(
                _mockManager.Object,
                _mapper,
                _mockJournal.Object,
                _mockManagerannonce.Object,
                mockHubContext.Object
            );

            var entity = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 2,
                IdAnnonce = 1
            };

            var dto = new CommandeUpdateDTO
            {
                IdCommande = 1,
                IdEtatCommande = 2, // Même état
                IdAnnonce = 1,
                IdAcheteur = 10,
                IdVendeur = 20
            };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await controllerWithHub.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            // Vérifie qu'aucune notification n'a été envoyée
            mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "CommandeStateChanged",
                    It.IsAny<object[]>(),
                    default
                ),
                Times.Never
            );
        }

        [TestMethod]
        public async Task Put_WorksCorrectly_WhenHubContextIsNull()
        {
            // Arrange
            var controllerWithoutHub = new CommandeController(
                _mockManager.Object,
                _mapper,
                _mockJournal.Object,
                _mockManagerannonce.Object,
                null // HubContext null
            );

            var entity = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 1,
                IdAnnonce = 1
            };

            var dto = new CommandeUpdateDTO
            {
                IdCommande = 1,
                IdEtatCommande = 3,
                IdAnnonce = 1,
                IdAcheteur = 10,
                IdVendeur = 20
            };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await controllerWithoutHub.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            // Pas d'exception levée malgré HubContext null
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var entity = new Commande { IdCommande = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region GETByCommande
        [TestMethod]
        public async Task GetCommandeByCompteID_ReturnsList_WhenExists()
        {
            // Arrange
            var data = new List<Commande> { new Commande { IdCommande = 1 } };

            _mockManager.Setup(m => m.GetCommandesByCompteId(1)).ReturnsAsync(data);

            // Act
            var result = await _controller.GetCommandeByCompteID(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(1, result.Value.Count());
        }

        [TestMethod]
        public async Task GetCommandeByCompteID_ReturnsNotFound_WhenEmpty()
        {
            // Arrange
            _mockManager.Setup(m => m.GetCommandesByCompteId(1)).ReturnsAsync(new List<Commande>());

            // Act
            var result = await _controller.GetCommandeByCompteID(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region SignalR Notification Tests

        [TestMethod]
        public async Task Put_SendsSignalRNotification_WhenStateChanges()
        {
            // Arrange
            var etatCommande = new EtatCommande
            {
                IdEtatCommande = 3,
                Libelle = "En cours de livraison"
            };

            var entity = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 1,
                IdAnnonce = 1,
                EtatCommandeCommandeNav = new EtatCommande
                {
                    IdEtatCommande = 1,
                    Libelle = "En attente"
                }
            };

            var commandeAfterUpdate = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 3,
                IdAnnonce = 1,
                EtatCommandeCommandeNav = etatCommande
            };

            var dto = new CommandeUpdateDTO
            {
                IdCommande = 1,
                IdEtatCommande = 3, // Changement d'état
                IdAnnonce = 1,
                IdAcheteur = 10,
                IdVendeur = 20
            };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                       .ReturnsAsync(entity);

            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()))
                       .Returns(Task.CompletedTask);

            // Simuler le retour après update avec le nouvel état
            _mockManager.SetupSequence(m => m.GetByIdAsync(1))
                       .ReturnsAsync(entity)
                       .ReturnsAsync(commandeAfterUpdate);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            // Vérifier que la notification SignalR a été envoyée au groupe de la commande
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "CommandeStateChanged",
                    It.Is<object[]>(args =>
                        args.Length == 1 &&
                        args[0] != null
                    ),
                    default
                ),
                Times.Once,
                "La notification SignalR devrait être envoyée une fois"
            );

            // Vérifier que le bon groupe a été ciblé (commande_{id})
            _mockClients.Verify(
                c => c.Group("commande_1"),
                Times.Once,
                "Le groupe de la commande devrait être ciblé"
            );
        }

        [TestMethod]
        public async Task Put_UpdatesAnnonceAndSendsNotification_WhenEtatCommandeIs5()
        {
            // Arrange
            var annonce = new Annonce { IdAnnonce = 1, IdEtatAnnonce = 1 };

            var etatCommande = new EtatCommande
            {
                IdEtatCommande = 5,
                Libelle = "Livrée"
            };

            var entity = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 1,
                IdAnnonce = 1,
                EtatCommandeCommandeNav = new EtatCommande
                {
                    IdEtatCommande = 1,
                    Libelle = "En attente"
                }
            };

            var commandeAfterUpdate = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 5,
                IdAnnonce = 1,
                EtatCommandeCommandeNav = etatCommande
            };

            var dto = new CommandeUpdateDTO
            {
                IdCommande = 1,
                IdEtatCommande = 5, // État "Livrée"
                IdAnnonce = 1,
                IdAcheteur = 1,
                IdVendeur = 2
            };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                       .ReturnsAsync(entity);

            _mockManagerannonce.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(annonce);
            _mockManagerannonce.Setup(m => m.UpdateAsync(It.IsAny<Annonce>(), It.Is<Annonce>(a => a.IdEtatAnnonce == 2)))
                              .Returns(Task.CompletedTask);

            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()))
                       .Returns(Task.CompletedTask);

            _mockManager.SetupSequence(m => m.GetByIdAsync(1))
                       .ReturnsAsync(entity)
                       .ReturnsAsync(commandeAfterUpdate);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            // Vérifier que l'annonce a été mise à jour
            _mockManagerannonce.Verify(m => m.GetByIdAsync(1), Times.Once);
            _mockManagerannonce.Verify(m => m.UpdateAsync(It.IsAny<Annonce>(), It.Is<Annonce>(a => a.IdEtatAnnonce == 2)), Times.Once);

            // Vérifier que la notification a été envoyée au groupe de la commande
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "CommandeStateChanged",
                    It.IsAny<object[]>(),
                    default
                ),
                Times.Once,
                "La notification devrait être envoyée une fois même quand l'annonce est mise à jour"
            );
        }

        #endregion
    }
}