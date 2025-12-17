using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Api_c_sharp.Models.Repository.Interfaces;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass]
    public class AvisControllerTests
    {
        private Mock<AvisManager> _mockManager;
        private Mock<IJournalService> _mockJournal;
        private IMapper _mapper;
        private AvisController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<AvisManager>(null);
            _mockJournal = new Mock<IJournalService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Avis, AvisDetailDTO>().ReverseMap();
                cfg.CreateMap<Avis, AvisListDTO>().ReverseMap();
                cfg.CreateMap<Avis, AvisCreateDTO>().ReverseMap();
                cfg.CreateMap<Avis, AvisUpdateDTO>().ReverseMap();
            });

            _mapper = config.CreateMapper();

            _controller = new AvisController(_mockManager.Object, _mapper, _mockJournal.Object);
        }

        // -----------------------------------------------------
        //                      GET BY ID
        // -----------------------------------------------------
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            var avis = new Avis
            {
                IdAvis = 1,
                IdJugee = 2,
                IdJugeur = 3,
                IdCommande = 4,
                ContenuAvis = "test",
                NoteAvis = 4
            };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(avis);

            var result = await _controller.GetByID(1);

            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(AvisDetailDTO));
            Assert.AreEqual(1, result.Value.IdAvis);
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync((Avis)null);

            var result = await _controller.GetByID(1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // -----------------------------------------------------
        //                      GET ALL
        // -----------------------------------------------------
        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            var data = new List<Avis>
            {
                new Avis { IdAvis = 1, ContenuAvis = "A", NoteAvis = 4 },
                new Avis { IdAvis = 2, ContenuAvis = "B", NoteAvis = 5 }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                        .ReturnsAsync(data);

            var result = await _controller.GetAll();

            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }

        // -----------------------------------------------------
        //                       POST
        // -----------------------------------------------------
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            var dto = new AvisCreateDTO
            {
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "test",
                NoteAvis = 4
            };

            var entity = new Avis
            {
                IdAvis = 10,
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "test",
                NoteAvis = 4
            };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Avis>()))
                        .ReturnsAsync(entity);

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new AvisCreateDTO();

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Avis>()), Times.Never);
        }

        // -----------------------------------------------------
        //                       PUT
        // -----------------------------------------------------
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            var existing = new Avis
            {
                IdAvis = 1,
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "old",
                NoteAvis = 2
            };

            var dto = new AvisUpdateDTO
            {
                IdAvis = 1,
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "updated",
                NoteAvis = 5
            };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(existing);

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new AvisUpdateDTO();

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Avis>(), It.IsAny<Avis>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync((Avis)null);

            var dto = new AvisUpdateDTO
            {
                IdAvis = 1,
                IdJugee = 1,
                IdJugeur = 2,
                IdCommande = 3,
                ContenuAvis = "updated",
                NoteAvis = 5
            };

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -----------------------------------------------------
        //                       DELETE
        // -----------------------------------------------------
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            var avis = new Avis { IdAvis = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(avis);

            var result = await _controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync((Avis)null);

            var result = await _controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -----------------------------------------------------
        //               GET AVIS BY COMPTE ID
        // -----------------------------------------------------
        [TestMethod]
        public async Task GetAvisByCompteID_ReturnsList_WhenExists()
        {
            var data = new List<Avis>
            {
                new Avis { IdAvis = 1, ContenuAvis = "A", NoteAvis = 4 },
                new Avis { IdAvis = 2, ContenuAvis = "B", NoteAvis = 5 }
            };

            _mockManager.Setup(m => m.GetAvisByCompteId(5))
                        .ReturnsAsync(data);

            var result = await _controller.GetAvisByCompteID(5);

            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task GetAvisByCompteID_ReturnsNotFound_WhenEmpty()
        {
            _mockManager.Setup(m => m.GetAvisByCompteId(5))
                        .ReturnsAsync(new List<Avis>());

            var result = await _controller.GetAvisByCompteID(5);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}
