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
    public class CommandeControllerTests
    {
        private Mock<CommandeManager> _mockManager;
        private IMapper _mapper;
        private Mock<IJournalService> _mockJournal;
        private CommandeController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<CommandeManager>(null);
            _mockJournal = new Mock<IJournalService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Commande, CommandeDTO>().ReverseMap();
                cfg.CreateMap<Commande, CommandeDetailDTO>().ReverseMap();
                cfg.CreateMap<CommandeCreateDTO, Commande>().ReverseMap();
                cfg.CreateMap<CommandeUpdateDTO, Commande>().ReverseMap();
            });

            _mapper = config.CreateMapper();
            _controller = new CommandeController(_mockManager.Object, _mapper, _mockJournal.Object);
        }

        // ---------------------------
        //       GET BY ID
        // ---------------------------
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            var entity = new Commande { IdCommande = 1, IdAcheteur = 2, IdVendeur = 3, IdAnnonce = 4, IdMoyenPaiement = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            var result = await _controller.GetByID(1);

            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CommandeDetailDTO));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);

            var result = await _controller.GetByID(1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // ---------------------------
        //       GET ALL
        // ---------------------------
        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            var data = new List<Commande>
            {
                new Commande { IdCommande = 1 },
                new Commande { IdCommande = 2 }
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
            var dto = new CommandeCreateDTO { IdAcheteur = 1, IdVendeur = 2, IdAnnonce = 3, IdMoyenPaiement = 1 };
            var entity = new Commande { IdCommande = 10, IdAcheteur = 1, IdVendeur = 2 };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Commande>())).ReturnsAsync(entity);

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");
            var dto = new CommandeCreateDTO();

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Commande>()), Times.Never);
        }

        // ---------------------------
        //       PUT
        // ---------------------------
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            var entity = new Commande { IdCommande = 1 };
            var dto = new CommandeUpdateDTO { IdCommande = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");
            var dto = new CommandeUpdateDTO { IdCommande = 1 };

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);
            var dto = new CommandeUpdateDTO { IdCommande = 1 };

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // ---------------------------
        //       DELETE
        // ---------------------------
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            var entity = new Commande { IdCommande = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            var result = await _controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);

            var result = await _controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // ---------------------------
        // GetCommandeByCompteID
        // ---------------------------
        [TestMethod]
        public async Task GetCommandeByCompteID_ReturnsList_WhenExists()
        {
            var data = new List<Commande> { new Commande { IdCommande = 1 } };

            _mockManager.Setup(m => m.GetCommandesByCompteId(1)).ReturnsAsync(data);

            var result = await _controller.GetCommandeByCompteID(1);

            Assert.IsNotNull(result.Value);
            Assert.AreEqual(1, result.Value.Count());
        }

        [TestMethod]
        public async Task GetCommandeByCompteID_ReturnsNotFound_WhenEmpty()
        {
            _mockManager.Setup(m => m.GetCommandesByCompteId(1)).ReturnsAsync(new List<Commande>());

            var result = await _controller.GetCommandeByCompteID(1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}
