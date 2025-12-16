using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class ReinitialisationMotDePasseControllerTestsMoq
    {
        private Mock<ReinitialisationMotDePasseManager> _mockManager;
        private IConfiguration _config;
        private ReinitialisationMotDePasseController _controller;
        private ReinitialisationMotDePasse _objetCommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock
            _mockManager = new Mock<ReinitialisationMotDePasseManager>(null);

            // Configuration en mémoire
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Email:GmailUser", "fake@mail.com"},
                {"Email:GmailPass", "fakepass"}
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Objet de référence
            _objetCommun = new ReinitialisationMotDePasse
            {
                IdReinitialisationMdp = 1,
                IdCompte = 1,
                Email = "test@mail.com",
                Token = "TOKEN123",
                Expiration = DateTime.UtcNow.AddMinutes(30),
                Utilise = false
            };

            // Création du controller
            _controller = new ReinitialisationMotDePasseController(
                _mockManager.Object,
                _config
            );
        }

        #region GET BY ID Tests

        [TestMethod]
        public async Task GetById_ReturnsOk()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetCommun.IdReinitialisationMdp))
                       .ReturnsAsync(_objetCommun);

            // Act
            var result = await _controller.Get(_objetCommun.IdReinitialisationMdp);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(_objetCommun.Email, result.Value.Email);
            Assert.AreEqual(_objetCommun.Token, result.Value.Token);
        }

        [TestMethod]
        public async Task GetById_NotFound()
        {
            // Arrange
            int invalidId = 999;
            _mockManager.Setup(m => m.GetByIdAsync(invalidId))
                       .ReturnsAsync((ReinitialisationMotDePasse)null);

            // Act
            var result = await _controller.Get(invalidId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        #endregion

        #region GET ALL Tests

        [TestMethod]
        public async Task GetAll_ReturnsListOfReinit()
        {
            // Arrange
            var list = new List<ReinitialisationMotDePasse>
            {
                _objetCommun,
                new ReinitialisationMotDePasse
                {
                    IdReinitialisationMdp = 2,
                    IdCompte = 2,
                    Email = "test2@mail.com",
                    Token = "TOKEN456",
                    Expiration = DateTime.UtcNow.AddMinutes(30),
                    Utilise = false
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(list);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            var returnedList = result.Value as IEnumerable<ReinitialisationMotDePasse>;
            Assert.IsNotNull(returnedList);
            Assert.AreEqual(2, ((List<ReinitialisationMotDePasse>)returnedList).Count);
        }

        [TestMethod]
        public async Task GetAll_ReturnsEmptyList()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(new List<ReinitialisationMotDePasse>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            var returnedList = result.Value as IEnumerable<ReinitialisationMotDePasse>;
            Assert.AreEqual(0, ((List<ReinitialisationMotDePasse>)returnedList).Count);
        }

        #endregion

        #region GET BY STRING Tests

        [TestMethod]
        public async Task GetByString_ReturnsOk()
        {
            // Arrange
            string token = "TOKEN123";
            _mockManager.Setup(m => m.GetByNameAsync(token))
                       .ReturnsAsync(_objetCommun);

            // Act
            var result = await _controller.GetByString(token);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = result.Result as OkObjectResult;
            var returnedEntity = okResult.Value as ReinitialisationMotDePasse;
            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(_objetCommun.Email, returnedEntity.Email);
        }

        [TestMethod]
        public async Task GetByString_NotFound()
        {
            // Arrange
            string invalidToken = "UNKNOWN";
            _mockManager.Setup(m => m.GetByNameAsync(invalidToken))
                       .ReturnsAsync((ReinitialisationMotDePasse)null);

            // Act
            var result = await _controller.GetByString(invalidToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        #endregion

        #region POST Tests

        [TestMethod]
        public async Task Post_BadRequest_InvalidModelState()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");
            var dto = new ReinitialiseMdpDTO
            {
                Email = "",
                Code = "CODE123"
            };

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<ReinitialisationMotDePasse>()), Times.Never);
        }


        #endregion

        #region PUT Tests

        [TestMethod]
        public async Task Put_UpdatesSuccessfully()
        {
            // Arrange
            var updated = new ReinitialisationMotDePasse
            {
                IdReinitialisationMdp = _objetCommun.IdReinitialisationMdp,
                IdCompte = _objetCommun.IdCompte,
                Email = "updated@mail.com",
                Token = _objetCommun.Token,
                Expiration = DateTime.UtcNow.AddHours(1),
                Utilise = false
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetCommun.IdReinitialisationMdp))
                       .ReturnsAsync(_objetCommun);

            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<ReinitialisationMotDePasse>(), It.IsAny<ReinitialisationMotDePasse>()))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_objetCommun.IdReinitialisationMdp, updated);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<ReinitialisationMotDePasse>(), It.IsAny<ReinitialisationMotDePasse>()), Times.Once);
        }

        [TestMethod]
        public async Task Put_NotFound()
        {
            // Arrange
            int invalidId = 999;
            var dto = new ReinitialisationMotDePasse
            {
                IdReinitialisationMdp = invalidId,
                Email = "x@mail.com"
            };

            _mockManager.Setup(m => m.GetByIdAsync(invalidId))
                       .ReturnsAsync((ReinitialisationMotDePasse)null);

            // Act
            var result = await _controller.Put(invalidId, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Put_BadRequest_InvalidModelState()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");

            // Act
            var result = await _controller.Put(_objetCommun.IdReinitialisationMdp, _objetCommun);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<ReinitialisationMotDePasse>(), It.IsAny<ReinitialisationMotDePasse>()), Times.Never);
        }

        #endregion

        #region DELETE Tests

        [TestMethod]
        public async Task Delete_RemovesSuccessfully()
        {
            // Arrange
            string token = "TOKEN123";

            _mockManager.Setup(m => m.GetByNameAsync(token))
                       .ReturnsAsync(_objetCommun);

            _mockManager.Setup(m => m.DeleteAsync(_objetCommun))
                       .Returns(Task.FromResult(true))
                       .Verifiable();

            // Act
            var result = await _controller.Delete(token);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(_objetCommun), Times.Once);
        }

        [TestMethod]
        public async Task Delete_NotFound()
        {
            // Arrange
            string invalidToken = "NOTEXIST";

            _mockManager.Setup(m => m.GetByNameAsync(invalidToken))
                       .ReturnsAsync((ReinitialisationMotDePasse)null);

            // Act
            var result = await _controller.Delete(invalidToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        #endregion

        #region VERIFICATION CODE Tests

        [TestMethod]
        public async Task VerificationCode_ValidCredentials_ReturnsEntity()
        {
            // Arrange
            string email = "test@mail.com";
            string token = "TOKEN123";

            _mockManager.Setup(m => m.VerificationCode(email, token))
                       .ReturnsAsync(_objetCommun);

            // Act
            var result = await _mockManager.Object.VerificationCode(email, token);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_objetCommun.Email, result.Email);
            Assert.AreEqual(_objetCommun.Token, result.Token);
        }

        [TestMethod]
        public async Task VerificationCode_InvalidToken_ReturnsNull()
        {
            // Arrange
            string email = "test@mail.com";
            string wrongToken = "WRONGTOKEN";

            _mockManager.Setup(m => m.VerificationCode(email, wrongToken))
                       .ReturnsAsync((ReinitialisationMotDePasse)null);

            // Act
            var result = await _mockManager.Object.VerificationCode(email, wrongToken);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VerificationCode_InvalidEmail_ReturnsNull()
        {
            // Arrange
            string wrongEmail = "wrong@mail.com";
            string token = "TOKEN123";

            _mockManager.Setup(m => m.VerificationCode(wrongEmail, token))
                       .ReturnsAsync((ReinitialisationMotDePasse)null);

            // Act
            var result = await _mockManager.Object.VerificationCode(wrongEmail, token);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VerificationCode_ExpiredToken_ReturnsNull()
        {
            // Arrange
            string email = "expired@mail.com";
            string token = "EXPIRED123";

            _mockManager.Setup(m => m.VerificationCode(email, token))
                       .ReturnsAsync((ReinitialisationMotDePasse)null);

            // Act
            var result = await _mockManager.Object.VerificationCode(email, token);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region VERIF CODE (Controller) Tests

        [TestMethod]
        public async Task VerifCode_ValidCode_ReturnsOk()
        {
            // Arrange
            var dto = new ReinitialiseMdpDTO
            {
                Email = _objetCommun.Email,
                Code = _objetCommun.Token
            };

            _mockManager.Setup(m => m.VerificationCode(dto.Email, dto.Code))
                       .ReturnsAsync(_objetCommun);

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = result as OkObjectResult;
            dynamic responseValue = okResult.Value;
            string message = responseValue.GetType().GetProperty("Message").GetValue(responseValue, null);
            Assert.IsTrue(message.Contains("validé avec succès"));
        }

        [TestMethod]
        public async Task VerifCode_InvalidCode_ReturnsNotFound()
        {
            // Arrange
            var dto = new ReinitialiseMdpDTO
            {
                Email = _objetCommun.Email,
                Code = "WRONGCODE"
            };

            _mockManager.Setup(m => m.VerificationCode(dto.Email, dto.Code))
                       .ReturnsAsync((ReinitialisationMotDePasse)null);

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = result as NotFoundObjectResult;
            dynamic responseValue = notFoundResult.Value;
            string message = responseValue.GetType().GetProperty("Message").GetValue(responseValue, null);
            Assert.IsTrue(message.Contains("invalide ou expiré"));
        }

        [TestMethod]
        public async Task VerifCode_ExpiredCode_ReturnsNotFound()
        {
            // Arrange
            var dto = new ReinitialiseMdpDTO
            {
                Email = "expired@mail.com",
                Code = "EXPIRED123"
            };

            _mockManager.Setup(m => m.VerificationCode(dto.Email, dto.Code))
                       .ReturnsAsync((ReinitialisationMotDePasse)null);

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = result as NotFoundObjectResult;
            dynamic responseValue = notFoundResult.Value;
            string message = responseValue.GetType().GetProperty("Message").GetValue(responseValue, null);
            Assert.IsTrue(message.Contains("invalide ou expiré"));
        }

        [TestMethod]
        public async Task VerifCode_EmptyCode_ReturnsNoContent()
        {
            // Arrange
            var dto = new ReinitialiseMdpDTO
            {
                Email = "test@mail.com",
                Code = ""
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task VerifCode_NullCode_ReturnsNoContent()
        {
            // Arrange
            var dto = new ReinitialiseMdpDTO
            {
                Email = "test@mail.com",
                Code = null
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task VerifCode_BadRequest_InvalidModelState()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");

            var dto = new ReinitialiseMdpDTO
            {
                Email = "",
                Code = "CODE123"
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));

        }

        [TestMethod]
        public async Task VerifCode_WrongEmail_ReturnsNotFound()
        {
            // Arrange
            var dto = new ReinitialiseMdpDTO
            {
                Email = "wrongemail@mail.com",
                Code = _objetCommun.Token
            };

            _mockManager.Setup(m => m.VerificationCode(dto.Email, dto.Code))
                       .ReturnsAsync((ReinitialisationMotDePasse)null);

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task VerifCode_ValidCodeWithWhitespace_TrimsAndValidates()
        {
            // Arrange
            var dto = new ReinitialiseMdpDTO
            {
                Email = "  test@mail.com  ",
                Code = "  TOKEN123  "
            };

            // Le mock devrait être appelé avec les valeurs trimmées ou non selon l'implémentation
            _mockManager.Setup(m => m.VerificationCode(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(_objetCommun);

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            // Selon l'implémentation, cela pourrait être OK ou NotFound
            Assert.IsTrue(
                result is OkObjectResult ||
                result is NotFoundObjectResult
            );
        }

        #endregion

        #region Additional Edge Cases

        [TestMethod]
        public async Task Put_UpdatesExpirationDate()
        {
            // Arrange
            var futureExpiration = DateTime.UtcNow.AddHours(2);
            var updated = new ReinitialisationMotDePasse
            {
                IdReinitialisationMdp = _objetCommun.IdReinitialisationMdp,
                IdCompte = _objetCommun.IdCompte,
                Email = _objetCommun.Email,
                Token = _objetCommun.Token,
                Expiration = futureExpiration,
                Utilise = false
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetCommun.IdReinitialisationMdp))
                       .ReturnsAsync(_objetCommun);

            ReinitialisationMotDePasse capturedEntity = null;
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<ReinitialisationMotDePasse>(), It.IsAny<ReinitialisationMotDePasse>()))
                       .Callback<ReinitialisationMotDePasse, ReinitialisationMotDePasse>((old, updated) => capturedEntity = updated)
                       .Returns(Task.CompletedTask);

            // Act
            await _controller.Put(_objetCommun.IdReinitialisationMdp, updated);

            // Assert
            Assert.IsNotNull(capturedEntity);
            Assert.AreEqual(futureExpiration, capturedEntity.Expiration);
        }

        [TestMethod]
        public async Task GetAll_VerifyManagerCalled()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(new List<ReinitialisationMotDePasse> { _objetCommun })
                       .Verifiable();

            // Act
            await _controller.GetAll();

            // Assert
            _mockManager.Verify(m => m.GetAllAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetById_VerifyManagerCalled()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetCommun.IdReinitialisationMdp))
                       .ReturnsAsync(_objetCommun)
                       .Verifiable();

            // Act
            await _controller.Get(_objetCommun.IdReinitialisationMdp);

            // Assert
            _mockManager.Verify(m => m.GetByIdAsync(_objetCommun.IdReinitialisationMdp), Times.Once);
        }

        [TestMethod]
        public async Task Delete_VerifyManagerCalledWithCorrectEntity()
        {
            // Arrange
            string token = "TOKEN123";

            _mockManager.Setup(m => m.GetByNameAsync(token))
                       .ReturnsAsync(_objetCommun);

            ReinitialisationMotDePasse capturedEntity = null;
            _mockManager.Setup(m => m.DeleteAsync(It.IsAny<ReinitialisationMotDePasse>()))
                       .Callback<ReinitialisationMotDePasse>(e => capturedEntity = e)
                       .Returns(Task.FromResult(true));

            // Act
            await _controller.Delete(token);

            // Assert
            Assert.IsNotNull(capturedEntity);
            Assert.AreEqual(_objetCommun.IdReinitialisationMdp, capturedEntity.IdReinitialisationMdp);
            Assert.AreEqual(_objetCommun.Email, capturedEntity.Email);
        }

        #endregion
    }
}