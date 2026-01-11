using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using System.Security.Cryptography;
using System.Text;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class CarteBancaireControllerTests
    {
        private Mock<CartebancaireManager> _mockManager;
        private CarteBancaireController _controller;
        private IMapper _mapper;
        private CarteBancaire _objetcommun;

        // Méthodes d'encryption pour générer les valeurs correctes dans les tests
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("CLE_SUPER_SECRETE_32_OCTETS!!!!!");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("INIT_VECTOR_16!!");

        private string EncryptCardNumber(string numeroCarte)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(numeroCarte);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        private string DecryptCardNumber(string encryptedCard)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            var decryptor = aes.CreateDecryptor();
            var bytes = Convert.FromBase64String(encryptedCard);
            var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }

        private string MaskLast4Digits(string encryptedCard)
        {
            var numeroCarte = DecryptCardNumber(encryptedCard);
            if (numeroCarte.Length < 4)
                throw new ArgumentException("Numéro invalide");
            return new string('*', numeroCarte.Length - 4) + numeroCarte[^4..];
        }

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock
            _mockManager = new Mock<CartebancaireManager>(null);

            // Création de l'objet de référence avec le numéro chiffré CORRECTEMENT
            _objetcommun = new CarteBancaire
            {
                IdCarteBancaire = 1,
                NumeroCarte = EncryptCardNumber("1234123412341234"), // Chiffrement correct
                CodeSecurite = EncryptCardNumber("123"), // Chiffrement correct
                DateExpiration = DateTime.Now,
                IdCompte = 1,
                TypeCarte = "VISA",
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new CarteBancaireController(_mockManager.Object, _mapper);
        }

        #region GET
        #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange - Le mock retourne une carte avec le numéro DÉJÀ MASQUÉ
            var carteMasquee = new CarteBancaire
            {
                IdCarteBancaire = 1,
                NumeroCarte = "************1234", // Déjà masqué
                CodeSecurite = "***", // Déjà masqué
                DateExpiration = DateTime.Now,
                IdCompte = 1,
                TypeCarte = "VISA",
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCarteBancaire))
                       .ReturnsAsync(carteMasquee);

            // Act
            var result = await _controller.GetByID(_objetcommun.IdCarteBancaire);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CarteBancaireDTO));
            Assert.AreEqual("************1234", result.Value.NumeroCarte);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((CarteBancaire)null);

            // Act
            var result = await _controller.GetByID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region GetAll
        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange - Les cartes retournées par le mock ont les numéros DÉJÀ MASQUÉS
            var carteList = new List<CarteBancaire>
            {
                new CarteBancaire
                {
                    IdCarteBancaire = 1,
                    NumeroCarte = "************1234", // Déjà masqué
                    CodeSecurite = "***", // Déjà masqué
                    DateExpiration = DateTime.Now,
                    IdCompte = 1,
                    TypeCarte = "VISA",
                    NomCarte = "carte perso",
                    NomTitulaire = "Jean Dupont"
                },
                new CarteBancaire
                {
                    IdCarteBancaire = 2,
                    NumeroCarte = "************5678", // Déjà masqué
                    CodeSecurite = "***", // Déjà masqué
                    DateExpiration = DateTime.Now,
                    IdCompte = 1,
                    TypeCarte = "VISA",
                    NomCarte = "carte pro",
                    NomTitulaire = "Jean Dupont"
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(carteList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CarteBancaireDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.NumeroCarte == "************1234"));
            Assert.AreEqual(2, result.Value.Count());
        }
        #endregion

        #region GetCarteBancaireByCompteID
        [TestMethod]
        public async Task GetCarteBancairesByCompteIDTest()
        {
            // Arrange - Les cartes retournées par le mock ont les numéros DÉJÀ MASQUÉS
            var carteList = new List<CarteBancaire>
            {
                new CarteBancaire
                {
                    IdCarteBancaire = 1,
                    NumeroCarte = "************1234", // Déjà masqué
                    CodeSecurite = "***", // Déjà masqué
                    DateExpiration = DateTime.Now,
                    IdCompte = 1,
                    TypeCarte = "VISA",
                    NomCarte = "carte perso",
                    NomTitulaire = "Jean Dupont"
                },
                new CarteBancaire
                {
                    IdCarteBancaire = 2,
                    NumeroCarte = "************5678", // Déjà masqué
                    CodeSecurite = "***", // Déjà masqué
                    DateExpiration = DateTime.Now,
                    IdCompte = 1,
                    TypeCarte = "VISA",
                    NomCarte = "carte pro",
                    NomTitulaire = "Jean Dupont"
                }
            };

            _mockManager.Setup(m => m.GetCarteBancaireByCompteId(_objetcommun.IdCompte))
                       .ReturnsAsync(carteList);

            // Act
            var result = await _controller.GetCarteBancaireByCompteID(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CarteBancaireDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.NumeroCarte == "************1234"));
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task NotFoundGetCarteBancairesByCompteIDTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetCarteBancaireByCompteId(0))
                       .ReturnsAsync((IEnumerable<CarteBancaire>)null);

            // Act
            var result = await _controller.GetCarteBancaireByCompteID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion
        #endregion

        #region POST
        [TestMethod]
        public async Task PostCarteBancaireTest()
        {
            // Arrange
            CarteBancaireCreateDTO carte = new CarteBancaireCreateDTO()
            {
                NumeroCarte = "1234123412341234",
                CodeSecurite = "123",
                DateExpiration = "11/26",
                IdCompte = 1,
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            var adresseEntity = _mapper.Map<CarteBancaire>(carte);
            adresseEntity.IdCarteBancaire = 2;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<CarteBancaire>()))
                       .ReturnsAsync(adresseEntity)
                       .Verifiable();

            // Act
            var actionResult = await _controller.Post(carte);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdCarteBancaire = (CarteBancaire)created.Value;
            Assert.AreEqual(carte.NumeroCarte, createdCarteBancaire.NumeroCarte);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<CarteBancaire>()), Times.Once);
        }

        [TestMethod]
        public async Task BadRequestPostCarteBancaireTest()
        {
            // Arrange
            CarteBancaireCreateDTO adresseDTO = new CarteBancaireCreateDTO
            {
                NumeroCarte = "1234123412341234",
                CodeSecurite = "123",
                DateExpiration = null,
                IdCompte = 1,
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            // Simuler une erreur de validation du modèle
            _controller.ModelState.AddModelError("DateExpiration", "La DateExpiration est requis");

            // Act
            var actionResult = await _controller.Post(adresseDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<CarteBancaire>()), Times.Never);
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteCarteBancaireTest()
        {
            // Arrange
            var carteMasquee = new CarteBancaire
            {
                IdCarteBancaire = 1,
                NumeroCarte = "************1234",
                CodeSecurite = "***",
                DateExpiration = DateTime.Now,
                IdCompte = 1,
                TypeCarte = "VISA",
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCarteBancaire))
                       .ReturnsAsync(carteMasquee);
            _mockManager.Setup(m => m.DeleteAsync(It.IsAny<CarteBancaire>()))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Delete(_objetcommun.IdCarteBancaire);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(It.IsAny<CarteBancaire>()), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundDeleteCarteBancaireTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((CarteBancaire)null);

            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutCarteBancaireTest()
        {
            // Arrange
            var existingCarteBancaire = new CarteBancaire
            {
                IdCarteBancaire = 1,
                NumeroCarte = "************1234", // Déjà masqué (comme retourné par GetByIdAsync)
                CodeSecurite = "***",
                DateExpiration = DateTime.Now,
                IdCompte = 1,
                TypeCarte = "VISA",
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            CarteBancaireUpdateDTO updatedCarteBancaireDTO = new CarteBancaireUpdateDTO()
            {
                IdCarteBancaire = 1,
                NumeroCarte = "1234123412341234", // En clair dans le DTO
                CodeSecurite = "123",
                DateExpiration = "12/26",
                IdCompte = 1,
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            var updatedCarteBancaire = _mapper.Map<CarteBancaire>(updatedCarteBancaireDTO);

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdCarteBancaire))
                       .ReturnsAsync(existingCarteBancaire);
            _mockManager.Setup(m => m.UpdateAsync(It.IsAny<CarteBancaire>(), It.IsAny<CarteBancaire>()))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_objetcommun.IdCarteBancaire, updatedCarteBancaireDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<CarteBancaire>(), It.IsAny<CarteBancaire>()), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundPutCarteBancaireTest()
        {
            // Arrange
            CarteBancaireUpdateDTO adresseDTO = new CarteBancaireUpdateDTO()
            {
                IdCarteBancaire = 1,
                NumeroCarte = "1234123412341234",
                CodeSecurite = "123",
                DateExpiration = "01/26",
                IdCompte = 1,
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((CarteBancaire)null);

            // Act
            var result = await _controller.Put(0, adresseDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutCarteBancaireTest()
        {
            // Arrange
            CarteBancaireUpdateDTO adresseDTO = new CarteBancaireUpdateDTO()
            {
                IdCarteBancaire = 1,
                NumeroCarte = "1234123412341234",
                CodeSecurite = "123",
                DateExpiration = null,
                IdCompte = 1,
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            _controller.ModelState.AddModelError("DateExpiration", "La DateExpiration est requis");

            // Act
            var result = await _controller.Put(_objetcommun.IdCarteBancaire, adresseDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<CarteBancaire>(), It.IsAny<CarteBancaire>()), Times.Never);
        }
        #endregion
    }
}