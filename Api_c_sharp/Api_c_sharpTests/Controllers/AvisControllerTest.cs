using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using App.Controllers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace App.Controllers.Tests
{
    [TestClass]
    public class AvisControllerTests
    {
        private AvisController _controller;
        private AutoPulseBdContext _context;
        private AvisManager _manager;
        private IMapper _mapper;
        private Avis _objetCommun;
        private IJournalService _journalService;

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

            _journalService = new JournalManager(_context, NullLogger<JournalManager>.Instance);
            _manager = new AvisManager(_context);
            _controller = new AvisController(_manager, _mapper,_journalService);

            // Nettoyage
            _context.Avis.RemoveRange(_context.Avis);
            await _context.SaveChangesAsync();

            // Création d’un avis commun
            var avis = new Avis()
            {
                IdAvis = 1,
                IdJugee = 1,
                IdCommande = 1,
                ContenuAvis = "Trés bonne avis",
                DateAvis = DateTime.Now,
                NoteAvis = 5
            };

            await _context.Avis.AddAsync(avis);
            await _context.SaveChangesAsync();

            _objetCommun = avis;
        }

        // -------------------------------------------------------------
        // GET BY ID
        // -------------------------------------------------------------
        [TestMethod]
        public async Task GetByIdTest()
        {
            var result = await _controller.GetByID(_objetCommun.IdAvis);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(AvisDetailDTO));
            Assert.AreEqual(_objetCommun.NoteAvis, result.Value.NoteAvis);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            var result = await _controller.GetByID(0);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // -------------------------------------------------------------
        // GET ALL
        // -------------------------------------------------------------
        [TestMethod]
        public async Task GetAllTest()
        {
            var result = await _controller.GetAll();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AvisListDTO>));
            Assert.IsTrue(result.Value.Any());
        }

        // -------------------------------------------------------------
        // POST
        // -------------------------------------------------------------
        [TestMethod]
        public async Task PostAvisTest()
        {
            var dto = new AvisCreateDTO()
            {
                IdJugee = 2,
                IdCommande = 3,
                ContenuAvis = "Correct",
                NoteAvis = 3
            };

            var actionResult = await _controller.Post(dto);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdAvis = (Avis)created.Value;
            Assert.AreEqual(dto.ContenuAvis, createdAvis.ContenuAvis);
        }

        [TestMethod]
        public async Task BadRequestPostAvisTest()
        {
            var dto = new AvisCreateDTO();
            _controller.ModelState.AddModelError("Commentaire", "Required");

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        // -------------------------------------------------------------
        // DELETE
        // -------------------------------------------------------------
        [TestMethod]
        public async Task DeleteAvisTest()
        {
            var result = await _controller.Delete(_objetCommun.IdAvis);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var verify = await _manager.GetByIdAsync(_objetCommun.IdAvis);
            Assert.IsNull(verify);
        }

        [TestMethod]
        public async Task NotFoundDeleteAvisTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -------------------------------------------------------------
        // PUT
        // -------------------------------------------------------------
        [TestMethod]
        public async Task PutAvisTest()
        {
            var dto = new AvisCreateDTO()
            {
                IdAvis = _objetCommun.IdAvis,
                IdJugee = 1,
                IdCommande = 1,
                ContenuAvis = "Modifié",
                NoteAvis = 4
            };

            var result = await _controller.Put(_objetCommun.IdAvis, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updated = await _manager.GetByIdAsync(_objetCommun.IdAvis);
            Assert.AreEqual(dto.ContenuAvis, updated.ContenuAvis);
        }

        [TestMethod]
        public async Task NotFoundPutAvisTest()
        {
            var dto = new AvisCreateDTO()
            {
                ContenuAvis = "Test"
            };

            var result = await _controller.Put(0, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutAvisTest()
        {
            var dto = new AvisCreateDTO()
            {
                IdAvis = _objetCommun.IdAvis,
                ContenuAvis = "",
            };

            _controller.ModelState.AddModelError("Commentaire", "Invalid");

            var result = await _controller.Put(_objetCommun.IdAvis, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }


        [TestMethod]
        public async Task GetAllByCompte()
        {
            var result = await _controller.GetAvisByCompteID(_objetCommun.IdJugee);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AvisListDTO>));
            Assert.IsTrue(result.Value.Any());
        }
    }
}
