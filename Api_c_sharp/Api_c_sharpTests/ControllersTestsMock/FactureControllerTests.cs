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
    public class FactureControllerMockTests
    {
        private Mock<FactureManager> _mockManager;
        private Mock<CommandeManager> _mockCommandeManager;
        private IMapper _mapper;
        private FactureController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<FactureManager>(null);
            _mockCommandeManager = new Mock<CommandeManager>(null);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Facture, FactureDTO>().ReverseMap();
            });

            _mapper = config.CreateMapper();
            _controller = new FactureController(_mockManager.Object, _mapper, _mockCommandeManager.Object);
        }

        #region GET
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            var entity = new Facture { IdFacture = 1, IdCommande = 2 };
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
        #endregion

        #region POST
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            var dto = new FactureDTO { IdFacture = 1, IdCommande = 2 };
            var entity = new Facture { IdFacture = 10, IdCommande = 1 };

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
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Facture>()), Times.Never);
        }
        #endregion

        #region PUT
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
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Facture>(), It.IsAny<Facture>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Facture)null);
            var dto = new FactureDTO { IdFacture = 1 };

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
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
        #endregion

        #region GetFacturePdf - Tests Mock de base

        [TestMethod]
        public async Task GetFacturePdf_ReturnsFileResult_WhenCommandeExists()
        {
            var commande = new Commande { IdCommande = 1 };
            var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(1)).Returns(fakePdfBytes);

            var result = await _controller.GetFacturePdf(1, download: true);

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
            var commande = new Commande { IdCommande = 1 };
            var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(1)).Returns(fakePdfBytes);

            var result = await _controller.GetFacturePdf(1, download: false);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FileContentResult));

            var fileResult = result as FileContentResult;
            Assert.AreEqual("application/pdf", fileResult.ContentType);
            Assert.IsTrue(string.IsNullOrEmpty(fileResult.FileDownloadName));
        }

        [TestMethod]
        public async Task GetFacturePdf_ReturnsNotFound_WhenCommandeDoesNotExist()
        {
            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Commande)null);

            var result = await _controller.GetFacturePdf(1, download: true);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Commande introuvable", notFoundResult.Value);

            _mockManager.Verify(m => m.GenererPdfFactureParCommande(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task GetFacturePdf_ReturnsNotFound_WhenPdfGenerationFails()
        {
            var commande = new Commande { IdCommande = 1 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(1)).Returns((byte[])null);

            var result = await _controller.GetFacturePdf(1, download: true);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Impossible de générer la facture", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetFacturePdf_CallsCorrectMethods()
        {
            var commande = new Commande { IdCommande = 42 };
            var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(42)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(42)).Returns(fakePdfBytes);

            await _controller.GetFacturePdf(42, download: true);

            _mockCommandeManager.Verify(m => m.GetByIdAsync(42), Times.Once);
            _mockManager.Verify(m => m.GenererPdfFactureParCommande(42), Times.Once);
        }

        #endregion

        #region GetFacturePdf - Tests Mock avancés

        [TestMethod]
        public async Task GetFacturePdf_WithLargePdf_ReturnsCorrectSize()
        {
            var commande = new Commande { IdCommande = 1 };
            var largePdfBytes = new byte[100000]; // 100KB
            largePdfBytes[0] = 0x25; // %
            largePdfBytes[1] = 0x50; // P
            largePdfBytes[2] = 0x44; // D
            largePdfBytes[3] = 0x46; // F

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(1)).Returns(largePdfBytes);

            var result = await _controller.GetFacturePdf(1, download: true);

            var fileResult = result as FileContentResult;
            Assert.AreEqual(100000, fileResult.FileContents.Length);
        }

        [TestMethod]
        public async Task GetFacturePdf_WithEmptyPdf_ReturnsEmptyFile()
        {
            var commande = new Commande { IdCommande = 1 };
            var emptyPdfBytes = new byte[0];

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(1)).Returns(emptyPdfBytes);

            var result = await _controller.GetFacturePdf(1, download: true);

            var fileResult = result as FileContentResult;
            Assert.AreEqual(0, fileResult.FileContents.Length);
        }

        [TestMethod]
        public async Task GetFacturePdf_MultipleCallsSameCommande_CallsManagerEachTime()
        {
            var commande = new Commande { IdCommande = 1 };
            var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(1)).Returns(fakePdfBytes);

            await _controller.GetFacturePdf(1, download: true);
            await _controller.GetFacturePdf(1, download: true);
            await _controller.GetFacturePdf(1, download: false);

            _mockCommandeManager.Verify(m => m.GetByIdAsync(1), Times.Exactly(3));
            _mockManager.Verify(m => m.GenererPdfFactureParCommande(1), Times.Exactly(3));
        }

        [TestMethod]
        public async Task GetFacturePdf_DifferentCommandes_CallsCorrectIds()
        {
            var commande1 = new Commande { IdCommande = 1 };
            var commande2 = new Commande { IdCommande = 2 };
            var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande1);
            _mockCommandeManager.Setup(m => m.GetByIdAsync(2)).ReturnsAsync(commande2);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(It.IsAny<int>())).Returns(fakePdfBytes);

            await _controller.GetFacturePdf(1, download: true);
            await _controller.GetFacturePdf(2, download: true);

            _mockCommandeManager.Verify(m => m.GetByIdAsync(1), Times.Once);
            _mockCommandeManager.Verify(m => m.GetByIdAsync(2), Times.Once);
            _mockManager.Verify(m => m.GenererPdfFactureParCommande(1), Times.Once);
            _mockManager.Verify(m => m.GenererPdfFactureParCommande(2), Times.Once);
        }

        [TestMethod]
        public async Task GetFacturePdf_VerifyFileContentResultProperties()
        {
            var commande = new Commande { IdCommande = 123 };
            var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(123)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(123)).Returns(fakePdfBytes);

            var result = await _controller.GetFacturePdf(123, download: true);

            var fileResult = result as FileContentResult;
            Assert.IsNotNull(fileResult);
            Assert.AreEqual("application/pdf", fileResult.ContentType);
            Assert.AreEqual("Facture_Commande_123.pdf", fileResult.FileDownloadName);
            Assert.IsNotNull(fileResult.FileContents);
            CollectionAssert.AreEqual(fakePdfBytes, fileResult.FileContents);
        }

        [TestMethod]
        public async Task GetFacturePdf_WithNegativeId_ReturnsNotFound()
        {
            _mockCommandeManager.Setup(m => m.GetByIdAsync(-1)).ReturnsAsync((Commande)null);

            var result = await _controller.GetFacturePdf(-1, download: true);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task GetFacturePdf_WithZeroId_ReturnsNotFound()
        {
            _mockCommandeManager.Setup(m => m.GetByIdAsync(0)).ReturnsAsync((Commande)null);

            var result = await _controller.GetFacturePdf(0, download: true);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task GetFacturePdf_ManagerThrowsException_PropagatesException()
        {
            var commande = new Commande { IdCommande = 1 };

            _mockCommandeManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(commande);
            _mockManager.Setup(m => m.GenererPdfFactureParCommande(1))
                .Throws(new Exception("Erreur de génération PDF"));

            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await _controller.GetFacturePdf(1, download: true);
            });
        }

        [TestMethod]
        public async Task GetFacturePdf_CommandeManagerThrowsException_PropagatesException()
        {
            _mockCommandeManager.Setup(m => m.GetByIdAsync(1))
                .Throws(new Exception("Erreur de récupération commande"));

            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await _controller.GetFacturePdf(1, download: true);
            });
        }

        #endregion
    }
}