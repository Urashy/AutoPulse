using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using App.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace App.Controllers.Tests
{
    [TestClass()]
    public class messageControllerTests
    {
        private MessageController _controller;
        private AutoPulseBdContext _context;
        private MessageManager _manager;
        private IMapper _mapper;
        private Message _objetcommun;

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

            _manager = new MessageManager(_context);
            _controller = new MessageController(_manager, _mapper);

            _context.Messages.RemoveRange(_context.Messages);
            await _context.SaveChangesAsync();

            TypeCompte typecompte = new TypeCompte()
            {
                IdTypeCompte = 1,
                Libelle = "Particulier"
            };

            Compte compte = new Compte()
            {
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

            Compte compte2 = new Compte()
            {
                Email = "john2@gmail.com",
                MotDePasse = "Password123!",
                Nom = "Doe2",
                Prenom = "John2",
                DateNaissance = new DateTime(1992, 1, 1),
                IdTypeCompte = typecompte.IdTypeCompte,
                Pseudo = "john_doe2",
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

            Message message = new Message()
            {
                IdMessage = 1,
                ContenuMessage = "Bonjour, je suis intéressé par votre annonce.",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = 1,
                IdCompte = 1
            };

            APourConversation apourConversation = new APourConversation()
            {
                IdConversation = 1,
                IdCompte = 1
            };
            APourConversation apourConversation2 = new APourConversation()
            {
                IdConversation = 1,
                IdCompte = 2
            };

            _context.TypesCompte.Add(typecompte);
            _context.Comptes.Add(compte);
            _context.Comptes.Add(compte2);
            _context.Annonces.Add(annonce);
            _context.Conversations.Add(conversation);
            _context.Messages.Add(message);
            _context.APourConversations.Add(apourConversation);
            _context.APourConversations.Add(apourConversation2);
            await _context.SaveChangesAsync(); 

            _objetcommun = message;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdMessage);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(AdresseDTO));
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AdresseDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Rue == _objetcommun.Rue));
        }

        [TestMethod]
        public async Task PostVoitureTest_Entity()
        {
            var adresse = new AdresseDTO()
            {
                Nom = "Travail",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 15,
                IdPays = 1,       // clé étrangère vers Pays
                IdCompte = 1
            }
            ;

            var actionResult = await _controller.Post(adresse);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdVoiture = (Adresse)created.Value;
            Assert.AreEqual(adresse.Rue, createdVoiture.Rue);
        }


        [TestMethod]
        public async Task DeleteVoitureTest()
        {
            var result = await _controller.Delete(_objetcommun.IdAdresse);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedVoiture = await _manager.GetByIdAsync(_objetcommun.IdAdresse);
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
            var adresse = new AdresseDTO()
            {
                IdAdresse = _objetcommun.IdAdresse,
                Nom = "Domicile",
                LibelleVille = "Chavanod",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = 1,  
                IdCompte = 1,
            };

            var result = await _controller.Put(_objetcommun.IdAdresse, adresse);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var adresseput = await _manager.GetByIdAsync(_objetcommun.IdAdresse);
            Assert.AreEqual(adresse.Nom, adresseput.Nom);
        }

        [TestMethod]
        public async Task NotFoundPutVoitureTest()
        {
            var adresse = new AdresseDTO()
            {
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = 1,
                IdCompte = 1,
            };

            var result = await _controller.Put(0, adresse);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutVoitureTest()
        {
            var adresse = new AdresseDTO()
            {
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = -12,
                IdPays = 1,
                IdCompte = 1,
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("Numero", "Le Numero doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdAdresse, adresse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task BadRequestPostVoitureTest()
        {
            var adresse = new AdresseDTO
            {
                Nom = null,
            };

            _controller.ModelState.AddModelError("Nom", "Required");

            var actionResult = await _controller.Post(adresse);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

    }
}