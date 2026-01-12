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
        private Mock<AnnonceManager> _mockManagerannonce;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<CommandeManager>(null);
            _mockManagerannonce = new Mock<AnnonceManager>(null);
            _mockJournal = new Mock<IJournalService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Commande, CommandeDTO>().ReverseMap();
                cfg.CreateMap<Commande, CommandeDetailDTO>().ReverseMap();
                cfg.CreateMap<CommandeCreateDTO, Commande>().ReverseMap();
                cfg.CreateMap<CommandeUpdateDTO, Commande>().ReverseMap();
            });

            _mapper = config.CreateMapper();
            _controller = new CommandeController(_mockManager.Object, _mapper, _mockJournal.Object, _mockManagerannonce.Object);
        }

        #region GET
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            var entity = new Commande { IdCommande = 1, IdAcheteur = 2, IdVendeur = 3, IdAnnonce = 4, IdMoyenPaiement = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            // Act
            var result = await _controller.GetByID(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CommandeDetailDTO));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);

            // Act
            var result = await _controller.GetByID(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            // Arrange
            var data = new List<Commande>
            {
                new Commande { IdCommande = 1 },
                new Commande { IdCommande = 2 }
            };

            _mockManager.Setup(m => m.GetAllAsync()).ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }

        #region GetCommandeByConversationID Tests
        [TestMethod]
        public async Task GetCommandeByConversationID_ReturnsOk_WhenExists()
        {
            // Arrange
            var commande = new Commande
            {
                IdCommande = 5,
                IdAcheteur = 2,
                IdVendeur = 3,
                IdAnnonce = 4,
                IdMoyenPaiement = 1,
                IdEtatCommande = 1,
                Date = DateTime.Now
            };

            _mockManager.Setup(m => m.GetCommandeByConversation(1))
                       .ReturnsAsync(commande)
                       .Verifiable();

            // Act
            var result = await _controller.GetCommandeByConversationID(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CommandeDTO));
            Assert.AreEqual(commande.IdCommande, result.Value.IdCommande);
            Assert.AreEqual(commande.IdEtatCommande, result.Value.IdEtatCommande);
            _mockManager.Verify(m => m.GetCommandeByConversation(1), Times.Once);
        }

        [TestMethod]
        public async Task GetCommandeByConversationID_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetCommandeByConversation(999))
                       .ReturnsAsync((Commande)null)
                       .Verifiable();

            // Act
            var result = await _controller.GetCommandeByConversationID(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockManager.Verify(m => m.GetCommandeByConversation(999), Times.Once);
        }
        #endregion
        #endregion

        #region POST
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            // Arrange
            var dto = new CommandeCreateDTO { IdAcheteur = 1, IdVendeur = 2, IdAnnonce = 3, IdMoyenPaiement = 1 };
            var entity = new Commande { IdCommande = 10, IdAcheteur = 1, IdVendeur = 2 };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Commande>())).ReturnsAsync(entity);

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
            var dto = new CommandeCreateDTO();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Commande>()), Times.Never);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var entity = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 1,
                IdAnnonce = 1
            };
            var dto = new CommandeUpdateDTO
            {
                IdCommande = 1,
                IdEtatCommande = 2,
                IdAnnonce = 1,
                IdAcheteur = 1,
                IdVendeur = 2
            };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()), Times.Once);
        }

        [TestMethod]
        public async Task Put_UpdatesAnnonce_WhenEtatCommandeIs5()
        {
            // Arrange
            var annonce = new Annonce { IdAnnonce = 1, IdEtatAnnonce = 1 };
            var entity = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 1,
                IdAnnonce = 1
            };
            var dto = new CommandeUpdateDTO
            {
                IdCommande = 1,
                IdEtatCommande = 5,
                IdAnnonce = 1,
                IdAcheteur = 1,
                IdVendeur = 2
            };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockManagerannonce.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(annonce);
            _mockManagerannonce.Setup(m => m.UpdateAsync(It.IsAny<Annonce>(), It.Is<Annonce>(a => a.IdEtatAnnonce == 2)))
                              .Returns(Task.CompletedTask)
                              .Verifiable();
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManagerannonce.Verify(m => m.GetByIdAsync(1), Times.Once);
            _mockManagerannonce.Verify(m => m.UpdateAsync(It.IsAny<Annonce>(), It.Is<Annonce>(a => a.IdEtatAnnonce == 2)), Times.Once);
        }

        [TestMethod]
        public async Task Put_DoesNotUpdateAnnonce_WhenEtatCommandeIsNot5()
        {
            // Arrange
            var entity = new Commande
            {
                IdCommande = 1,
                IdEtatCommande = 1,
                IdAnnonce = 1
            };
            var dto = new CommandeUpdateDTO
            {
                IdCommande = 1,
                IdEtatCommande = 3,
                IdAnnonce = 1,
                IdAcheteur = 1,
                IdVendeur = 2
            };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManagerannonce.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManagerannonce.Verify(m => m.UpdateAsync(It.IsAny<Annonce>(), It.IsAny<Annonce>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Invalid Model");
            var dto = new CommandeUpdateDTO { IdCommande = 1 };

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);
            var dto = new CommandeUpdateDTO { IdCommande = 1 };

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Commande>(), It.IsAny<Commande>()), Times.Never);
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var entity = new Commande { IdCommande = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region GETByCommande
        [TestMethod]
        public async Task GetCommandeByCompteID_ReturnsList_WhenExists()
        {
            // Arrange
            var data = new List<Commande> { new Commande { IdCommande = 1 } };

            _mockManager.Setup(m => m.GetCommandesByCompteId(1)).ReturnsAsync(data);

            // Act
            var result = await _controller.GetCommandeByCompteID(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(1, result.Value.Count());
        }

        [TestMethod]
        public async Task GetCommandeByCompteID_ReturnsNotFound_WhenEmpty()
        {
            // Arrange
            _mockManager.Setup(m => m.GetCommandesByCompteId(1)).ReturnsAsync(new List<Commande>());

            // Act
            var result = await _controller.GetCommandeByCompteID(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion
    }
}