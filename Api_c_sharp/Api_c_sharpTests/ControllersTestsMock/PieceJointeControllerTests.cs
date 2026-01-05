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
            // Création du mock du manager avec un paramètre null pour le context
            _mockManager = new Mock<PieceJointeManager>(null);

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new PieceJointeController(_mockManager.Object, _mapper);

            // Création de la piece jointe de référence
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

        #region GET
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
        #endregion

        #region UPLOAD
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
        public async Task Upload_IgnoresFile_WhenTooLarge()
        {
            // Arrange
            var dto = new PieceJointeUploadDTO
            {
                IdPieceJointe = _objetcommun.IdPieceJointe,
                NomFichier = "big.pdf",
                Extension = ".pdf",
                TypeMime = "application/pdf",
                TailleFichier = 11 * 1024 * 1024,
                ContenuBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("test")),
                IdMessage = 1
            };

            // Act
            var result = await _controller.Upload(new List<PieceJointeUploadDTO> { dto });

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var list = ok.Value as List<PieceJointeDTO>;
            Assert.AreEqual(0, list.Count);

            _mockManager.Verify(m => m.AddAsync(It.IsAny<PieceJointe>()), Times.Never);
        }
        [TestMethod]
        public async Task Upload_ReturnsMultipleFiles_WhenMultipleValid()
        {
            // Arrange
            var dtos = new List<PieceJointeUploadDTO>
    {
        new()
        {
            NomFichier = "a.txt",
            Extension = ".txt",
            TypeMime = "text/plain",
            TailleFichier = 100,
            ContenuBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("a")),
            IdMessage = 1
        },
        new()
        {
            NomFichier = "b.pdf",
            Extension = ".pdf",
            TypeMime = "application/pdf",
            TailleFichier = 100,
            ContenuBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("b")),
            IdMessage = 1
        }
    };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<PieceJointe>()))
                        .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.Upload(dtos);

            // Assert
            var ok = result.Result as OkObjectResult;
            var list = ok.Value as List<PieceJointeDTO>;

            Assert.AreEqual(2, list.Count);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<PieceJointe>()), Times.Exactly(2));
        }


        [TestMethod]
        public async Task Upload_IgnoresFile_WhenExtensionInvalid()
        {
            // Arrange
            var dto = new PieceJointeUploadDTO
            {
                IdPieceJointe = _objetcommun.IdPieceJointe,
                NomFichier = "virus.exe",
                Extension = ".exe",
                TypeMime = "application/exe",
                TailleFichier = 100,
                ContenuBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("test")),
                IdMessage = 1
            };

            // Act
            var result = await _controller.Upload(new List<PieceJointeUploadDTO> { dto });

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var list = ok.Value as List<PieceJointeDTO>;
            Assert.AreEqual(0, list.Count);

            _mockManager.Verify(m => m.AddAsync(It.IsAny<PieceJointe>()), Times.Never);
        }

        [TestMethod]
        public async Task Upload_IgnoresFile_WhenBase64Invalid()
        {
            // Arrange
            var dto = new PieceJointeUploadDTO
            {
                IdPieceJointe = _objetcommun.IdPieceJointe,
                NomFichier = "file.txt",
                Extension = ".txt",
                TypeMime = "text/plain",
                TailleFichier = 100,
                ContenuBase64 = "INVALID_BASE64",
                IdMessage = 1
            };

            // Act
            var result = await _controller.Upload(new List<PieceJointeUploadDTO> { dto });

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var list = ok.Value as List<PieceJointeDTO>;
            Assert.AreEqual(0, list.Count);

            _mockManager.Verify(m => m.AddAsync(It.IsAny<PieceJointe>()), Times.Never);
        }

        [TestMethod]
        public async Task Upload_Continues_WhenAddAsyncThrows()
        {
            // Arrange
            var dto = new PieceJointeUploadDTO
            {
                IdPieceJointe = _objetcommun.IdPieceJointe,
                NomFichier = "file.txt",
                Extension = ".txt",
                TypeMime = "text/plain",
                TailleFichier = 100,
                ContenuBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("test")),
                IdMessage = 1
            };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<PieceJointe>()))
                        .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.Upload(new List<PieceJointeUploadDTO> { dto });

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var list = ok.Value as List<PieceJointeDTO>;
            Assert.AreEqual(0, list.Count);
        }


        [TestMethod]
        public async Task Upload_ReturnsBadRequest_WhenEmpty()
        {
            // Act
            var result = await _controller.Upload(new List<PieceJointeUploadDTO>());

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region DOWNLOAD
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
        #endregion

        #region POST
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
            var dto = new PieceJointeCreateDTO();
            _controller.ModelState.AddModelError("NomFichier", "Required");

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<PieceJointe>()), Times.Never);
        }
        #endregion

        #region PUT
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
        public async Task Put_PJ_ReturnsNotFound()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync((PieceJointe)null);

            // Act
            var result = await _controller.Put(1, new PieceJointeUploadDTO());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutPJTest()
        {
            // Arrange
            var dto = new PieceJointeUploadDTO
            {
                NomFichier = null
            };

            _controller.ModelState.AddModelError("NomFichier", "Required");

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<PieceJointe>(), It.IsAny<PieceJointe>()), Times.Never);
        }
        #endregion

        #region DELETE
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
        public async Task Delete_CallsDeleteAsync_WhenExists()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdWithContentAsync(1))
                        .ReturnsAsync(_objetcommun);

            // Act
            await _controller.Delete(1);

            // Assert
            _mockManager.Verify(m => m.DeleteAsync(_objetcommun), Times.Once);
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
        #endregion
    }
}
