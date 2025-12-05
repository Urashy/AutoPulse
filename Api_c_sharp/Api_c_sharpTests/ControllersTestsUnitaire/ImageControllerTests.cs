using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace App.ControllersUnitaires.Tests
{
    [TestClass]
    public class ImageControllerTests
    {
        private AutoPulseBdContext _context;
        private ImageManager _manager;
        private IMapper _mapper;
        private ImageController _controller;

        private Image _imageCommun;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });

            _mapper = config.CreateMapper();

            _manager = new ImageManager(_context);
            _controller = new ImageController(_manager, _mapper);

            _context.Images.RemoveRange(_context.Images);
            await _context.SaveChangesAsync();

            _imageCommun = new Image
            {
                IdImage = 1,
                IdCompte = 10,
                IdVoiture = 20,
                Fichier = Encoding.UTF8.GetBytes("fakeImageData")
            };

            await _context.Images.AddAsync(_imageCommun);
            await _context.SaveChangesAsync();
        }

        // -----------------------------------------
        // GET BY ID
        // -----------------------------------------
        [TestMethod]
        public async Task GetById_OK()
        {
            var result = await _controller.GetById(_imageCommun.IdImage);

            Assert.IsInstanceOfType(result.Result, typeof(FileContentResult));
        }

        [TestMethod]
        public async Task GetById_NotFound()
        {
            var result = await _controller.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // -----------------------------------------
        // GET ALL
        // -----------------------------------------
        [TestMethod]
        public async Task GetAll_OK()
        {
            var result = await _controller.GetAll();

            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        // -----------------------------------------
        // POST
        // -----------------------------------------
        [TestMethod]
        public async Task Post_OK()
        {
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

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        // -----------------------------------------
        // PUT
        // -----------------------------------------
        [TestMethod]
        public async Task Put_OK()
        {
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

            var result = await _controller.Put(_imageCommun.IdImage, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_BadRequest()
        {
            var dto = new ImageUploadDTO
            {
                IdImage = 999
            };

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Put_NotFound()
        {
            var fileMock = new FormFile(new MemoryStream(new byte[1]), 0, 1, "file", "img.jpg");

            var dto = new ImageUploadDTO
            {
                IdImage = 50,
                File = fileMock
            };

            var result = await _controller.Put(50, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -----------------------------------------
        // DELETE
        // -----------------------------------------
        [TestMethod]
        public async Task Delete_OK()
        {
            var result = await _controller.Delete(_imageCommun.IdImage);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_NotFound()
        {
            var result = await _controller.Delete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -----------------------------------------
        // GET FIRST IMAGE BY VOITURE
        // -----------------------------------------
        [TestMethod]
        public async Task GetImagesByVoitureID_OK()
        {
            var result = await _controller.GetImagesByVoitureId(20);

            Assert.IsInstanceOfType(result.Result, typeof(FileContentResult));
        }

        [TestMethod]
        public async Task GetImagesByVoitureID_NotFound()
        {
            var result = await _controller.GetImagesByVoitureId(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // -----------------------------------------
        // GET ALL IMAGES BY VOITURE
        // -----------------------------------------
        [TestMethod]
        public async Task GetAllImagesByVoiture_OK()
        {
            var result = await _controller.GetAllImagesByVoitureId(20);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetAllImagesByVoiture_NoContent()
        {
            var result = await _controller.GetAllImagesByVoitureId(999);

            Assert.IsInstanceOfType(result.Result, typeof(NoContentResult));
        }

        // -----------------------------------------
        // GET IMAGE BY COMPTE
        // -----------------------------------------
        [TestMethod]
        public async Task GetImageByCompteID_OK()
        {
            var result = await _controller.GetImageByCompteID(10);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetImageByCompteID_NotFound()
        {
            var result = await _controller.GetImageByCompteID(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}
