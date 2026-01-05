using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class SignalementControllerTestsMoq
    {
        private Mock<SignalementManager> _mockManager;
        private Mock<IJournalService> _mockJournalService;
        private SignalementController _controller;
        private IMapper _mapper;
        private Signalement _signalementCommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création des mocks
            _mockManager = new Mock<SignalementManager>(null);
            _mockJournalService = new Mock<IJournalService>();

            // Création du signalement de référence
            _signalementCommun = new Signalement()
            {
                IdSignalement = 1,
                DescriptionSignalement = "Comportement suspect",
                IdCompteSignalant = 1,
                IdCompteSignale = 2,
                IdEtatSignalement = 1,
                IdTypeSignalement = 1,
                DateCreationSignalement = DateTime.UtcNow
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new SignalementController(
                _mockManager.Object,
                _mapper,
                _mockJournalService.Object
            );
        }

        #region GET
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_signalementCommun.IdSignalement))
                       .ReturnsAsync(_signalementCommun);

            // Act
            var result = await _controller.GetByID(_signalementCommun.IdSignalement);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(SignalementDTO));
            Assert.AreEqual(_signalementCommun.IdSignalement, result.Value.IdSignalement);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Signalement)null);

            // Act
            var result = await _controller.GetByID(0);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange
            var signalementsList = new List<Signalement>
            {
                _signalementCommun,
                new Signalement
                {
                    IdSignalement = 2,
                    DescriptionSignalement = "Autre signalement",
                    IdCompteSignalant = 1,
                    IdCompteSignale = 3,
                    IdEtatSignalement = 1,
                    IdTypeSignalement = 1,
                    DateCreationSignalement = DateTime.UtcNow
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(signalementsList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
            Assert.AreEqual(2, result.Value.Count());
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task PostSignalementCompteTest()
        {
            // Arrange
            SignalementCreateDTO signalementCreateDTO = new SignalementCreateDTO
            {
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 2,
                IdTypeSignalement = 1
            };

            var signalementEntity = new Signalement
            {
                IdSignalement = 3,
                DescriptionSignalement = signalementCreateDTO.DescriptionSignalement,
                IdCompteSignalant = 1,
                IdCompteSignale = signalementCreateDTO.IdCompteSignale,
                IdEtatSignalement = 1,
                IdTypeSignalement = signalementCreateDTO.IdTypeSignalement,
                DateCreationSignalement = DateTime.UtcNow
            };

            ClaimCookie(1);

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Signalement>()))
                       .ReturnsAsync(signalementEntity)
                       .Verifiable();

            _mockManager.Setup(m => m.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync(signalementEntity);

            _mockJournalService.Setup(j => j.LogSignalementCompteAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(signalementCreateDTO);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Signalement>()), Times.Once);
            _mockJournalService.Verify(j => j.LogSignalementCompteAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task PostSignalementAnnonceTest()
        {
            // Arrange
            SignalementCreateDTO signalementCreateDTO = new SignalementCreateDTO
            {
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdAnnonceSignale = 1,
                IdTypeSignalement = 1
            };

            var signalementEntity = new Signalement
            {
                IdSignalement = 3,
                DescriptionSignalement = signalementCreateDTO.DescriptionSignalement,
                IdCompteSignalant = 1,
                IdAnnonceSignale = signalementCreateDTO.IdAnnonceSignale,
                IdEtatSignalement = 1,
                IdTypeSignalement = signalementCreateDTO.IdTypeSignalement,
                DateCreationSignalement = DateTime.UtcNow
            };

            ClaimCookie(1);

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Signalement>()))
                       .ReturnsAsync(signalementEntity)
                       .Verifiable();

            _mockManager.Setup(m => m.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync(signalementEntity);

            _mockJournalService.Setup(j => j.LogSignalementAnnonceAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(signalementCreateDTO);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Signalement>()), Times.Once);
            _mockJournalService.Verify(j => j.LogSignalementAnnonceAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task BadRequestPostSignalementTest()
        {
            // Arrange
            SignalementCreateDTO dto = new SignalementCreateDTO();
            _controller.ModelState.AddModelError("Erreur", "Required");

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Signalement>()), Times.Never);
        }

        [TestMethod]
        public async Task PostBadRequestAnnonceEtCompteTest()
        {
            // Arrange
            SignalementCreateDTO dto = new SignalementCreateDTO
            {
                DescriptionSignalement = "Description test",
                IdAnnonceSignale = 1,
                IdCompteSignale = 2,
                IdTypeSignalement = 1
            };
            ClaimCookie(1);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.AreEqual("Un signalement ne peut pas cibler à la fois une annonce et un compte",
                            badRequestResult.Value);
        }

        [TestMethod]
        public async Task PostBadRequestAucuneCibleTest()
        {
            // Arrange
            SignalementCreateDTO dto = new SignalementCreateDTO
            {
                DescriptionSignalement = "Description test",
                IdAnnonceSignale = null,
                IdCompteSignale = null,
                IdTypeSignalement = 1
            };
            ClaimCookie(1);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.AreEqual("Un signalement doit cibler soit une annonce soit un compte",
                            badRequestResult.Value);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutSignalementTest()
        {
            // Arrange
            var existingSignalement = new Signalement
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Ancien contenu",
                IdCompteSignalant = 1,
                IdCompteSignale = 2,
                IdEtatSignalement = 1,
                IdTypeSignalement = 1
            };

            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement
            };

            _mockManager.Setup(m => m.GetByIdAsync(_signalementCommun.IdSignalement))
                       .ReturnsAsync(existingSignalement);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Signalement>(), It.IsAny<Signalement>()))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_signalementCommun.IdSignalement, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Signalement>(), It.IsAny<Signalement>()), Times.Once);
        }

        [TestMethod]
        public async Task PutBadRequestTest()
        {
            // Arrange
            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = null,
            };

            _controller.ModelState.AddModelError("DescriptionSignalement", "Required");

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Signalement>(), It.IsAny<Signalement>()), Times.Never);
        }

        [TestMethod]
        public async Task PutNotFoundTest()
        {
            // Arrange
            var dto = new SignalementUpdateDTO() { IdSignalement = 10 };

            _mockManager.Setup(m => m.GetByIdAsync(10))
                       .ReturnsAsync((Signalement)null);

            // Act
            var result = await _controller.Put(10, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteSignalementTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_signalementCommun.IdSignalement))
                       .ReturnsAsync(_signalementCommun);
            _mockManager.Setup(m => m.DeleteAsync(_signalementCommun))
                       .Returns(Task.FromResult(true))
                       .Verifiable();

            // Act
            var result = await _controller.Delete(_signalementCommun.IdSignalement);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(_signalementCommun), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundDeleteSignalementTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Signalement)null);

            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region FILTERED
        [TestMethod]
        public async Task GetFilteredCompteTest()
        {
            // Arrange
            var signalementsList = new List<Signalement> { _signalementCommun };

            _mockManager.Setup(m => m.GetSignalementsByEtatAndType(
                _signalementCommun.IdEtatSignalement, 2, "suspect"))
                       .ReturnsAsync(signalementsList);

            // Act
            var result = await _controller.GetFilteredSignalement(
                _signalementCommun.IdEtatSignalement, 2, "suspect");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetFilteredAnnonceTest()
        {
            // Arrange
            var signalementAnnonce = new Signalement
            {
                IdSignalement = 1,
                DescriptionSignalement = "Comportement suspect",
                IdCompteSignalant = 1,
                IdAnnonceSignale = 1,
                IdEtatSignalement = 1,
                IdTypeSignalement = 1
            };

            var signalementsList = new List<Signalement> { signalementAnnonce };

            _mockManager.Setup(m => m.GetSignalementsByEtatAndType(1, 1, "suspect"))
                       .ReturnsAsync(signalementsList);

            // Act
            var result = await _controller.GetFilteredSignalement(1, 1, "suspect");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task EmptyResultGetFilteredSignalementTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetSignalementsByEtatAndType(0, 0, "existe pas"))
                       .ReturnsAsync(new List<Signalement>());

            // Act
            var result = await _controller.GetFilteredSignalement(0, 0, "existe pas");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsFalse(result.Value.Any());
        }

        [TestMethod]
        public async Task GetFilteredSignalement_FilterByEtatOnlyTest()
        {
            // Arrange
            var signalementsList = new List<Signalement> { _signalementCommun };

            _mockManager.Setup(m => m.GetSignalementsByEtatAndType(
                _signalementCommun.IdEtatSignalement, 0, ""))
                       .ReturnsAsync(signalementsList);

            // Act
            var result = await _controller.GetFilteredSignalement(
                _signalementCommun.IdEtatSignalement, 0, null);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetFilteredSignalement_CaseInsensitiveSearchTest()
        {
            // Arrange
            var signalementsList = new List<Signalement> { _signalementCommun };

            _mockManager.Setup(m => m.GetSignalementsByEtatAndType(0, 0, "CoMpOrTeMeNt"))
                       .ReturnsAsync(signalementsList);

            // Act
            var result = await _controller.GetFilteredSignalement(0, 0, "CoMpOrTeMeNt");

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetFilteredSignalement_NoFiltersTest()
        {
            // Arrange
            var signalementsList = new List<Signalement> { _signalementCommun };

            _mockManager.Setup(m => m.GetSignalementsByEtatAndType(0, 0, ""))
                       .ReturnsAsync(signalementsList);

            // Act
            var result = await _controller.GetFilteredSignalement(0, 0, null);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetFilteredSignalement_SearchInPseudoTest()
        {
            // Arrange
            var signalementsList = new List<Signalement> { _signalementCommun };

            _mockManager.Setup(m => m.GetSignalementsByEtatAndType(0, 0, "alice"))
                       .ReturnsAsync(signalementsList);

            // Act
            var result = await _controller.GetFilteredSignalement(0, 0, "alice");

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }
        #endregion

        #region UPDATE ETAT
        [TestMethod]
        public async Task UpdateEtatTest()
        {
            // Arrange
            var existingSignalement = new Signalement
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Ancien contenu",
                IdCompteSignalant = 1,
                IdCompteSignale = 1,
                IdEtatSignalement = 1,
                IdTypeSignalement = 1
            };

            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement,
                IdEtatSignalement = 2,
            };

            _mockManager.Setup(m => m.GetByIdAsync(_signalementCommun.IdSignalement))
                       .ReturnsAsync(existingSignalement);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Signalement>(), It.IsAny<Signalement>()))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.UpdateEtat(_signalementCommun.IdSignalement, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Signalement>(), It.IsAny<Signalement>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateEtatNotFoundTest()
        {
            // Arrange
            var idInexistant = 999;
            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement,
                IdEtatSignalement = 2
            };

            _mockManager.Setup(m => m.GetByIdAsync(idInexistant))
                       .ReturnsAsync((Signalement)null);

            // Act
            var result = await _controller.UpdateEtat(idInexistant, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task UpdateEtatBadRequestInvalidState5Test()
        {
            // Arrange
            var existingSignalement = new Signalement
            {
                IdSignalement = _signalementCommun.IdSignalement,
                IdEtatSignalement = 1
            };

            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = "Il a fait un truc pas bien",
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement,
                IdEtatSignalement = 5
            };

            _mockManager.Setup(m => m.GetByIdAsync(_signalementCommun.IdSignalement))
                       .ReturnsAsync(existingSignalement);

            // Act
            var result = await _controller.UpdateEtat(_signalementCommun.IdSignalement, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task BadRequestUpdateEtatTest()
        {
            // Arrange
            SignalementUpdateDTO dto = new SignalementUpdateDTO
            {
                IdSignalement = _signalementCommun.IdSignalement,
                DescriptionSignalement = null,
                IdCompteSignale = 1,
                IdCompteSignalant = _signalementCommun.IdCompteSignalant,
                IdTypeSignalement = _signalementCommun.IdTypeSignalement,
                IdEtatSignalement = 2,
            };

            _controller.ModelState.AddModelError("DescriptionSignalement", "Required");

            // Act
            var result = await _controller.UpdateEtat(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Signalement>(), It.IsAny<Signalement>()), Times.Never);
        }
        #endregion


        private void ClaimCookie(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim("idUser", userId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}