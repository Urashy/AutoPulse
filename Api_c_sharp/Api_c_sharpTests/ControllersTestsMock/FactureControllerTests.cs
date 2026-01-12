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
        private Mock<CommandeManager> _mockCommandeManager;
        private IMapper _mapper;
        private Mock<IJournalService> _mockJournal;
        private FactureController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<FactureManager>(null);
            _mockCommandeManager = new Mock<CommandeManager>(null);
            _mockJournal = new Mock<IJournalService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Facture, FactureDTO>().ReverseMap();
            });

            _mapper = config.CreateMapper();
            _controller = new FactureController(_mockManager.Object, _mapper,_mockCommandeManager.Object);
        }

        #region GET
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            var entity = new Facture { IdFacture = 1, IdCommande = 2};

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            // Act
            var result = await _controller.GetByID(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(FactureDTO));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Facture)null);

            // Act
            var result = await _controller.GetByID(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        
        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            // Arrange
            var data = new List<Facture>
            {
                new Facture { IdFacture = 1 },
                new Facture { IdFacture = 2 }
            };

            _mockManager.Setup(m => m.GetAllAsync()).ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            // Arrange
            var dto = new FactureDTO { IdFacture = 1, IdCommande = 2};
            var entity = new Facture { IdFacture = 10, IdCommande = 1};

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Facture>())).ReturnsAsync(entity);

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
            var dto = new FactureDTO();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Facture>()), Times.Never);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var entity = new Facture { IdFacture = 1 };
            var dto = new FactureDTO { IdFacture = 1 };

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(entity);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("x", "invalid");
            var dto = new FactureDTO { IdFacture = 5 };

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Facture>(), It.IsAny<Facture>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Facture)null);
            var dto = new FactureDTO { IdFacture = 1 };

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            // Arrange
            var entity = new Facture { IdFacture = 1 };

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
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Facture)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        // Ajoutez ces tests à la fin de votre classe FactureControllerTests (fichier avec mocks)
        // Juste avant la dernière accolade fermante de la classe

        #region GetFacturePdf

        [TestMethod]
        public async Task GetFacturePdf_ReturnsFileResult_WhenCommandeExists()
        {
            // Arrange
            var commande = new Commande { IdCommande = 1 };
            var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF en ASCII

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(1)).Returns(fakePdfBytes);

            // Act
            var result = await _controller.GetFacturePdf(1, download: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FileContentResult));

            var fileResult = result as FileContentResult;
            Assert.AreEqual("application/pdf", fileResult.ContentType);
            Assert.AreEqual("Facture_Commande_1.pdf", fileResult.FileDownloadName);
            Assert.AreEqual(fakePdfBytes.Length, fileResult.FileContents.Length);
        }

        [TestMethod]
        public async Task GetFacturePdf_ReturnsFileResult_WithoutDownload()
        {
            // Arrange
            var commande = new Commande { IdCommande = 1 };
            var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(1)).Returns(fakePdfBytes);

            // Act
            var result = await _controller.GetFacturePdf(1, download: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FileContentResult));

            var fileResult = result as FileContentResult;
            Assert.AreEqual("application/pdf", fileResult.ContentType);
            Assert.AreEqual(fileResult.FileDownloadName,"");
        }

        [TestMethod]
        public async Task GetFacturePdf_ReturnsNotFound_WhenCommandeDoesNotExist()
        {
            // Arrange
            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);

            // Act
            var result = await _controller.GetFacturePdf(1, download: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Commande introuvable", notFoundResult.Value);

            _mockManager.Verify(m => m.GenererPdfFactureParCommande(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task GetFacturePdf_ReturnsNotFound_WhenPdfGenerationFails()
        {
            // Arrange
            var commande = new Commande { IdCommande = 1 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(1)).Returns((byte[])null);

            // Act
            var result = await _controller.GetFacturePdf(1, download: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Impossible de générer la facture", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetFacturePdf_CallsCorrectMethods()
        {
            // Arrange
            var commande = new Commande { IdCommande = 42 };
            var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(42)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(42)).Returns(fakePdfBytes);

            // Act
            await _controller.GetFacturePdf(42, download: true);

            // Assert
            _mockCommandeManager.Verify(m => m.GetByIdAsync(42), Times.Once);
            _mockManager.Verify(m => m.GenererPdfFactureParCommande(42), Times.Once);
        }

        #endregion
    }
}
