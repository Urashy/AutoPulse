using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass]
    public class APourConversationControllerTests
    {
        private Mock<APourConversationManager> _mockManager;
        private IMapper _mapper;
        private APourConversationController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<APourConversationManager>(null);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<APourConversationDTO, APourConversation>().ReverseMap();
            });

            _mapper = config.CreateMapper();

            _controller = new APourConversationController(_mockManager.Object, _mapper);
        }


        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            var entity = new APourConversation { IdCompte = 1, IdConversation = 2 };
            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync(entity);

            var result = await _controller.GetByID(2, 1);

            Assert.IsInstanceOfType(result.Value, typeof(APourConversationDTO));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync((APourConversation)null);

            var result = await _controller.GetByID(2, 1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }


        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            var data = new List<APourConversation>
            {
                new APourConversation { IdCompte = 1, IdConversation = 10 },
                new APourConversation { IdCompte = 2, IdConversation = 20 }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                        .ReturnsAsync(data);

            var result = await _controller.GetAll();

            Assert.IsNotNull(result.Value);
            var list = result.Value;

            Assert.AreEqual(2, ((List<APourConversationDTO>)list).Count);
        }

        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            var dto = new APourConversationDTO { IdCompte = 1, IdConversation = 2 };

            var entity = new APourConversation { IdCompte = 1, IdConversation = 2 };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<APourConversation>()))
                        .ReturnsAsync(entity);

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new APourConversationDTO();

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            var dto = new APourConversationDTO { IdCompte = 1, IdConversation = 2 };

            var entity = new APourConversation { IdCompte = 1, IdConversation = 2 };

            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync(entity);

            var result = await _controller.Put(2, 1, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new APourConversationDTO();

            var result = await _controller.Put(2, 1, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync((APourConversation)null);

            var dto = new APourConversationDTO { IdCompte = 1, IdConversation = 2 };

            var result = await _controller.Put(2, 1, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            var entity = new APourConversation { IdCompte = 1, IdConversation = 2 };

            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync(entity);

            var result = await _controller.Delete(2, 1);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync((APourConversation)null);

            var result = await _controller.Delete(2, 1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
