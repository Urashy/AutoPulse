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
    public class BoiteDeVitesseControllerTests
    {
        private BoiteDeVitesseController _controller;
        private AutoPulseBdContext _context;
        private BoiteDeVitesseManager _manager;
        private IMapper _mapper;
        private BoiteDeVitesse _objetcommun;

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

            _manager = new BoiteDeVitesseManager(_context);
            _controller = new BoiteDeVitesseController(_manager, _mapper);

            _context.BoitesDeVitesses.RemoveRange(_context.BoitesDeVitesses);
            await _context.SaveChangesAsync();

            var objet = new BoiteDeVitesse()
            {
                LibelleBoite = "Test"
            };

            await _context.BoitesDeVitesses.AddAsync(objet);
            await _context.SaveChangesAsync();

            _objetcommun = objet;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetcommun.IdBoiteDeVitesse);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(BoiteDeVitesseDTO));
            Assert.AreEqual(_objetcommun.LibelleBoite, result.Value.LibelleBoite);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<BoiteDeVitesseDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleBoite == _objetcommun.LibelleBoite));
        }
    }
}