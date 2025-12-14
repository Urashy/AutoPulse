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
    public class MessageControllerTestsMoq
    {
        private Mock<MessageManager> _mockManager;
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
                _mockJournalService.Object,
                _mockHubContext.Object
            );
        }

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
        public async Task PostMessageTest_Entity()
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

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .ReturnsAsync(messageEntity)
                       .Verifiable();

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.Post(messageDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdMessage = (Message)created.Value;
            Assert.AreEqual(messageDTO.ContenuMessage, createdMessage.ContenuMessage);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Message>()), Times.Once);
            _mockJournalService.Verify(j => j.LogEnvoiMessageAsync(
                messageDTO.IdCompte,
                messageDTO.IdConversation,
                messageDTO.ContenuMessage), Times.Once);
        }

        [TestMethod]
        public async Task PostMessageWithHubNotificationTest()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = "Message avec notification"
            };

            var messageEntity = _mapper.Map<Message>(messageDTO);
            messageEntity.IdMessage = 4;
            messageEntity.DateEnvoiMessage = DateTime.UtcNow;
            messageEntity.EstLu = false;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Message>()))
                       .ReturnsAsync(messageEntity);

            _mockJournalService.Setup(j => j.LogEnvoiMessageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.Post(messageDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            // Vérifier que SignalR a été appelé avec "ReceiveMessage"
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "ReceiveMessage",
                    It.Is<object[]>(args =>
                        args.Length == 4 &&
                        (int)args[0] == messageEntity.IdConversation &&
                        (int)args[1] == messageEntity.IdCompte &&
                        (string)args[2] == messageEntity.ContenuMessage &&
                        args[3] != null
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [TestMethod]
        public async Task BadRequestPostMessageTest()
        {
            // Arrange
            MessageCreateDTO messageDTO = new MessageCreateDTO()
            {
                IdCompte = 1,
                ContenuMessage = null
            };

            _controller.ModelState.AddModelError("ContenuMessage", "Required");

            // Act
            var actionResult = await _controller.Post(messageDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

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

            // ✅ CORRECTION: Il faut d'abord setup le GetByIdAsync pour que le message existe
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

            _controller.ModelState.AddModelError("ContenuMessage", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdMessage, messageDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task GetUnreadCountTest()
        {
            // Arrange
            // ✅ CORRECTION: Le controller fait un Ok(count), pas un simple retour de valeur
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
    }
}