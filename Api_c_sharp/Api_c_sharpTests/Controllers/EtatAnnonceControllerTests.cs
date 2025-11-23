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
    public class EtatAnnonceControllerTests
    {
        private EtatAnnonceController _controller;
        private AutoPulseBdContext _context;
        private EtatAnnonceManager _manager;
        private IMapper _mapper;
        private EtatAnnonce _objetcommun;

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

            _manager = new EtatAnnonceManager(_context);
            _controller = new EtatAnnonceController(_manager, _mapper);

            _context.TypesJournal.RemoveRange(_context.TypesJournal);
            await _context.SaveChangesAsync();

            var objet = new EtatAnnonce()
            {
                LibelleEtatAnnonce = "Test"
            };

            await _context.EtatAnnonces.AddAsync(objet);
            await _context.SaveChangesAsync();

            _objetcommun = objet;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetcommun.IdEtatAnnonce);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(EtatAnnonceDTO));
            Assert.AreEqual(_objetcommun.LibelleEtatAnnonce, result.Value.LibelleEtatAnnonce);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Act
            var result = await _controller.GetById(0);

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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<EtatAnnonceDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleEtatAnnonce == _objetcommun.LibelleEtatAnnonce));
        }
    }
}