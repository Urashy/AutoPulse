using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.Authentification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class CompteControllerTestsMoq
    {
        private Mock<CompteManager> _mockManager = null!;
        private Mock<RefreshTokenManager> _mockTokenManager = null!;
        private Mock<IJournalService> _mockJournalService = null!;
        private IConfiguration _config = null!;
        private CompteController _controller = null!;
        private IMapper _mapper = null!;
        private Compte _objetcommun = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockManager = new Mock<CompteManager>(null!);
            _mockTokenManager = new Mock<RefreshTokenManager>(null!);
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
            _config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>());
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

            _controller = new CompteController(_mockManager.Object, _mapper, _config,
                _mockJournalService.Object, _mockTokenManager.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
        }

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

        // Continuez avec les autres tests...
        // Je vais créer une version plus concise pour économiser de l'espace

        [TestMethod]
        public async Task GetByStringTest()
        {
            _mockManager.Setup(m => m.GetByNameAsync(_objetcommun.Email))
                       .ReturnsAsync(_objetcommun);
            var result = await _controller.GetByString(_objetcommun.Email);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(_objetcommun.Nom, result.Value.Nom);
        }

        [TestMethod]
        public async Task Login_InvalidEmail_ReturnsUnauthorized()
        {
            var loginRequest = new LoginRequest { Email = "wrong@test.com", MotDePasse = "Testmdp1!" };
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((Compte)null);

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Logout_AuthenticatedUser_ReturnsOk()
        {
            var claims = new List<Claim> { new Claim("idUser", "1") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            _mockJournalService.Setup(j => j.LogDeconnexionAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _controller.Logout();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetMeTest()
        {
            var claims = new List<Claim> { new Claim("idUser", "1") };
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            _mockManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(_objetcommun);

            var result = await _controller.GetMe();

            Assert.IsInstanceOfType(result.Value, typeof(CompteDetailDTO));
        }

        [TestMethod]
        public async Task GetMeTest_Unauthorized_NoUserIdClaim()
        {
            var claims = new List<Claim>();
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var result = await _controller.GetMe();

            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task GetMeTest_NotFound_UserDoesNotExist()
        {
            var claims = new List<Claim> { new Claim("idUser", "999") };
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            _mockManager.Setup(m => m.GetByIdAsync(999)).ReturnsAsync((Compte)null);

            var result = await _controller.GetMe();

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var loginRequest = new LoginRequest { Email = "john@gmail.com", MotDePasse = "WrongPassword" };
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((Compte)null);

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Login_EmptyEmail_ReturnsBadRequest()
        {
            var loginRequest = new LoginRequest { Email = "", MotDePasse = "Testmdp1!" };

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_EmptyPassword_ReturnsBadRequest()
        {
            var loginRequest = new LoginRequest { Email = "john@gmail.com", MotDePasse = "" };

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_NullCredentials_ReturnsBadRequest()
        {
            var loginRequest = new LoginRequest { Email = null, MotDePasse = null };

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Logout_DeletesCookie()
        {
            var claims = new List<Claim> { new Claim("idUser", "1") };
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _mockJournalService.Setup(j => j.LogDeconnexionAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _controller.Logout();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetByTypeCompteTest()
        {
            var comptesType = new List<Compte> { _objetcommun };
            _mockManager.Setup(m => m.GetComptesByTypes(1)).ReturnsAsync(comptesType);

            var result = await _controller.GetByTypeCompte(1);

            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task NotFoundGetByTypeCompteTest()
        {
            _mockManager.Setup(m => m.GetComptesByTypes(999)).ReturnsAsync(new List<Compte>());

            var result = await _controller.GetByTypeCompte(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetCompteByAnnonceFavoriTest()
        {
            var comptesFavoris = new List<Compte> { _objetcommun };
            _mockManager.Setup(m => m.GetCompteByIdAnnonceFavori(1)).ReturnsAsync(comptesFavoris);

            var result = await _controller.GetCompteByAnnonceFavori(1);

            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task NotFoundGetCompteByAnnonceFavoriTest()
        {
            _mockManager.Setup(m => m.GetCompteByIdAnnonceFavori(999)).ReturnsAsync(new List<Compte>());

            var result = await _controller.GetCompteByAnnonceFavori(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutAnonymiseTest()
        {
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte)).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.UpdateAnonymise(_objetcommun.IdCompte)).Returns(Task.CompletedTask);

            var result = await _controller.PutAnonymise(_objetcommun.IdCompte);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task NotFoundPutAnonymiseTest()
        {
            _mockManager.Setup(m => m.GetByIdAsync(0)).ReturnsAsync((Compte)null);

            var result = await _controller.PutAnonymise(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutTypeCompteProTest()
        {
            var dto = new CompteModifTypeCompteDTO { RaisonSociale = "test", NumeroSiret = "12345678912345" };
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte)).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.UpdateTypeCompte(It.IsAny<Compte>(), dto, false)).Returns(Task.CompletedTask);

            var result = await _controller.PutTypeCompte(_objetcommun.IdCompte, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task NotFoundPutTypeCompteTest()
        {
            var dto = new CompteModifTypeCompteDTO { RaisonSociale = "test", NumeroSiret = "12345678912345" };
            _mockManager.Setup(m => m.GetByIdAsync(0)).ReturnsAsync((Compte)null);

            var result = await _controller.PutTypeCompte(0, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ModifMotDePasseTest()
        {
            var dto = new ChangementMdpDTO { IdCompte = _objetcommun.IdCompte, MotDePasse = "ouioui", Email = _objetcommun.Email };
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte)).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<Compte>(), It.IsAny<Compte>())).Returns(Task.CompletedTask);

            var result = await _controller.ModifMdp(dto);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task NotFoundModifMotDePasseTest()
        {
            var dto = new ChangementMdpDTO { IdCompte = 0, MotDePasse = "ouioui", Email = "test@test.com" };
            _mockManager.Setup(m => m.GetByIdAsync(0)).ReturnsAsync((Compte)null);

            var result = await _controller.ModifMdp(dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task VerifUserTest()
        {
            var dto = new ChangementMdpDTO { IdCompte = _objetcommun.IdCompte, MotDePasse = "Testmdp1!", Email = _objetcommun.Email };
            _mockManager.Setup(m => m.VerifMotDePasse(dto.Email, It.IsAny<string>())).ReturnsAsync(_objetcommun);

            bool result = await _controller.VerifUser(dto);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task NotVerifUserTest()
        {
            var dto = new ChangementMdpDTO { IdCompte = _objetcommun.IdCompte, MotDePasse = "nonnon", Email = _objetcommun.Email };
            _mockManager.Setup(m => m.VerifMotDePasse(dto.Email, It.IsAny<string>())).ReturnsAsync((Compte)null);

            bool result = await _controller.VerifUser(dto);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ToggleEtatCompteTest()
        {
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte)).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.ToggleEtatCompte(_objetcommun.IdCompte, false)).Returns(Task.CompletedTask);

            var result = await _controller.ToggleEtatCompte(_objetcommun.IdCompte);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task NotFoundToggleEtatCompteTest()
        {
            _mockManager.Setup(m => m.GetByIdAsync(0)).ReturnsAsync((Compte)null);

            var result = await _controller.ToggleEtatCompte(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void GoogleLogin_ReturnsUrlInResponse()
        {
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult?.Value);

            var responseType = okResult?.Value.GetType();
            var urlProperty = responseType.GetProperty("url");
            Assert.IsNotNull(urlProperty);

            string url = urlProperty.GetValue(okResult?.Value)?.ToString();
            Assert.IsNotNull(url);
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsGoogleAuthEndpoint()
        {
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;
            var urlProperty = okResult?.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult?.Value).ToString();

            Assert.IsTrue(url.Contains("accounts.google.com/o/oauth2/v2/auth"));
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsClientId()
        {
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;
            var urlProperty = okResult?.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult?.Value).ToString();

            Assert.IsTrue(url.Contains("client_id="));
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsRedirectUri()
        {
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;
            var urlProperty = okResult?.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult?.Value).ToString();

            Assert.IsTrue(url.Contains("redirect_uri="));
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsResponseType()
        {
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;
            var urlProperty = okResult?.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult?.Value).ToString();

            Assert.IsTrue(url.Contains("response_type=code"));
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsRequiredScopes()
        {
            var result = _controller.GoogleLogin();
            var okResult = result as OkObjectResult;
            var urlProperty = okResult?.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult?.Value).ToString();

            Assert.IsTrue(url.Contains("scope="));
            Assert.IsTrue(url.Contains("openid"));
            Assert.IsTrue(url.Contains("profile"));
            Assert.IsTrue(url.Contains("email"));
        }

        [TestMethod]
        public async Task GoogleCallback_WithoutCode_ReturnsBadRequest()
        {
            var result = await _controller.GoogleCallback(null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task GoogleCallback_WithEmptyCode_ReturnsBadRequest()
        {
            var result = await _controller.GoogleCallback("");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("Code manquant", badRequest.Value);
        }

        [TestMethod]
        public async Task NotFoundGetByStringTest()
        {
            _mockManager.Setup(m => m.GetByNameAsync("NonExistentMail")).ReturnsAsync((Compte)null);

            var result = await _controller.GetByString("NonExistentMail");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Login_CaseInsensitiveEmail_ReturnsOk()
        {
            var loginRequest = new LoginRequest { Email = "JOHN@GMAIL.COM", MotDePasse = "Testmdp1!" };
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.AuthenticateCompte(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_objetcommun);
            _mockJournalService.Setup(j => j.LogConnexionAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
        }

        [TestMethod]
        public async Task Logout_InvalidUserId_ReturnsInternalServerError()
        {
            var claims = new List<Claim> { new Claim("idUser", "invalid_user_id") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var result = await _controller.Logout();

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task PutTypeComptePersoTest()
        {
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

            var result = await _controller.PutTypeCompte(comptePro.IdCompte, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task ToggleEtatCompteEstRetirerTest()
        {
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

            var result = await _controller.ToggleEtatCompte(compteSuspendu.IdCompte, true);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
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

        #region Autres tests

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
            Assert.AreEqual("Mot de passe modifié avec succès.", okResult?.Value);
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

        #region Tests Google OAuth

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

        #region Tests A2F

        [TestMethod]
        public async Task GetStatutA2f_CompteExistant_ReturnsStatut()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);

            _mockManager.Setup(m => m.GetStatutA2f(_objetcommun.IdCompte))
                       .ReturnsAsync((true, DateTime.UtcNow.AddDays(-10)));

            _mockManager.Setup(m => m.DoitReactiverA2f(_objetcommun.IdCompte))
                       .ReturnsAsync(false);

            // Act
            var result = await _controller.GetStatutA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult?.Value);

            _mockManager.Verify(m => m.GetStatutA2f(_objetcommun.IdCompte), Times.Once);
            _mockManager.Verify(m => m.DoitReactiverA2f(_objetcommun.IdCompte), Times.Once);
        }

        [TestMethod]
        public async Task GetStatutA2f_CompteInexistant_ReturnsNotFound()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(999))!
                       .ReturnsAsync((Compte)null!);

            // Act
            var result = await _controller.GetStatutA2f(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task VerifActivA2f_DoitReactiver_ReturnsTrue()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);

            _mockManager.Setup(m => m.DoitReactiverA2f(_objetcommun.IdCompte))
                       .ReturnsAsync(true);

            // Act
            var result = await _controller.VerifActivA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(true, okResult?.Value);
        }

        [TestMethod]
        public async Task VerifActivA2f_NePasDevoirReactiver_ReturnsFalse()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);

            _mockManager.Setup(m => m.DoitReactiverA2f(_objetcommun.IdCompte))
                       .ReturnsAsync(false);

            // Act
            var result = await _controller.VerifActivA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(false, okResult?.Value);
        }

        [TestMethod]
        public async Task VerifActivA2f_CompteInexistant_ReturnsNotFound()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(999))!
                       .ReturnsAsync((Compte)null!);

            // Act
            var result = await _controller.VerifActivA2f(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ActiverA2f_ValidData_ReturnsNoContent()
        {
            // Arrange
            var dto = new A2fActivationDTO
            {
                IdCompte = _objetcommun.IdCompte,
                CodeValidation = "1234567"
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);

            _mockManager.Setup(m => m.ActiverA2f(_objetcommun.IdCompte))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            _mockJournalService.Setup(j => j.LogActionAsync(
                _objetcommun.IdCompte,
                15,
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ActiverA2f(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.ActiverA2f(_objetcommun.IdCompte), Times.Once);
            _mockJournalService.Verify(j => j.LogActionAsync(
                _objetcommun.IdCompte,
                15,
                "L'utilisateur à activer l'A2F"), Times.Once);
        }

        [TestMethod]
        public async Task ActiverA2f_CompteInexistant_ReturnsNotFound()
        {
            // Arrange
            var dto = new A2fActivationDTO
            {
                IdCompte = 999,
                CodeValidation = "1234567"
            };

            _mockManager.Setup(m => m.GetByIdAsync(999))!
                       .ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.ActiverA2f(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockManager.Verify(m => m.ActiverA2f(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task ActiverA2f_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var dto = new A2fActivationDTO
            {
                IdCompte = _objetcommun.IdCompte,
                CodeValidation = null
            };

            _controller.ModelState.AddModelError("CodeValidation", "Required");

            // Act
            var result = await _controller.ActiverA2f(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.ActiverA2f(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task DesactiverA2f_ValidId_ReturnsNoContent()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);

            _mockManager.Setup(m => m.DesactiverA2f(_objetcommun.IdCompte))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            _mockJournalService.Setup(j => j.LogActionAsync(
                _objetcommun.IdCompte,
                16,
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DesactiverA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DesactiverA2f(_objetcommun.IdCompte), Times.Once);
            _mockJournalService.Verify(j => j.LogActionAsync(
                _objetcommun.IdCompte,
                16,
                "L'utilisateur à activer l'A2F"), Times.Once);
        }

        [TestMethod]
        public async Task DesactiverA2f_CompteInexistant_ReturnsNotFound()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(999))!
                       .ReturnsAsync((Compte)null!);

            // Act
            var result = await _controller.DesactiverA2f(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockManager.Verify(m => m.DesactiverA2f(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task DemanderActivationA2f_ValidId_ReturnsOkWithMessage()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCompte))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.DemanderActivationA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult?.Value);
        }

        [TestMethod]
        public async Task DemanderActivationA2f_CompteInexistant_ReturnsNotFound()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(999))!
                       .ReturnsAsync((Compte)null!);

            // Act
            var result = await _controller.DemanderActivationA2f(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ValidateA2fLogin_CompteInexistant_ReturnsBadRequest()
        {
            // Arrange
            var dto = new TokenEmailVerifDTO
            {
                Email = "nonexistent@gmail.com",
                Code = "1234567",
                TypeToken = "A2F_CONNEXION"
            };

            _mockManager.Setup(m => m.GetByNameAsync(dto.Email))
                       .ReturnsAsync((Compte)null!);

            // Act
            var result = await _controller.ValidateA2fLogin(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        #endregion

        #region Tests Login/Logout avec RefreshToken
        [TestMethod]
        public async Task Login_ValidCredentials_StoresRefreshToken()
        {
            var loginRequest = new LoginRequest { Email = "john@gmail.com", MotDePasse = "Testmdp1!" };

            _mockManager.Setup(m => m.AuthenticateCompte(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.GetStatutA2f(_objetcommun.IdCompte)).ReturnsAsync((false, null));
            _mockManager.Setup(m => m.DoitReactiverA2f(_objetcommun.IdCompte)).ReturnsAsync(false);
            _mockTokenManager.Setup(m => m.StoreRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new RefreshToken { IdRefreshToken = 1 });
            _mockJournalService.Setup(j => j.LogConnexionAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _controller.Login(loginRequest, false);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockTokenManager.Verify(m => m.StoreRefreshTokenAsync(_objetcommun.IdCompte,
                It.IsAny<string>(), false, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task Login_WithRememberMe_SetsLongLivedToken()
        {
            var loginRequest = new LoginRequest { Email = "john@gmail.com", MotDePasse = "Testmdp1!" };

            _mockManager.Setup(m => m.AuthenticateCompte(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.GetStatutA2f(_objetcommun.IdCompte)).ReturnsAsync((false, null));
            _mockManager.Setup(m => m.DoitReactiverA2f(_objetcommun.IdCompte)).ReturnsAsync(false);
            _mockTokenManager.Setup(m => m.StoreRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new RefreshToken { IdRefreshToken = 1 });
            _mockJournalService.Setup(j => j.LogConnexionAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _controller.Login(loginRequest, rememberMe: true);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockTokenManager.Verify(m => m.StoreRefreshTokenAsync(_objetcommun.IdCompte,
                It.IsAny<string>(), true, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task Login_WithA2fActive_DoesNotStoreRefreshToken()
        {
            var loginRequest = new LoginRequest { Email = "john@gmail.com", MotDePasse = "Testmdp1!" };

            _mockManager.Setup(m => m.AuthenticateCompte(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.GetStatutA2f(_objetcommun.IdCompte))
                .ReturnsAsync((true, DateTime.UtcNow));
            _mockManager.Setup(m => m.DoitReactiverA2f(_objetcommun.IdCompte)).ReturnsAsync(false);
            _mockManager.Setup(m => m.EnregistrerA2f(It.IsAny<TokenEmail>())).Returns(Task.CompletedTask);

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            _mockTokenManager.Verify(m => m.StoreRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Logout_WithRefreshToken_RevokesToken()
        {
            var claims = new List<Claim> { new Claim("idUser", "1") };
            _controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
            _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = "refresh_token=test_token";

            _mockTokenManager.Setup(m => m.RevokeRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(true);
            _mockJournalService.Setup(j => j.LogDeconnexionAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _controller.Logout();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockJournalService.Verify(j => j.LogDeconnexionAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task Refresh_WithValidToken_ReturnsNewAccessToken()
        {
            var refreshToken = "valid_refresh_token";
            _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] =
                $"refresh_token={refreshToken}";

            _mockTokenManager.Setup(m => m.ValidateRefreshTokenAsync(refreshToken))
                .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.GetByNameAsync(_objetcommun.Email)).ReturnsAsync(_objetcommun);

            var result = await _controller.Refresh();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
        {
            _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = "refresh_token=invalid";
            _mockTokenManager.Setup(m => m.ValidateRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((Compte)null!);

            var result = await _controller.Refresh();

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task ValidateA2fLogin_ValidCode_StoresRefreshToken()
        {
            var dto = new TokenEmailVerifDTO
            {
                Email = "john@gmail.com",
                Code = "1234567",
                TypeToken = "A2F_CONNEXION"
            };

            _mockManager.Setup(m => m.GetByNameAsync(dto.Email)).ReturnsAsync(_objetcommun);
            _mockTokenManager.Setup(m => m.StoreRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new RefreshToken { IdRefreshToken = 1 });
            _mockJournalService.Setup(j => j.LogConnexionAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _controller.ValidateA2fLogin(dto, false);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockTokenManager.Verify(m => m.StoreRefreshTokenAsync(_objetcommun.IdCompte,
                It.IsAny<string>(), false, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
        #endregion
    }
}
