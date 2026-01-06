using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class ConversationControllerTests
    {
        private ConversationController _controller;
        private AutoPulseBdContext _context;
        private ConversationManager _manager;
        private IMapper _mapper;
        private Conversation _objetcommun;
        private IConversationEnrichmentService _conversationEnrichmentService;

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

            _manager = new ConversationManager(_context);
            var messageManager = new MessageManager(_context);
            _conversationEnrichmentService = new ConversationEnrichmentService(messageManager, _mapper);
            _controller = new ConversationController(_manager, _conversationEnrichmentService, _mapper);

            _context.Marques.Add(new Marque { IdMarque = 1, LibelleMarque = "TestMarque" });
            _context.Motricites.Add(new Motricite { IdMotricite = 1, LibelleMotricite = "4x4" });
            _context.Carburants.Add(new Carburant { IdCarburant = 1, LibelleCarburant = "Essence" });
            _context.BoitesDeVitesses.Add(new BoiteDeVitesse { IdBoiteDeVitesse = 1, LibelleBoite = "Manuelle" });
            _context.Categories.Add(new Categorie { IdCategorie = 1, LibelleCategorie = "SUV" });
            _context.Modeles.Add(new Modele { IdModele = 1, LibelleModele = "Modele Test" });

            TypeCompte typeCompte = new TypeCompte
            {
                IdTypeCompte= 1,
                Libelle = "Acheteur"
            };

            Compte vendeur = new Compte
            {
                IdCompte = 1,
                Nom = "Test",
                Prenom = "John",
                Email = "john.Test@gmail.com",
                Pseudo = "testjohn",
                MotDePasse = "Password123!",
                DateCreation = DateTime.Now,
                DateNaissance = new DateTime(1990, 1, 1),
                DateDerniereConnexion = DateTime.Now,
                IdTypeCompte = typeCompte.IdTypeCompte
            };

            Voiture voiture = new Voiture
            {
                IdVoiture = 1,
                IdMarque = 1,
                IdModele = 1,
                IdCategorie = 1,
                IdCarburant = 1,
                IdBoiteDeVitesse = 1,
                IdMotricite = 1,
                Kilometrage = 5000,
                Annee = 2021,
                Puissance = 150,
                MiseEnCirculation = new DateTime(2021, 6, 15)
            };

            Annonce annnonce = new Annonce
            {
                IdAnnonce = 1,
                Libelle = "Annonce Test",
                Description = "Description de l'annonce test",
                Prix = 10000,
                DatePublication = DateTime.Now,
                IdCompte = vendeur.IdCompte,
                IdVoiture = 1,
            };

            Conversation conversation = new Conversation
            {
                IdConversation = 1,
                DateDernierMessage = DateTime.Now,
                IdAnnonce = annnonce.IdAnnonce,
            };

            APourConversation compteConversation = new APourConversation
            {
                IdCompte = vendeur.IdCompte,
                IdConversation = conversation.IdConversation
            };

            await _context.TypesCompte.AddAsync(typeCompte);
            await _context.Comptes.AddAsync(vendeur);
            await _context.Voitures.AddAsync(voiture);
            await _context.Annonces.AddAsync(annnonce);
            await _context.Conversations.AddAsync(conversation);
            await _context.APourConversations.AddAsync(compteConversation);
            await _context.SaveChangesAsync();

            _objetcommun = conversation;
        }

        #region GET

            #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
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
            // Act
            var result = await _controller.GetByID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

            #region GetAll
        [TestMethod]
        public async Task GetAllTest()
        {
            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<ConversationListDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.IdConversation == _objetcommun.IdConversation));
        }
        #endregion

            #region GetConversationByCompteID
        [TestMethod]
        public async Task GetConversationByCompteIDTest()
        {
            // Act
            var result = await _controller.GetConversationsByCompteID(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<ConversationListDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.IdConversation == _objetcommun.IdConversation));
        }

        [TestMethod]
        public async Task NotFoundGetConversationByCompteIDTest()
        {
            // Act
            var result = await _controller.GetConversationsByCompteID(0);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #endregion

        #region POST

            #region PostConversation
        [TestMethod]
        public async Task PostConversationTest_Entity()
        {
            // Arrange
            var Conversation = new ConversationCreateDTO
            {
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = _objetcommun.DateDernierMessage,
            };

            // Act
            var actionResult = await _controller.Post(Conversation);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdConversation = (Conversation)created.Value;
            Assert.AreEqual(Conversation.DateDernierMessage.ToUniversalTime(), createdConversation.DateDernierMessage);
        }

        [TestMethod]
        public async Task BadRequestPostConversationTest()
        {
            // Arrange
            ConversationCreateDTO Conversation = new ConversationCreateDTO()
            {
                IdAnnonce = -1,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            _controller.ModelState.AddModelError("IdAnnonce", "Le IdAnnonce doit être supérieur à 0");

            // Act
            var actionResult = await _controller.Post(Conversation);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }
        #endregion

            #region PostComplet
        [TestMethod]
        public async Task PostCompletTest_Success()
        {
            // Arrange
            var acheteur = new Compte
            {
                IdCompte = 2,
                Nom = "Acheteur",
                Prenom = "Jean",
                Email = "jean.acheteur@gmail.com",
                Pseudo = "jeanacheteur",
                MotDePasse = "Password123!",
                DateCreation = DateTime.Now,
                DateNaissance = new DateTime(1995, 5, 10),
                DateDerniereConnexion = DateTime.Now,
                IdTypeCompte = 1
            };
            await _context.Comptes.AddAsync(acheteur);
            await _context.SaveChangesAsync();

            var conversationDto = new ConversationCreateDTO
            {
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = DateTime.Now,
                message = "Bonjour, je suis intéressé par votre annonce"
            };

            // Act
            var actionResult = await _controller.PostComplet(acheteur.IdCompte, 1, conversationDto);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            Assert.IsNotNull(created.Value);

            var createdConversation = (Conversation)created.Value;
            Assert.IsTrue(createdConversation.IdConversation > 0);

            var aPourConversations = await _context.APourConversations
                .Where(apc => apc.IdConversation == createdConversation.IdConversation)
                .ToListAsync();

            Assert.AreEqual(2, aPourConversations.Count);
            Assert.IsTrue(aPourConversations.Any(apc => apc.IdCompte == acheteur.IdCompte));
            Assert.IsTrue(aPourConversations.Any(apc => apc.IdCompte == 1));

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.IdConversation == createdConversation.IdConversation);

            Assert.IsNotNull(message);
            Assert.AreEqual(conversationDto.message, message.ContenuMessage);
            Assert.AreEqual(acheteur.IdCompte, message.IdCompte);
            Assert.IsFalse(message.EstLu);
            Assert.IsTrue((DateTime.UtcNow - message.DateEnvoiMessage).TotalSeconds < 5);
        }

        [TestMethod]
        public async Task PostCompletTest_ExistingConversation_AddsMessageOnly()
        {
            // Arrange
            var acheteur = new Compte
            {
                IdCompte = 2,
                Nom = "Acheteur",
                Prenom = "Jean",
                Email = "jean.acheteur@gmail.com",
                Pseudo = "jeanacheteur",
                MotDePasse = "Password123!",
                DateCreation = DateTime.Now,
                DateNaissance = new DateTime(1995, 5, 10),
                DateDerniereConnexion = DateTime.Now,
                IdTypeCompte = 1
            };
            await _context.Comptes.AddAsync(acheteur);

            var oldDate = DateTime.UtcNow.AddHours(-2);
            var existingConversation = new Conversation
            {
                IdConversation = 10,
                DateDernierMessage = oldDate,
                IdAnnonce = _objetcommun.IdAnnonce
            };
            await _context.Conversations.AddAsync(existingConversation);

            var participant1 = new APourConversation
            {
                IdCompte = 1,
                IdConversation = existingConversation.IdConversation
            };
            var participant2 = new APourConversation
            {
                IdCompte = acheteur.IdCompte,
                IdConversation = existingConversation.IdConversation
            };
            await _context.APourConversations.AddAsync(participant1);
            await _context.APourConversations.AddAsync(participant2);

            var initialMessage = new Message
            {
                IdMessage = 1,
                IdConversation = existingConversation.IdConversation,
                ContenuMessage = "Premier message",
                DateEnvoiMessage = DateTime.UtcNow.AddHours(-2),
                EstLu = true,
                IdCompte = acheteur.IdCompte
            };
            await _context.Messages.AddAsync(initialMessage);
            await _context.SaveChangesAsync();

            var conversationsCountBefore = await _context.Conversations.CountAsync();
            var messagesCountBefore = await _context.Messages.CountAsync();

            var conversationDto = new ConversationCreateDTO
            {
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = DateTime.Now,
                message = "Nouveau message dans conversation existante"
            };

            // Act
            var actionResult = await _controller.PostComplet(acheteur.IdCompte, 1, conversationDto);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            var returnedConversation = (Conversation)created.Value;

            var conversationsCountAfter = await _context.Conversations.CountAsync();
            Assert.AreEqual(conversationsCountBefore, conversationsCountAfter,
                "Aucune nouvelle conversation ne devrait être créée");

            Assert.AreEqual(existingConversation.IdConversation, returnedConversation.IdConversation);

            var messagesCountAfter = await _context.Messages.CountAsync();
            Assert.AreEqual(messagesCountBefore + 1, messagesCountAfter,
                "Un nouveau message devrait être ajouté");

            var newMessage = await _context.Messages
                .Where(m => m.IdConversation == existingConversation.IdConversation)
                .OrderByDescending(m => m.DateEnvoiMessage)
                .FirstOrDefaultAsync();

            Assert.IsNotNull(newMessage);
            Assert.AreEqual(conversationDto.message, newMessage.ContenuMessage);
            Assert.AreEqual(acheteur.IdCompte, newMessage.IdCompte);
            Assert.IsFalse(newMessage.EstLu);

            var updatedConversation = await _context.Conversations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdConversation == existingConversation.IdConversation);

            Assert.IsNotNull(updatedConversation);
            Assert.IsTrue(updatedConversation.DateDernierMessage > oldDate,
                $"DateDernierMessage devrait être mis à jour. Ancienne: {oldDate:O}, Nouvelle: {updatedConversation.DateDernierMessage:O}");
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
            var actionResult = await _controller.PostComplet(1, 2, conversationDto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteConversationTest()
        {
            // Act
            var result = await _controller.Delete(_objetcommun.IdConversation);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedConversation = await _manager.GetByIdAsync(_objetcommun.IdConversation);
            Assert.IsNull(deletedConversation);
        }

        [TestMethod]
        public async Task NotFoundDeleteConversationTest()
        {
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
            ConversationUpdateDTO Conversation = new ConversationUpdateDTO()
            {
                IdConversation = _objetcommun.IdConversation,
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdConversation, Conversation);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var fetchedConversation = await _manager.GetByIdAsync(_objetcommun.IdConversation);
            Assert.AreEqual(Conversation.IdConversation, fetchedConversation.IdConversation);
        }

        [TestMethod]
        public async Task NotFoundPutConversationTest()
        {
            // Arrange
            ConversationUpdateDTO Conversation = new ConversationUpdateDTO()
            {
                IdConversation = _objetcommun.IdConversation,
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            // Act
            var result = await _controller.Put(0, Conversation);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutConversationTest()
        {
            // Arrange
            ConversationUpdateDTO Conversation = new ConversationUpdateDTO()
            {
                IdConversation = _objetcommun.IdConversation,
                IdAnnonce = -1,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            // Forcer l'erreur de validation
            _controller.ModelState.AddModelError("IdAnnonce", "Le IdAnnonce doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdConversation, Conversation);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

        


        

        
    }
}