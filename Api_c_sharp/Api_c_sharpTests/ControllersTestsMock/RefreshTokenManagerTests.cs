using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class RefreshTokenManagerTests
    {
        private Mock<RefreshTokenManager> _mockManager;
        private RefreshToken _objetcommun;
        private Compte _compteCommun;
        private string _tokenCommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock
            _mockManager = new Mock<RefreshTokenManager>(null);

            // Création du compte de référence
            _compteCommun = new Compte
            {
                IdCompte = 1,
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "jean.dupont@test.fr"
            };

            // Token de test
            _tokenCommun = "test-refresh-token-12345";

            // Création du RefreshToken de référence
            _objetcommun = new RefreshToken
            {
                IdRefreshToken = 1,
                TokenHash = ComputeSha256HashLocal(_tokenCommun),
                IdCompte = _compteCommun.IdCompte,
                DateCreation = DateTime.UtcNow,
                DateExpiration = DateTime.UtcNow.AddDays(30),
                RememberMe = true,
                IpCreation = "192.168.1.1",
                EstRevoque = false,
                UserAgent = "Mozilla/5.0",
                CompteRefreshTokenNav = _compteCommun
            };
        }

        #region StoreRefreshTokenAsync

        [TestMethod]
        public async Task StoreRefreshTokenWithRememberMeTest()
        {
            // Arrange
            var token = "new-refresh-token";
            var ipAddress = "192.168.1.100";
            var userAgent = "Chrome/120.0";

            var expectedToken = new RefreshToken
            {
                IdRefreshToken = 2,
                TokenHash = ComputeSha256HashLocal(token),
                IdCompte = _compteCommun.IdCompte,
                DateCreation = DateTime.UtcNow,
                DateExpiration = DateTime.UtcNow.AddDays(30),
                RememberMe = true,
                IpCreation = ipAddress,
                EstRevoque = false,
                UserAgent = userAgent
            };

            _mockManager.Setup(m => m.StoreRefreshTokenAsync(
                    _compteCommun.IdCompte,
                    token,
                    true,
                    ipAddress,
                    userAgent))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _mockManager.Object.StoreRefreshTokenAsync(
                _compteCommun.IdCompte,
                token,
                true,
                ipAddress,
                userAgent);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_compteCommun.IdCompte, result.IdCompte);
            Assert.IsTrue(result.RememberMe);
            Assert.AreEqual(ipAddress, result.IpCreation);
            Assert.AreEqual(userAgent, result.UserAgent);
            Assert.IsFalse(result.EstRevoque);
        }

        [TestMethod]
        public async Task StoreRefreshTokenWithoutRememberMeTest()
        {
            // Arrange
            var token = "short-lived-token";

            var expectedToken = new RefreshToken
            {
                IdRefreshToken = 3,
                TokenHash = ComputeSha256HashLocal(token),
                IdCompte = _compteCommun.IdCompte,
                DateCreation = DateTime.UtcNow,
                DateExpiration = DateTime.UtcNow.AddDays(1),
                RememberMe = false,
                EstRevoque = false
            };

            _mockManager.Setup(m => m.StoreRefreshTokenAsync(
                    _compteCommun.IdCompte,
                    token,
                    false,
                    null,
                    null))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _mockManager.Object.StoreRefreshTokenAsync(
                _compteCommun.IdCompte,
                token,
                false,
                null,
                null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.RememberMe);
            Assert.IsNull(result.IpCreation);
            Assert.IsNull(result.UserAgent);
        }

        #endregion

        #region ValidateRefreshTokenAsync

        [TestMethod]
        public async Task ValidateRefreshTokenSuccessTest()
        {
            // Arrange
            _mockManager.Setup(m => m.ValidateRefreshTokenAsync(_tokenCommun))
                .ReturnsAsync(_compteCommun);

            // Act
            var result = await _mockManager.Object.ValidateRefreshTokenAsync(_tokenCommun);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_compteCommun.IdCompte, result.IdCompte);
            Assert.AreEqual(_compteCommun.Email, result.Email);
        }

        [TestMethod]
        public async Task ValidateRefreshTokenExpiredTest()
        {
            // Arrange
            var expiredToken = "expired-token";
            _mockManager.Setup(m => m.ValidateRefreshTokenAsync(expiredToken))
                .ReturnsAsync((Compte)null);

            // Act
            var result = await _mockManager.Object.ValidateRefreshTokenAsync(expiredToken);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ValidateRefreshTokenRevokedTest()
        {
            // Arrange
            var revokedToken = "revoked-token";
            _mockManager.Setup(m => m.ValidateRefreshTokenAsync(revokedToken))
                .ReturnsAsync((Compte)null);

            // Act
            var result = await _mockManager.Object.ValidateRefreshTokenAsync(revokedToken);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ValidateRefreshTokenNotFoundTest()
        {
            // Arrange
            var invalidToken = "non-existent-token";
            _mockManager.Setup(m => m.ValidateRefreshTokenAsync(invalidToken))
                .ReturnsAsync((Compte)null);

            // Act
            var result = await _mockManager.Object.ValidateRefreshTokenAsync(invalidToken);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region RevokeRefreshTokenAsync

        [TestMethod]
        public async Task RevokeRefreshTokenSuccessTest()
        {
            // Arrange
            _mockManager.Setup(m => m.RevokeRefreshTokenAsync(_tokenCommun))
                .ReturnsAsync(true);

            // Act
            var result = await _mockManager.Object.RevokeRefreshTokenAsync(_tokenCommun);

            // Assert
            Assert.IsTrue(result);
            _mockManager.Verify(m => m.RevokeRefreshTokenAsync(_tokenCommun), Times.Once);
        }

        [TestMethod]
        public async Task RevokeRefreshTokenNotFoundTest()
        {
            // Arrange
            var invalidToken = "non-existent-token";
            _mockManager.Setup(m => m.RevokeRefreshTokenAsync(invalidToken))
                .ReturnsAsync(false);

            // Act
            var result = await _mockManager.Object.RevokeRefreshTokenAsync(invalidToken);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region RevokeAllUserTokensAsync

        [TestMethod]
        public async Task RevokeAllUserTokensTest()
        {
            // Arrange
            _mockManager.Setup(m => m.RevokeAllUserTokensAsync(_compteCommun.IdCompte))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _mockManager.Object.RevokeAllUserTokensAsync(_compteCommun.IdCompte);

            // Assert
            _mockManager.Verify(m => m.RevokeAllUserTokensAsync(_compteCommun.IdCompte), Times.Once);
        }

        [TestMethod]
        public async Task RevokeAllUserTokensNoActiveTokensTest()
        {
            // Arrange
            var idCompteWithoutTokens = 999;
            _mockManager.Setup(m => m.RevokeAllUserTokensAsync(idCompteWithoutTokens))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _mockManager.Object.RevokeAllUserTokensAsync(idCompteWithoutTokens);

            // Assert
            _mockManager.Verify(m => m.RevokeAllUserTokensAsync(idCompteWithoutTokens), Times.Once);
        }

        #endregion

        #region CleanupExpiredTokensAsync

        [TestMethod]
        public async Task CleanupExpiredTokensTest()
        {
            // Arrange
            var expectedDeletedCount = 5;
            _mockManager.Setup(m => m.CleanupExpiredTokensAsync())
                .ReturnsAsync(expectedDeletedCount);

            // Act
            var result = await _mockManager.Object.CleanupExpiredTokensAsync();

            // Assert
            Assert.AreEqual(expectedDeletedCount, result);
            _mockManager.Verify(m => m.CleanupExpiredTokensAsync(), Times.Once);
        }

        [TestMethod]
        public async Task CleanupExpiredTokensNoTokensTest()
        {
            // Arrange
            _mockManager.Setup(m => m.CleanupExpiredTokensAsync())
                .ReturnsAsync(0);

            // Act
            var result = await _mockManager.Object.CleanupExpiredTokensAsync();

            // Assert
            Assert.AreEqual(0, result);
        }

        #endregion

        #region GetActiveUserTokensAsync

        [TestMethod]
        public async Task GetActiveUserTokensTest()
        {
            // Arrange
            var activeTokens = new List<RefreshToken>
            {
                _objetcommun,
                new RefreshToken
                {
                    IdRefreshToken = 2,
                    TokenHash = ComputeSha256HashLocal("another-token"),
                    IdCompte = _compteCommun.IdCompte,
                    DateCreation = DateTime.UtcNow.AddDays(-1),
                    DateExpiration = DateTime.UtcNow.AddDays(29),
                    RememberMe = true,
                    EstRevoque = false
                }
            };

            _mockManager.Setup(m => m.GetActiveUserTokensAsync(_compteCommun.IdCompte))
                .ReturnsAsync(activeTokens);

            // Act
            var result = await _mockManager.Object.GetActiveUserTokensAsync(_compteCommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(t => t.IdCompte == _compteCommun.IdCompte));
            Assert.IsTrue(result.All(t => !t.EstRevoque));
        }

        [TestMethod]
        public async Task GetActiveUserTokensNoActiveTokensTest()
        {
            // Arrange
            var emptyList = new List<RefreshToken>();
            _mockManager.Setup(m => m.GetActiveUserTokensAsync(_compteCommun.IdCompte))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _mockManager.Object.GetActiveUserTokensAsync(_compteCommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetActiveUserTokensOrderedByDateTest()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var activeTokens = new List<RefreshToken>
            {
                new RefreshToken
                {
                    IdRefreshToken = 3,
                    TokenHash = ComputeSha256HashLocal("oldest-token"),
                    IdCompte = _compteCommun.IdCompte,
                    DateCreation = now.AddDays(-5),
                    DateExpiration = now.AddDays(25),
                    RememberMe = true,
                    EstRevoque = false
                },
                new RefreshToken
                {
                    IdRefreshToken = 2,
                    TokenHash = ComputeSha256HashLocal("newest-token"),
                    IdCompte = _compteCommun.IdCompte,
                    DateCreation = now,
                    DateExpiration = now.AddDays(30),
                    RememberMe = true,
                    EstRevoque = false
                }
            };

            _mockManager.Setup(m => m.GetActiveUserTokensAsync(_compteCommun.IdCompte))
                .ReturnsAsync(activeTokens);

            // Act
            var result = await _mockManager.Object.GetActiveUserTokensAsync(_compteCommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(2, result[0].IdRefreshToken);
        }

        #endregion

        #region Helper Methods

        private static string ComputeSha256HashLocal(string rawData)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            var builder = new System.Text.StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        #endregion
    }
}