using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass]
    [TestCategory("integration")]
    public class RefreshTokenManagerIntegrationTests
    {
        private AutoPulseBdContext _context = null!;
        private RefreshTokenManager _manager = null!;
        private Compte _testCompte = null!;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_RefreshToken_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);
            _manager = new RefreshTokenManager(_context);

            // Créer un compte de test
            var typeCompte = new TypeCompte { Libelle = "Utilisateur" };
            _context.TypesCompte.Add(typeCompte);
            await _context.SaveChangesAsync();

            _testCompte = new Compte
            {
                Pseudo = "testuser",
                MotDePasse = "pwd123",
                Nom = "Test",
                Prenom = "User",
                Email = "test@test.com",
                DateCreation = DateTime.Now,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = typeCompte.IdTypeCompte
            };

            _context.Comptes.Add(_testCompte);
            await _context.SaveChangesAsync();
        }

        #region StoreRefreshTokenAsync Tests

        [TestMethod]
        public async Task StoreRefreshTokenAsync_RememberMeTrue_CreatesTokenWith30DaysExpiration()
        {
            // Arrange
            var token = "test_refresh_token_" + Guid.NewGuid();
            var rememberMe = true;
            var ipAddress = "192.168.1.1";
            var userAgent = "Mozilla/5.0";

            var beforeCreation = DateTime.UtcNow;

            // Act
            var result = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token, rememberMe, ipAddress, userAgent);

            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_testCompte.IdCompte, result.IdCompte);
            Assert.IsTrue(result.RememberMe);
            Assert.IsFalse(result.EstRevoque);
            Assert.AreEqual(ipAddress, result.IpCreation);
            Assert.AreEqual(userAgent, result.UserAgent);
            Assert.IsTrue(result.DateExpiration > beforeCreation.AddDays(29));
            Assert.IsTrue(result.DateExpiration < afterCreation.AddDays(31));
        }

        [TestMethod]
        public async Task StoreRefreshTokenAsync_RememberMeFalse_CreatesTokenWith1DayExpiration()
        {
            // Arrange
            var token = "test_refresh_token_" + Guid.NewGuid();
            var rememberMe = false;

            var beforeCreation = DateTime.UtcNow;

            // Act
            var result = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token, rememberMe);

            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.RememberMe);
            Assert.IsTrue(result.DateExpiration > beforeCreation.AddHours(23));
            Assert.IsTrue(result.DateExpiration < afterCreation.AddDays(2));
        }

        [TestMethod]
        public async Task StoreRefreshTokenAsync_TokenHashIsHashed()
        {
            // Arrange
            var token = "test_token_123";

            // Act
            var result = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token, false);

            // Assert
            Assert.IsNotNull(result.TokenHash);
            Assert.AreNotEqual(token, result.TokenHash); // Le hash ne doit pas être égal au token
            Assert.AreEqual(64, result.TokenHash.Length); // SHA256 = 64 caractères hex
        }

        [TestMethod]
        public async Task StoreRefreshTokenAsync_MultipleCalls_CreatesMultipleTokens()
        {
            // Arrange
            var token1 = "token_" + Guid.NewGuid();
            var token2 = "token_" + Guid.NewGuid();

            // Act
            var result1 = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token1, false);
            var result2 = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token2, false);

            // Assert
            Assert.AreNotEqual(result1.IdRefreshToken, result2.IdRefreshToken);
            Assert.AreNotEqual(result1.TokenHash, result2.TokenHash);
        }

        #endregion

        #region ValidateRefreshTokenAsync Tests

        [TestMethod]
        public async Task ValidateRefreshTokenAsync_ValidToken_ReturnsCompte()
        {
            // Arrange
            var token = "valid_token_" + Guid.NewGuid();
            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token, false);

            // Act
            var result = await _manager.ValidateRefreshTokenAsync(token);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_testCompte.IdCompte, result.IdCompte);
            Assert.AreEqual(_testCompte.Email, result.Email);
        }

        [TestMethod]
        public async Task ValidateRefreshTokenAsync_InvalidToken_ReturnsNull()
        {
            // Arrange
            var invalidToken = "invalid_token_" + Guid.NewGuid();

            // Act
            var result = await _manager.ValidateRefreshTokenAsync(invalidToken);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ValidateRefreshTokenAsync_RevokedToken_ReturnsNull()
        {
            // Arrange
            var token = "revoked_token_" + Guid.NewGuid();
            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token, false);
            await _manager.RevokeRefreshTokenAsync(token);

            // Act
            var result = await _manager.ValidateRefreshTokenAsync(token);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ValidateRefreshTokenAsync_ExpiredToken_ReturnsNull()
        {
            // Arrange
            var token = "expired_token_" + Guid.NewGuid();
            var storedToken = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token, false);

            // Modifier manuellement la date d'expiration pour la mettre dans le passé
            storedToken.DateExpiration = DateTime.UtcNow.AddHours(-1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.ValidateRefreshTokenAsync(token);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region RevokeRefreshTokenAsync Tests

        [TestMethod]
        public async Task RevokeRefreshTokenAsync_ValidToken_RevokesSuccessfully()
        {
            // Arrange
            var token = "token_to_revoke_" + Guid.NewGuid();
            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token, false);

            // Act
            var result = await _manager.RevokeRefreshTokenAsync(token);

            // Assert
            Assert.IsTrue(result);

            // Vérifier que le token est bien révoqué
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.IdCompte == _testCompte.IdCompte && !rt.EstRevoque);
            Assert.IsNull(refreshToken);
        }

        [TestMethod]
        public async Task RevokeRefreshTokenAsync_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var invalidToken = "invalid_token_" + Guid.NewGuid();

            // Act
            var result = await _manager.RevokeRefreshTokenAsync(invalidToken);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task RevokeRefreshTokenAsync_SetsRevocationDate()
        {
            // Arrange
            var token = "token_revoke_date_" + Guid.NewGuid();
            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token, false);

            var beforeRevocation = DateTime.UtcNow;

            // Act
            await _manager.RevokeRefreshTokenAsync(token);

            var afterRevocation = DateTime.UtcNow;

            // Assert
            var revokedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.EstRevoque);

            Assert.IsNotNull(revokedToken);
            Assert.IsTrue(revokedToken.DateRevocation >= beforeRevocation);
            Assert.IsTrue(revokedToken.DateRevocation <= afterRevocation);
        }

        #endregion

        #region RevokeAllUserTokensAsync Tests

        [TestMethod]
        public async Task RevokeAllUserTokensAsync_RevokesAllActiveTokens()
        {
            // Arrange
            var token1 = "token1_" + Guid.NewGuid();
            var token2 = "token2_" + Guid.NewGuid();
            var token3 = "token3_" + Guid.NewGuid();

            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token1, false);
            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token2, false);
            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token3, false);

            // Act
            await _manager.RevokeAllUserTokensAsync(_testCompte.IdCompte);

            // Assert
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.IdCompte == _testCompte.IdCompte && !rt.EstRevoque)
                .ToListAsync();

            Assert.AreEqual(0, activeTokens.Count);
        }

        [TestMethod]
        public async Task RevokeAllUserTokensAsync_DoesNotAffectOtherUsers()
        {
            // Arrange
            var autre_compte = new Compte
            {
                Pseudo = "otheruser",
                MotDePasse = "pwd123",
                Nom = "Other",
                Prenom = "User",
                Email = "other@test.com",
                DateCreation = DateTime.Now,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = _testCompte.IdTypeCompte
            };
            _context.Comptes.Add(autre_compte);
            await _context.SaveChangesAsync();

            var token1 = "token_user1_" + Guid.NewGuid();
            var token2 = "token_user2_" + Guid.NewGuid();

            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token1, false);
            await _manager.StoreRefreshTokenAsync(autre_compte.IdCompte, token2, false);

            // Act
            await _manager.RevokeAllUserTokensAsync(_testCompte.IdCompte);

            // Assert
            var otherUserTokens = await _context.RefreshTokens
                .Where(rt => rt.IdCompte == autre_compte.IdCompte && !rt.EstRevoque)
                .ToListAsync();

            Assert.AreEqual(1, otherUserTokens.Count);
        }

        #endregion

        #region CleanupExpiredTokensAsync Tests

        [TestMethod]
        public async Task CleanupExpiredTokensAsync_RemovesExpiredTokens()
        {
            // Arrange
            var token1 = "valid_token_" + Guid.NewGuid();
            var token2 = "expired_token_" + Guid.NewGuid();

            var storedToken1 = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token1, false);
            var storedToken2 = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token2, false);

            // Rendre le token2 expiré
            storedToken2.DateExpiration = DateTime.UtcNow.AddHours(-1);
            await _context.SaveChangesAsync();

            // Act
            var deletedCount = await _manager.CleanupExpiredTokensAsync();

            // Assert
            Assert.AreEqual(1, deletedCount);

            var remainingTokens = await _context.RefreshTokens.ToListAsync();
            Assert.AreEqual(1, remainingTokens.Count);
        }

        [TestMethod]
        public async Task CleanupExpiredTokensAsync_RemovesRevokedTokensOlderThan7Days()
        {
            // Arrange
            var token = "old_revoked_token_" + Guid.NewGuid();
            var storedToken = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token, false);

            // Révoquer et antidater la révocation
            storedToken.EstRevoque = true;
            storedToken.DateRevocation = DateTime.UtcNow.AddDays(-8);
            await _context.SaveChangesAsync();

            // Act
            var deletedCount = await _manager.CleanupExpiredTokensAsync();

            // Assert
            Assert.AreEqual(1, deletedCount);
        }

        [TestMethod]
        public async Task CleanupExpiredTokensAsync_KeepsRecentRevokedTokens()
        {
            // Arrange
            var token = "recent_revoked_token_" + Guid.NewGuid();
            var storedToken = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token, false);

            // Révoquer avec une date récente
            storedToken.EstRevoque = true;
            storedToken.DateRevocation = DateTime.UtcNow.AddDays(-3);
            await _context.SaveChangesAsync();

            // Act
            var deletedCount = await _manager.CleanupExpiredTokensAsync();

            // Assert
            Assert.AreEqual(0, deletedCount);
        }

        #endregion

        #region GetActiveUserTokensAsync Tests

        [TestMethod]
        public async Task GetActiveUserTokensAsync_ReturnsOnlyActiveTokens()
        {
            // Arrange
            var token1 = "active_token1_" + Guid.NewGuid();
            var token2 = "active_token2_" + Guid.NewGuid();
            var token3 = "revoked_token_" + Guid.NewGuid();

            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token1, false);
            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token2, false);
            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token3, false);
            await _manager.RevokeRefreshTokenAsync(token3);

            // Act
            var result = await _manager.GetActiveUserTokensAsync(_testCompte.IdCompte);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(t => !t.EstRevoque));
            Assert.IsTrue(result.All(t => t.DateExpiration > DateTime.UtcNow));
        }

        [TestMethod]
        public async Task GetActiveUserTokensAsync_ExcludesExpiredTokens()
        {
            // Arrange
            var token1 = "active_token_" + Guid.NewGuid();
            var token2 = "expired_token_" + Guid.NewGuid();

            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token1, false);
            var expiredToken = await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token2, false);

            expiredToken.DateExpiration = DateTime.UtcNow.AddHours(-1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetActiveUserTokensAsync(_testCompte.IdCompte);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(token1, result[0].TokenHash == ComputeSha256Hash(token1) ? token1 : "");
        }

        [TestMethod]
        public async Task GetActiveUserTokensAsync_OrderedByCreationDateDescending()
        {
            // Arrange
            var token1 = "token_first_" + Guid.NewGuid();
            var token2 = "token_second_" + Guid.NewGuid();
            var token3 = "token_third_" + Guid.NewGuid();

            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token1, false);
            await Task.Delay(100);
            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token2, false);
            await Task.Delay(100);
            await _manager.StoreRefreshTokenAsync(_testCompte.IdCompte, token3, false);

            // Act
            var result = await _manager.GetActiveUserTokensAsync(_testCompte.IdCompte);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result[0].DateCreation > result[1].DateCreation);
            Assert.IsTrue(result[1].DateCreation > result[2].DateCreation);
        }

        [TestMethod]
        public async Task GetActiveUserTokensAsync_EmptyListForUserWithNoTokens()
        {
            // Arrange
            var autre_compte = new Compte
            {
                Pseudo = "emptyuser",
                MotDePasse = "pwd123",
                Nom = "Empty",
                Prenom = "User",
                Email = "empty@test.com",
                DateCreation = DateTime.Now,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = _testCompte.IdTypeCompte
            };
            _context.Comptes.Add(autre_compte);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetActiveUserTokensAsync(autre_compte.IdCompte);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        private static string ComputeSha256Hash(string rawData)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}