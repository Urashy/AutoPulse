using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoPulse.Shared.DTO;
using App.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;

namespace App.Controllers.Tests
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


        [TestMethod]
        public async Task PostVoitureTest_Entity()
        {
            var voiture = new ConversationCreateDTO
            {
                IdConversation = 2,
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = _objetcommun.DateDernierMessage,
            };


            var actionResult = await _controller.Post(voiture);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdVoiture = (Conversation)created.Value;
            Assert.AreEqual(voiture.IdConversation, createdVoiture.IdConversation);
        }


        [TestMethod]
        public async Task DeleteVoitureTest()
        {
            var result = await _controller.Delete(_objetcommun.IdConversation);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedVoiture = await _manager.GetByIdAsync(_objetcommun.IdConversation);
            Assert.IsNull(deletedVoiture);
        }

        [TestMethod]
        public async Task NotFoundDeleteVoitureTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutVoitureTest()
        {
            var voiture = new ConversationCreateDTO()
            {
                IdConversation = _objetcommun.IdConversation,
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            var result = await _controller.Put(_objetcommun.IdConversation, voiture);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var fetchedVoiture = await _manager.GetByIdAsync(_objetcommun.IdConversation);
            Assert.AreEqual(voiture.IdConversation, fetchedVoiture.IdConversation);
        }

        [TestMethod]
        public async Task NotFoundPutVoitureTest()
        {
            var voiture = new ConversationCreateDTO()
            {
                IdConversation = _objetcommun.IdConversation,
                IdAnnonce = _objetcommun.IdAnnonce,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            var result = await _controller.Put(0, voiture);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutVoitureTest()
        {
            // Arrange : voiture avec kilométrage invalide
            var voiture = new ConversationCreateDTO()
            {
                IdConversation = _objetcommun.IdConversation,
                IdAnnonce = -1,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("IdAnnonce", "Le IdAnnonce doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdConversation, voiture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task BadRequestPostVoitureTest()
        {
            var voiture = new ConversationCreateDTO()
            {
                IdConversation = _objetcommun.IdConversation,
                IdAnnonce = -1,
                DateDernierMessage = DateTime.Now.AddDays(1),
            };

            _controller.ModelState.AddModelError("IdAnnonce", "Le IdAnnonce doit être supérieur à 0");

            var actionResult = await _controller.Post(voiture);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }


        [TestMethod]
        public async Task GetConversationByCompteIDTest()
        {
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
            // Acts
            var result = await _controller.GetConversationsByCompteID(0);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}