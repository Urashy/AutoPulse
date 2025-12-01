using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
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

namespace App.Controllers.Tests
{
    [TestClass()]
    public class VoitureControllerTests
    {
        private VoitureController _controller;
        private AutoPulseBdContext _context;
        private VoitureManager _manager;
        private IMapper _mapper;
        private Voiture _objetcommun;

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

            _manager = new VoitureManager(_context);
            _controller = new VoitureController(_manager, _mapper);

            _context.Voitures.RemoveRange(_context.Voitures);
            await _context.SaveChangesAsync();

            _context.Marques.Add(new Marque { IdMarque = 1, LibelleMarque = "TestMarque" });
            _context.Motricites.Add(new Motricite { IdMotricite = 1, LibelleMotricite = "4x4" });
            _context.Carburants.Add(new Carburant { IdCarburant = 1, LibelleCarburant = "Essence" });
            _context.BoitesDeVitesses.Add(new BoiteDeVitesse { IdBoiteDeVitesse = 1, LibelleBoite = "Manuelle" });
            _context.Categories.Add(new Categorie { IdCategorie = 1, LibelleCategorie = "SUV" });
            _context.Modeles.Add(new Modele { IdModele = 1, LibelleModele = "Modele Test" });

            await _context.SaveChangesAsync();

            var objet = new Voiture()
            {
                IdMarque = 1,
                IdMotricite = 1,
                IdCarburant = 1,
                IdBoiteDeVitesse = 1,
                IdCategorie = 1,
                Kilometrage = 10000,
                Annee = 2020,
                Puissance = 150,
                MiseEnCirculation = DateTime.Now,
                IdModele = 1,
                NbPlace = 5,
                NbPorte = 5
            };

            await _context.Voitures.AddAsync(objet);
            await _context.SaveChangesAsync();

            _objetcommun = objet;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdBoiteDeVitesse);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(VoitureDetailDTO));
            Assert.AreEqual(_objetcommun.Puissance, result.Value.Puissance);
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
            Assert.IsTrue(result.Value.Any(o => o.Kilometrage == _objetcommun.Kilometrage));
        }

        [TestMethod]
        public async Task PostVoitureTest_Entity()
        {
            var voiture = new VoitureCreateDTO
            {
                IdMarque = 1,
                IdMotricite = 1,
                IdCarburant = 1,
                IdBoiteDeVitesse = 1,
                IdCategorie = 1,
                Kilometrage = 12000,
                Annee = 2022,
                Puissance = 140,
                MiseEnCirculation = DateTime.Now,
                IdModele = 1,
                NbPlace = 5,
                NbPorte = 5
            };

            var actionResult = await _controller.Post(voiture);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdVoiture = (Voiture)created.Value;
            Assert.AreEqual(voiture.Kilometrage, createdVoiture.Kilometrage);
        }


        [TestMethod]
        public async Task DeleteVoitureTest()
        {
            var result = await _controller.Delete(_objetcommun.IdVoiture);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedVoiture = await _manager.GetByIdAsync(_objetcommun.IdVoiture);
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