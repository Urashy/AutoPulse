using Api_c_sharp.Mapper;
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
using Api_c_sharp.Models.Entity;

namespace App.ControllersUnitaires.Tests
{
    [TestClass()]
    public class TypeJournalControllerTests
    {
        private TypeJournalController _controller;
        private AutoPulseBdContext _context;
        private TypeJournalManager _manager;
        private IMapper _mapper;
        private TypeJournal _objetcommun;

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

            _manager = new TypeJournalManager(_context);
            _controller = new TypeJournalController(_manager, _mapper);

            _context.TypesJournal.RemoveRange(_context.TypesJournal);
            await _context.SaveChangesAsync();

            var objet = new TypeJournal()
            {
                LibelleTypeJournaux = "TestTypeJournal"
            };

            await _context.TypesJournal.AddAsync(objet);
            await _context.SaveChangesAsync();

            // Initialisation de l'objet commun après l'avoir sauvegardé
            _objetcommun = objet;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetcommun.IdTypeJournaux);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(TypeJournalDTO));
            Assert.AreEqual(_objetcommun.LibelleTypeJournaux, result.Value.LibelleTypeJournaux);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<TypeJournalDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleTypeJournaux == _objetcommun.LibelleTypeJournaux));
        }
    }
}