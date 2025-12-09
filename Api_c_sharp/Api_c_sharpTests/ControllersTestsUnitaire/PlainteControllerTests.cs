using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Api_c_sharp.Controllers;
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

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class PlainteControllerTests
    {
        private PlainteController _controller;
        private AutoPulseBdContext _context;
        private PlainteManager _manager;
        private IMapper _mapper;
        private Plainte _objetcommun;

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

            _manager = new PlainteManager(_context);
            _controller = new PlainteController(_manager, _mapper);

            _context.Adresses.RemoveRange(_context.Adresses);
            await _context.SaveChangesAsync();

            TypeCompte typeCompte = new TypeCompte
            {
                Libelle = "Utilisateur"
            };

            EtatCompte etatCompte = new EtatCompte
            {
                IdEtatCompte = 1,
                Libelle = "Actif"
            };

            Compte compte = new Compte
            {
                Pseudo = "testuser",
                MotDePasse = "Password123!",
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@gmail.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = new DateTime(2000, 1, 1),
                IdTypeCompte = 1,
                IdEtatCompte = 1
            };

            TypeSignalement typeSignalement = new TypeSignalement
            {
                IdTypeSignalement = 1,
                LibelleTypeSignalement = "Bug"
            };

            Signalement signalement = new Signalement
            {
                DateCreationSignalement = DateTime.Now,
                DescriptionSignalement = "Ceci est un signalement de test.",
                IdCompteSignalant = 1,
                IdCompteSignale = 1,
                IdTypeSignalement = 1
            };

            Plainte plainte = new Plainte
            {
                IdPlainte = 1,
                Description = "Ceci est une plainte de test. ",
                DateCreation = DateTime.Now,
                IdSignalement = 1
            };
            _context.TypesCompte.Add(typeCompte);
            _context.EtatComptes.Add(etatCompte);
            _context.Comptes.Add(compte);
            _context.TypesSignalement.Add(typeSignalement);
            _context.Signalements.Add(signalement);
            _context.Plaintes.Add(plainte);
            await _context.SaveChangesAsync();

            _objetcommun = plainte;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdPlainte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(PlainteDTO));
            Assert.AreEqual(_objetcommun.Description, result.Value.Description);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<PlainteDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Description == _objetcommun.Description));
        }

        [TestMethod]
        public async Task PostAdresseTest_Entity()
        {
            PlainteCreateDTO dto = new PlainteCreateDTO
            {
                Description = "Nouvelle plainte de test",
                IdSignalement = 1,
                IdCompte = 1
            };

            var actionResult = await _controller.Post(dto);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdAdresse = (Plainte)created.Value;
            Assert.AreEqual(dto.Description, createdAdresse.Description);
        }


        [TestMethod]
        public async Task DeleteAdresseTest()
        {
            var result = await _controller.Delete(_objetcommun.IdPlainte);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedAdresse = await _manager.GetByIdAsync(_objetcommun.IdPlainte);
            Assert.IsNull(deletedAdresse);
        }

        [TestMethod]
        public async Task NotFoundDeleteAdresseTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutAdresseTest()
        {
            PlainteUpdateDTO dto = new PlainteUpdateDTO()
            {
                IdPlainte = _objetcommun.IdPlainte,
                Description = "Plainte modifiée",
                IdSignalement = 1,
                IdCompte = 1
            };

            var result = await _controller.Put(_objetcommun.IdPlainte, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var adresseput = await _manager.GetByIdAsync(_objetcommun.IdPlainte);
            Assert.AreEqual(dto.Description, adresseput.Description);
        }

        [TestMethod]
        public async Task NotFoundPutAdresseTest()
        {
            PlainteUpdateDTO dto = new PlainteUpdateDTO()
            {
                IdPlainte = _objetcommun.IdPlainte,
                Description = "Plainte modifiée",
                IdSignalement = 1,
                IdCompte = 1
            };
            var result = await _controller.Put(0, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutAdresseTest()
        {
            PlainteUpdateDTO dto = new PlainteUpdateDTO()
            {
                IdPlainte = _objetcommun.IdPlainte,
                Description = null,
                IdSignalement = 1,
                IdCompte = 1
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("Description", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdPlainte, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task BadRequestPostAdresseTest()
        {
            PlainteCreateDTO dto = new PlainteCreateDTO()
            {
                Description = null,
                IdSignalement = 1,
                IdCompte = 1
            };

            _controller.ModelState.AddModelError("Description", "Required");

            var actionResult = await _controller.Post(dto);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }
    }
}