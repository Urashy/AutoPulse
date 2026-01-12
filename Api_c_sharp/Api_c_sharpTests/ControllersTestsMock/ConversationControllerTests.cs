using Api_c_sharp.Controllers;
using Api_c_sharp.Hubs;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class ConversationControllerTests
    {
        private Mock<ConversationManager> _mockManager;
        private Mock<IConversationEnrichmentService> _mockEnrichmentService;
        private ConversationController _controller;
        private IMapper _mapper;
        private Conversation _objetcommun;
        private Mock<IHubContext<MessageHub>> _mockHubContext;
        private Mock<IHubClients> _mockHubClients;
        private Mock<IClientProxy> _mockClientProxy;

        [TestInitialize]
        public void Initialize()
        {
            // Création des mocks existants
            _mockManager = new Mock<ConversationManager>(null);
            _mockEnrichmentService = new Mock<IConversationEnrichmentService>();

            // Création des mocks pour SignalR Hub
            _mockHubContext = new Mock<IHubContext<MessageHub>>();
            _mockHubClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();

            // Configuration du mock HubContext
            _mockHubContext.Setup(h => h.Clients).Returns(_mockHubClients.Object);
            _mockHubClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
            _mockClientProxy.Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Création de la conversation de référence
            _objetcommun = new Conversation
            {
                IdConversation = 1,
                DateDernierMessage = DateTime.Now,
                IdAnnonce = 1
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller avec HubContext
            _controller = new ConversationController(
                _mockManager.Object,
                _mockEnrichmentService.Object,
                _mapper,
                _mockHubContext.Object);
        }

        #region GET
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            var conversationDetailDTO = new ConversationDetailDTO
            {
                IdConversation = _objetcommun.IdConversation,
                DateDernierMessage = _objetcommun.DateDernierMessage,
                IdAnnonce = _objetcommun.IdAnnonce
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdConversation))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetByID(_objetcommun.IdConversation);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(ConversationDetailDTO));
            Assert.AreEqual(_objetcommun.IdConversation, result.Value.IdConversation);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Conversation)null);

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
            var conversationsList = new List<Conversation>
            {
                _objetcommun,
                new Conversation
                {
                    IdConversation = 2,
                    DateDernierMessage = DateTime.Now.AddDays(-1),
                    IdAnnonce = 2
                }
            };

            var conversationListDTOs = new List<ConversationListDTO>
            {
                new ConversationListDTO
                {
                    IdConversation = _objetcommun.IdConversation,
                    DateDernierMessage = _objetcommun.DateDernierMessage,
                    IdAnnonce = _objetcommun.IdAnnonce
                },
                new ConversationListDTO
                {
                    IdConversation = 2,
                    DateDernierMessage = DateTime.Now.AddDays(-1),
                    IdAnnonce = 2
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(conversationsList);
            _mockEnrichmentService.Setup(s => s.EnrichConversationsAsync(conversationsList, It.IsAny<int>()))
                                 .ReturnsAsync(conversationListDTOs);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<ConversationListDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.IdConversation == _objetcommun.IdConversation));
            Assert.AreEqual(2, result.Value.Count());
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task PostConversationTest_Entity()
        {
            // Arrange
            var conversationDTO = new ConversationCreateDTO
            {
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = _objetcommun.DateDernierMessage,
            };

            var conversationEntity = _mapper.Map<Conversation>(conversationDTO);
            conversationEntity.IdConversation = 2;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Conversation>()))
                       .ReturnsAsync(conversationEntity)
                       .Verifiable();

            // Act
            var actionResult = await _controller.Post(conversationDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdConversation = (Conversation)created.Value;
            Assert.AreEqual(conversationDTO.DateDernierMessage.ToUniversalTime(), createdConversation.DateDernierMessage);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Conversation>()), Times.Once);
        }

        [TestMethod]
        public async Task BadRequestPostConversationTest()
        {
            // Arrange
            ConversationCreateDTO conversationDTO = new ConversationCreateDTO()
            {
                IdAnnonce = -1,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            _controller.ModelState.AddModelError("IdAnnonce", "Le IdAnnonce doit être supérieur à 0");

            // Act
            var actionResult = await _controller.Post(conversationDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Conversation>()), Times.Never);
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteConversationTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdConversation))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.DeleteAsync(_objetcommun))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Delete(_objetcommun.IdConversation);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(_objetcommun), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundDeleteConversationTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Conversation)null);

            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutConversationTest()
        {
            // Arrange
            var existingConversation = new Conversation
            {
                IdConversation = _objetcommun.IdConversation,
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = _objetcommun.DateDernierMessage,
            };

            ConversationUpdateDTO conversationDTO = new ConversationUpdateDTO()
            {
                IdConversation = _objetcommun.IdConversation,
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            var updatedConversation = _mapper.Map<Conversation>(conversationDTO);

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdConversation))
                       .ReturnsAsync(existingConversation);
            _mockManager.Setup(m => m.UpdateAsync(existingConversation, updatedConversation))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_objetcommun.IdConversation, conversationDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Conversation>(), It.IsAny<Conversation>()), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundPutConversationTest()
        {
            // Arrange
            ConversationUpdateDTO conversationDTO = new ConversationUpdateDTO()
            {
                IdConversation = 0,
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Conversation)null);

            // Act
            var result = await _controller.Put(0, conversationDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutConversationTest()
        {
            // Arrange
            ConversationUpdateDTO conversationDTO = new ConversationUpdateDTO()
            {
                IdConversation = _objetcommun.IdConversation,
                IdAnnonce = -1,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            _controller.ModelState.AddModelError("IdAnnonce", "Le IdAnnonce doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdConversation, conversationDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Conversation>(), It.IsAny<Conversation>()), Times.Never);
        }
        #endregion

        #region GETConversation
        [TestMethod]
        public async Task GetConversationByCompteIDTest()
        {
            // Arrange
            var conversationsList = new List<Conversation>
            {
                _objetcommun,
                new Conversation
                {
                    IdConversation = 2,
                    DateDernierMessage = DateTime.Now.AddDays(-2),
                    IdAnnonce = 2
                }
            };

            var conversationListDTOs = new List<ConversationListDTO>
            {
                new ConversationListDTO
                {
                    IdConversation = _objetcommun.IdConversation,
                    DateDernierMessage = _objetcommun.DateDernierMessage,
                    IdAnnonce = _objetcommun.IdAnnonce
                },
                new ConversationListDTO
                {
                    IdConversation = 2,
                    DateDernierMessage = DateTime.Now.AddDays(-2),
                    IdAnnonce = 2
                }
            };

            _mockManager.Setup(m => m.GetConversationsByCompteID(1,0))
                       .ReturnsAsync(conversationsList);
            _mockEnrichmentService.Setup(s => s.EnrichConversationsAsync(conversationsList, 1))
                                 .ReturnsAsync(conversationListDTOs);

            // Act
            var result = await _controller.GetConversationsByCompteID(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<ConversationListDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.IdConversation == _objetcommun.IdConversation));
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task NotFoundGetConversationByCompteIDTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetConversationsByCompteID(0,0))
                       .ReturnsAsync((IEnumerable<Conversation>)null);

            // Act
            var result = await _controller.GetConversationsByCompteID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task PostCompletTest_Success()
        {
            // Arrange
            var conversationDto = new ConversationCreateDTO
            {
                IdAnnonce = 1,
                DateDernierMessage = DateTime.Now,
                message = "Bonjour, je suis intéressé par votre annonce"
            };

            var createdConversation = new Conversation
            {
                IdConversation = 3,
                IdAnnonce = conversationDto.IdAnnonce,
                DateDernierMessage = conversationDto.DateDernierMessage
            };

            int idCompteEnvoi = 2;
            int idCompteRecoi = 1;

            _mockManager.Setup(m => m.PostComplet(
                    It.IsAny<Conversation>(),
                    conversationDto.message,
                    idCompteEnvoi,
                    idCompteRecoi))
                .ReturnsAsync(createdConversation)
                .Callback<Conversation, string, int, int>((conv, msg, idEnvoi, idRecoi) =>
                {
                    // Simuler l'attribution de l'ID
                    conv.IdConversation = createdConversation.IdConversation;
                })
                .Verifiable();

            // Act
            var actionResult = await _controller.PostComplet(idCompteEnvoi, idCompteRecoi,conversationDto);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            Assert.IsNotNull(created.Value);

            var returnedConversation = (Conversation)created.Value;
            Assert.AreEqual(createdConversation.IdConversation, returnedConversation.IdConversation);
            Assert.AreEqual(conversationDto.IdAnnonce, returnedConversation.IdAnnonce);

            // Vérifier que PostComplet a été appelé avec les bons paramètres
            _mockManager.Verify(m => m.PostComplet(
                It.Is<Conversation>(c => c.IdAnnonce == conversationDto.IdAnnonce),
                conversationDto.message,
                idCompteEnvoi,
                idCompteRecoi),
                Times.Once);
        }

        [TestMethod]
        public async Task BadRequestPostCompletTest()
        {
            // Arrange
            var conversationDto = new ConversationCreateDTO
            {
                IdAnnonce = -1,
                DateDernierMessage = DateTime.Now,
                message = "Message test"
            };

            _controller.ModelState.AddModelError("IdAnnonce", "Le IdAnnonce doit être supérieur à 0");

            // Act
            var actionResult = await _controller.PostComplet( 1, 2,conversationDto);
            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));

            // Vérifier que PostComplet n'a jamais été appelé car le ModelState est invalide
            _mockManager.Verify(m => m.PostComplet(
                It.IsAny<Conversation>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()),
                Times.Never);
        }

        [TestMethod]
        public async Task PostCompletTest_WithHubContext_SendsNotifications()
        {
            // Arrange
            var conversationDto = new ConversationCreateDTO
            {
                IdAnnonce = 1,
                DateDernierMessage = DateTime.Now,
                message = "Test message pour notification"
            };

            var createdConversation = new Conversation
            {
                IdConversation = 5,
                IdAnnonce = conversationDto.IdAnnonce,
                DateDernierMessage = conversationDto.DateDernierMessage
            };

            int idCompteEnvoi = 2;
            int idCompteRecoi = 1;

            _mockManager.Setup(m => m.PostComplet(
                    It.IsAny<Conversation>(),
                    conversationDto.message,
                    idCompteEnvoi,
                    idCompteRecoi))
                .ReturnsAsync(createdConversation)
                .Callback<Conversation, string, int, int>((conv, msg, idEnvoi, idRecoi) =>
                {
                    conv.IdConversation = createdConversation.IdConversation;
                });

            // Act
            var actionResult = await _controller.PostComplet( idCompteEnvoi, idCompteRecoi,conversationDto);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            // Vérifier que le Hub a été utilisé pour envoyer le message au groupe de conversation
            _mockHubClients.Verify(
                c => c.Group($"conversation_{createdConversation.IdConversation}"),
                Times.Once,
                "Le groupe de conversation devrait être ciblé");

            // Vérifier que SendCoreAsync a été appelé (c'est la méthode sous-jacente de SendAsync)
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "ReceiveMessage",
                    It.Is<object[]>(args =>
                        args.Length == 4 &&
                        (int)args[0] == createdConversation.IdConversation &&
                        (int)args[1] == idCompteRecoi &&
                        (string)args[2] == conversationDto.message),
                    default(CancellationToken)),
                Times.Once,
                "Le message ReceiveMessage devrait être envoyé avec les bons paramètres");
        }

        [TestMethod]
        public async Task PostCompletTest_ExistingConversation_AddsMessageOnly()
        {
            // Arrange
            var conversationDto = new ConversationCreateDTO
            {
                IdAnnonce = 1,
                DateDernierMessage = DateTime.Now,
                message = "Nouveau message dans conversation existante"
            };

            // Simuler une conversation existante
            var existingConversation = new Conversation
            {
                IdConversation = 10,
                IdAnnonce = conversationDto.IdAnnonce,
                DateDernierMessage = DateTime.Now.AddHours(-1)
            };

            int idCompteEnvoi = 2;
            int idCompteRecoi = 1;

            // Configurer le mock pour retourner la conversation existante
            _mockManager.Setup(m => m.PostComplet(
                    It.IsAny<Conversation>(),
                    conversationDto.message,
                    idCompteEnvoi,
                    idCompteRecoi))
                .ReturnsAsync(existingConversation)
                .Callback<Conversation, string, int, int>((conv, msg, idEnvoi, idRecoi) =>
                {
                    // Simuler la mise à jour de DateDernierMessage
                    existingConversation.DateDernierMessage = DateTime.UtcNow;
                })
                .Verifiable();

            // Act
            var actionResult = await _controller.PostComplet(idCompteEnvoi, idCompteRecoi, conversationDto);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            Assert.IsNotNull(created.Value);

            var returnedConversation = (Conversation)created.Value;

            // Vérifier que la conversation existante est retournée (même ID)
            Assert.AreEqual(existingConversation.IdConversation, returnedConversation.IdConversation);
            Assert.AreEqual(conversationDto.IdAnnonce, returnedConversation.IdAnnonce);

            // Vérifier que PostComplet a été appelé avec les bons paramètres
            _mockManager.Verify(m => m.PostComplet(
                It.Is<Conversation>(c => c.IdAnnonce == conversationDto.IdAnnonce),
                conversationDto.message,
                idCompteEnvoi,
                idCompteRecoi),
                Times.Once);

            // Vérifier que le HubContext a été utilisé pour la notification
            _mockHubClients.Verify(
                c => c.Group($"conversation_{existingConversation.IdConversation}"),
                Times.Once,
                "Le groupe de conversation devrait être ciblé même pour une conversation existante");

            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "ReceiveMessage",
                    It.Is<object[]>(args =>
                        args.Length == 4 &&
                        (int)args[0] == existingConversation.IdConversation &&
                        (int)args[1] == idCompteRecoi &&
                        (string)args[2] == conversationDto.message),
                    default(CancellationToken)),
                Times.Once);
        }

        [TestMethod]
        public async Task PostCompletTest_ExistingConversation_UpdatesDateDernierMessage()
        {
            // Arrange
            var conversationDto = new ConversationCreateDTO
            {
                IdAnnonce = 1,
                DateDernierMessage = DateTime.Now,
                message = "Message de test"
            };

            var oldDate = DateTime.Now.AddDays(-2);
            var existingConversation = new Conversation
            {
                IdConversation = 15,
                IdAnnonce = conversationDto.IdAnnonce,
                DateDernierMessage = oldDate
            };

            int idCompteEnvoi = 2;
            int idCompteRecoi = 1;

            _mockManager.Setup(m => m.PostComplet(
                    It.IsAny<Conversation>(),
                    conversationDto.message,
                    idCompteEnvoi,
                    idCompteRecoi))
                .ReturnsAsync(existingConversation)
                .Callback<Conversation, string, int, int>((conv, msg, idEnvoi, idRecoi) =>
                {
                    // Simuler la mise à jour de la date
                    existingConversation.DateDernierMessage = DateTime.UtcNow;
                });

            // Act
            var actionResult = await _controller.PostComplet(idCompteEnvoi, idCompteRecoi, conversationDto);

            // Assert
            var created = (CreatedAtActionResult)actionResult.Result;
            var returnedConversation = (Conversation)created.Value;

            // Vérifier que la date a été mise à jour
            Assert.IsTrue(returnedConversation.DateDernierMessage > oldDate,
                "DateDernierMessage devrait être mis à jour pour une conversation existante");

            // Vérifier que c'est bien la conversation existante qui est retournée
            Assert.AreEqual(existingConversation.IdConversation, returnedConversation.IdConversation);
        }
        #endregion
    }
}