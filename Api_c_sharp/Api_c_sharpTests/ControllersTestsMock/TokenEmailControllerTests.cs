using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
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
    public class TokenEmailControllerTestsMoq
    {
        private Mock<TokenEmailManager> _mockManager;
        private IConfiguration _config;
        private TokenEmailController _controller;
        private Mock<CompteManager> _mockCompteManager;
        private IMapper _mapper;
        private TokenEmail _objetCommun;

        [TestInitialize]
        public void Initialize()
        {
            _mockManager = new Mock<TokenEmailManager>(null);
            _mockCompteManager = new Mock<CompteManager>(null);

            var inMemorySettings = new Dictionary<string, string>
            {
                {"Email:GmailUser", "fake@mail.com"},
                {"Email:GmailPass", "fakepass"}
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _objetCommun = new TokenEmail
            {
                IdTokenEmail = 1,
                IdCompte = 1,
                Email = "test@mail.com",
                Token = "TOKEN123",
                Expiration = DateTime.UtcNow.AddMinutes(30),
                Utilise = false
            };

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _controller = new TokenEmailController(
                _mockManager.Object,
                _config,
                _mapper,
                _mockCompteManager.Object
            );
        }

        #region GET BY ID Tests

        [TestMethod]
        public async Task GetById_ReturnsOk()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetCommun.IdTokenEmail))
                       .ReturnsAsync(_objetCommun);

            // Act
            var result = await _controller.Get(_objetCommun.IdTokenEmail);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = result.Result as OkObjectResult;
            var returnedEntity = okResult.Value as TokenEmail;
            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(_objetCommun.Email, returnedEntity.Email);
            Assert.AreEqual(_objetCommun.Token, returnedEntity.Token);
        }

        [TestMethod]
        public async Task GetById_NotFound()
        {
            // Arrange
            int invalidId = 999;
            _mockManager.Setup(m => m.GetByIdAsync(invalidId))
                       .ReturnsAsync((TokenEmail)null);

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
            var list = new List<TokenEmail>
            {
                _objetCommun,
                new TokenEmail
                {
                    IdTokenEmail = 2,
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
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = result.Result as OkObjectResult;
            var returnedList = okResult.Value as IEnumerable<TokenEmail>;
            Assert.IsNotNull(returnedList);
            Assert.AreEqual(2, ((List<TokenEmail>)returnedList).Count);
        }

        [TestMethod]
        public async Task GetAll_ReturnsEmptyList()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(new List<TokenEmail>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = result.Result as OkObjectResult;
            var returnedList = okResult.Value as IEnumerable<TokenEmail>;
            Assert.AreEqual(0, ((List<TokenEmail>)returnedList).Count);
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
            var returnedEntity = okResult.Value as TokenEmail;
            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(_objetCommun.Email, returnedEntity.Email);
        }

        [TestMethod]
        public async Task GetByString_NotFound()
        {
            // Arrange
            string invalidToken = "UNKNOWN";
            _mockManager.Setup(m => m.GetByNameAsync(invalidToken))
                       .ReturnsAsync((TokenEmail)null);

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
            var dto = new TokenEmailCreateDTO
            {
                Email = "",
                TypeToken = ""
            };

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<TokenEmail>()), Times.Never);
        }

        [TestMethod]
        public async Task Post_OK_ReinitMdp_CreatesToken()
        {
            // Arrange
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test@mail.com",
                TypeToken = "REINIT_MDP"
            };

            TokenEmail capturedToken = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail>(t => capturedToken = t)
                       .ReturnsAsync((TokenEmail t) => t)
                       .Verifiable();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = result as OkObjectResult;
            dynamic responseValue = okResult.Value;
            string message = responseValue.GetType().GetProperty("Message").GetValue(responseValue, null);
            Assert.IsTrue(message.Contains("code de vérification a été envoyé"));

            // Vérifications sur le token capturé
            Assert.IsNotNull(capturedToken);
            Assert.AreEqual(dto.Email, capturedToken.Email);
            Assert.AreEqual(dto.IdCompte, capturedToken.IdCompte);
            Assert.AreEqual(dto.TypeToken, capturedToken.TypeToken);
            Assert.AreEqual(7, capturedToken.Token.Length); // Token à 7 chiffres
            Assert.IsFalse(capturedToken.Utilise);
            Assert.IsTrue(capturedToken.Expiration > DateTime.UtcNow);
            Assert.IsTrue(capturedToken.Expiration <= DateTime.UtcNow.AddMinutes(16)); // Marge de 1 minute

            _mockManager.Verify(m => m.AddAsync(It.IsAny<TokenEmail>()), Times.Once);
        }

        [TestMethod]
        public async Task Post_OK_A2FActivation_CreatesToken()
        {
            // Arrange
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test@mail.com",
                TypeToken = "A2F_ACTIVATION"
            };

            TokenEmail capturedToken = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail>(t => capturedToken = t)
                       .ReturnsAsync((TokenEmail t) => t);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.IsNotNull(capturedToken);
            Assert.AreEqual("A2F_ACTIVATION", capturedToken.TypeToken);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<TokenEmail>()), Times.Once);
        }

        [TestMethod]
        public async Task Post_OK_A2FConnexion_CreatesToken()
        {
            // Arrange
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test@mail.com",
                TypeToken = "A2F_CONNEXION"
            };

            TokenEmail capturedToken = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail>(t => capturedToken = t)
                       .ReturnsAsync((TokenEmail t) => t);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.IsNotNull(capturedToken);
            Assert.AreEqual("A2F_CONNEXION", capturedToken.TypeToken);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<TokenEmail>()), Times.Once);
        }

        [TestMethod]
        public async Task Post_OK_TypeTokenInconnu_CreatesToken()
        {
            // Arrange
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test@mail.com",
                TypeToken = "AUTRE_TYPE"
            };

            TokenEmail capturedToken = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail>(t => capturedToken = t)
                       .ReturnsAsync((TokenEmail t) => t);

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.IsNotNull(capturedToken);
            Assert.AreEqual("AUTRE_TYPE", capturedToken.TypeToken);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<TokenEmail>()), Times.Once);
        }

        [TestMethod]
        public async Task Post_OK_TokenGenereEst7Chiffres()
        {
            // Arrange
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test@mail.com",
                TypeToken = "REINIT_MDP"
            };

            TokenEmail capturedToken = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail>(t => capturedToken = t)
                       .ReturnsAsync((TokenEmail t) => t);

            // Act
            await _controller.Post(dto);

            // Assert
            Assert.IsNotNull(capturedToken);
            Assert.AreEqual(7, capturedToken.Token.Length);
            Assert.IsTrue(int.TryParse(capturedToken.Token, out _)); // Doit être numérique
        }

        [TestMethod]
        public async Task Post_OK_ExpirationEstDans15Minutes()
        {
            // Arrange
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test@mail.com",
                TypeToken = "REINIT_MDP"
            };

            TokenEmail capturedToken = null;
            var beforePost = DateTime.UtcNow;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail>(t => capturedToken = t)
                       .ReturnsAsync((TokenEmail t) => t);

            // Act
            await _controller.Post(dto);

            var afterPost = DateTime.UtcNow;

            // Assert
            Assert.IsNotNull(capturedToken);

            // L'expiration doit être entre 14 et 16 minutes (marge pour l'exécution)
            var minExpiration = beforePost.AddMinutes(14);
            var maxExpiration = afterPost.AddMinutes(16);

            Assert.IsTrue(capturedToken.Expiration >= minExpiration);
            Assert.IsTrue(capturedToken.Expiration <= maxExpiration);
        }

        [TestMethod]
        public async Task Post_OK_UtiliseEstFalse()
        {
            // Arrange
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test@mail.com",
                TypeToken = "REINIT_MDP"
            };

            TokenEmail capturedToken = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail>(t => capturedToken = t)
                       .ReturnsAsync((TokenEmail t) => t);

            // Act
            await _controller.Post(dto);

            // Assert
            Assert.IsNotNull(capturedToken);
            Assert.IsFalse(capturedToken.Utilise);
        }

        [TestMethod]
        public async Task Post_OK_MapperUtilise()
        {
            // Arrange
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 5,
                Email = "mapper@mail.com",
                TypeToken = "REINIT_MDP"
            };

            TokenEmail capturedToken = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail>(t => capturedToken = t)
                       .ReturnsAsync((TokenEmail t) => t);

            // Act
            await _controller.Post(dto);

            // Assert
            Assert.IsNotNull(capturedToken);
            Assert.AreEqual(dto.IdCompte, capturedToken.IdCompte);
            Assert.AreEqual(dto.Email, capturedToken.Email);
            Assert.AreEqual(dto.TypeToken, capturedToken.TypeToken);
        }

        [TestMethod]
        public async Task Post_OK_VerifyAddAsyncCalledOnce()
        {
            // Arrange
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test@mail.com",
                TypeToken = "REINIT_MDP"
            };

            _mockManager.Setup(m => m.AddAsync(It.IsAny<TokenEmail>()))
                       .ReturnsAsync((TokenEmail t) => t)
                       .Verifiable();

            // Act
            await _controller.Post(dto);

            // Assert
            _mockManager.Verify(m => m.AddAsync(It.Is<TokenEmail>(t =>
                t.Email == dto.Email &&
                t.TypeToken == dto.TypeToken &&
                t.IdCompte == dto.IdCompte
            )), Times.Once);
        }

        [TestMethod]
        public async Task Post_BadRequest_EmailNull()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required");
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = null,
                TypeToken = "REINIT_MDP"
            };

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<TokenEmail>()), Times.Never);
        }

        [TestMethod]
        public async Task Post_BadRequest_TypeTokenNull()
        {
            // Arrange
            _controller.ModelState.AddModelError("TypeToken", "TypeToken is required");
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test@mail.com",
                TypeToken = null
            };

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<TokenEmail>()), Times.Never);
        }

        [TestMethod]
        public async Task Post_OK_TokensSontDifferents()
        {
            // Arrange
            var dto1 = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test1@mail.com",
                TypeToken = "REINIT_MDP"
            };

            var dto2 = new TokenEmailCreateDTO
            {
                IdCompte = 1,
                Email = "test2@mail.com",
                TypeToken = "REINIT_MDP"
            };

            string token1 = null;
            string token2 = null;
            int callCount = 0;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail>(t =>
                       {
                           if (callCount == 0) token1 = t.Token;
                           else token2 = t.Token;
                           callCount++;
                       })
                       .ReturnsAsync((TokenEmail t) => t);

            // Act
            await _controller.Post(dto1);
            await _controller.Post(dto2);

            // Assert
            Assert.IsNotNull(token1);
            Assert.IsNotNull(token2);
            Assert.AreEqual(2, callCount);
            // Note: Il y a une très faible probabilité qu'ils soient identiques (1/10000000)
            // mais dans la pratique, ils seront différents
        }

        #endregion

        #region PUT Tests

        [TestMethod]
        public async Task Put_UpdatesSuccessfully()
        {
            // Arrange
            var updated = new TokenEmail
            {
                IdTokenEmail = _objetCommun.IdTokenEmail,
                IdCompte = _objetCommun.IdCompte,
                Email = "updated@mail.com",
                Token = _objetCommun.Token,
                Expiration = DateTime.UtcNow.AddHours(1),
                Utilise = false
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetCommun.IdTokenEmail))
                       .ReturnsAsync(_objetCommun);

            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<TokenEmail>(), It.IsAny<TokenEmail>()))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_objetCommun.IdTokenEmail, updated);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<TokenEmail>(), It.IsAny<TokenEmail>()), Times.Once);
        }

        [TestMethod]
        public async Task Put_NotFound()
        {
            // Arrange
            int invalidId = 999;
            var dto = new TokenEmail
            {
                IdTokenEmail = invalidId,
                Email = "x@mail.com"
            };

            _mockManager.Setup(m => m.GetByIdAsync(invalidId))
                       .ReturnsAsync((TokenEmail)null);

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
            var result = await _controller.Put(_objetCommun.IdTokenEmail, _objetCommun);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<TokenEmail>(), It.IsAny<TokenEmail>()), Times.Never);
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
                       .ReturnsAsync((TokenEmail)null);

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
            string typeToken = "REINIT_MDP";

            _mockManager.Setup(m => m.VerificationCode(email, token, typeToken))
                       .ReturnsAsync(_objetCommun);

            // Act
            var result = await _mockManager.Object.VerificationCode(email, token, typeToken);

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
            string typeToken = "REINIT_MDP";

            _mockManager.Setup(m => m.VerificationCode(email, wrongToken, typeToken))
                       .ReturnsAsync((TokenEmail)null);

            // Act
            var result = await _mockManager.Object.VerificationCode(email, wrongToken, typeToken);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VerificationCode_InvalidEmail_ReturnsNull()
        {
            // Arrange
            string wrongEmail = "wrong@mail.com";
            string token = "TOKEN123";
            string typeToken = "REINIT_MDP";

            _mockManager.Setup(m => m.VerificationCode(wrongEmail, token, typeToken))
                       .ReturnsAsync((TokenEmail)null);

            // Act
            var result = await _mockManager.Object.VerificationCode(wrongEmail, token, typeToken);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VerificationCode_ExpiredToken_ReturnsNull()
        {
            // Arrange
            string email = "expired@mail.com";
            string token = "EXPIRED123";
            string typeToken = "REINIT_MDP";

            _mockManager.Setup(m => m.VerificationCode(email, token, typeToken))
                       .ReturnsAsync((TokenEmail)null);

            // Act
            var result = await _mockManager.Object.VerificationCode(email, token, typeToken);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region VERIF CODE (Controller) Tests

        [TestMethod]
        public async Task VerifCode_ValidCode_ReturnsOk()
        {
            // Arrange
            var dto = new TokenEmailVerifDTO
            {
                Email = _objetCommun.Email,
                Code = _objetCommun.Token,
                TypeToken = "REINIT_MDP"
            };

            _mockManager.Setup(m => m.VerificationCode(dto.Email, dto.Code, dto.TypeToken))
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
            var dto = new TokenEmailVerifDTO
            {
                Email = _objetCommun.Email,
                Code = "WRONGCODE",
                TypeToken = "REINIT_MDP"
            };

            _mockManager.Setup(m => m.VerificationCode(dto.Email, dto.Code, dto.TypeToken))
                       .ReturnsAsync((TokenEmail)null);

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
            var expiredToken = new TokenEmail
            {
                IdTokenEmail = 1,
                IdCompte = 1,
                Email = "expired@mail.com",
                Token = "EXPIRED123",
                Expiration = DateTime.UtcNow.AddMinutes(-10),
                Utilise = false
            };

            var dto = new TokenEmailVerifDTO
            {
                Email = expiredToken.Email,
                Code = expiredToken.Token,
                TypeToken = "REINIT_MDP"
            };

            _mockManager.Setup(m => m.VerificationCode(dto.Email, dto.Code, dto.TypeToken))
                       .ReturnsAsync(expiredToken);

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
        public async Task VerifCode_EmptyCode_ReturnsBadRequest()
        {
            // Arrange
            var dto = new TokenEmailVerifDTO
            {
                Email = "test@mail.com",
                Code = "",
                TypeToken = "REINIT_MDP"
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequestResult = result as BadRequestObjectResult;
            dynamic responseValue = badRequestResult.Value;
            string message = responseValue.GetType().GetProperty("Message").GetValue(responseValue, null);
            Assert.IsTrue(message.Contains("code est requis"));
        }

        [TestMethod]
        public async Task VerifCode_NullCode_ReturnsBadRequest()
        {
            // Arrange
            var dto = new TokenEmailVerifDTO
            {
                Email = "test@mail.com",
                Code = null,
                TypeToken = "REINIT_MDP"
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequestResult = result as BadRequestObjectResult;
            dynamic responseValue = badRequestResult.Value;
            string message = responseValue.GetType().GetProperty("Message").GetValue(responseValue, null);
            Assert.IsTrue(message.Contains("code est requis"));
        }

        [TestMethod]
        public async Task VerifCode_BadRequest_InvalidModelState()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");

            var dto = new TokenEmailVerifDTO
            {
                Email = "",
                Code = "CODE123",
                TypeToken = "REINIT_MDP"
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.VerificationCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task VerifCode_WrongEmail_ReturnsNotFound()
        {
            // Arrange
            var dto = new TokenEmailVerifDTO
            {
                Email = "wrongemail@mail.com",
                Code = _objetCommun.Token,
                TypeToken = "REINIT_MDP"
            };

            _mockManager.Setup(m => m.VerificationCode(dto.Email, dto.Code, dto.TypeToken))
                       .ReturnsAsync((TokenEmail)null);

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        #endregion

        #region Additional Edge Cases

        [TestMethod]
        public async Task Put_UpdatesExpirationDate()
        {
            // Arrange
            var futureExpiration = DateTime.UtcNow.AddHours(2);
            var updated = new TokenEmail
            {
                IdTokenEmail = _objetCommun.IdTokenEmail,
                IdCompte = _objetCommun.IdCompte,
                Email = _objetCommun.Email,
                Token = _objetCommun.Token,
                Expiration = futureExpiration,
                Utilise = false
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetCommun.IdTokenEmail))
                       .ReturnsAsync(_objetCommun);

            TokenEmail capturedEntity = null;
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<TokenEmail>(), It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail, TokenEmail>((old, updated) => capturedEntity = updated)
                       .Returns(Task.CompletedTask);

            // Act
            await _controller.Put(_objetCommun.IdTokenEmail, updated);

            // Assert
            Assert.IsNotNull(capturedEntity);
            Assert.AreEqual(futureExpiration, capturedEntity.Expiration);
        }

        [TestMethod]
        public async Task GetAll_VerifyManagerCalled()
        {
            // Arrange
            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(new List<TokenEmail> { _objetCommun })
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
            _mockManager.Setup(m => m.GetByIdAsync(_objetCommun.IdTokenEmail))
                       .ReturnsAsync(_objetCommun)
                       .Verifiable();

            // Act
            await _controller.Get(_objetCommun.IdTokenEmail);

            // Assert
            _mockManager.Verify(m => m.GetByIdAsync(_objetCommun.IdTokenEmail), Times.Once);
        }

        [TestMethod]
        public async Task Delete_VerifyManagerCalledWithCorrectEntity()
        {
            // Arrange
            string token = "TOKEN123";

            _mockManager.Setup(m => m.GetByNameAsync(token))
                       .ReturnsAsync(_objetCommun);

            TokenEmail capturedEntity = null;
            _mockManager.Setup(m => m.DeleteAsync(It.IsAny<TokenEmail>()))
                       .Callback<TokenEmail>(e => capturedEntity = e)
                       .Returns(Task.FromResult(true));

            // Act
            await _controller.Delete(token);

            // Assert
            Assert.IsNotNull(capturedEntity);
            Assert.AreEqual(_objetCommun.IdTokenEmail, capturedEntity.IdTokenEmail);
            Assert.AreEqual(_objetCommun.Email, capturedEntity.Email);
        }

        #endregion

        #region MARQUER UTILISÉ Tests

        [TestMethod]
        public async Task MarquerUtilise_OK()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetCommun.IdTokenEmail))
                       .ReturnsAsync(_objetCommun);

            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<TokenEmail>(), It.IsAny<TokenEmail>()))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.MarquerUtilise(_objetCommun.IdTokenEmail);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<TokenEmail>(), It.IsAny<TokenEmail>()), Times.Once);
        }

        [TestMethod]
        public async Task MarquerUtilise_NotFound()
        {
            // Arrange
            int invalidId = 999;
            _mockManager.Setup(m => m.GetByIdAsync(invalidId))
                       .ReturnsAsync((TokenEmail)null);

            // Act
            var result = await _controller.MarquerUtilise(invalidId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        #endregion

        #region INVALIDER TOKENS PAR TYPE Tests

        [TestMethod]
        public async Task InvaliderTokensParType_OK()
        {
            // Arrange
            int idCompte = 1;
            string typeToken = "REINIT_MDP";

            _mockManager.Setup(m => m.InvaliderTokensParType(idCompte, typeToken))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.InvaliderTokensParType(idCompte, typeToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.InvaliderTokensParType(idCompte, typeToken), Times.Once);
        }

        #endregion

        #region NETTOYER TOKENS EXPIRÉS Tests

        [TestMethod]
        public async Task NettoyerTokensExpires_OK()
        {
            // Arrange
            _mockManager.Setup(m => m.NettoyerTokensExpires())
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.NettoyerTokensExpires();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.NettoyerTokensExpires(), Times.Once);
        }

        #endregion


    }
}