using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using AutoPulseBdContext = Api_c_sharp.Models.Repository.AutoPulseBdContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Controllers.Tests
{
    [TestClass]
    public class BloqueControllerTests
    {
        private BloqueController _controller;
        private AutoPulseBdContext _context;
        private BloqueManager _manager;
        private IMapper _mapper;
        private Bloque _objetCommun;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Bloque_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _manager = new BloqueManager(_context);
            _controller = new BloqueController(_manager, _mapper);

            var compte1 = new Compte
            {
                IdCompte = 1,
                Pseudo = "User1",
                MotDePasse = "Pass1",
                Nom = "Nom1",
                Prenom = "Prenom1",
                Email = "user1@test.com",
                DateCreation = DateTime.UtcNow,
                DateDerniereConnexion = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = 1
            };

            var compte2 = new Compte
            {
                IdCompte = 2,
                Pseudo = "User2",
                MotDePasse = "Pass2",
                Nom = "Nom2",
                Prenom = "Prenom2",
                Email = "user2@test.com",
                DateCreation = DateTime.UtcNow,
                DateDerniereConnexion = DateTime.UtcNow,
                DateNaissance = new DateTime(1992, 1, 1),
                IdTypeCompte = 1
            };

            _context.Comptes.Add(compte1);
            _context.Comptes.Add(compte2);

            // Création d'entités nécessaires
            var entity = new Bloque()
            {
                IdBloquant = 1,
                IdBloque = 2,
                DateBloque = DateTime.UtcNow
            };

            

            await _context.Bloques.AddAsync(entity);
            await _context.SaveChangesAsync();

            _objetCommun = entity;
        }

        // -------------------------------------
        // GET BY ID
        // -------------------------------------

        [TestMethod]
        public async Task GetByID_Returns_OK()
        {
            var result = await _controller.GetByID(_objetCommun.IdBloquant, _objetCommun.IdBloque);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(BloqueDTO));
            Assert.AreEqual(_objetCommun.IdBloquant, result.Value.IdBloquant);
        }

        [TestMethod]
        public async Task GetByID_Returns_NotFound()
        {
            var result = await _controller.GetByID(111, 222);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // -------------------------------------
        // GET ALL
        // -------------------------------------

        [TestMethod]
        public async Task GetAll_Returns_Data()
        {
            var result = await _controller.GetAll();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);

            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<BloqueDTO>));

            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.IdBloque == _objetCommun.IdBloque));
        }

        // -------------------------------------
        // POST
        // -------------------------------------

        [TestMethod]
        public async Task Post_Creates_Entity()
        {
            var dto = new BloqueDTO()
            {
                IdBloquant = 3,
                IdBloque = 4,
            };

            var actionResult = await _controller.Post(dto);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            var entity = created.Value as Bloque;

            Assert.IsNotNull(entity);
            Assert.AreEqual(dto.IdBloquant, entity.IdBloquant);
            Assert.AreEqual(dto.IdBloque, entity.IdBloque);
        }

        [TestMethod]
        public async Task Post_BadRequest_When_ModelState_Invalid()
        {
            var dto = new BloqueDTO();

            _controller.ModelState.AddModelError("IdBloquant", "Required");

            var actionResult = await _controller.Post(dto);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        // -------------------------------------
        // DELETE
        // -------------------------------------

        [TestMethod]
        public async Task Delete_Returns_NoContent()
        {
            var result = await _controller.Delete(_objetCommun.IdBloquant, _objetCommun.IdBloque);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deleted = await _manager.GetByIdsAsync(_objetCommun.IdBloque, _objetCommun.IdBloquant);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task Delete_Returns_NotFound()
        {
            var result = await _controller.Delete(999, 888);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -------------------------------------
        // PUT
        // -------------------------------------

        [TestMethod]
        public async Task Put_Updates_Entity()
        {
            var dto = new BloqueDTO()
            {
                IdBloquant = _objetCommun.IdBloquant,
                IdBloque = _objetCommun.IdBloque
            };

            var result = await _controller.Put(_objetCommun.IdBloquant, _objetCommun.IdBloque, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updated = await _manager.GetByIdsAsync(_objetCommun.IdBloque, _objetCommun.IdBloquant);
            Assert.IsNotNull(updated);
        }

        [TestMethod]
        public async Task Put_Returns_BadRequest_When_Id_Mismatch()
        {
            var dto = new BloqueDTO()
            {
                IdBloquant = 1,
                IdBloque = 2
            };

            var result = await _controller.Put(5, 6, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Put_Returns_NotFound()
        {
            var dto = new BloqueDTO()
            {
                IdBloquant = 111,
                IdBloque = 222
            };

            var result = await _controller.Put(111, 222, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Put_Returns_BadRequest_When_ModelState_Invalid()
        {
            var dto = new BloqueDTO()
            {
                IdBloquant = _objetCommun.IdBloquant,
                IdBloque = _objetCommun.IdBloque
            };

            _controller.ModelState.AddModelError("IdBloquant", "Invalid");

            var result = await _controller.Put(_objetCommun.IdBloquant, _objetCommun.IdBloque, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
    }
}
