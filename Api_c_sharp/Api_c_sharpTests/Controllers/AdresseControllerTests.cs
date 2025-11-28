using Api_c_sharp.Controllers;
using Api_c_sharp.DTO;
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
    public class AdresseControllerTests
    {
        private AdresseController _controller;
        private AutoPulseBdContext _context;
        private AdresseManager _manager;
        private IMapper _mapper;
        private Adresse _objetcommun;

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

            _manager = new AdresseManager(_context);
            _controller = new AdresseController(_manager, _mapper);

            _context.Adresses.RemoveRange(_context.Adresses);
            await _context.SaveChangesAsync();

            var pays = new Pays
            {
                Libelle = "France"
            };

            var typeCompte = new TypeCompte
            {
                Libelle = "Utilisateur"
            };
            var compte = new Compte
            {
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

            var adresse = new Adresse
            {
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = 1,
                IdCompte = 1
            };
            _context.TypesCompte.Add(typeCompte);
            _context.Pays.Add(pays);
            _context.Comptes.Add(compte);
            _context.Adresses.Add(adresse);
            await _context.SaveChangesAsync(); 

            _objetcommun = adresse;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdAdresse);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(AdresseDTO));
            Assert.AreEqual(_objetcommun.Rue, result.Value.Rue);
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