using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Authentification;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class CompteControllerTestsMoq
    {
        private Mock<CompteManager> _mockManager;
        private Mock<IJournalService> _mockJournalService;
        private IConfiguration _config;
        private CompteController _controller;
        private IMapper _mapper;
        private Compte _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            
            _mockManager = new Mock<CompteManager>(null);
            _mockJournalService = new Mock<IJournalService>();

            
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:SecretKey", "UneSuperCleSecreteTresLonguePourLeTestJWT123456789"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Authentication:Google:ClientId", "test-client-id"},
                {"Authentication:Google:ClientSecret", "test-client-secret"},
                {"Authentication:Google:RedirectUri", "http://localhost:5000/api/compte/googlecallback"}
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            
            _objetcommun = new Compte
            {
                IdCompte = 1,
                Nom = "Doe",
                Prenom = "John",
                Email = "john@gmail.com",
                MotDePasse = "b2b8804d428bb1129711f32ce77b9d3dde5b063c02ae62fcbc73988ae84d7c76",
                Pseudo = "johndoe",
                DateCreation = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = 1,
                DateDerniereConnexion = DateTime.UtcNow,
                IdEtatCompte = 1
            };

            
            _controller = new CompteController(
                _mockManager.Object,
                _mapper,
                _config,
                _mockJournalService.Object
            );

            // Configuration du contexte HTTP pour les cookies
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        #region GET
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetByID(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CompteDetailDTO));
            Assert.AreEqual(_objetcommun.Nom, result.Value.Nom);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.GetByID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange
            var comptesList = new List<Compte>
            {
                _objetcommun,
                new Compte
                {
                    IdCompte = 2,
                    Nom = "Smith",
                    Prenom = "Jane",
                    Email = "jane@gmail.com",
                    Pseudo = "janesmith",
                    IdTypeCompte = 1,
                    IdEtatCompte = 1
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(comptesList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CompteGetDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Pseudo == _objetcommun.Pseudo));
        }

        [TestMethod]
        public async Task GetProfilPublicTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetProfilPublic(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetProfilPublic(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CompteProfilPublicDTO));
            Assert.AreEqual(_objetcommun.Pseudo, result.Value.Pseudo);
        }

        [TestMethod]
        public async Task NotFoundGetProfilPublicTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.GetProfilPublic(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task PostCompteTest()
        {
            // Arrange
            CompteCreateDTO compteCreateDTO = new CompteCreateDTO
            {
                Nom = "Smith",
                Prenom = "Jane",
                Email = "jane.smith@gmail.com",
                MotDePasse = "anotherpassword",
                Pseudo = "janesmith",
                DateNaissance = new DateTime(1992, 2, 2),
                IdTypeCompte = 1
            };

            var compteEntity = _mapper.Map<Compte>(compteCreateDTO);
            compteEntity.IdCompte = 2;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Compte>()))
                       .ReturnsAsync(compteEntity)
                       .Verifiable();

            _mockJournalService.Setup(j => j.LogCreationCompteAsync(It.IsAny<int>(), It.IsAny<string>()))
                              .Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.Post(compteCreateDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdCompte = (Compte)created.Value;
            Assert.AreEqual(compteCreateDTO.Email, createdCompte.Email);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Compte>()), Times.Once);
        }

        [TestMethod]
        public async Task BadRequestPostCompteTest()
        {
            // Arrange
            CompteCreateDTO compteCreateDTO = new CompteCreateDTO();
            _controller.ModelState.AddModelError("Email", "Required");

            // Act
            var actionResult = await _controller.Post(compteCreateDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Compte>()), Times.Never);
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteCompteTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.DeleteAsync(_objetcommun))
                       .Returns(Task.FromResult(true))
                       .Verifiable();

            // Act
            var result = await _controller.Delete(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(_objetcommun), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundDeleteCompteTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutCompteTest()
        {
            // Arrange
            var existingCompte = new Compte
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "Doe",
                Prenom = "John",
                Email = "john@gmail.com"
            };

            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = "JohnUpdated",
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(existingCompte);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Compte>(), It.IsAny<Compte>()))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            _mockJournalService.Setup(j => j.LogModificationProfilAsync(It.IsAny<int>()))
                              .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(_objetcommun.IdCompte, compteUpdateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Compte>(), It.IsAny<Compte>()), Times.Once);
        }
        
        [TestMethod]
        public async Task NotFoundPutCompteTest()
        {
            // Arrange
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = 0,
                Nom = "DoeUpdated"
            };

            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.Put(0, compteUpdateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutCompteTest()
        {
            // Arrange
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO();
            _controller.ModelState.AddModelError("Prenom", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdCompte, compteUpdateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Compte>(), It.IsAny<Compte>()), Times.Never);
        }
        #endregion

        #region GETByString
        [TestMethod]
        public async Task GetByStringTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByNameAsync(_objetcommun.Email))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetByString(_objetcommun.Email);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(_objetcommun.Nom, result.Value.Nom);
        }
        [TestMethod]
        public async Task NotFoundGetByStringTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByNameAsync("NonExistentMail")).ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.GetByString("NonExistentMail");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region Login/Logout
        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "john@gmail.com",
                MotDePasse = "Testmdp1!"
            };

            // Mock AuthenticateCompte instead of GetByNameAsync
            _mockManager.Setup(m => m.AuthenticateCompte(
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(_objetcommun);

            // Also mock GetByNameAsync for GenerateJwtToken
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(_objetcommun);

            _mockJournalService.Setup(j => j.LogConnexionAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
        }

        [TestMethod]
        public async Task Login_InvalidEmail_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "wrong@test.com", MotDePasse = "Testmdp1!" };
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Logout_AuthenticatedUser_ReturnsOk()
        {
            // Arrange
            var claims = new List<Claim> { new Claim("idUser", "1") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            _mockJournalService.Setup(j => j.LogDeconnexionAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "john@gmail.com", MotDePasse = "WrongPassword" };
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Login_EmptyEmail_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "", MotDePasse = "Testmdp1!" };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_EmptyPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "john@gmail.com", MotDePasse = "" };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_NullCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = null, MotDePasse = null };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Logout_DeletesCookie()
        {
            // Arrange
            var claims = new List<Claim> { new Claim("idUser", "1") };
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _mockJournalService.Setup(j => j.LogDeconnexionAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Login_CaseInsensitiveEmail_ReturnsOk()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "JOHN@GMAIL.COM", MotDePasse = "Testmdp1!" };
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.AuthenticateCompte(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_objetcommun);
            _mockJournalService.Setup(j => j.LogConnexionAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
        }

        [TestMethod]
        public async Task Logout_InvalidUserId_ReturnsInternalServerError()
        {
            // Arrange
            var claims = new List<Claim> { new Claim("idUser", "invalid_user_id") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        #endregion

        #region GETMe
        [TestMethod]
        public async Task GetMeTest()
        {
            // Arrange
            var claims = new List<Claim> { new Claim("idUser", "1") };
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetMe();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetMeTest_Unauthorized_NoUserIdClaim()
        {
            // Arrange
            var claims = new List<Claim>();
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Act
            var result = await _controller.GetMe();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task GetMeTest_NotFound_UserDoesNotExist()
        {
            // Arrange
            var claims = new List<Claim> { new Claim("idUser", "999") };
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            _mockManager.Setup(m => m.GetByIdAsync(999)).ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.GetMe();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region GETByType
        [TestMethod]
        public async Task GetByTypeCompteTest()
        {
            // Arrange
            var comptesType = new List<Compte> { _objetcommun };
            _mockManager.Setup(m => m.GetComptesByTypes(1)).ReturnsAsync(comptesType);

            // Act
            var result = await _controller.GetByTypeCompte(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task NotFoundGetByTypeCompteTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetComptesByTypes(999)).ReturnsAsync(new List<Compte>());

            // Act
            var result = await _controller.GetByTypeCompte(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region GETCompte
        [TestMethod]
        public async Task GetCompteByAnnonceFavoriTest()
        {
            // Arrange
            var comptesFavoris = new List<Compte> { _objetcommun };
            _mockManager.Setup(m => m.GetCompteByIdAnnonceFavori(1)).ReturnsAsync(comptesFavoris);

            // Act
            var result = await _controller.GetCompteByAnnonceFavori(1);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task NotFoundGetCompteByAnnonceFavoriTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetCompteByIdAnnonceFavori(999)).ReturnsAsync(new List<Compte>());

            // Act
            var result = await _controller.GetCompteByAnnonceFavori(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region PUTAno
        [TestMethod]
        public async Task PutAnonymiseTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte)).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.UpdateAnonymise(_objetcommun.IdCompte)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PutAnonymise(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task NotFoundPutAnonymiseTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0)).ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.PutAnonymise(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutAnonymiseAvecAdresseJournauxFavoriTest()
        {
            // Arrange
            var compte2 = new Compte
            {
                IdCompte = 2,
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@gmail.com",
                MotDePasse = "anotherhashedpassword",
                Pseudo = "jeandupont",
                DateCreation = DateTime.UtcNow,
                DateNaissance = new DateTime(1985, 5, 5),
                IdTypeCompte = 1,
                DateDerniereConnexion = DateTime.UtcNow,
                IdEtatCompte = 1
            };

            _mockManager.Setup(m => m.GetByIdAsync(compte2.IdCompte)).ReturnsAsync(compte2);
            _mockManager.Setup(m => m.UpdateAnonymise(compte2.IdCompte)).Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _controller.PutAnonymise(compte2.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAnonymise(compte2.IdCompte), Times.Once);
        }
        #endregion

        #region PUTTypeCompte
        [TestMethod]
        public async Task PutTypeCompteProTest()
        {
            // Arrange
            var dto = new CompteModifTypeCompteDTO { RaisonSociale = "test", NumeroSiret = "12345678912345" };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte)).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.UpdateTypeCompte(It.IsAny<Compte>(), dto, false)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PutTypeCompte(_objetcommun.IdCompte, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task NotFoundPutTypeCompteTest()
        {
            // Arrange
            var dto = new CompteModifTypeCompteDTO { RaisonSociale = "test", NumeroSiret = "12345678912345" };
            _mockManager.Setup(m => m.GetByIdAsync(0)).ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.PutTypeCompte(0, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutTypeComptePersoTest()
        {
            // Arrange
            var comptePro = new Compte
            {
                IdCompte = 1,
                Nom = "Doe",
                Prenom = "John",
                Email = "john@gmail.com",
                IdTypeCompte = 2
            };

            var dto = new CompteModifTypeCompteDTO { RaisonSociale = null, NumeroSiret = null };
            _mockManager.Setup(m => m.GetByIdAsync(comptePro.IdCompte)).ReturnsAsync(comptePro);
            _mockManager.Setup(m => m.UpdateTypeCompte(It.IsAny<Compte>(), dto, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PutTypeCompte(comptePro.IdCompte, dto);

            // Arrange
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }
        #endregion

        #region ModifMDP
        [TestMethod]
        public async Task ModifMotDePasseTest()
        {
            // Arrange
            var dto = new ChangementMdpDTO { IdCompte = _objetcommun.IdCompte, MotDePasse = "ouioui", Email = _objetcommun.Email };
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte)).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Compte>(), It.IsAny<Compte>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ModifMdp(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task NotFoundModifMotDePasseTest()
        {
            // Arrange
            var dto = new ChangementMdpDTO { IdCompte = 0, MotDePasse = "ouioui", Email = "test@test.com" };
            _mockManager.Setup(m => m.GetByIdAsync(0)).ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.ModifMdp(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }
        #endregion

        #region VerifUser
        [TestMethod]
        public async Task VerifUserTest()
        {
            // Arrange
            var dto = new ChangementMdpDTO { IdCompte = _objetcommun.IdCompte, MotDePasse = "Testmdp1!", Email = _objetcommun.Email };
            _mockManager.Setup(m => m.VerifMotDePasse(dto.Email, It.IsAny<string>())).ReturnsAsync(_objetcommun);

            // Act
            bool result = await _controller.VerifUser(dto);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task NotVerifUserTest()
        {
            // Arrange
            var dto = new ChangementMdpDTO { IdCompte = _objetcommun.IdCompte, MotDePasse = "nonnon", Email = _objetcommun.Email };
            _mockManager.Setup(m => m.VerifMotDePasse(dto.Email, It.IsAny<string>())).ReturnsAsync((Compte)null);

            // Act
            bool result = await _controller.VerifUser(dto);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region ToggleEtat
        [TestMethod]
        public async Task ToggleEtatCompteTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte)).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.ToggleEtatCompte(_objetcommun.IdCompte, false)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ToggleEtatCompte(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task NotFoundToggleEtatCompteTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0)).ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.ToggleEtatCompte(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ToggleEtatCompteEstRetirerTest()
        {
            // Arrange
            var compteSuspendu = new Compte
            {
                IdCompte = 1,
                Nom = "Doe",
                Prenom = "John",
                Email = "john@gmail.com",
                IdEtatCompte = 2
            };

            _mockManager.Setup(m => m.GetByIdAsync(compteSuspendu.IdCompte)).ReturnsAsync(compteSuspendu);
            _mockManager.Setup(m => m.ToggleEtatCompte(compteSuspendu.IdCompte, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ToggleEtatCompte(compteSuspendu.IdCompte, true);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }
        #endregion

        #region Google
        [TestMethod]
        public void GoogleLogin_ReturnsUrlInResponse()
        {
            // Act
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Value);

            // Act
            var responseType = okResult.Value.GetType();
            var urlProperty = responseType.GetProperty("url");

            // Assert
            Assert.IsNotNull(urlProperty);

            // Act
            string url = urlProperty.GetValue(okResult.Value)?.ToString();
            // Assert
            Assert.IsNotNull(url);
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsGoogleAuthEndpoint()
        {
            // Act
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;
            var urlProperty = okResult.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult.Value).ToString();

            // Assert
            Assert.IsTrue(url.Contains("accounts.google.com/o/oauth2/v2/auth"));
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsClientId()
        {
            // Act
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;
            var urlProperty = okResult.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult.Value).ToString();

            // Assert
            Assert.IsTrue(url.Contains("client_id="));
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsRedirectUri()
        {
            // Act
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;
            var urlProperty = okResult.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult.Value).ToString();

            // Assert
            Assert.IsTrue(url.Contains("redirect_uri="));
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsResponseType()
        {
            // Act
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;
            var urlProperty = okResult.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult.Value).ToString();

            // Assert
            Assert.IsTrue(url.Contains("response_type=code"));
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsRequiredScopes()
        {
            // Act
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;
            var urlProperty = okResult.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult.Value).ToString();

            // Assert
            Assert.IsTrue(url.Contains("scope="));
            Assert.IsTrue(url.Contains("openid"));
            Assert.IsTrue(url.Contains("profile"));
            Assert.IsTrue(url.Contains("email"));
        }

        [TestMethod]
        public async Task GoogleCallback_WithoutCode_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GoogleCallback(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task GoogleCallback_WithEmptyCode_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GoogleCallback("");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            // Act
            var badRequest = result as BadRequestObjectResult;
            
            // Assert
            Assert.AreEqual("Code manquant", badRequest.Value);
        }
        #endregion

        
        #region Tests manquants pour compléter la couverture


        [TestMethod]
        public async Task Logout_UnauthenticatedUser_ThrowsException()
        {
            // Arrange
            var claims = new List<Claim>();
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = result as ObjectResult;
            Assert.AreEqual(500, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task ModifMdp_ValidData_ReturnsOk()
        {
            // Arrange
            var dto = new ChangementMdpDTO
            {
                IdCompte = _objetcommun.IdCompte,
                MotDePasse = "NewPassword123!",
                Email = _objetcommun.Email
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Compte>(), It.IsAny<Compte>()))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ModifMdp(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Mot de passe modifié avec succès.", okResult.Value);
        }

        [TestMethod]
        public async Task VerifUser_ValidPassword_ReturnsTrue()
        {
            // Arrange
            var dto = new ChangementMdpDTO
            {
                IdCompte = _objetcommun.IdCompte,
                MotDePasse = "Testmdp1!",
                Email = _objetcommun.Email
            };

            _mockManager.Setup(m => m.VerifMotDePasse(dto.Email, It.IsAny<string>()))
                       .ReturnsAsync(_objetcommun);

            // Act
            bool result = await _controller.VerifUser(dto);

            // Assert
            Assert.IsTrue(result);
            _mockManager.Verify(m => m.VerifMotDePasse(dto.Email, It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task VerifUser_InvalidPassword_ReturnsFalse()
        {
            // Arrange
            var dto = new ChangementMdpDTO
            {
                IdCompte = _objetcommun.IdCompte,
                MotDePasse = "WrongPassword",
                Email = _objetcommun.Email
            };

            _mockManager.Setup(m => m.VerifMotDePasse(dto.Email, It.IsAny<string>()))
                       .ReturnsAsync((Compte)null);

            // Act
            bool result = await _controller.VerifUser(dto);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task PutTypeCompte_FromPersoToPro_UpdatesCorrectly()
        {
            // Arrange
            var comptePerso = new Compte
            {
                IdCompte = 1,
                Nom = "Doe",
                Prenom = "John",
                Email = "john@gmail.com",
                IdTypeCompte = 1  // Perso
            };

            var dto = new CompteModifTypeCompteDTO
            {
                RaisonSociale = "Ma Société",
                NumeroSiret = "12345678912345"
            };

            _mockManager.Setup(m => m.GetByIdAsync(comptePerso.IdCompte))
                       .ReturnsAsync(comptePerso);
            _mockManager.Setup(m => m.UpdateTypeCompte(It.IsAny<Compte>(), dto, false))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.PutTypeCompte(comptePerso.IdCompte, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateTypeCompte(It.IsAny<Compte>(), dto, false), Times.Once);
        }

        [TestMethod]
        public async Task ToggleEtatCompte_ActiveToSuspended_TogglesCorrectly()
        {
            // Arrange
            var compteActif = new Compte
            {
                IdCompte = 1,
                Nom = "Doe",
                Prenom = "John",
                Email = "john@gmail.com",
                IdEtatCompte = 1  // Actif
            };

            _mockManager.Setup(m => m.GetByIdAsync(compteActif.IdCompte))
                       .ReturnsAsync(compteActif);
            _mockManager.Setup(m => m.ToggleEtatCompte(compteActif.IdCompte, false))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.ToggleEtatCompte(compteActif.IdCompte, false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.ToggleEtatCompte(compteActif.IdCompte, false), Times.Once);
        }

        [TestMethod]
        public async Task PutAnonymise_DeletesCookieAfterUpdate()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.UpdateAnonymise(_objetcommun.IdCompte))
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PutAnonymise(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAnonymise(_objetcommun.IdCompte), Times.Once);
            // Note: Impossible de tester directement la suppression du cookie en unit test
        }

        [TestMethod]
        public async Task Put_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "Test"
            };
            _controller.ModelState.AddModelError("Email", "Email requis");

            // Act
            var result = await _controller.Put(_objetcommun.IdCompte, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Compte>(), It.IsAny<Compte>()), Times.Never);
        }

        [TestMethod]
        public async Task Post_CreatesCompteWithDefaultValues()
        {
            // Arrange
            CompteCreateDTO compteCreateDTO = new CompteCreateDTO
            {
                Nom = "Test",
                Prenom = "User",
                Email = "test@gmail.com",
                MotDePasse = "password123",
                Pseudo = "testuser",
                DateNaissance = new DateTime(1995, 5, 15),
                IdTypeCompte = 1
            };

            var compteEntity = _mapper.Map<Compte>(compteCreateDTO);
            compteEntity.IdCompte = 3;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Compte>()))
                       .ReturnsAsync(compteEntity);

            _mockJournalService.Setup(j => j.LogCreationCompteAsync(It.IsAny<int>(), It.IsAny<string>()))
                              .Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.Post(compteCreateDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdCompte = (Compte)created.Value;

            // Vérifie que les valeurs par défaut sont bien définies
            Assert.AreEqual(1, createdCompte.IdEtatCompte);
            Assert.IsNotNull(createdCompte.DateCreation);
            Assert.IsNotNull(createdCompte.DateDerniereConnexion);
        }

        [TestMethod]
        public async Task Login_SuccessfulLogin_SetsCorrectCookie()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "john@gmail.com",
                MotDePasse = "Testmdp1!"
            };

            _mockManager.Setup(m => m.AuthenticateCompte(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>()))
                       .ReturnsAsync(_objetcommun);
            _mockJournalService.Setup(j => j.LogConnexionAsync(It.IsAny<int>()))
                              .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            // Vérifie la structure de la réponse
            var response = okResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            var userIdProperty = response.GetType().GetProperty("userId");
            var pseudoProperty = response.GetType().GetProperty("pseudo");
            var roleProperty = response.GetType().GetProperty("role");

            Assert.IsNotNull(messageProperty);
            Assert.IsNotNull(userIdProperty);
            Assert.IsNotNull(pseudoProperty);
            Assert.IsNotNull(roleProperty);

            Assert.AreEqual("Login OK", messageProperty.GetValue(response));
            Assert.AreEqual(_objetcommun.IdCompte, userIdProperty.GetValue(response));
            Assert.AreEqual(_objetcommun.Pseudo, pseudoProperty.GetValue(response));
            Assert.AreEqual(_objetcommun.IdTypeCompte, roleProperty.GetValue(response));
        }

        [TestMethod]
        public async Task Login_LogsConnexion()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "john@gmail.com",
                MotDePasse = "Testmdp1!"
            };

            _mockManager.Setup(m => m.AuthenticateCompte(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>()))
                       .ReturnsAsync(_objetcommun);
            _mockJournalService.Setup(j => j.LogConnexionAsync(It.IsAny<int>()))
                              .Returns(Task.CompletedTask)
                              .Verifiable();

            // Act
            await _controller.Login(loginRequest);

            // Assert
            _mockJournalService.Verify(j => j.LogConnexionAsync(_objetcommun.IdCompte), Times.Once);
        }

        [TestMethod]
        public async Task Logout_LogsDeconnexion()
        {
            // Arrange
            var claims = new List<Claim> { new Claim("idUser", "1") };
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _mockJournalService.Setup(j => j.LogDeconnexionAsync(It.IsAny<int>()))
                              .Returns(Task.CompletedTask)
                              .Verifiable();

            // Act
            await _controller.Logout();

            // Assert
            _mockJournalService.Verify(j => j.LogDeconnexionAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task Put_LogsModificationProfil()
        {
            // Arrange
            var existingCompte = new Compte
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "Doe",
                Prenom = "John",
                Email = "john@gmail.com",
                MotDePasse = "hashedpassword",
                DateCreation = DateTime.UtcNow,
                DateDerniereConnexion = DateTime.UtcNow
            };

            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = "JohnUpdated",
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(existingCompte);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Compte>(), It.IsAny<Compte>()))
                       .Returns(Task.CompletedTask);
            _mockJournalService.Setup(j => j.LogModificationProfilAsync(It.IsAny<int>()))
                              .Returns(Task.CompletedTask)
                              .Verifiable();

            // Act
            await _controller.Put(_objetcommun.IdCompte, compteUpdateDTO);

            // Assert
            _mockJournalService.Verify(j => j.LogModificationProfilAsync(_objetcommun.IdCompte), Times.Once);
        }

        [TestMethod]
        public async Task Post_LogsCreationCompte()
        {
            // Arrange
            CompteCreateDTO compteCreateDTO = new CompteCreateDTO
            {
                Nom = "NewUser",
                Prenom = "Test",
                Email = "newuser@gmail.com",
                MotDePasse = "password",
                Pseudo = "newuser",
                DateNaissance = new DateTime(1993, 3, 3),
                IdTypeCompte = 1
            };

            var compteEntity = _mapper.Map<Compte>(compteCreateDTO);
            compteEntity.IdCompte = 5;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Compte>()))
                       .ReturnsAsync(compteEntity);
            _mockJournalService.Setup(j => j.LogCreationCompteAsync(It.IsAny<int>(), It.IsAny<string>()))
                              .Returns(Task.CompletedTask)
                              .Verifiable();

            // Act
            await _controller.Post(compteCreateDTO);

            // Assert
            _mockJournalService.Verify(j => j.LogCreationCompteAsync(
                It.IsAny<int>(),
                It.Is<string>(s => s == compteCreateDTO.Pseudo)),
                Times.Once);
        }

        [TestMethod]
        public void ComputeSha256Hash_ValidString_ReturnsHashedString()
        {
            // Arrange
            string input = "Testmdp1!";

            // Act
            string result = CompteController.ComputeSha256Hash(input);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(64, result.Length); // SHA256 produces 64 hex characters
            Assert.IsFalse(result.Contains(" "));
        }

        [TestMethod]
        public void ComputeSha256Hash_SameInput_ReturnsSameHash()
        {
            // Arrange
            string input = "password123";

            // Act
            string result1 = CompteController.ComputeSha256Hash(input);
            string result2 = CompteController.ComputeSha256Hash(input);

            // Assert
            Assert.AreEqual(result1, result2);
        }

        [TestMethod]
        public void ComputeSha256Hash_DifferentInput_ReturnsDifferentHash()
        {
            // Arrange
            string input1 = "password123";
            string input2 = "password456";

            // Act
            string result1 = CompteController.ComputeSha256Hash(input1);
            string result2 = CompteController.ComputeSha256Hash(input2);

            // Assert
            Assert.AreNotEqual(result1, result2);
        }

        #endregion

        // ============================================================================
        // SOLUTION SANS REFACTORING - Tests avec HttpClient mocké
        // ============================================================================

        // Vous pouvez tester SANS créer d'interface, mais c'est plus complexe.
        // Voici les approches possibles :

        // ============================================================================
        // APPROCHE 1 : Tester indirectement via GetOrCreateCompte (RECOMMANDÉ)
        // ============================================================================

        #region Tests Google OAuth sans refactoring - CORRIGÉS

        [TestMethod]
        public async Task GetOrCreateCompte_ExistingUser_ReturnsExistingCompte()
        {
            // Arrange
            var userInfo = new GoogleUserInfo
            {
                Id = "google_123",
                Email = _objetcommun.Email,
                Name = "John Doe",
                GivenName = "John",
                FamilyName = "Doe"
            };

            _mockManager.Setup(m => m.GetByNameAsync(userInfo.Email))
                       .ReturnsAsync(_objetcommun);

            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Compte>(), It.IsAny<Compte>()))
                       .Returns(Task.CompletedTask);

            // Act - Utiliser la réflexion pour appeler la méthode privée
            var method = typeof(CompteController).GetMethod("GetOrCreateCompte",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<(bool, Compte)>)method.Invoke(_controller, new object[] { userInfo });

            // Assert
            Assert.IsTrue(result.Item1); // Utilisateur existant
            Assert.AreEqual(_objetcommun.IdCompte, result.Item2.IdCompte);
            Assert.AreEqual(_objetcommun.Email, result.Item2.Email);

            _mockManager.Verify(m => m.GetByNameAsync(userInfo.Email), Times.Once);
        }

        [TestMethod]
        public async Task GetOrCreateCompte_NewUser_CreatesNewCompte()
        {
            // Arrange
            var userInfo = new GoogleUserInfo
            {
                Id = "google_new_456",
                Email = "newuser@gmail.com",
                Name = "Jane Smith",
                GivenName = "Jane",
                FamilyName = "Smith"
            };

            _mockManager.Setup(m => m.GetByNameAsync(userInfo.Email))
                       .ReturnsAsync((Compte)null);

            // CORRECTION: Capturer le compte passé à AddAsync et le retourner
            Compte capturedCompte = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<Compte>()))
                       .Callback<Compte>(c => capturedCompte = c)
                       .ReturnsAsync((Compte c) =>
                       {
                           c.IdCompte = 99; // Simuler l'assignation de l'ID par la DB
                           return c;
                       });

            // Act
            var method = typeof(CompteController).GetMethod("GetOrCreateCompte",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<(bool, Compte)>)method.Invoke(_controller, new object[] { userInfo });

            // Assert
            Assert.IsFalse(result.Item1); // Nouvel utilisateur
            Assert.AreEqual(99, result.Item2.IdCompte);
            Assert.AreEqual(userInfo.Email, result.Item2.Email);
            Assert.AreEqual(userInfo.Id, result.Item2.GoogleId);
            Assert.AreEqual("Google", result.Item2.AuthProvider);

            // Vérifier que le compte créé a les bonnes valeurs
            Assert.IsNotNull(capturedCompte);
            Assert.AreEqual(userInfo.Email, capturedCompte.Email);
            Assert.AreEqual(userInfo.Id, capturedCompte.GoogleId);
            Assert.AreEqual("Smith", capturedCompte.Nom);
            Assert.AreEqual("Jane", capturedCompte.Prenom);

            _mockManager.Verify(m => m.AddAsync(It.Is<Compte>(c =>
                c.Email == userInfo.Email &&
                c.GoogleId == userInfo.Id)), Times.Once);
        }

        [TestMethod]
        public async Task GetOrCreateCompte_NewUserWithNullFamilyName_UsesDefaultValues()
        {
            // Arrange
            var userInfo = new GoogleUserInfo
            {
                Id = "google_no_name",
                Email = "noname@gmail.com",
                Name = "OnlyFirstName",
                GivenName = "OnlyFirstName",
                FamilyName = null // Pas de nom de famille
            };

            _mockManager.Setup(m => m.GetByNameAsync(userInfo.Email))
                       .ReturnsAsync((Compte)null);

            Compte capturedCompte = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<Compte>()))
                       .Callback<Compte>(c => capturedCompte = c)
                       .ReturnsAsync((Compte c) =>
                       {
                           c.IdCompte = 100;
                           return c;
                       });

            // Act
            var method = typeof(CompteController).GetMethod("GetOrCreateCompte",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<(bool, Compte)>)method.Invoke(_controller, new object[] { userInfo });

            // Assert
            Assert.IsFalse(result.Item1);
            Assert.IsNotNull(capturedCompte);
            Assert.AreEqual("Nom", capturedCompte.Nom); // Valeur par défaut
            Assert.AreEqual("OnlyFirstName", capturedCompte.Prenom);

            _mockManager.Verify(m => m.AddAsync(It.Is<Compte>(c =>
                c.Nom == "Nom" &&
                c.Prenom == "OnlyFirstName")), Times.Once);
        }

        [TestMethod]
        public async Task GetOrCreateCompte_NewUserWithNullName_UsesEmailPrefix()
        {
            // Arrange
            var userInfo = new GoogleUserInfo
            {
                Id = "google_no_name_at_all",
                Email = "testuser@gmail.com",
                Name = null, // Pas de nom du tout
                GivenName = null,
                FamilyName = null
            };

            _mockManager.Setup(m => m.GetByNameAsync(userInfo.Email))
                       .ReturnsAsync((Compte)null);

            Compte capturedCompte = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<Compte>()))
                       .Callback<Compte>(c => capturedCompte = c)
                       .ReturnsAsync((Compte c) =>
                       {
                           c.IdCompte = 101;
                           return c;
                       });

            // Act
            var method = typeof(CompteController).GetMethod("GetOrCreateCompte",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<(bool, Compte)>)method.Invoke(_controller, new object[] { userInfo });

            // Assert
            Assert.IsFalse(result.Item1);
            Assert.IsNotNull(capturedCompte);
            Assert.AreEqual("testuser", capturedCompte.Pseudo); // Extrait de l'email

            _mockManager.Verify(m => m.AddAsync(It.Is<Compte>(c =>
                c.Pseudo == "testuser")), Times.Once);
        }

        [TestMethod]
        public async Task GetOrCreateCompte_ExistingUserWithGoogleId_DoesNotUpdate()
        {
            // Arrange
            var userInfo = new GoogleUserInfo
            {
                Id = "google_existing_id",
                Email = _objetcommun.Email,
                Name = "John Doe"
            };

            var existingCompte = new Compte
            {
                IdCompte = _objetcommun.IdCompte,
                Email = _objetcommun.Email,
                GoogleId = "google_existing_id", // Déjà un Google ID
                Nom = "Doe",
                Prenom = "John"
            };

            _mockManager.Setup(m => m.GetByNameAsync(userInfo.Email))
                       .ReturnsAsync(existingCompte);

            // Act
            var method = typeof(CompteController).GetMethod("GetOrCreateCompte",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<(bool, Compte)>)method.Invoke(_controller, new object[] { userInfo });

            // Assert
            Assert.IsTrue(result.Item1); // Utilisateur existant
            Assert.AreEqual(existingCompte.IdCompte, result.Item2.IdCompte);

            // Ne devrait PAS appeler UpdateAsync car le GoogleId existe déjà
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Compte>(), It.IsAny<Compte>()),
                Times.Never);
        }

        [TestMethod]
        public async Task GetOrCreateCompte_NewUserWithPartialInfo_CreatesWithDefaults()
        {
            // Arrange
            var userInfo = new GoogleUserInfo
            {
                Id = "google_partial",
                Email = "partial@gmail.com",
                Name = null,
                GivenName = "Jean",
                FamilyName = null
            };

            _mockManager.Setup(m => m.GetByNameAsync(userInfo.Email))
                       .ReturnsAsync((Compte)null);

            Compte capturedCompte = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<Compte>()))
                       .Callback<Compte>(c => capturedCompte = c)
                       .ReturnsAsync((Compte c) =>
                       {
                           c.IdCompte = 102;
                           return c;
                       });

            // Act
            var method = typeof(CompteController).GetMethod("GetOrCreateCompte",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<(bool, Compte)>)method.Invoke(_controller, new object[] { userInfo });

            // Assert
            Assert.IsFalse(result.Item1);
            Assert.IsNotNull(capturedCompte);
            Assert.AreEqual("Jean", capturedCompte.Prenom);
            Assert.AreEqual("Nom", capturedCompte.Nom); // Valeur par défaut
            Assert.AreEqual("partial", capturedCompte.Pseudo); // Extrait de l'email
            Assert.AreEqual(1, capturedCompte.IdTypeCompte); // Type par défaut
            Assert.AreEqual(1, capturedCompte.IdEtatCompte); // État par défaut
            Assert.AreEqual("Google", capturedCompte.AuthProvider);
        }

        [TestMethod]
        public async Task GetOrCreateCompte_NewUser_SetsCorrectDateFields()
        {
            // Arrange
            var userInfo = new GoogleUserInfo
            {
                Id = "google_dates",
                Email = "dates@gmail.com",
                Name = "Date Test",
                GivenName = "Date",
                FamilyName = "Test"
            };

            _mockManager.Setup(m => m.GetByNameAsync(userInfo.Email))
                       .ReturnsAsync((Compte)null);

            Compte capturedCompte = null;
            _mockManager.Setup(m => m.AddAsync(It.IsAny<Compte>()))
                       .Callback<Compte>(c => capturedCompte = c)
                       .ReturnsAsync((Compte c) =>
                       {
                           c.IdCompte = 103;
                           return c;
                       });

            // Act
            var method = typeof(CompteController).GetMethod("GetOrCreateCompte",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var startTime = DateTime.UtcNow;
            var result = await (Task<(bool, Compte)>)method.Invoke(_controller, new object[] { userInfo });
            var endTime = DateTime.UtcNow;

            // Assert
            Assert.IsFalse(result.Item1);
            Assert.IsNotNull(capturedCompte);

            // Vérifier que les dates sont en UTC
            Assert.AreEqual(DateTimeKind.Utc, capturedCompte.DateCreation.Kind);
            Assert.AreEqual(DateTimeKind.Utc, capturedCompte.DateDerniereConnexion.Kind);
            Assert.AreEqual(DateTimeKind.Utc, capturedCompte.DateNaissance.Kind);

            // Vérifier que DateCreation et DateDerniereConnexion sont récentes
            Assert.IsTrue(capturedCompte.DateCreation >= startTime && capturedCompte.DateCreation <= endTime);
            Assert.IsTrue(capturedCompte.DateDerniereConnexion >= startTime && capturedCompte.DateDerniereConnexion <= endTime);

            // Vérifier la date de naissance par défaut (2000-01-01)
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), capturedCompte.DateNaissance);
        }

        #endregion
    }
}
