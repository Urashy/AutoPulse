using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class PiecejointeControllerTests
    {
        private PieceJointeController _controller;
        private AutoPulseBdContext _context;
        private PieceJointeManager _manager;
        private IMapper _mapper;
        private PieceJointe _objetcommun;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _manager = new PieceJointeManager(_context);
            _controller = new PieceJointeController(_manager, _mapper);

            _context.Adresses.RemoveRange(_context.Adresses);
            await _context.SaveChangesAsync();

             Message message = new Message
             {
                 ContenuMessage = "Message pour test",
                 DateEnvoiMessage = DateTime.Now,
                 IdConversation = 1,
                 IdCompte = 1,
                 EstLu = false
             };

            PieceJointe pieceJointe = new PieceJointe
            {
                IdPieceJointe = 1,
                NomFichier = "fichier_test.txt",
                TypeMime = "text/plain",
                Extension = ".txt",
                TailleFichier = 100,
                Contenu = new byte[] { 0x0, 0x1, 0x2 },
                DateUpload = DateTime.Now,
                IdMessage = message.IdMessage
            };

            await _context.Messages.AddAsync(message);
            await _context.PiecesJointes.AddAsync(pieceJointe);
            await _context.SaveChangesAsync();

            _objetcommun = pieceJointe;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetcommun.IdPieceJointe);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(PieceJointeDTO));
            Assert.AreEqual(_objetcommun.NomFichier, result.Value.NomFichier);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Act
            var result = await _controller.GetById(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<PieceJointeDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.NomFichier == _objetcommun.NomFichier));
        }

        [TestMethod]
        public async Task PostAdresseTest_Entity()
        {
            PieceJointeUploadDTO pj = new PieceJointeUploadDTO
            {
                NomFichier = "fichier_testpost.txt",
                TypeMime = "text/plain",
                Extension = ".txt",
                TailleFichier = 100,
                IdMessage = 1,
                ContenuBase64 = Convert.ToBase64String(new byte[] { 0x0, 0x1, 0x2 })
            };

            var actionResult = await _controller.Post(pj);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdAdresse = (PieceJointe)created.Value;
            Assert.AreEqual(pj.NomFichier, createdAdresse.NomFichier);
        }


        [TestMethod]
        public async Task DeleteAdresseTest()
        {
            var result = await _controller.Delete(_objetcommun.IdPieceJointe);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedAdresse = await _manager.GetByIdAsync(_objetcommun.IdPieceJointe);
            Assert.IsNull(deletedAdresse);
        }

        [TestMethod]
        public async Task NotFoundDeleteAdresseTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutAdresseTest()
        {
            PieceJointeDTO pj = new PieceJointeDTO
            {
                IdPieceJointe = 1,
                NomFichier = "fichier_testput.txt",
                TypeMime = "text/plain",
                Extension = ".txt",
                TailleFichier = 100,
                IdMessage = 1
            };

            var result = await _controller.Put(_objetcommun.IdPieceJointe, pj);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var adresseput = await _manager.GetByIdAsync(_objetcommun.IdPieceJointe);
            Assert.AreEqual(pj.NomFichier, adresseput.NomFichier);
        }

        [TestMethod]
        public async Task NotFoundPutAdresseTest()
        {
            PieceJointeDTO pj = new PieceJointeDTO
            {
                IdPieceJointe = 1,
                NomFichier = "fichier_testput.txt",
                TypeMime = "text/plain",
                Extension = ".txt",
                TailleFichier = 100,
                IdMessage = 1
            };

            var result = await _controller.Put(0, pj);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutAdresseTest()
        {
            PieceJointeDTO pj = new PieceJointeDTO
            {
                IdPieceJointe = 1,
                NomFichier = null,
                TypeMime = "text/plain",
                Extension = ".txt",
                TailleFichier = 100,
                IdMessage = 1
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("NomFichier", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdPieceJointe, pj);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task BadRequestPostAdresseTest()
        {
            PieceJointeUploadDTO pj = new PieceJointeUploadDTO
            {
                NomFichier = "fichier_testpost.txt",
                TypeMime = "text/plain",
                Extension = ".txt",
                TailleFichier = 100,
                IdMessage = 1,
                ContenuBase64 = Convert.ToBase64String(new byte[] { 0x0, 0x1, 0x2 })
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("NomFichier", "Required");

            var actionResult = await _controller.Post(pj);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task UploadTest_SingleFile()
        {
            // Arrange
            var uploadDtos = new List<PieceJointeUploadDTO>
    {
        new PieceJointeUploadDTO
        {
            NomFichier = "upload_test.txt",
            TypeMime = "text/plain",
            Extension = ".txt",
            TailleFichier = 100,
            IdMessage = 1,
            ContenuBase64 = Convert.ToBase64String(new byte[] { 0x0, 0x1, 0x2 })
        }
    };

            // Act
            var result = await _controller.Upload(uploadDtos);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var uploadedFiles = (List<PieceJointeDTO>)okResult.Value;

            Assert.IsNotNull(uploadedFiles);
            Assert.AreEqual(1, uploadedFiles.Count);
            Assert.AreEqual("upload_test.txt", uploadedFiles[0].NomFichier);
            Assert.IsNull(uploadedFiles[0].ContenuBase64); // Le contenu ne doit pas être retourné
        }

        [TestMethod]
        public async Task UploadTest_MultipleFiles()
        {
            // Arrange
            var uploadDtos = new List<PieceJointeUploadDTO>
    {
        new PieceJointeUploadDTO
        {
            NomFichier = "file1.jpg",
            TypeMime = "image/jpeg",
            Extension = ".jpg",
            TailleFichier = 200,
            IdMessage = 1,
            ContenuBase64 = Convert.ToBase64String(new byte[] { 0x0, 0x1 })
        },
        new PieceJointeUploadDTO
        {
            NomFichier = "file2.pdf",
            TypeMime = "application/pdf",
            Extension = ".pdf",
            TailleFichier = 300,
            IdMessage = 1,
            ContenuBase64 = Convert.ToBase64String(new byte[] { 0x0, 0x1, 0x2, 0x3 })
        }
    };

            // Act
            var result = await _controller.Upload(uploadDtos);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var uploadedFiles = (List<PieceJointeDTO>)okResult.Value;

            Assert.AreEqual(2, uploadedFiles.Count);
            Assert.AreEqual("file1.jpg", uploadedFiles[0].NomFichier);
            Assert.AreEqual("file2.pdf", uploadedFiles[1].NomFichier);
        }

        [TestMethod]
        public async Task UploadTest_EmptyList()
        {
            // Arrange
            var uploadDtos = new List<PieceJointeUploadDTO>();

            // Act
            var result = await _controller.Upload(uploadDtos);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task UploadTest_NullList()
        {
            // Act
            var result = await _controller.Upload(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task UploadTest_FileTooLarge()
        {
            // Arrange
            var uploadDtos = new List<PieceJointeUploadDTO>
    {
        new PieceJointeUploadDTO
        {
            NomFichier = "large_file.txt",
            TypeMime = "text/plain",
            Extension = ".txt",
            TailleFichier = 11 * 1024 * 1024, // 11 MB (dépasse la limite de 10 MB)
            IdMessage = 1,
            ContenuBase64 = Convert.ToBase64String(new byte[] { 0x0, 0x1, 0x2 })
        }
    };

            // Act
            var result = await _controller.Upload(uploadDtos);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var uploadedFiles = (List<PieceJointeDTO>)okResult.Value;

            Assert.AreEqual(0, uploadedFiles.Count); // Le fichier doit être rejeté
        }

        [TestMethod]
        public async Task UploadTest_InvalidExtension()
        {
            // Arrange
            var uploadDtos = new List<PieceJointeUploadDTO>
    {
        new PieceJointeUploadDTO
        {
            NomFichier = "file.exe",
            TypeMime = "application/exe",
            Extension = ".exe",
            TailleFichier = 100,
            IdMessage = 1,
            ContenuBase64 = Convert.ToBase64String(new byte[] { 0x0, 0x1, 0x2 })
        }
    };

            // Act
            var result = await _controller.Upload(uploadDtos);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var uploadedFiles = (List<PieceJointeDTO>)okResult.Value;

            Assert.AreEqual(0, uploadedFiles.Count); // Le fichier doit être rejeté
        }

        [TestMethod]
        public async Task UploadTest_InvalidBase64()
        {
            // Arrange
            var uploadDtos = new List<PieceJointeUploadDTO>
    {
        new PieceJointeUploadDTO
        {
            NomFichier = "file.txt",
            TypeMime = "text/plain",
            Extension = ".txt",
            TailleFichier = 100,
            IdMessage = 1,
            ContenuBase64 = "Invalid-Base64-String!!!"
        }
    };

            // Act
            var result = await _controller.Upload(uploadDtos);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var uploadedFiles = (List<PieceJointeDTO>)okResult.Value;

            Assert.AreEqual(0, uploadedFiles.Count); // Le fichier doit être rejeté
        }

        [TestMethod]
        public async Task DownloadTest()
        {
            // Act
            var result = await _controller.Download(_objetcommun.IdPieceJointe);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FileContentResult));

            var fileResult = (FileContentResult)result;
            Assert.AreEqual(_objetcommun.TypeMime, fileResult.ContentType);
            Assert.AreEqual(_objetcommun.NomFichier, fileResult.FileDownloadName);
            CollectionAssert.AreEqual(_objetcommun.Contenu, fileResult.FileContents);
        }

        [TestMethod]
        public async Task NotFoundDownloadTest()
        {
            // Act
            var result = await _controller.Download(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetByMessageTest()
        {
            // Arrange
            PieceJointe pieceJointe2 = new PieceJointe
            {
                IdPieceJointe = 2,
                NomFichier = "fichier_test2.txt",
                TypeMime = "text/plain",
                Extension = ".txt",
                TailleFichier = 150,
                Contenu = new byte[] { 0x4, 0x5, 0x6 },
                DateUpload = DateTime.Now,
                IdMessage = _objetcommun.IdMessage
            };
            await _context.PiecesJointes.AddAsync(pieceJointe2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetByMessage(_objetcommun.IdMessage);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var piecesJointes = (IEnumerable<PieceJointeDTO>)okResult.Value;

            Assert.IsNotNull(piecesJointes);
            Assert.AreEqual(2, piecesJointes.Count());
            Assert.IsTrue(piecesJointes.All(pj => pj.ContenuBase64 != null)); // Le contenu doit être présent
        }

        [TestMethod]
        public async Task GetByMessageTest_EmptyResult()
        {
            // Act
            var result = await _controller.GetByMessage(999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var piecesJointes = (IEnumerable<PieceJointeDTO>)okResult.Value;

            Assert.IsNotNull(piecesJointes);
            Assert.AreEqual(0, piecesJointes.Count());
        }

        [TestMethod]
        public async Task GetMetadataByMessageTest()
        {
            // Arrange
            PieceJointe pieceJointe2 = new PieceJointe
            {
                IdPieceJointe = 2,
                NomFichier = "fichier_metadata.txt",
                TypeMime = "text/plain",
                Extension = ".txt",
                TailleFichier = 200,
                Contenu = new byte[] { 0x7, 0x8, 0x9 },
                DateUpload = DateTime.Now,
                IdMessage = _objetcommun.IdMessage
            };
            await _context.PiecesJointes.AddAsync(pieceJointe2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetMetadataByMessage(_objetcommun.IdMessage);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var piecesJointes = (IEnumerable<PieceJointeDTO>)okResult.Value;

            Assert.IsNotNull(piecesJointes);
            Assert.AreEqual(2, piecesJointes.Count());
            Assert.IsTrue(piecesJointes.All(pj => pj.ContenuBase64 == null)); // Le contenu NE doit PAS être présent
            Assert.AreEqual("fichier_test.txt", piecesJointes.First().NomFichier);
        }

        [TestMethod]
        public async Task GetMetadataByMessageTest_EmptyResult()
        {
            // Act
            var result = await _controller.GetMetadataByMessage(999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var piecesJointes = (IEnumerable<PieceJointeDTO>)okResult.Value;

            Assert.IsNotNull(piecesJointes);
            Assert.AreEqual(0, piecesJointes.Count());
        }
    }
}