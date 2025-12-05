using Api_c_sharp.Controllers;
using Api_c_sharp.Hubs;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using App.Controllers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace App.Controllers.Tests
{
    [TestClass()]
    public class MessageControllerTests
    {
        private MessageController _controller;
        private AutoPulseBdContext _context;
        private MessageManager _manager;
        private IMapper _mapper;
        private Message _objetcommun;
        private IJournalService _journalService;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _journalService = new JournalManager(_context, NullLogger<JournalManager>.Instance);
            _manager = new MessageManager(_context);
            _controller = new MessageController(_manager, _mapper, _journalService);

            _context.Messages.RemoveRange(_context.Messages);
            await _context.SaveChangesAsync();

            TypeCompte typecompte = new TypeCompte()
            {
                IdTypeCompte = 1,
                Libelle = "Particulier"
            };

            // ✅ PREMIER UTILISATEUR (vendeur)
            Compte compte1 = new Compte()
            {
                IdCompte = 1,
                Email = "john@gmail.com",
                MotDePasse = "Password123!",
                Nom = "Doe",
                Prenom = "John",
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = typecompte.IdTypeCompte,
                Pseudo = "john_doe",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now
            };

            // ✅ DEUXIÈME UTILISATEUR (acheteur)
            Compte compte2 = new Compte()
            {
                IdCompte = 2,
                Email = "jane@gmail.com",
                MotDePasse = "Password123!",
                Nom = "Smith",
                Prenom = "Jane",
                DateNaissance = new DateTime(1992, 5, 15),
                IdTypeCompte = typecompte.IdTypeCompte,
                Pseudo = "jane_smith",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now
            };

            Annonce annonce = new Annonce()
            {
                Libelle = "Annonce de test",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                IdVoiture = 1,
                Prix = 10000,
                Description = "Description de test"
            };

            Conversation conversation = new Conversation()
            {
                IdConversation = 1,
                IdAnnonce = 1
            };

            // ✅ Message envoyé par le compte 1
            Message message1 = new Message()
            {
                IdMessage = 1,
                ContenuMessage = "Bonjour, je suis intéressé par votre annonce.",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = 1,
                IdCompte = 1,
                EstLu = false,
            };

            // ✅ Message envoyé par le compte 2 (NON LU)
            Message message2 = new Message()
            {
                IdMessage = 2,
                ContenuMessage = "Oui, elle est toujours disponible.",
                DateEnvoiMessage = DateTime.Now.AddMinutes(5),
                IdConversation = 1,
                IdCompte = 2,  // Envoyé par l'autre utilisateur
                EstLu = false, // NON LU
            };

            _context.TypesCompte.Add(typecompte);
            _context.Comptes.Add(compte1);
            _context.Comptes.Add(compte2);  // ✅ Ajouter le 2ème compte
            _context.Annonces.Add(annonce);
            _context.Conversations.Add(conversation);
            _context.Messages.Add(message1);
            _context.Messages.Add(message2);  // ✅ Ajouter le 2ème message
            await _context.SaveChangesAsync();

            _objetcommun = message1;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
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
            // Act
            var result = await _controller.GetByID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Act
            var result = await _controller.GetAll();
           
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<MessageDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.ContenuMessage == _objetcommun.ContenuMessage));
        }

        [TestMethod]
        public async Task PostMessageTest_Entity()
        {
            MessageCreateDTO message = new MessageCreateDTO()
            {
                IdConversation = 1,
                IdCompte = 1,
                ContenuMessage = _objetcommun.ContenuMessage,
            };

            var actionResult = await _controller.Post(message);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdmessage = (Message)created.Value;
            Assert.AreEqual(message.ContenuMessage, createdmessage.ContenuMessage);
        }

        [TestMethod]
        public async Task DeleteMessageTest()
        {
            var result = await _controller.Delete(_objetcommun.IdMessage);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedMessage = await _manager.GetByIdAsync(_objetcommun.IdMessage);
            Assert.IsNull(deletedMessage);
        }

        [TestMethod]
        public async Task NotFoundDeleteMessageTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutMessageTest()
        {
            MessageDTO message = new MessageDTO()
            {
                IdMessage = _objetcommun.IdMessage,
                IdCompte = 1,
                ContenuMessage = _objetcommun.ContenuMessage,
                DateEnvoiMessage = DateTime.Now,
            };

            var result = await _controller.Put(_objetcommun.IdMessage, message);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            Message messageput = await _manager.GetByIdAsync(_objetcommun.IdMessage);
            Assert.AreEqual(message.ContenuMessage, messageput.ContenuMessage);
        }

        [TestMethod]
        public async Task NotFoundPutMessageTest()
        {
            MessageDTO message = new MessageDTO()
            {
                IdCompte = 1,
                ContenuMessage = _objetcommun.ContenuMessage,
                DateEnvoiMessage = DateTime.Now,
            };

            var result = await _controller.Put(0, message);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutMessageTest()
        {
            MessageDTO message = new MessageDTO()
            {
                IdCompte = 1,
                ContenuMessage = null,
                DateEnvoiMessage = DateTime.Now,
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("Contenu", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdMessage, message);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task BadRequestPostMessageTest()
        {
            MessageCreateDTO message = new MessageCreateDTO()
            {
                IdCompte = 1,
                ContenuMessage = _objetcommun.ContenuMessage,
            };

            _controller.ModelState.AddModelError("ContenuMessage", "Required");

            var actionResult = await _controller.Post(message);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task GetUnreadCountTests()
        {
            // Act
            var result = await _controller.GetUnreadCount(1,1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(int));
            Assert.IsTrue(result.Value >= 0);
        }

        [TestMethod]
        public async Task GetByConversationAndMarkAsReadTests()
        {

            var result = await _controller.GetByConversationAndMarkAsRead(1, 1);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<MessageDTO>));
            Assert.IsTrue(result.Value.Any());

            Assert.IsTrue(result.Value.Any(o => o.EstLu == true));

            var messageFromOtherUser = result.Value.FirstOrDefault(m => m.IdCompte == 2);
            Assert.IsNotNull(messageFromOtherUser);
            Assert.IsTrue(messageFromOtherUser.EstLu);
        }
        [TestMethod]
        public async Task NotFoundGetByConversationAndMarkAsReadTest_AucunMessage()
        {
            // Arrange
            // Créer une conversation sans messages
            Conversation conversationVide = new Conversation
            {
                IdConversation = 10,
                IdAnnonce = 1,
                DateDernierMessage = DateTime.UtcNow
            };
            await _context.Conversations.AddAsync(conversationVide);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetByConversationAndMarkAsRead(
                conversationVide.IdConversation,
                _objetcommun.IdCompte
            );

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}