using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
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
using Api_c_sharp.Models.Entity;
using Microsoft.Extensions.Configuration;

namespace App.Controllers.Tests
{
    [TestClass()]
    public class CompteControllerTests
    {
        private CompteController _controller;
        private AutoPulseBdContext _context;
        private CompteManager _manager;
        private IMapper _mapper;
        private Compte _objetcommun;
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            var mapperconfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = mapperconfig.CreateMapper();

            _manager = new CompteManager(_context);
            _controller = new CompteController(_manager, _mapper,config);

            _context.Comptes.RemoveRange(_context.Comptes);
            await _context.SaveChangesAsync();
            
            TypeCompte typeCompte = new TypeCompte
            {
                IdTypeCompte = 1,
                Libelle = "Standard"
            };

            Compte compte = new Compte
            {
                IdCompte = 1,
                Nom = "Doe",
                Prenom = "John",
                Email = "john@gmail.com",
                MotDePasse = "hashedpassword",
                Pseudo = "johndoe",
                DateCreation = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = typeCompte.IdTypeCompte,
                DateDerniereConnexion = DateTime.UtcNow
            };

            await _context.TypesCompte.AddAsync(typeCompte);
            await _context.Comptes.AddAsync(compte);
            await _context.SaveChangesAsync();

            _objetcommun = compte;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CompteDetailDTO));
            Assert.AreEqual(_objetcommun.Nom, result.Value.Nom);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CompteGetDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Pseudo == _objetcommun.Pseudo));
        }

        [TestMethod]
        public async Task PostAdresseTest_Entity()
        {
            CompteCreateDTO compteCreateDTO = new CompteCreateDTO
            {
                Nom = "Smith",
                Prenom = "Jane",
                Email = "jane.smith@gmail.com",
                MotDePasse = "anotherhashedpassword",
                Pseudo = "janesmith",
                DateNaissance = new DateTime(1992, 2, 2),
                IdTypeCompte = 1,
            };

            var actionResult = await _controller.Post(compteCreateDTO);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdcompte = (Compte)created.Value;
            Assert.AreEqual(compteCreateDTO.Email, createdcompte.Email);
        }


        [TestMethod]
        public async Task DeleteAdresseTest()
        {
            var result = await _controller.Delete(_objetcommun.IdCompte);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedAdresse = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.IsNull(deletedAdresse);
        }

        [TestMethod]
        public async Task NotFoundDeleteAdresseTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutAdresseTest()
        {
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = "JohnUpdated",
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
             };

            var result = await _controller.Put(_objetcommun.IdCompte, compteUpdateDTO);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var compteput = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(compteUpdateDTO.Nom, compteput.Nom);
        }

        [TestMethod]
        public async Task NotFoundPutAdresseTest()
        {
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = "JohnUpdated",
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
            };

            var result = await _controller.Put(0, compteUpdateDTO);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutAdresseTest()
        {
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = null,
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("Prenom", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdCompte, compteUpdateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task BadRequestPostAdresseTest()
        {
            CompteCreateDTO compteUpdateDTO = new CompteCreateDTO
            {
                Nom = "DoeUpdated",
                Prenom = "john",
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
                MotDePasse = "hashedpassword",
                Pseudo = "johndoe",
                NumeroSiret = null, 
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("NumeroSiret", "Required");

            var actionResult = await _controller.Post(compteUpdateDTO);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

    }
}