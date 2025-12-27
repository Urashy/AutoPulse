using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
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
    public class PlainteControllerTests
    {
        private Mock<PlainteManager> _mockManager;
        private IMapper _mapper;
        private PlainteController _controller;
        private Plainte _objetcommun;

        [TestInitialize]
        public void Setup()
        {
            _mockManager = new Mock<PlainteManager>(null);

            _objetcommun = new Plainte
            {
                IdPlainte = 2,
                Description = "une plainte",
                DateCreation = DateTime.Now,
                IdCompte = 1,
                IdSignalement = 2,
            };

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Plainte, PlainteDTO>().ReverseMap();

                cfg.CreateMap<PlainteCreateDTO, Plainte>()
                    .ForMember(dest => dest.DateCreation, opt => opt.MapFrom(src => DateTime.UtcNow))
                    .ReverseMap();

                cfg.CreateMap<PlainteUpdateDTO, Plainte>().ReverseMap();
            });

            _mapper = config.CreateMapper();

            _controller = new PlainteController(_mockManager.Object, _mapper);
        }

        // -----------------------------------------------------
        //                     GET BY ID
        // -----------------------------------------------------
        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            var entity = new Plainte
            {
                IdPlainte = 1,
                Description = "une plainte",
                IdCompte = 1,
                IdSignalement = 1
            };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(entity);

            var result = await _controller.GetByID(1);

            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(PlainteDTO));
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync((Plainte)null);

            var result = await _controller.GetByID(1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // -----------------------------------------------------
        //                     GET ALL
        // -----------------------------------------------------
        [TestMethod]
        public async Task GetAll_ReturnsList()
        {
            var data = new List<Plainte>
            {
                new Plainte
            {
                IdPlainte = 1,
                Description = "une plainte",
                IdCompte = 1,
                IdSignalement = 1
            },
               new Plainte
            {
                IdPlainte = 2,
                Description = "une plainte",
                IdCompte = 2,
                IdSignalement = 2
            }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                        .ReturnsAsync(data);

            var result = await _controller.GetAll();

            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }

        // -----------------------------------------------------
        //                      POST
        // -----------------------------------------------------
        [TestMethod]
        public async Task Post_ReturnsCreated()
        {
            var dto = new PlainteCreateDTO
            {
                Description = "une plainte",
                IdCompte = 2,
                IdSignalement = 1,
                IdEtat = 1 // N'oubliez pas d'ajouter IdEtat
            };

            var entity = new Plainte
            {
                IdPlainte = 2,
                Description = "une plainte",
                IdCompte = 2,
                IdSignalement = 1,
                IdEtat = 1,
                DateCreation = DateTime.UtcNow
            };

            // Mock pour GetPlainteByCompteID - retourne une liste vide 
            // (pas de plainte en attente pour ce compte)
            _mockManager.Setup(m => m.GetPlainteByCompteID(2))
                        .ReturnsAsync(new List<Plainte>());

            // Mock pour AddAsync
            _mockManager.Setup(m => m.AddAsync(It.IsAny<Plainte>()))
                        .ReturnsAsync(entity);

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual("GetByID", createdResult.ActionName);
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenPlainteEnAttente()
        {
            var dto = new PlainteCreateDTO
            {
                Description = "une plainte",
                IdCompte = 2,
                IdSignalement = 1,
                IdEtat = 1
            };

            // Mock pour simuler une plainte déjà en attente (IdEtat = 1)
            var plaintesExistantes = new List<Plainte>
            {
                new Plainte
                {
                    IdPlainte = 1,
                    Description = "plainte existante",
                    IdCompte = 2,
                    IdSignalement = 1,
                    IdEtat = 1, // Plainte en attente
                    DateCreation = DateTime.UtcNow
                }
            };

            _mockManager.Setup(m => m.GetPlainteByCompteID(2))
                        .ReturnsAsync(plaintesExistantes);

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequest = result.Result as BadRequestObjectResult;
            Assert.AreEqual("Vous avez déjà une plainte en attente de traitement.", badRequest.Value);

            // Vérifier que AddAsync n'a jamais été appelé
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Plainte>()), Times.Never);
        }

        [TestMethod]
        public async Task Post_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new PlainteCreateDTO();

            var result = await _controller.Post(dto);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Plainte>()), Times.Never);
        }

        // -----------------------------------------------------
        //                       PUT
        // -----------------------------------------------------
        [TestMethod]
        public async Task Put_ReturnsNoContent_WhenOk()
        {
            var existing = new Plainte
            { 
                IdPlainte = 2, 
                Description = "une plainte", 
                IdCompte = 1, 
                IdSignalement = 2 
            };
            var dto = new PlainteUpdateDTO
            {
                IdPlainte = 2,
                Description = "une plainte",
                IdCompte = 1,
                IdSignalement = 2,
            };

            _mockManager.Setup(m => m.GetByIdAsync(2))
                .ReturnsAsync(existing);

            var result = await _controller.Put(2, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Put_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("x", "invalid");

            var dto = new PlainteUpdateDTO();

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Plainte>(), It.IsAny<Plainte>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_ReturnsNotFound_WhenEntityMissing()
        {
            _mockManager.Setup(m => m.GetByIdAsync(2))
                        .ReturnsAsync((Plainte)null);

            var dto = new PlainteUpdateDTO 
            { 
                IdPlainte = 2,
                Description = "une plainte",
                IdCompte = 1,
                IdSignalement = 2,
            };

            var result = await _controller.Put(1, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // -----------------------------------------------------
        //                       DELETE
        // -----------------------------------------------------
        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenOk()
        {
            var existing = new Plainte { 
                IdPlainte = 2,
                Description = "c'est une plainte"
            };

            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync(existing);

            var result = await _controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            _mockManager.Setup(m => m.GetByIdAsync(1))
                        .ReturnsAsync((Plainte)null);

            var result = await _controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
