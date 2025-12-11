using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass]
    public class FactureControllerTests
    {
        private Mock<FactureManager> _mockManager;
        private IMapper _mapper;
        private Mock<IJournalService> _mockJournal;
        private FactureController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<FactureManager>(null);
            _mockJournal = new Mock<IJournalService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Facture, FactureDTO>().ReverseMap();
            });

            _mapper = config.CreateMapper();
            _controller = new FactureController(_mockManager.Object, _mapper);
        }

        // ---------------------------
        //       GET BY ID
        // ---------------------------
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            var entity = new Facture { IdFacture = 1, IdCommande = 2};

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            var result = await _controller.GetByID(1);

            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(FactureDTO));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Facture)null);

            var result = await _controller.GetByID(1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // ---------------------------
        //       GET ALL
        // ---------------------------
        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            var data = new List<Facture>
            {
                new Facture { IdFacture = 1 },
                new Facture { IdFacture = 2 }
            };

            _mockManager.Setup(m => m.GetAllAsync()).ReturnsAsync(data);

            var result = await _controller.GetAll();

            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }

        // ---------------------------
        //       POST
        // ---------------------------
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            var dto = new FactureDTO { IdFacture = 1, IdCommande = 2};
            var entity = new Facture { IdFacture = 10, IdCommande = 1};

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Facture>())).ReturnsAsync(entity);

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");
            var dto = new FactureDTO();

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        // ---------------------------
        //       PUT
        // ---------------------------
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            var entity = new Facture { IdFacture = 1 };
            var dto = new FactureDTO { IdFacture = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");
            var dto = new FactureDTO { IdFacture = 5 };

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Facture)null);
            var dto = new FactureDTO { IdFacture = 1 };

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // ---------------------------
        //       DELETE
        // ---------------------------
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            var entity = new Facture { IdFacture = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            var result = await _controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Facture)null);

            var result = await _controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
