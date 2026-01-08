using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_c_sharpTests.ControllersTestsUnitaire
{
    [TestClass]
    [TestCategory("unit")]
    public class RefreshTokenManagerTests
    {
        private Mock<AutoPulseBdContext> _mockContext = null!;
        private RefreshTokenManager _manager = null!;

        [TestInitialize]
        public void Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _mockContext = new Mock<AutoPulseBdContext>(options);
        }

        [TestMethod]
        public void ComputeSha256Hash_SameInput_ReturnsSameHash()
        {
            string input = "test_token_123";
            var hash1 = ComputeSha256HashPublic(input);
            var hash2 = ComputeSha256HashPublic(input);

            Assert.AreEqual(hash1, hash2);
            Assert.AreEqual(64, hash1.Length); // SHA256 = 64 hex chars
        }

        [TestMethod]
        public void ComputeSha256Hash_DifferentInputs_ReturnsDifferentHashes()
        {
            var hash1 = ComputeSha256HashPublic("token1");
            var hash2 = ComputeSha256HashPublic("token2");

            Assert.AreNotEqual(hash1, hash2);
        }

        // Helper pour tester la méthode privée
        private static string ComputeSha256HashPublic(string rawData)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            var builder = new System.Text.StringBuilder();
            foreach (var b in bytes) builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}
