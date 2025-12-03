using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
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
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace App.Controllers.Tests
{
    [TestClass()]
    public class JournalControllerTests
    {
        private JournalController _controller;
        private AutoPulseBdContext _context;
        private JournalManager _manager;
        private IMapper _mapper;
        private Journal _objetcommun;
        private ILogger<JournalManager> logger;

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

            _manager = new JournalManager(_context, logger);
            _controller = new JournalController(_manager, _mapper);

            _context.Adresses.RemoveRange(_context.Adresses);
            await _context.SaveChangesAsync();


            TypeCompte typeCompte = new TypeCompte
            {
                IdTypeCompte = 1,
                Libelle = "Utilisateur"
            };
            Compte compte = new Compte
            {
                IdCompte = 1,
                Pseudo = "testuser",
                MotDePasse = "Password123!",
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@gmail.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = new DateTime(2000, 1, 1),
                IdTypeCompte = 1
            };

            TypeJournal typeJournal = new TypeJournal
            {
                IdTypeJournaux = 1,
                LibelleTypeJournaux = "Info"
            };

            Journal journal = new Journal
            {
                IdJournal = 1,
                ContenuJournal = "Ceci est le contenu du premier journal.",
                DateJournal = DateTime.Now,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            _context.TypesCompte.Add(typeCompte);
            _context.Comptes.Add(compte);
            _context.Journaux.Add(journal);
            await _context.SaveChangesAsync(); 

            _objetcommun = journal;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdJournal);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(JournalDTO));
            Assert.AreEqual(_objetcommun.ContenuJournal, result.Value.ContenuJournal);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<JournalDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.ContenuJournal == _objetcommun.ContenuJournal));
        }

        [TestMethod]
        public async Task PostJournalTest_Entity()
        {
            JournalDTO adresse = new JournalDTO
            {
                ContenuJournal = "Nouveau journal de test",
                DateJournal = DateTime.Now,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            var actionResult = await _controller.Post(adresse);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            Journal createdjournal = (Journal)created.Value;
            Assert.AreEqual(adresse.ContenuJournal, createdjournal.ContenuJournal);
        }


        [TestMethod]
        public async Task DeleteJournalTest()
        {
            var result = await _controller.Delete(_objetcommun.IdJournal);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedJournal = await _manager.GetByIdAsync(_objetcommun.IdJournal);
            Assert.IsNull(deletedJournal);
        }

        [TestMethod]
        public async Task NotFoundDeleteJournalTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutJournalTest()
        {
            JournalDTO journal = new JournalDTO()
            {
                IdJournal = _objetcommun.IdJournal,
                ContenuJournal = "Journal modifié",
                DateJournal = DateTime.Now,
                IdCompte = 1,
                IdTypeJournal = 1
            };


            var result = await _controller.Put(_objetcommun.IdJournal, journal);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            Journal journalput = await _manager.GetByIdAsync(_objetcommun.IdJournal);
            Assert.AreEqual(journal.ContenuJournal, journal.ContenuJournal);
        }

        [TestMethod]
        public async Task NotFoundPutJournalTest()
        {
            JournalDTO journal = new JournalDTO()
            {
                ContenuJournal = "Journal modifié",
                DateJournal = DateTime.Now,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            var result = await _controller.Put(0, journal);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutJournalTest()
        {
            JournalDTO journal = new JournalDTO()
            {
                ContenuJournal = null,
                DateJournal = DateTime.Now,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("ContenuJournal", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdJournal, journal);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task BadRequestPostJournalTest()
        {
            JournalDTO journal = new JournalDTO()
            {
                ContenuJournal = null,
                DateJournal = DateTime.Now,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            _controller.ModelState.AddModelError("ContenuJournal", "Required");

            var actionResult = await _controller.Post(journal);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

    }
}