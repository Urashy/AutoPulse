using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
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
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.ControllersUnitaires.Tests
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
            var apourvonv2 = new APourConversation
            {
                IdCompte = 2,
                IdConversation = 10
            };

            await _context.Comptes.AddAsync(compte);
            await _context.Conversations.AddAsync(conversation);
            await _context.APourConversations.AddAsync(entry);
            await _context.APourConversations.AddAsync(apourvonv2);
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

            var compte3 = new Compte
            {
                IdCompte = 3,
                Pseudo = "testPseudo3",
                MotDePasse = "testPsw3",
                Nom = "testNom3",
                Prenom = "testPrenom3",
                Email = "test.test@gmail.com3",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = DateTime.Now,
            };
            await _context.Comptes.AddAsync(compte2);
            await _context.Comptes.AddAsync(compte3);



            var conversation2 = new Conversation
            {
                IdConversation = 11,
                IdAnnonce = 1,

            };
            await _context.Conversations.AddAsync(conversation2);
            await _context.SaveChangesAsync();

            _objetCommun = entry;
        }
        #region GET
            #region GetByID
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetCommun.IdConversation, _objetCommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(_objetCommun.IdConversation, result.Value.IdConversation);
            Assert.AreEqual(_objetCommun.IdCompte, result.Value.IdCompte);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Act
            var result = await _controller.GetById(9999, _objetCommun.IdCompte);

            // Assert
            Assert.IsNotNull(result.Result);
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

            var list = result.Value.ToList();
            Assert.IsTrue(list.Any());
            Assert.IsTrue(list.Any(x => x.IdConversation == _objetCommun.IdConversation));
        }
        #endregion

        #endregion

        #region POST
        [TestMethod]
        public async Task PostTest()
        {
                 
            // Arrange
            var dto = new APourConversationDTO
            {
                IdCompte = 2,
                IdConversation = 11
            };

            // Act
            var actionResult = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            var createdEntity = (APourConversation)created.Value;

            Assert.AreEqual(dto.IdConversation, createdEntity.IdConversation);
            Assert.AreEqual(dto.IdCompte, createdEntity.IdCompte);
        }

        [TestMethod]
        public async Task BadRequestPostTest()
        {
            // Arrange
            var dto = new APourConversationDTO();
            _controller.ModelState.AddModelError("IdConversation", "Required");

            // Act
            var actionResult = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutTest()
        {
            // Arrange
            var dto = new APourConversationDTO
            {
                IdCompte = _objetCommun.IdCompte,
                IdConversation = _objetCommun.IdConversation
            };

            // Act
            var result = await _controller.Put(_objetCommun.IdConversation,_objetCommun.IdCompte, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updated = await _manager.GetAPourConversationByIDS( _objetCommun.IdCompte, _objetCommun.IdConversation);
            Assert.AreEqual(dto.IdCompte, updated.IdCompte);
        }

        [TestMethod]
        public async Task NotFoundPutTest()
        {
            // Arrange
            var dto = new APourConversationDTO
            {
                IdCompte = 1,
                IdConversation = 9999
            };

            // Act
            var result = await _controller.Put(9999,1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutTest()
        {
            // Arrange
            var dto = new APourConversationDTO
            {
                IdCompte = 1,
                IdConversation = _objetCommun.IdConversation
            };
            _controller.ModelState.AddModelError("IdConversation", "Invalid");

           // Act
            var result = await _controller.Put(_objetCommun.IdConversation,1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteTest()
        {
            // Act
            var result = await _controller.Delete(_objetCommun.IdConversation, _objetCommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deleted = await _manager.GetAPourConversationByIDS(_objetCommun.IdConversation, _objetCommun.IdCompte);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task NotFoundDeleteTest()
        {
            // Act
            var result = await _controller.Delete(9999, _objetCommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region EXISTS

        [TestMethod]
        public async Task ExistsTrueTest()
        {
            var result = await _controller.Exists(1, 2, 1);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Value);
        }

        [TestMethod]
        public async Task ExistsFalseTest()
        {
            var result = await _controller.Exists(1, 3, 1);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Value);
        }

        [TestMethod]
        public async Task ExistsAucuneConversationTest()
        {
            var result = await _controller.Exists(1, 3, 2);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Value);
        }

        #endregion
    }
}
