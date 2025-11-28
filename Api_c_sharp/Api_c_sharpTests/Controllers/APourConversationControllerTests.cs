using Api_c_sharp.Controllers;
using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Controllers.Tests
{
    [TestClass]
    public class APourConversationControllerTests
    {
        private APourConversationController _controller;
        private AutoPulseBdContext _context;
        private APourConversationManager _manager;
        private IMapper _mapper;
        private APourConversation _objetCommun;

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

            _manager = new APourConversationManager(_context);
            _controller = new APourConversationController(_manager, _mapper);

            // Nettoyage
            _context.APourConversations.RemoveRange(_context.APourConversations);
            await _context.SaveChangesAsync();

            var compte = new Compte
            {
                IdCompte = 1,
                Pseudo = "testPseudo",
                MotDePasse = "testPsw",
                Nom = "testNom",
                Prenom = "testPrenom",
                Email = "test.test@gmail.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = DateTime.Now,
            };

            var conversation = new Conversation
            {
                IdConversation = 10,
                IdAnnonce = 1,

            };

            // Création d’un objet de test
            var entry = new APourConversation
            {
                IdCompte = 1,
                IdConversation = 10
            };

            await _context.Comptes.AddAsync(compte);
            await _context.Conversations.AddAsync(conversation);
            await _context.APourConversations.AddAsync(entry);
            await _context.SaveChangesAsync();
            var compte2 = new Compte
            {
                IdCompte = 2,
                Pseudo = "testPseudo",
                MotDePasse = "testPsw",
                Nom = "testNom",
                Prenom = "testPrenom",
                Email = "test.test@gmail.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = DateTime.Now,
            };
            await _context.Comptes.AddAsync(compte2);
            await _context.SaveChangesAsync();


            var conversation2 = new Conversation
            {
                IdConversation = 11,
                IdAnnonce = 1,

            };
            await _context.Conversations.AddAsync(conversation2);
            await _context.SaveChangesAsync();

            _objetCommun = entry;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Given : un enregistrement existant en base (_objetCommun)

            // When : on appelle le contrôleur pour récupérer l'objet par son ID
            var result = await _controller.GetByID(_objetCommun.IdConversation, _objetCommun.IdCompte);

            // Then : l'objet est retrouvé et correspond aux valeurs attendues
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(_objetCommun.IdConversation, result.Value.IdConversation);
            Assert.AreEqual(_objetCommun.IdCompte, result.Value.IdUtilisateur);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Given : un ID inexistant

            // When : on demande un objet avec cet ID
            var result = await _controller.GetByID(9999, _objetCommun.IdCompte);

            // Then : la réponse est NotFound
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Given : au moins un enregistrement en base (_objetCommun)

            // When : on récupère toute la liste
            var result = await _controller.GetAll();

            // Then : la liste contient des éléments dont celui inséré en setup
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);

            var list = result.Value.ToList();
            Assert.IsTrue(list.Any());
            Assert.IsTrue(list.Any(x => x.IdConversation == _objetCommun.IdConversation));
        }

        [TestMethod]
        public async Task PostTest()
        {
            // Given : un DTO valide à insérer
            


            var dto = new APourConversationDTO
            {
                IdUtilisateur = 2,
                IdConversation = 11
            };

            // When : on appelle le POST
            var actionResult = await _controller.Post(dto);

            // Then : l'objet est créé et renvoyé dans un CreatedAtActionResult
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            var createdEntity = (APourConversation)created.Value;

            Assert.AreEqual(dto.IdConversation, createdEntity.IdConversation);
            Assert.AreEqual(dto.IdUtilisateur, createdEntity.IdCompte);
        }

        [TestMethod]
        public async Task BadRequestPostTest()
        {
            // Given : un DTO invalide + ModelState en erreur
            var dto = new APourConversationDTO();
            _controller.ModelState.AddModelError("IdConversation", "Required");

            // When : on appelle POST avec un modèle invalide
            var actionResult = await _controller.Post(dto);

            // Then : la réponse est BadRequest
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task PutTest()
        {
            // Given : un DTO valide pour mettre à jour un élément existant
            var dto = new APourConversationDTO
            {
                IdUtilisateur = 99,
                IdConversation = _objetCommun.IdConversation
            };

            // When : on appelle PUT
            var result = await _controller.Put(_objetCommun.IdConversation,_objetCommun.IdCompte, dto);

            // Then : la réponse est NoContent et l'objet est mis à jour
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updated = await _manager.GetByIdAsync(_objetCommun.IdConversation);
            Assert.AreEqual(dto.IdUtilisateur, updated.IdCompte);
        }

        [TestMethod]
        public async Task NotFoundPutTest()
        {
            // Given : un DTO avec un ID inexistant
            var dto = new APourConversationDTO
            {
                IdUtilisateur = 1,
                IdConversation = 9999
            };

            // When : on tente de mettre à jour cet ID
            var result = await _controller.Put(9999,1, dto);

            // Then : le contrôleur retourne NotFound
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutTest()
        {
            // Given : un DTO valide mais ModelState invalide
            var dto = new APourConversationDTO
            {
                IdUtilisateur = 1,
                IdConversation = _objetCommun.IdConversation
            };
            _controller.ModelState.AddModelError("IdConversation", "Invalid");

            // When : on appelle PUT
            var result = await _controller.Put(_objetCommun.IdConversation,1, dto);

            // Then : le contrôleur retourne BadRequest
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task DeleteTest()
        {
            // Given : un enregistrement existant à supprimer (_objetCommun)

            // When : on appelle DELETE
            var result = await _controller.Delete(_objetCommun.IdConversation, _objetCommun.IdCompte);

            // Then : la réponse est NoContent et l'objet est supprimé
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deleted = await _manager.GetByIdsAsync(_objetCommun.IdConversation, _objetCommun.IdCompte);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task NotFoundDeleteTest()
        {
            // Given : un ID inexistant

            // When : on appelle DELETE
            var result = await _controller.Delete(9999, _objetCommun.IdCompte);

            // Then : la réponse est NotFound
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
