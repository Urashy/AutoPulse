using Api_c_sharp.Controllers;
using Api_c_sharp.Hubs;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
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
using System.Threading;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class MessageControllerTests
    {
        private Mock<MessageManager> _mockManager;
        private Mock<OffreManager> _mockOffreManager;
        private Mock<IJournalService> _mockJournalService;
        private Mock<IHubContext<MessageHub>> _mockHubContext;
        private Mock<IHubClients> _mockClients;
        private Mock<IClientProxy> _mockClientProxy;
        private MessageController _controller;
        private IMapper _mapper;
        private Message _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création des mocks
            _mockManager = new Mock<MessageManager>(null);
            _mockOffreManager = new Mock<OffreManager>(null);
            _mockJournalService = new Mock<IJournalService>();
            _mockHubContext = new Mock<IHubContext<MessageHub>>();
            _mockClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();

            // Configuration du hub context
            _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
            _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);

            // Création du message de référence
            _objetcommun = new Message
            {
                IdMessage = 1,
                ContenuMessage = "Bonjour, je suis intéressé par votre annonce.",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = 1,
                IdCompte = 1,
                EstLu = false
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new MessageController(
                _mockManager.Object,
                _mapper,
                _mockOffreManager.Object,
                _mockJournalService.Object,
                _mockHubContext.Object
            );
        }
        #region GET
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdMessage))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetByID(_objetcommun.IdMessage);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(MessageDTO));
            Assert.AreEqual(_objetcommun.ContenuMessage, result.Value.ContenuMessage);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Message)null);

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
            var messagesList = new List<Message>
            {
                _objetcommun,
                new Message
                {
                    IdMessage = 2,
                    ContenuMessage = "Oui, elle est toujours disponible.",
                    DateEnvoiMessage = DateTime.Now.AddMinutes(5),
                    IdConversation = 1,
                    IdCompte = 2,
                    EstLu = false
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(messagesList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<MessageDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.ContenuMessage == _objetcommun.ContenuMessage));
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task GetUnreadCountTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetUnreadMessageCount(1, 1))
                       .ReturnsAsync(5);

            // Act
            var result = await _controller.GetUnreadCount(1, 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);

            // Le résultat est un OkObjectResult avec Value
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(5, okResult.Value);
        }

        [TestMethod]
        public async Task GetByConversationAndMarkAsReadTest()
        {
            // Arrange
            var message1 = new Message
            {
                IdMessage = 1,
                ContenuMessage = "Message 1",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = 1,
                IdCompte = 1,
                EstLu = true
            };

            var message2 = new Message
            {
                IdMessage = 2,
                ContenuMessage = "Message 2",
                DateEnvoiMessage = DateTime.Now.AddMinutes(5),
                IdConversation = 1,
                IdCompte = 2,
                EstLu = true // Marqué comme lu par la fonction
            };

            var messagesList = new List<Message> { message1, message2 };

            _mockManager.Setup(m => m.GetMessagesByConversationAndMarkAsRead(1, 1))
                       .ReturnsAsync(messagesList);

            // Act
            var result = await _controller.GetByConversationAndMarkAsRead(1, 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<MessageDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.All(m => m.EstLu == true));

            var messageFromOtherUser = result.Value.FirstOrDefault(m => m.IdCompte == 2);
            Assert.IsNotNull(messageFromOtherUser);
            Assert.IsTrue(messageFromOtherUser.EstLu);
        }

        [TestMethod]
        public async Task GetByConversationAndMarkAsReadWithHubNotificationTest()
        {
            // Arrange
            var messagesList = new List<Message>
            {
                new Message
                {
                    IdMessage = 1,
                    ContenuMessage = "Message 1",
                    DateEnvoiMessage = DateTime.Now,
                    IdConversation = 1,
                    IdCompte = 1,
                    EstLu = true
                }
            };

            _mockManager.Setup(m => m.GetMessagesByConversationAndMarkAsRead(1, 1))
                       .ReturnsAsync(messagesList);

            // Act
            var result = await _controller.GetByConversationAndMarkAsRead(1, 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);

            // Vérifier que SignalR a été appelé avec "MessagesRead"
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "MessagesRead",
                    It.Is<object[]>(args =>
                        args.Length == 2 &&
                        (int)args[0] == 1 &&
                        (int)args[1] == 1
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [TestMethod]
        public async Task NotFoundGetByConversationAndMarkAsReadTest_AucunMessage()
        {
            // Arrange
            _mockManager.Setup(m => m.GetMessagesByConversationAndMarkAsRead(10, 1))
                       .ReturnsAsync((IEnumerable<Message>)null);

            // Act
            var result = await _controller.GetByConversationAndMarkAsRead(10, 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task NotFoundGetByConversationAndMarkAsReadTest_ListeVide()
        {
            // Arrange
            _mockManager.Setup(m => m.GetMessagesByConversationAndMarkAsRead(10, 1))
                       .ReturnsAsync(new List<Message>());

            // Act
            var result = await _controller.GetByConversationAndMarkAsRead(10, 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        #endregion

        #region POST

        [TestMethod]
        public async Task Post_OK_WithoutOffre_CreatesMessage()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = "Nouveau message"
            };

            var messageEntity = _mapper.Map<Message>(messageDTO);
            messageEntity.IdMessage = 3;
            messageEntity.DateEnvoiMessage = DateTime.UtcNow;
            messageEntity.EstLu = false;
            messageEntity.Offres = new List<Offre>(); // Collection vide

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .ReturnsAsync(messageEntity)
                       .Verifiable();

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>()))
                .Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.Post(messageDTO, withOffre: false);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            var createdMessage = (Message)created.Value;

            Assert.AreEqual(messageDTO.ContenuMessage, createdMessage.ContenuMessage);
            Assert.AreEqual(messageDTO.IdConversation, createdMessage.IdConversation);
            Assert.AreEqual(messageDTO.IdCompte, createdMessage.IdCompte);
            Assert.IsFalse(createdMessage.EstLu);
            Assert.IsNotNull(createdMessage.DateEnvoiMessage);

            _mockManager.Verify(m => m.AddAsync(It.IsAny<Message>()), Times.Once);
            _mockJournalService.Verify(j => j.LogEnvoiMessageAsync(
                messageDTO.IdCompte,
                messageDTO.IdConversation,
                messageDTO.ContenuMessage,
                null), Times.Once);
        }

        [TestMethod]
        public async Task Post_OK_WithOffre_CreatesMessageWithoutSignalR()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = "Message avec offre"
            };

            var messageEntity = _mapper.Map<Message>(messageDTO);
            messageEntity.IdMessage = 4;
            messageEntity.DateEnvoiMessage = DateTime.UtcNow;
            messageEntity.EstLu = false;
            messageEntity.Offres = new List<Offre> { new Offre() }; // Avec offre

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .ReturnsAsync(messageEntity);

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>()))
                .Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.Post(messageDTO, withOffre: true);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            // SignalR ne doit PAS être appelé quand withOffre = true
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "ReceiveMessage",
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Never
            );
        }

        [TestMethod]
        public async Task Post_OK_WithHubNotification_SendsSignalR()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = "Message avec notification"
            };

            var messageEntity = _mapper.Map<Message>(messageDTO);
            messageEntity.IdMessage = 5;
            messageEntity.DateEnvoiMessage = DateTime.UtcNow;
            messageEntity.EstLu = false;
            messageEntity.Offres = new List<Offre>();

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .ReturnsAsync(messageEntity);

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>()))
                .Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.Post(messageDTO, withOffre: false);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            // Vérifier que SignalR a été appelé avec les bons paramètres
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "ReceiveMessage",
                    It.Is<object[]>(args =>
                        args.Length == 4 &&
                        (int)args[0] == messageEntity.IdConversation &&
                        (int)args[1] == messageEntity.IdCompte &&
                        (string)args[2] == messageEntity.ContenuMessage &&
                        args[3] is DateTime
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [TestMethod]
        public async Task Post_BadRequest_InvalidModelState()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdCompte = 1,
                ContenuMessage = null
            };

            _controller.ModelState.AddModelError("ContenuMessage", "Required");

            // Act
            var actionResult = await _controller.Post(messageDTO, withOffre: false);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Message>()), Times.Never);
            _mockJournalService.Verify(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>()), Times.Never);
        }

        [TestMethod]
        public async Task Post_OK_SetsDateEnvoiToUtcNow()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = "Test date"
            };

            Message capturedMessage = null;
            var beforePost = DateTime.UtcNow;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .Callback<Message>(m => capturedMessage = m)
                       .ReturnsAsync((Message m) => { m.IdMessage = 10; return m; });

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Post(messageDTO, withOffre: false);

            var afterPost = DateTime.UtcNow;

            // Assert
            Assert.IsNotNull(capturedMessage);
            Assert.IsNotNull(capturedMessage.DateEnvoiMessage);
            Assert.IsTrue(capturedMessage.DateEnvoiMessage >= beforePost);
            Assert.IsTrue(capturedMessage.DateEnvoiMessage <= afterPost);
        }

        [TestMethod]
        public async Task Post_OK_SetsEstLuToFalse()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = "Test EstLu"
            };

            Message capturedMessage = null;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .Callback<Message>(m => capturedMessage = m)
                       .ReturnsAsync((Message m) => { m.IdMessage = 11; return m; });

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Post(messageDTO, withOffre: false);

            // Assert
            Assert.IsNotNull(capturedMessage);
            Assert.IsFalse(capturedMessage.EstLu);
        }

        [TestMethod]
        public async Task Post_OK_JournalServiceCalledBeforeAddAsync()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = "Test ordre"
            };

            var callOrder = new List<string>();

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>()))
                .Callback(() => callOrder.Add("Journal"))
                .Returns(Task.CompletedTask);

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .Callback(() => callOrder.Add("AddAsync"))
                       .ReturnsAsync(new Message
                       {
                           IdMessage = 12,
                           Offres = new List<Offre>(),
                           DateEnvoiMessage = DateTime.UtcNow,
                           EstLu = false
                       });

            // Act
            await _controller.Post(messageDTO, withOffre: false);

            // Assert
            Assert.AreEqual(2, callOrder.Count);
            Assert.AreEqual("Journal", callOrder[0]);
            Assert.AreEqual("AddAsync", callOrder[1]);
        }

        [TestMethod]
        public async Task Post_OK_ReturnsCreatedAtActionWithCorrectRoute()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = "Test route"
            };

            var messageEntity = _mapper.Map<Message>(messageDTO);
            messageEntity.IdMessage = 99;
            messageEntity.DateEnvoiMessage = DateTime.UtcNow;
            messageEntity.EstLu = false;
            messageEntity.Offres = new List<Offre>();

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .ReturnsAsync(messageEntity);

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>()))
                .Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.Post(messageDTO, withOffre: false);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            Assert.AreEqual("GetByID", created.ActionName);
            Assert.IsNotNull(created.RouteValues);
            Assert.AreEqual(99, created.RouteValues["id"]);
        }

        [TestMethod]
        public async Task Post_OK_WithNullHub_DoesNotCrash()
        {
            // Arrange - Créer un contrôleur avec hubContext = null
            var controllerWithoutHub = new MessageController(
                _mockManager.Object,
                _mapper,
                _mockOffreManager.Object,
                _mockJournalService.Object,
                null // hubContext null
            );

            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = "Test sans hub"
            };

            var messageEntity = _mapper.Map<Message>(messageDTO);
            messageEntity.IdMessage = 13;
            messageEntity.DateEnvoiMessage = DateTime.UtcNow;
            messageEntity.EstLu = false;
            messageEntity.Offres = new List<Offre>();

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .ReturnsAsync(messageEntity);

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>()))
                .Returns(Task.CompletedTask);

            // Act
            var actionResult = await controllerWithoutHub.Post(messageDTO, withOffre: false);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            // Pas d'exception levée même avec hubContext null
        }

        [TestMethod]
        public async Task Post_OK_MapperUsedCorrectly()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 5,
                IdCompte = 10,
                ContenuMessage = "Test mapper"
            };

            Message capturedMessage = null;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .Callback<Message>(m => capturedMessage = m)
                       .ReturnsAsync((Message m) => { m.IdMessage = 15; return m; });

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Post(messageDTO, withOffre: false);

            // Assert
            Assert.IsNotNull(capturedMessage);
            Assert.AreEqual(messageDTO.IdConversation, capturedMessage.IdConversation);
            Assert.AreEqual(messageDTO.IdCompte, capturedMessage.IdCompte);
            Assert.AreEqual(messageDTO.ContenuMessage, capturedMessage.ContenuMessage);
        }

        [TestMethod]
        public async Task Post_BadRequest_EmptyContent()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = ""
            };

            _controller.ModelState.AddModelError("ContenuMessage", "Content cannot be empty");

            // Act
            var actionResult = await _controller.Post(messageDTO, withOffre: false);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Message>()), Times.Never);
        }

        [TestMethod]
        public async Task Post_BadRequest_InvalidIdConversation()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 0,
                IdCompte = 1,
                ContenuMessage = "Test"
            };

            _controller.ModelState.AddModelError("IdConversation", "Invalid conversation ID");

            // Act
            var actionResult = await _controller.Post(messageDTO, withOffre: false);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteMessageTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdMessage))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.DeleteAsync(_objetcommun))
                       .Returns(Task.FromResult(true))
                       .Verifiable();

            // Act
            var result = await _controller.Delete(_objetcommun.IdMessage);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(_objetcommun), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundDeleteMessageTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Message)null);

            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutMessageTest()
        {
            // Arrange
            var existingMessage = new Message
            {
                IdMessage = _objetcommun.IdMessage,
                ContenuMessage = "Ancien contenu",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = 1,
                IdCompte = 1,
                EstLu = false
            };

            MessageUpdateDTO messageDTO = new MessageUpdateDTO()
            {
                IdMessage = _objetcommun.IdMessage,
                IdCompte = 1,
                ContenuMessage = "Nouveau contenu",
                DateEnvoiMessage = DateTime.Now
            };

            var updatedMessage = _mapper.Map<Message>(messageDTO);

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdMessage))
                       .ReturnsAsync(existingMessage);
            _mockManager.Setup(m => m.UpdateAsync(existingMessage, updatedMessage))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_objetcommun.IdMessage, messageDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Message>(), It.IsAny<Message>()), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundPutMessageTest()
        {
            // Arrange
            MessageUpdateDTO messageDTO = new MessageUpdateDTO()
            {
                IdMessage = 0,
                IdCompte = 1,
                ContenuMessage = "Contenu",
                DateEnvoiMessage = DateTime.Now
            };

            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Message)null);

            // Act
            var result = await _controller.Put(0, messageDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutMessageTest()
        {
            // Arrange
            MessageUpdateDTO messageDTO = new MessageUpdateDTO()
            {
                IdMessage = _objetcommun.IdMessage,
                IdCompte = 1,
                ContenuMessage = null,
                DateEnvoiMessage = DateTime.Now
            };

            
            var existingMessage = new Message
            {
                IdMessage = _objetcommun.IdMessage,
                ContenuMessage = "Ancien contenu",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = 1,
                IdCompte = 1,
                EstLu = false
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdMessage))
                       .ReturnsAsync(existingMessage);

            _controller.ModelState.AddModelError("Contenu", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdMessage, messageDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Message>(), It.IsAny<Message>()), Times.Never);
        }
        #endregion
        
    }
}