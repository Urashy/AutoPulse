using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass]
    [TestCategory("unit")]
    public class PieceJointeControllerTests
    {
        private Mock<PieceJointeManager> _mockManager;
        private PieceJointeController _controller;
        private IMapper _mapper;
        private PieceJointe _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            _mockManager = new Mock<PieceJointeManager>(null);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _controller = new PieceJointeController(_mockManager.Object, _mapper);

            _objetcommun = new PieceJointe
            {
                IdPieceJointe = 1,
                IdMessage = 10,
                NomFichier = "test.pdf",
                TypeMime = "application/pdf",
                Extension = ".pdf",
                TailleFichier = 1024,
                Contenu = Encoding.UTF8.GetBytes("contenu"),
                DateUpload = DateTime.UtcNow
            };
        }

        // -----------------------------------------------------
        //                     GET BY ID
        // -----------------------------------------------------
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdWithContentAsync(1))
                        .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(PieceJointeDTO));
            Assert.IsFalse(string.IsNullOrEmpty(result.Value.ContenuBase64));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdWithContentAsync(1))
                        .ReturnsAsync((PieceJointe)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // -----------------------------------------------------
        //                     GET ALL
        // -----------------------------------------------------
        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAllAsync())
                        .ReturnsAsync(new List<PieceJointe> { _objetcommun });

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(1, result.Value.Count());
        }

        // -----------------------------------------------------
        //                     GET BY MESSAGE
        // -----------------------------------------------------
        [TestMethod]
        public async Task GetByMessage_ReturnsOk()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByMessageIdAsync(10))
                        .ReturnsAsync(new List<PieceJointe> { _objetcommun });

            // Act
            var result = await _controller.GetByMessage(10);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var list = ok.Value as IEnumerable<PieceJointeDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count());
            Assert.IsFalse(string.IsNullOrEmpty(list.First().ContenuBase64));
        }

        [TestMethod]
        public async Task GetMetadataByMessage_ReturnsWithoutContent()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByMessageIdAsync(10))
                        .ReturnsAsync(new List<PieceJointe> { _objetcommun });

            // Act
            var result = await _controller.GetMetadataByMessage(10);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var list = ok.Value as IEnumerable<PieceJointeDTO>;
            Assert.IsNotNull(list);
            Assert.IsNull(list.First().ContenuBase64);
        }

        // -----------------------------------------------------
        //                     UPLOAD
        // -----------------------------------------------------
        [TestMethod]
        public async Task Upload_ReturnsOk_WithValidFile()
        {
            // Arrange
            var uploadDto = new PieceJointeUploadDTO
            {
                IdPieceJointe =_objetcommun.IdPieceJointe,
                IdMessage = 10,
                NomFichier = "test.pdf",
                TypeMime = "application/pdf",
                Extension = ".pdf",
                TailleFichier = 1024,
                ContenuBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("test"))
            };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<PieceJointe>()))
                        .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.Upload(new List<PieceJointeUploadDTO> { uploadDto });

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var list = ok.Value as List<PieceJointeDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
            Assert.IsNull(list.First().ContenuBase64);
        }

        [TestMethod]
        public async Task Upload_ReturnsBadRequest_WhenEmpty()
        {
            // Act
            var result = await _controller.Upload(new List<PieceJointeUploadDTO>());

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        // -----------------------------------------------------
        //                     DOWNLOAD
        // -----------------------------------------------------
        [TestMethod]
        public async Task Download_ReturnsFile_WhenExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdWithContentAsync(1))
                        .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.Download(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
        }

        [TestMethod]
        public async Task Download_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdWithContentAsync(1))
                        .ReturnsAsync((PieceJointe)null);

            // Act
            var result = await _controller.Download(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -----------------------------------------------------
        //                     POST
        // -----------------------------------------------------
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            // Arrange
            var dto = new PieceJointeCreateDTO
            {
                IdMessage = 10,
                NomFichier = "test.pdf",
                TypeMime = "application/pdf",
                Extension = ".pdf",
                TailleFichier = 1024,
                ContenuBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("test"))
            };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<PieceJointe>()))
                        .Callback<PieceJointe>(pj => pj.IdPieceJointe = 1)
                        .ReturnsAsync((PieceJointe pj) => pj);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Invalid");

            // Act
            var result = await _controller.Post(new PieceJointeCreateDTO());

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        // -----------------------------------------------------
        //                     PUT
        // -----------------------------------------------------
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            // Arrange
            PieceJointeUploadDTO dto = new PieceJointeUploadDTO
            {
                NomFichier = "fichier_testput.txt",
                TypeMime = "text/plain",
                Extension = ".txt",
                TailleFichier = 100,
                IdMessage = 1
            };
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync((PieceJointe)null);

            // Act
            var result = await _controller.Put(1, new PieceJointeUploadDTO());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -----------------------------------------------------
        //                     DELETE
        // -----------------------------------------------------
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdWithContentAsync(1))
                        .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdWithContentAsync(1))
                        .ReturnsAsync((PieceJointe)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
