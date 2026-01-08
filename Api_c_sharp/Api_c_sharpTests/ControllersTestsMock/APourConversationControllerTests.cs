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
            // Création des mocks
            _mockManager = new Mock<APourConversationManager>(null);

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<APourConversationDTO, APourConversation>().ReverseMap();
            });

            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new APourConversationController(_mockManager.Object, _mapper);
        }

        #region GET
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            var entity = new APourConversation { IdCompte = 1, IdConversation = 2 };
            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync(entity);
            // Act
            var result = await _controller.GetById(2, 1);

            // Assert
            Assert.IsInstanceOfType(result.Value, typeof(APourConversationDTO));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync((APourConversation)null);
            
            // Act
            var result = await _controller.GetById(2, 1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }


        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            // Arrange
            var data = new List<APourConversation>
            {
                new APourConversation { IdCompte = 1, IdConversation = 10 },
                new APourConversation { IdCompte = 2, IdConversation = 20 }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                        .ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            var list = result.Value;

            Assert.AreEqual(2, ((List<APourConversationDTO>)list).Count);
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            // Arrange
            var dto = new APourConversationDTO { IdCompte = 1, IdConversation = 2 };

            var entity = new APourConversation { IdCompte = 1, IdConversation = 2 };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<APourConversation>()))
                        .ReturnsAsync(entity);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new APourConversationDTO();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<APourConversation>()), Times.Never);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var dto = new APourConversationDTO { IdCompte = 1, IdConversation = 2 };

            var entity = new APourConversation { IdCompte = 1, IdConversation = 2 };

            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync(entity);

            // Act
            var result = await _controller.Put(2, 1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new APourConversationDTO();

            // Act
            var result = await _controller.Put(2, 1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<APourConversation>(), It.IsAny<APourConversation>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            // Arrrange
            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync((APourConversation)null);

            var dto = new APourConversationDTO { IdCompte = 1, IdConversation = 2 };

            // Act
            var result = await _controller.Put(2, 1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var entity = new APourConversation { IdCompte = 1, IdConversation = 2 };

            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync(entity);
            // Act
            var result = await _controller.Delete(2, 1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAPourConversationByIDS(1, 2))
                        .ReturnsAsync((APourConversation)null);

            // Act
            var result = await _controller.Delete(2, 1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region EXISTS

        [TestMethod]
        public async  Task ExistsTrueTest()
        {
            var conv = new Conversation { IdAnnonce = 1 , IdConversation = 1};
            var entity = new APourConversation { IdCompte = 1, IdConversation = 2 };
            var entity2 = new APourConversation { IdCompte = 2, IdConversation = 2 };

            _mockManager.Setup(m => m.Exists(1, 2,1))
            .ReturnsAsync(true);
            var result = await _controller.Exists(1, 2, 1);
            Assert.IsTrue(result.Value);
        }

        [TestMethod]
        public async Task ExistsFalseTest()
        {
            var conv = new Conversation { IdAnnonce = 1, IdConversation = 1 };
            var entity = new APourConversation { IdCompte = 1, IdConversation = 2 };
            var entity2 = new APourConversation { IdCompte = 2, IdConversation = 2 };

            _mockManager.Setup(m => m.Exists(1, 3, 1))
            .ReturnsAsync(false);
            var result = await _controller.Exists(1, 3, 1);
            Assert.IsFalse(result.Value);
        }


        [TestMethod]
        public async Task ExistsAucuneConversationTest()
        {

            _mockManager.Setup(m => m.Exists(1, 3, 2))
            .ReturnsAsync(false);

            var result = await _controller.Exists(1, 3, 2);

            Assert.IsFalse(result.Value);
        }

        #endregion
    }
}
