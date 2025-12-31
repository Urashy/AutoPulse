using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
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
        private TokenEmailController _controller;

        [TestInitialize]
        public async Task Initialize()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);
            _manager = new TokenEmailManager(_context);

            var fakeConfig = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { "Email:GmailUser", "fake@mail.com" },
                    { "Email:GmailPass", "fakepass" }
                }
            ).Build();

            _controller = new TokenEmailController(_manager, fakeConfig);

            TokenEmail seed = new TokenEmail
            {
                IdTokenEmail = 1,
                Email = "test@mail.com",
                Token = "TOKEN123",
                Expiration = DateTime.UtcNow.AddMinutes(30),
                Utilise = false
            };

            await _context.TokenEmails.AddAsync(seed);
            await _context.SaveChangesAsync();
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
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(entity.Email, result.Value.Email);
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
            Assert.IsNotNull(result.Value);
            var list = (IEnumerable<TokenEmail>)result.Value;
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
            var dto = new TokenEmailDTO();

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
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
                Email = "x"
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
        // VERIFICATION CODE
        // ---------------------------------------------------------

        [TestMethod]
        public async Task VerificationCode_OK()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            // Act
            var result = await _manager.VerificationCode(entity.Email, entity.Token);

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
            var result = await _manager.VerificationCode(entity.Email, "WRONGTOKEN");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VerificationCode_BadEmail()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            // Act
            var result = await _manager.VerificationCode("wrong@mail.com", entity.Token);

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
                Email = "expired@mail.com",
                Token = "EXPIRED123",
                Expiration = DateTime.UtcNow.AddMinutes(-10),
                Utilise = false
            };

            await _context.TokenEmails.AddAsync(expired);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerificationCode("expired@mail.com", "EXPIRED123");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VerifCode_OK()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            var dto = new TokenEmailDTO
            {
                Email = entity.Email,
                Code = entity.Token
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

            var dto = new TokenEmailDTO
            {
                Email = entity.Email,
                Code = "WRONGCODE"
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
                Email = "expired@mail.com",
                Token = "EXPIRED123",
                Expiration = DateTime.UtcNow.AddMinutes(-10),
                Utilise = false
            };

            await _context.TokenEmails.AddAsync(expired);
            await _context.SaveChangesAsync();

            var dto = new TokenEmailDTO
            {
                Email = expired.Email,
                Code = expired.Token
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
            var dto = new TokenEmailDTO
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
        public async Task VerifCode_NullCode()
        {
            // Arrange
            var dto = new TokenEmailDTO
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
        public async Task VerifCode_BadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");

            var dto = new TokenEmailDTO
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
        public async Task VerifCode_WrongEmail()
        {
            // Arrange
            var entity = await _context.TokenEmails.FirstAsync();

            var dto = new TokenEmailDTO
            {
                Email = "wrongemail@mail.com",
                Code = entity.Token
            };

            // Act
            var result = await _controller.VerifCode(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }
    }
}
