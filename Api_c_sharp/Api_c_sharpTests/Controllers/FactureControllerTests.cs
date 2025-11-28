using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Api_c_sharp.DTO;
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
    public class FactureControllerTests
    {
        private FactureController _controller;
        private AutoPulseBdContext _context;
        private FactureManager _manager;
        private IMapper _mapper;
        private Facture _objetcommun;

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

            _manager = new FactureManager(_context);
            _controller = new FactureController(_manager, _mapper);

            _context.Voitures.RemoveRange(_context.Voitures);
            await _context.SaveChangesAsync();


            await _context.SaveChangesAsync();

            var objet = new Facture()
            {
                IdCommande = int.Parse(null),
            };

            await _context.Factures.AddAsync(objet);
            await _context.SaveChangesAsync();

            _objetcommun = objet;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdFacture);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(FactureDTO));
            Assert.AreEqual(_objetcommun.IdFacture, result.Value.IdFacture);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<FactureDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.IdFacture == _objetcommun.IdFacture));
        }

        [TestMethod]
        public async Task PostVoitureTest_Entity()
        {
            var voiture = new FactureDTO
            {
                IdCommande = 1,
            };

            var actionResult = await _controller.Post(voiture);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdVoiture = (Facture)created.Value;
            Assert.AreEqual(voiture.IdCommande, createdVoiture.IdCommande);
        }


        [TestMethod]
        public async Task DeleteVoitureTest()
        {
            var result = await _controller.Delete(_objetcommun.IdFacture);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedVoiture = await _manager.GetByIdAsync(_objetcommun.IdFacture);
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
            var voiture = new FactureDTO()
            {
                IdCommande = 1,
            };

            var result = await _controller.Put(_objetcommun.IdFacture, voiture);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var fetchedVoiture = await _manager.GetByIdAsync(_objetcommun.IdFacture);
            Assert.AreEqual(voiture.IdCommande, fetchedVoiture.IdCommande);
        }

        [TestMethod]
        public async Task NotFoundPutVoitureTest()
        {
            var voiture = new FactureDTO()
            {
                IdCommande = 9999,
            };

            var result = await _controller.Put(0, voiture);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutVoitureTest()
        {
            // Arrange : voiture avec kilométrage invalide
            var voiture = new FactureDTO()
            {
                IdCommande = -20,
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("IdCommande", "Le IdCommande doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdFacture, voiture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }


        [TestMethod]
        public async Task BadRequestPostVoitureTest()
        {
            var voiture = new FactureDTO
            {
                IdCommande = int.Parse(null),
            };

            _controller.ModelState.AddModelError("IdCommande", "Required");

            var actionResult = await _controller.Post(voiture);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

    }
}