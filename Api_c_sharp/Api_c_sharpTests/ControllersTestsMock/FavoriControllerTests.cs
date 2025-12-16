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
    public class FavoriControllerTests
    {
        private Mock<FavoriManager> _mockManager;
        private IMapper _mapper;
        private Mock<IJournalService> _mockJournal;
        private FavoriController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<FavoriManager>(null);
            _mockJournal = new Mock<IJournalService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Favori, FavoriDTO>().ReverseMap();
            });

            _mapper = config.CreateMapper();
            _controller = new FavoriController(_mockManager.Object, _mapper, _mockJournal.Object);
        }

        // ---------------------------
        //     GET BY IDS
        // ---------------------------
        [TestMethod]
        public async Task GetByIDS_ReturnsOk_WhenExists()
        {
            var entity = new Favori { IdCompte = 1, IdAnnonce = 2 };

            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 2))
                        .ReturnsAsync(entity);

            var result = await _controller.GetByIDS(1, 2);

            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(FavoriDTO));
        }

        [TestMethod]
        public async Task GetByIDS_ReturnsNotFound_WhenNotExists()
        {
            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 2))
                        .ReturnsAsync((Favori)null);

            var result = await _controller.GetByIDS(1, 2);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // ---------------------------
        //       GET ALL
        // ---------------------------
        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            var data = new List<Favori>
            {
                new Favori { IdCompte = 1 , IdAnnonce = 1 },
                new Favori { IdCompte = 2 , IdAnnonce = 2 }
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
            var dto = new FavoriDTO { IdCompte = 1, IdAnnonce = 2 };
            var entity = new Favori { IdCompte = 1, IdAnnonce = 2 };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Favori>()))
                        .ReturnsAsync(entity);

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");
            var dto = new FavoriDTO();

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Favori>()), Times.Never);
        }

        // ---------------------------
        //       PUT
        // ---------------------------
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            var existing = new Favori { IdCompte = 1, IdAnnonce = 1 };
            var dto = new FavoriDTO { IdCompte = 1, IdAnnonce = 1 };

            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 1))
                        .ReturnsAsync(existing);

            _mockManager.Setup(m => m.UpdateAsync(existing, It.IsAny<Favori>()))
                        .Returns(Task.CompletedTask);

            var result = await _controller.Put(1, 1, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");
            var dto = new FavoriDTO { IdAnnonce = 1 };

            var result = await _controller.Put(1, 1, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Favori>(), It.IsAny<Favori>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 1))
                        .ReturnsAsync((Favori)null);

            var dto = new FavoriDTO { IdCompte = 1, IdAnnonce = 1 };

            var result = await _controller.Put(1, 1, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // ---------------------------
        //       DELETE
        // ---------------------------
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            var existing = new Favori { IdCompte = 1, IdAnnonce = 1 };

            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 1))
                        .ReturnsAsync(existing);

            _mockManager.Setup(m => m.DeleteAsync(existing))
                        .Returns(Task.CompletedTask);

            var result = await _controller.Delete(1, 1);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            _mockManager.Setup(m => m.GetFavoriByIdsAsync(1, 1))
                        .ReturnsAsync((Favori)null);

            var result = await _controller.Delete(1, 1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
