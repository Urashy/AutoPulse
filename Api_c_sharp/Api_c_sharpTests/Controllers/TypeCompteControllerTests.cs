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
    public class TypeCompteControllerTests
    {
        private TypeCompteController _controller;
        private AutoPulseBdContext _context;
        private TypeCompteManager _manager;
        private IMapper _mapper;
        private TypeCompte _objetcommun;

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

            _manager = new TypeCompteManager(_context);
            _controller = new TypeCompteController(_manager, _mapper);

            _context.TypesJournal.RemoveRange(_context.TypesJournal);
            await _context.SaveChangesAsync();

            var objet = new TypeCompte()
            {
                Libelle = "TestTypeCompte"
            };

            await _context.TypesCompte.AddAsync(objet);
            await _context.SaveChangesAsync();

            // Initialisation de l'objet commun après l'avoir sauvegardé
            _objetcommun = objet;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetcommun.IdTypeCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(TypeCompteDTO));
            Assert.AreEqual(_objetcommun.Libelle, result.Value.Libelle);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<TypeCompteDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
        }
    }
}