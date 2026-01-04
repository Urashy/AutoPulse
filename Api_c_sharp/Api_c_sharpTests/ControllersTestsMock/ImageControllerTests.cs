using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class ImageControllerTests
    {
        private Mock<ImageManager> _mockManager;
        private ImageController _controller;
        private IMapper _mapper;
        private Image _imageCommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock
            _mockManager = new Mock<ImageManager>(null);

            // Création de l'image de référence
            _imageCommun = new Image
            {
                IdImage = 1,
                IdCompte = 10,
                IdVoiture = 20,
                Fichier = Encoding.UTF8.GetBytes("fakeImageData")
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new ImageController(_mockManager.Object, _mapper);
        }

        #region GET
        [TestMethod]
        public async Task GetById_OK()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_imageCommun.IdImage))
                       .ReturnsAsync(_imageCommun);

            // Act
            var result = await _controller.GetById(_imageCommun.IdImage);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(FileContentResult));
            var fileResult = result.Result as FileContentResult;
            Assert.IsNotNull(fileResult);
            Assert.AreEqual("image/jpeg", fileResult.ContentType);
        }

        [TestMethod]
        public async Task GetById_NotFound()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(999))
                       .ReturnsAsync((Image)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAll_OK()
        {
            // Arrange
            var imagesList = new List<Image>
            {
                _imageCommun,
                new Image
                {
                    IdImage = 2,
                    IdCompte = 11,
                    IdVoiture = 21,
                    Fichier = Encoding.UTF8.GetBytes("anotherImage")
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(imagesList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task GetImagesByVoitureID_OK()
        {
            // Arrange
            _mockManager.Setup(m => m.GetFirstImageByVoitureID(20))
                       .ReturnsAsync(_imageCommun);

            // Act
            var result = await _controller.GetImagesByVoitureId(20);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(FileContentResult));
            var fileResult = result.Result as FileContentResult;
            Assert.IsNotNull(fileResult);
            Assert.AreEqual("image/jpeg", fileResult.ContentType);
        }

        [TestMethod]
        public async Task GetImagesByVoitureID_NotFound()
        {
            // Arrange
            _mockManager.Setup(m => m.GetFirstImageByVoitureID(999))
                       .ReturnsAsync((Image)null);

            // Act
            var result = await _controller.GetImagesByVoitureId(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllImagesByVoiture_OK()
        {
            // Arrange
            var imageIdsList = new List<int> { 1, 3 };

            _mockManager.Setup(m => m.GetAllImagesByVoitureId(20))
                       .ReturnsAsync(imageIdsList);

            // Act
            var result = await _controller.GetAllImagesByVoitureId(20);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var imageIds = okResult.Value as IEnumerable<int>;
            Assert.IsNotNull(imageIds);
            Assert.AreEqual(2, imageIds.Count());
        }

        [TestMethod]
        public async Task GetAllImagesByVoiture_NoContent()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAllImagesByVoitureId(999))
                       .ReturnsAsync((IEnumerable<int>)null);

            // Act
            var result = await _controller.GetAllImagesByVoitureId(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task GetImageByCompteID_OK()
        {
            // Arrange
            _mockManager.Setup(m => m.GetImageByCompteID(10))
                       .ReturnsAsync(_imageCommun);

            // Act
            var result = await _controller.GetImageByCompteID(10);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
        }

        [TestMethod]
        public async Task GetImageByCompteID_NotFound()
        {
            // Arrange
            _mockManager.Setup(m => m.GetImageByCompteID(999))
                       .ReturnsAsync((Image)null);

            // Act
            var result = await _controller.GetImageByCompteID(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task Post_OK()
        {
            // Arrange
            var fileMock = new FormFile(
                new MemoryStream(Encoding.UTF8.GetBytes("imageData")),
                0,
                10,
                "file",
                "image.jpg"
            );

            var dto = new ImageUploadDTO
            {
                IdImage = 2,
                IdVoiture = 20,
                IdCompte = 10,
                File = fileMock
            };

            var imageEntity = new Image
            {
                IdImage = 2,
                IdCompte = 10,
                IdVoiture = 20,
                Fichier = Encoding.UTF8.GetBytes("imageData")
            };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Image>()))
                       .ReturnsAsync(imageEntity)
                       .Verifiable();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Image>()), Times.Once);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task Put_OK()
        {
            // Arrange
            var existingImage = new Image
            {
                IdImage = _imageCommun.IdImage,
                IdCompte = 10,
                IdVoiture = 20,
                Fichier = Encoding.UTF8.GetBytes("oldData")
            };

            var fileMock = new FormFile(
                new MemoryStream(Encoding.UTF8.GetBytes("updated")),
                0,
                10,
                "file",
                "image.jpg"
            );

            var dto = new ImageUploadDTO
            {
                IdImage = _imageCommun.IdImage,
                IdCompte = 10,
                IdVoiture = 20,
                File = fileMock
            };

            _mockManager.Setup(m => m.GetByIdAsync(_imageCommun.IdImage))
                       .ReturnsAsync(existingImage);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Image>(), It.IsAny<Image>()))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_imageCommun.IdImage, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Image>(), It.IsAny<Image>()), Times.Once);
        }

        [TestMethod]
        public async Task Put_BadRequest()
        {
            // Arrange
            var dto = new ImageUploadDTO
            {
                IdImage = 999
            };

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Put_NotFound()
        {
            // Arrange
            var fileMock = new FormFile(
                new MemoryStream(new byte[1]),
                0,
                1,
                "file",
                "img.jpg"
            );

            var dto = new ImageUploadDTO
            {
                IdImage = 50,
                File = fileMock
            };

            _mockManager.Setup(m => m.GetByIdAsync(50))
                       .ReturnsAsync((Image)null);

            // Act
            var result = await _controller.Put(50, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task Delete_OK()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_imageCommun.IdImage))
                       .ReturnsAsync(_imageCommun);
            _mockManager.Setup(m => m.DeleteAsync(_imageCommun))
                       .Returns(Task.FromResult(true))
                       .Verifiable();

            // Act
            var result = await _controller.Delete(_imageCommun.IdImage);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(_imageCommun), Times.Once);
        }

        [TestMethod]
        public async Task Delete_NotFound()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(999))
                       .ReturnsAsync((Image)null);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion
        
    }
}