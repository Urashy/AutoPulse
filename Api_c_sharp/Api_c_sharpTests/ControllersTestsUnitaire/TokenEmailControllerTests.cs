using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass]
    public class TokenEmailControllerTests
    {
        private AutoPulseBdContext _context;
        private TokenEmailManager _manager;
        private CompteManager _compteManager;
        private TokenEmailController _controller;
        private IMapper _mapper;

        [TestInitialize]
        public async Task Initialize()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);
            _manager = new TokenEmailManager(_context);
            _compteManager = new CompteManager(_context);

            var fakeConfig = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { "Email:GmailUser", "fake@mail.com" },
                    { "Email:GmailPass", "fakepass" }
                }
            ).Build();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _controller = new TokenEmailController(_manager, fakeConfig, _mapper, _compteManager);

            TokenEmail seed = new TokenEmail
            {
                IdTokenEmail = 1,
                IdCompte = 1,
                Email = "test@mail.com",
                Token = "TOKEN123",
                TypeToken = "REINIT_MDP",
                Expiration = DateTime.UtcNow.AddMinutes(30),
                Utilise = false
            };

            await _context.TokenEmails.AddAsync(seed);
            await _context.SaveChangesAsync();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ---------------------------------------------------------
        // GET BY ID
        // ---------------------------------------------------------

        [TestMethod]
        public async Task GetById_OK()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            // Act
            var result = await _controller.Get(entity.IdTokenEmail);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = result.Result as OkObjectResult;
            var returnedEntity = okResult.Value as TokenEmail;

            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(entity.Email, returnedEntity.Email);
            Assert.AreEqual(entity.Token, returnedEntity.Token);
        }

        [TestMethod]
        public async Task GetById_NotFound()
        {
            // Arrange
            int invalidId = 999;

            // Act
            var result = await _controller.Get(invalidId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // ---------------------------------------------------------
        // GET ALL
        // ---------------------------------------------------------

        [TestMethod]
        public async Task GetAll_OK()
        {
            // Arrange
            // (Nothing to prepare – DB already seeded)

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = result.Result as OkObjectResult;
            var list = okResult.Value as IEnumerable<TokenEmail>;

            Assert.IsNotNull(list);
            Assert.AreEqual(1, ((List<TokenEmail>)list).Count);
        }

        // ---------------------------------------------------------
        // GET BY STRING
        // ---------------------------------------------------------

        [TestMethod]
        public async Task GetByString_OK()
        {
            // Arrange
            string token = "TOKEN123";

            // Act
            var result = await _controller.GetByString(token);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = result.Result as OkObjectResult;
            var returnedEntity = okResult.Value as TokenEmail;

            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(token, returnedEntity.Token);
        }

        [TestMethod]
        public async Task GetByString_NotFound()
        {
            // Arrange
            string invalidToken = "UNKNOWN";

            // Act
            var result = await _controller.GetByString(invalidToken);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // ---------------------------------------------------------
        // POST
        // ---------------------------------------------------------

        [TestMethod]
        public async Task Post_BadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");
            var dto = new TokenEmailCreateDTO
            {
                Email = "",
                TypeToken = "REINIT_MDP"
            };

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        // ---------------------------------------------------------
        // PUT
        // ---------------------------------------------------------

        [TestMethod]
        public async Task Put_OK()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            var updated = new TokenEmail
            {
                IdTokenEmail = entity.IdTokenEmail,
                IdCompte = entity.IdCompte,
                Email = "updated@mail.com",
                Token = entity.Token,
                Expiration = DateTime.UtcNow.AddHours(1),
                Utilise = false
            };

            // Act
            var result = await _controller.Put(entity.IdTokenEmail, updated);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var dbEntity = await _context.TokenEmails.FindAsync(entity.IdTokenEmail);
            Assert.AreEqual("updated@mail.com", dbEntity.Email);
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

            // Act
            var result = await _controller.Put(invalidId, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Put_BadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");
            var entity = await _context.TokenEmails.FirstAsync();

            // Act
            var result = await _controller.Put(entity.IdTokenEmail, entity);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        // ---------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------

        [TestMethod]
        public async Task Delete_OK()
        {
            // Arrange
            string token = "TOKEN123";

            // Act
            var result = await _controller.Delete(token);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var entity = await _manager.GetByNameAsync(token);
            Assert.IsNull(entity);
        }

        [TestMethod]
        public async Task Delete_NotFound()
        {
            // Arrange
            string invalidToken = "NOTEXIST";

            // Act
            var result = await _controller.Delete(invalidToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // ---------------------------------------------------------
        // VERIFICATION CODE (Manager)
        // ---------------------------------------------------------

        [TestMethod]
        public async Task VerificationCode_OK()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            // Act
            var result = await _manager.VerificationCode(entity.Email, entity.Token, "REINIT_MDP");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(entity.Email, result.Email);
        }

        [TestMethod]
        public async Task VerificationCode_BadToken()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            // Act
            var result = await _manager.VerificationCode(entity.Email, "WRONGTOKEN", "REINIT_MDP");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VerificationCode_BadEmail()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            // Act
            var result = await _manager.VerificationCode("wrong@mail.com", entity.Token, "REINIT_MDP");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VerificationCode_Expired()
        {
            // Arrange
            var expired = new TokenEmail
            {
                IdTokenEmail = 10,
                IdCompte = 1,
                Email = "expired@mail.com",
                Token = "EXPIRED123",
                TypeToken = "REINIT_MDP",
                Expiration = DateTime.UtcNow.AddMinutes(-10),
                Utilise = false
            };

            await _context.TokenEmails.AddAsync(expired);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerificationCode("expired@mail.com", "EXPIRED123", "REINIT_MDP");

            // Assert
            Assert.IsNull(result);
        }

        // ---------------------------------------------------------
        // VERIF CODE (Controller)
        // ---------------------------------------------------------

        [TestMethod]
        public async Task VerifCode_OK()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            var dto = new TokenEmailVerifDTO
            {
                Email = entity.Email,
                Code = entity.Token,
                TypeToken = "REINIT_MDP"
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result;
            dynamic responseValue = okResult.Value;
            string message = responseValue.GetType().GetProperty("Message").GetValue(responseValue, null);

            Assert.IsTrue(message.Contains("validé avec succès"));
        }

        [TestMethod]
        public async Task VerifCode_InvalidCode()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            var dto = new TokenEmailVerifDTO
            {
                Email = entity.Email,
                Code = "WRONGCODE",
                TypeToken = "REINIT_MDP"
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = (NotFoundObjectResult)result;
            dynamic responseValue = notFoundResult.Value;
            string message = responseValue.GetType().GetProperty("Message").GetValue(responseValue, null);

            Assert.IsTrue(message.Contains("invalide ou expiré"));
        }

        [TestMethod]
        public async Task VerifCode_ExpiredCode()
        {
            // Arrange
            var expired = new TokenEmail
            {
                IdTokenEmail = 10,
                IdCompte = 1,
                Email = "expired@mail.com",
                Token = "EXPIRED123",
                TypeToken = "REINIT_MDP",
                Expiration = DateTime.UtcNow.AddMinutes(-10),
                Utilise = false
            };

            await _context.TokenEmails.AddAsync(expired);
            await _context.SaveChangesAsync();

            var dto = new TokenEmailVerifDTO
            {
                Email = expired.Email,
                Code = expired.Token,
                TypeToken = "REINIT_MDP"
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = (NotFoundObjectResult)result;
            dynamic responseValue = notFoundResult.Value;
            string message = responseValue.GetType().GetProperty("Message").GetValue(responseValue, null);

            Assert.IsTrue(message.Contains("invalide ou expiré"));
        }

        [TestMethod]
        public async Task VerifCode_EmptyCode()
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
        public async Task VerifCode_NullCode()
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
        public async Task VerifCode_BadRequest()
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
        }

        [TestMethod]
        public async Task VerifCode_WrongEmail()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            var dto = new TokenEmailVerifDTO
            {
                Email = "wrongemail@mail.com",
                Code = entity.Token,
                TypeToken = "REINIT_MDP"
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        // ---------------------------------------------------------
        // MARQUER UTILISÉ
        // ---------------------------------------------------------

        [TestMethod]
        public async Task MarquerUtilise_OK()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            // Act
            var result = await _controller.MarquerUtilise(entity.IdTokenEmail);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updatedEntity = await _context.TokenEmails.FindAsync(entity.IdTokenEmail);
            Assert.IsTrue(updatedEntity.Utilise);
        }

        [TestMethod]
        public async Task MarquerUtilise_NotFound()
        {
            // Arrange
            int invalidId = 999;

            // Act
            var result = await _controller.MarquerUtilise(invalidId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // ---------------------------------------------------------
        // INVALIDER TOKENS PAR TYPE
        // ---------------------------------------------------------

        [TestMethod]
        public async Task InvaliderTokensParType_OK()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            // Act
            var result = await _controller.InvaliderTokensParType(entity.IdCompte, "REINIT_MDP");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        // ---------------------------------------------------------
        // NETTOYER TOKENS EXPIRÉS
        // ---------------------------------------------------------

        [TestMethod]
        public async Task NettoyerTokensExpires_OK()
        {
            // Arrange
            var expired = new TokenEmail
            {
                IdTokenEmail = 20,
                IdCompte = 1,
                Email = "expired2@mail.com",
                Token = "EXPIRED456",
                TypeToken = "REINIT_MDP",
                Expiration = DateTime.UtcNow.AddMinutes(-30),
                Utilise = false
            };

            await _context.TokenEmails.AddAsync(expired);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.NettoyerTokensExpires();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deletedEntity = await _context.TokenEmails.FindAsync(expired.IdTokenEmail);
            Assert.IsNull(deletedEntity);
        }
    }
}