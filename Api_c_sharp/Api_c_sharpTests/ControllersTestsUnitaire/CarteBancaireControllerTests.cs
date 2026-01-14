using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class CarteBancaireControllerTests
    {
        private CarteBancaireController _controller;
        private AutoPulseBdContext _context;
        private CartebancaireManager _manager;
        private IMapper _mapper;
        private CarteBancaire _objetcommun;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _manager = new CartebancaireManager(_context);
            _controller = new CarteBancaireController(_manager, _mapper);

            _context.CarteBancaires.RemoveRange(_context.CarteBancaires);
            await _context.SaveChangesAsync();

            var typeCompte = new TypeCompte
            {
                Libelle = "Utilisateur"
            };
            var compte = new Compte
            {
                Pseudo = "testuser",
                MotDePasse = "Password123!",
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@gmail.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = new DateTime(2000, 1, 1),
                IdTypeCompte = 1
            };

            var carteBancaire = new CarteBancaire
            {
                IdCarteBancaire = 1,
                NumeroCarte = "c4sn80fEeFLXETmUBD0XeIsm5Jt57CxBs8kZY718bRM=",
                CodeSecurite = "aUjuu/kOAi3zIU2NIANtgw==",
                DateExpiration = DateTime.Now,
                IdCompte = 1,
                TypeCarte = "VISA",
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };
            _context.TypesCompte.Add(typeCompte);
            _context.Comptes.Add(compte);
            _context.CarteBancaires.Add(carteBancaire);
            await _context.SaveChangesAsync();

            _objetcommun = carteBancaire;
        }
        #region GET
            #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdCarteBancaire);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CarteBancaireDTO));
            Assert.AreEqual(result.Value.NumeroCarte ,"************1234");
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
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
            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CarteBancaireDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.NumeroCarte == "************1234"));
        }
        #endregion

            #region GetCarteBancaireByCompteID
        [TestMethod]
        public async Task GetCarteBancaireByCompteIDTest()
        {
            // Act
            var result = await _controller.GetCarteBancaireByCompteID(_objetcommun.IdCompte);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CarteBancaireDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.NumeroCarte == "************1234"));
        }

        [TestMethod]
        public async Task NotFoundGetCarteBancaireByCompteIDTest()
        {
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
        public async Task PostCarteBancaireTest_Entity()
        {
            // Arrange
            CarteBancaireCreateDTO carteBancaire = new CarteBancaireCreateDTO()
            {
                NumeroCarte = "1234 1234 1234 1234",
                CodeSecurite = "123",
                DateExpiration = "11/25",
                IdCompte = 1,
                NomCarte = "carte perso"
            }
            ;

            // Act
            var actionResult = await _controller.Post(carteBancaire);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdCarteBancaire = (CarteBancaire)created.Value;
            Assert.AreEqual("8yAA6RWP19e4TstU7XTMxaZjclPH26oQmaWWX0+wc0c=", createdCarteBancaire.NumeroCarte);
        }

        [TestMethod]
        public async Task BadRequestPostCarteBancaireTest()
        {
            // Arrange
            CarteBancaireCreateDTO carteBancaire = new CarteBancaireCreateDTO
            {
                NomTitulaire = null,
            };

            _controller.ModelState.AddModelError("NomTitulaire", "Required");

            // Act
            var actionResult = await _controller.Post(carteBancaire);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteCarteBancaireTest()
        {
            // Act
            var result = await _controller.Delete(_objetcommun.IdCarteBancaire);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedCarteBancaire = await _manager.GetByIdAsync(_objetcommun.IdCarteBancaire);
            Assert.IsNull(deletedCarteBancaire);
        }

        [TestMethod]
        public async Task DeleteCarteBancaireWithPaiementsTest()
        {
            // Arrange - Créer un paiement lié à la carte bancaire
            var paiement = new Paiement
            {
                IdPaiement = 1,
                IdCarteBancaire = _objetcommun.IdCarteBancaire,
                IdAnnonce = 1,
                DatePaiement = DateTime.Now,
                IdCompte = 1
            };
            _context.Paiements.Add(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Delete(_objetcommun.IdCarteBancaire);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            // Vérifier que la carte a été supprimée
            var deletedCarteBancaire = await _manager.GetByIdAsync(_objetcommun.IdCarteBancaire);
            Assert.IsNull(deletedCarteBancaire);

            // Vérifier que le paiement existe toujours mais que IdCarteBancaire est null
            var paiementAfterDelete = await _context.Paiements.FindAsync(1);
            Assert.IsNotNull(paiementAfterDelete);
            Assert.IsNull(paiementAfterDelete.IdCarteBancaire);
        }

        [TestMethod]
        public async Task NotFoundDeleteCarteBancaireTest()
        {
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
            CarteBancaireUpdateDTO carteBancaire = new CarteBancaireUpdateDTO()
            {
                IdCarteBancaire = 1,
                NumeroCarte = "c4sn80fEeFLXETmUBD0XeIsm5Jt57CxBs8kZY718bRM=",
                CodeSecurite = "aUjuu/kOAi3zIU2NIANtgw==",
                DateExpiration = "11/25",
                IdCompte = 1,
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdCarteBancaire, carteBancaire);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var carteBancaireput = await _manager.GetByIdAsync(_objetcommun.IdCarteBancaire);
            Assert.AreEqual(carteBancaire.NomTitulaire, carteBancaireput.NomTitulaire);
        }

        [TestMethod]
        public async Task NotFoundPutCarteBancaireTest()
        {
            // Arrange
            CarteBancaireUpdateDTO carteBancaire = new CarteBancaireUpdateDTO()
            {
                IdCarteBancaire = 1,
                NumeroCarte = "c4sn80fEeFLXETmUBD0XeIsm5Jt57CxBs8kZY718bRM=",
                CodeSecurite = "aUjuu/kOAi3zIU2NIANtgw==",
                DateExpiration = "11/25",
                IdCompte = 1,
                NomCarte = "carte perso",
                NomTitulaire = "Jean Dupont"
            };

            // Act
            var result = await _controller.Put(0, carteBancaire);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutCarteBancaireTest()
        {
            // Arrange
            CarteBancaireUpdateDTO carteBancaire = new CarteBancaireUpdateDTO()
            {
                IdCarteBancaire = 1,
                NumeroCarte = "c4sn80fEeFLXETmUBD0XeIsm5Jt57CxBs8kZY718bRM=",
                CodeSecurite = "aUjuu/kOAi3zIU2NIANtgw==",
                DateExpiration = "11/25",
                IdCompte = 1,
                NomCarte = "carte perso",
                NomTitulaire = null
            };
            _controller.ModelState.AddModelError("NomTitulaire", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdCarteBancaire, carteBancaire);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

        #region Tests Types de Cartes

        [TestMethod]
        public async Task PostCarteBancaire_VisaTest()
        {
            // Arrange
            CarteBancaireCreateDTO carteBancaire = new CarteBancaireCreateDTO()
            {
                NumeroCarte = "4111111111111111", // Commence par 4 = Visa
                CodeSecurite = "123",
                DateExpiration = "11/25",
                IdCompte = 1,
                NomCarte = "carte Visa",
                NomTitulaire = "Jean Dupont"
            };

            // Act
            var actionResult = await _controller.Post(carteBancaire);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdCarteBancaire = (CarteBancaire)created.Value;
            Assert.AreEqual("Visa", createdCarteBancaire.TypeCarte);
        }

        [TestMethod]
        public async Task PostCarteBancaire_MasterCardTest()
        {
            // Arrange
            CarteBancaireCreateDTO carteBancaire = new CarteBancaireCreateDTO()
            {
                NumeroCarte = "5555555555554444", // Commence par 51-55 = MasterCard
                CodeSecurite = "123",
                DateExpiration = "11/25",
                IdCompte = 1,
                NomCarte = "carte MasterCard",
                NomTitulaire = "Jean Dupont"
            };

            // Act
            var actionResult = await _controller.Post(carteBancaire);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdCarteBancaire = (CarteBancaire)created.Value;
            Assert.AreEqual("MasterCard", createdCarteBancaire.TypeCarte);
        }

        [TestMethod]
        public async Task PostCarteBancaire_AmericanExpressTest()
        {
            // Arrange
            CarteBancaireCreateDTO carteBancaire = new CarteBancaireCreateDTO()
            {
                NumeroCarte = "378282246310005", // Commence par 34 ou 37 = American Express
                CodeSecurite = "1234",
                DateExpiration = "11/25",
                IdCompte = 1,
                NomCarte = "carte Amex",
                NomTitulaire = "Jean Dupont"
            };

            // Act
            var actionResult = await _controller.Post(carteBancaire);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdCarteBancaire = (CarteBancaire)created.Value;
            Assert.AreEqual("American Express", createdCarteBancaire.TypeCarte);
        }

        [TestMethod]
        public async Task PostCarteBancaire_DiscoverTest()
        {
            // Arrange
            CarteBancaireCreateDTO carteBancaire = new CarteBancaireCreateDTO()
            {
                NumeroCarte = "6011111111111117", // Commence par 6011 ou 65 = Discover
                CodeSecurite = "123",
                DateExpiration = "11/25",
                IdCompte = 1,
                NomCarte = "carte Discover",
                NomTitulaire = "Jean Dupont"
            };

            // Act
            var actionResult = await _controller.Post(carteBancaire);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdCarteBancaire = (CarteBancaire)created.Value;
            Assert.AreEqual("Discover", createdCarteBancaire.TypeCarte);
        }

        [TestMethod]
        public async Task PostCarteBancaire_ClassiqueTest()
        {
            // Arrange
            CarteBancaireCreateDTO carteBancaire = new CarteBancaireCreateDTO()
            {
                NumeroCarte = "9999999999999999", // Ne correspond à aucun type = Classique
                CodeSecurite = "123",
                DateExpiration = "11/25",
                IdCompte = 1,
                NomCarte = "carte Classique",
                NomTitulaire = "Jean Dupont"
            };

            // Act
            var actionResult = await _controller.Post(carteBancaire);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdCarteBancaire = (CarteBancaire)created.Value;
            Assert.AreEqual("Classique", createdCarteBancaire.TypeCarte);
        }

        [TestMethod]
        public async Task PostCarteBancaire_EmptyCardNumberTest()
        {
            // Arrange
            CarteBancaireCreateDTO carteBancaire = new CarteBancaireCreateDTO()
            {
                NumeroCarte = "", // Vide = Classique
                CodeSecurite = "123",
                DateExpiration = "11/25",
                IdCompte = 1,
                NomCarte = "carte vide",
                NomTitulaire = "Jean Dupont"
            };

            // Act
            var actionResult = await _controller.Post(carteBancaire);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdCarteBancaire = (CarteBancaire)created.Value;
            Assert.AreEqual("Classique", createdCarteBancaire.TypeCarte);
        }

        #endregion

        #region Tests MaskLast4Digits Error

        [TestMethod]
        public async Task GetById_ShortCardNumber_ThrowsException()
        {
            // Arrange - Créer une carte avec un numéro trop court (moins de 4 caractères)
            var shortCardEntity = new CarteBancaire
            {
                IdCarteBancaire = 999,
                NumeroCarte = "123", // Seulement 3 caractères
                CodeSecurite = "123",
                DateExpiration = DateTime.Now.AddYears(1),
                IdCompte = 1,
                TypeCarte = "VISA",
                NomCarte = "carte invalide",
                NomTitulaire = "Test User"
            };

            // Crypter le numéro court
            using var aes = Aes.Create();
            var key = Encoding.UTF8.GetBytes("CLE_SUPER_SECRETE_32_OCTETS!!!!!");
            var iv = Encoding.UTF8.GetBytes("INIT_VECTOR_16!!");
            aes.Key = key;
            aes.IV = iv;
            var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(shortCardEntity.NumeroCarte);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            shortCardEntity.NumeroCarte = Convert.ToBase64String(encrypted);

            _context.CarteBancaires.Add(shortCardEntity);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _controller.GetByID(999);
            });
        }

        [TestMethod]
        public async Task GetAll_WithShortCardNumber_ThrowsException()
        {
            // Arrange
            var shortCardEntity = new CarteBancaire
            {
                IdCarteBancaire = 998,
                NumeroCarte = "12", // Seulement 2 caractères
                CodeSecurite = "123",
                DateExpiration = DateTime.Now.AddYears(1),
                IdCompte = 1,
                TypeCarte = "VISA",
                NomCarte = "carte invalide 2",
                NomTitulaire = "Test User"
            };

            // Crypter le numéro court
            using var aes = Aes.Create();
            var key = Encoding.UTF8.GetBytes("CLE_SUPER_SECRETE_32_OCTETS!!!!!");
            var iv = Encoding.UTF8.GetBytes("INIT_VECTOR_16!!");
            aes.Key = key;
            aes.IV = iv;
            var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(shortCardEntity.NumeroCarte);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            shortCardEntity.NumeroCarte = Convert.ToBase64String(encrypted);

            _context.CarteBancaires.Add(shortCardEntity);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _controller.GetAll();
            });
        }

        [TestMethod]
        public async Task GetCarteBancaireByCompteID_WithShortCardNumber_ThrowsException()
        {
            // Arrange
            var shortCardEntity = new CarteBancaire
            {
                IdCarteBancaire = 997,
                NumeroCarte = "1", // Seulement 1 caractère
                CodeSecurite = "123",
                DateExpiration = DateTime.Now.AddYears(1),
                IdCompte = 1,
                TypeCarte = "VISA",
                NomCarte = "carte invalide 3",
                NomTitulaire = "Test User"
            };

            // Crypter le numéro court
            using var aes = Aes.Create();
            var key = Encoding.UTF8.GetBytes("CLE_SUPER_SECRETE_32_OCTETS!!!!!");
            var iv = Encoding.UTF8.GetBytes("INIT_VECTOR_16!!");
            aes.Key = key;
            aes.IV = iv;
            var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(shortCardEntity.NumeroCarte);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            shortCardEntity.NumeroCarte = Convert.ToBase64String(encrypted);

            _context.CarteBancaires.Add(shortCardEntity);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _controller.GetCarteBancaireByCompteID(1);
            });
        }

        #endregion
    }
}