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

namespace App.ControllersUnitaires.Tests
{
    [TestClass()]
    public class ModeleControllerTests
    {
        private ModeleController _controller;
        private AutoPulseBdContext _context;
        private ModeleManager _manager;
        private IMapper _mapper;
        private Modele _objetcommun;

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

            _manager = new ModeleManager(_context);
            _controller = new ModeleController(_manager, _mapper);

            _context.Marques.RemoveRange(_context.Marques);
            await _context.SaveChangesAsync();

            Marque marque = new Marque()
            {
                IdMarque = 1,
                LibelleMarque = "Test"
            };

            Modele modele = new Modele()
            {
                IdModele = 1,
                LibelleModele = "ModTest",
                IdMarque = marque.IdMarque
            };

            await _context.Marques.AddAsync(marque);
            await _context.Modeles.AddAsync(modele);
            await _context.SaveChangesAsync();

            _objetcommun = modele;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.Get(_objetcommun.IdMarque);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(ModeleDTO));
            Assert.AreEqual(_objetcommun.LibelleModele, result.Value.LibelleModele);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Act
            var result = await _controller.Get(0);

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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<ModeleDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleModele == _objetcommun.LibelleModele));
        }

        [TestMethod]
        public async Task GetModelesByMarqueIdTest()
        {
            // Act
            var result = await _controller.GetAllByMarque(_objetcommun.IdMarque);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<ModeleDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleModele == _objetcommun.LibelleModele));
        }

        [TestMethod]
        public async Task NotFoundGetModelesByMarqueIdTest()
        {
            // Act
            var result = await _controller.GetAllByMarque(0);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}