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
using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;

namespace App.Controllers.Tests
{
    [TestClass()]
    public class MiseEnAvantControllerTests
    {
        private MiseEnAvantController _controller;
        private AutoPulseBdContext _context;
        private MiseEnAvantManager _manager;
        private IMapper _mapper;
        private MiseEnAvant _objetcommun;

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

            _manager = new MiseEnAvantManager(_context);
            _controller = new MiseEnAvantController(_manager, _mapper);

            _context.MisesEnAvant.RemoveRange(_context.MisesEnAvant);
            await _context.SaveChangesAsync();

            var objet = new MiseEnAvant()
            {
                LibelleMiseEnAvant = "Test"
            };

            await _context.MisesEnAvant.AddAsync(objet);
            await _context.SaveChangesAsync();

            _objetcommun = objet;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetcommun.IdMiseEnAvant);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(MiseEnAvantDTO));
            Assert.AreEqual(_objetcommun.LibelleMiseEnAvant, result.Value.LibelleMiseEnAvant);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<MiseEnAvantDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleMiseEnAvant == _objetcommun.LibelleMiseEnAvant));
        }
    }
}