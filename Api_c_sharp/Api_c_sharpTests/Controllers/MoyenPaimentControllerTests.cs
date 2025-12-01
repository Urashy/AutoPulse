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
    public class MoyenPaimentControllerTests
    {
        private MoyenPaiementController _controller;
        private AutoPulseBdContext _context;
        private MoyenPaiementManager _manager;
        private IMapper _mapper;
        private MoyenPaiement _objetcommun;

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

            _manager = new MoyenPaiementManager(_context);
            _controller = new MoyenPaiementController(_manager, _mapper);

            _context.MoyensPaiements.RemoveRange(_context.MoyensPaiements);
            await _context.SaveChangesAsync();

            var objet = new MoyenPaiement()
            {
                TypePaiement = "Test"
            };

            await _context.MoyensPaiements.AddAsync(objet);
            await _context.SaveChangesAsync();

            _objetcommun = objet;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetcommun.IdMoyenPaiement);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(MoyenPaiementDTO));
            Assert.AreEqual(_objetcommun.TypePaiement, result.Value.TypePaiement);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<MoyenPaiementDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.TypePaiement == _objetcommun.TypePaiement));
        }
    }
}