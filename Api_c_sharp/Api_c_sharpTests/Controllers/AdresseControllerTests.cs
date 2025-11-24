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

            _context.Pays.Add(pays);
            await _context.SaveChangesAsync(); // important pour récupérer l'IdPays


            // 2. Création du compte
            var compte = new Compte
            {
                Pseudo = "testuser",
                MotDePasse = "Password123!",
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "jean.dupont@test.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = null,
                DateNaissance = new DateTime(2000, 1, 1),
                IdTypeCompte = 1 // à adapter selon ta table TypeCompte
            };

            _context.Comptes.Add(compte);
            await _context.SaveChangesAsync(); // pour récupérer l'IdCompte


            // 3. Création de l'adresse (avec les FKs)
            var objet = new Adresse
            {
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = pays.IdPays,         // clé étrangère vers Pays
                IdCompte = compte.IdCompte    // clé étrangère vers Compte
            };

            _context.Adresses.Add(objet);
            await _context.SaveChangesAsync();

            _objetcommun = objet;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdAdresse);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(VoitureDetailDTO));
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<VoitureDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Rue == _objetcommun.Rue));
        }

        [TestMethod]
        public async Task PostVoitureTest_Entity()
        {
            var adresse = new AdresseDTO()
            {
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
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
            var voiture = new VoitureCreateDTO()
            {
                IdVoiture = _objetcommun.IdVoiture,
                IdMarque = 1,
                IdMotricite = 1,
                IdCarburant = 1,
                IdBoiteDeVitesse = 1,
                IdCategorie = 1,
                Kilometrage = 15000,
                Annee = 2021,
                Puissance = 180,
                IdModele = 1,
                NbPlace = 5,
                NbPorte = 5,
            };

            var result = await _controller.Put(_objetcommun.IdVoiture, voiture);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var fetchedVoiture = await _manager.GetByIdAsync(_objetcommun.IdVoiture);
            Assert.AreEqual(voiture.Puissance, fetchedVoiture.Puissance);
        }

        [TestMethod]
        public async Task NotFoundPutVoitureTest()
        {
            var voiture = new VoitureCreateDTO()
            {
                IdVoiture = 0,
                Kilometrage = 10000
            };

            var result = await _controller.Put(0, voiture);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutVoitureTest()
        {
            // Arrange : voiture avec kilométrage invalide
            var voiture = new VoitureCreateDTO()
            {
                IdVoiture = _objetcommun.IdVoiture,
                Kilometrage = -20
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("Kilometrage", "Le kilométrage doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdVoiture, voiture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }


        [TestMethod]
        public async Task BadRequestPostVoitureTest()
        {
            var voiture = new VoitureCreateDTO
            {
                Kilometrage = 0
            };

            _controller.ModelState.AddModelError("Kilometrage", "Required");

            var actionResult = await _controller.Post(voiture);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

    }
}