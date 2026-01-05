using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class AdresseControllerTests
    {
        private AdresseController _controller;
        private AutoPulseBdContext _context;
        private AdresseManager _manager;
        private IMapper _mapper;
        private Adresse _objetcommun;

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

            _manager = new AdresseManager(_context);
            _controller = new AdresseController(_manager, _mapper);

            _context.Adresses.RemoveRange(_context.Adresses);
            await _context.SaveChangesAsync();

            var pays = new Pays
            {
                Libelle = "France"
            };

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

            var adresse = new Adresse
            {
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = 1,
                IdCompte = 1
            };
            _context.TypesCompte.Add(typeCompte);
            _context.Pays.Add(pays);
            _context.Comptes.Add(compte);
            _context.Adresses.Add(adresse);
            await _context.SaveChangesAsync();

            _objetcommun = adresse;
        }
        #region GET
            #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetcommun.IdAdresse);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(AdresseDTO));
            Assert.AreEqual(_objetcommun.Rue, result.Value.Rue);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Act
            var result = await _controller.GetById(0);

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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AdresseDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Rue == _objetcommun.Rue));
        }
        #endregion

            #region GetByCompteID
        [TestMethod]
        public async Task GetAdresseByCompteIDTest()
        {
            // Act
            var result = await _controller.GetAdressesByCompteId(_objetcommun.IdCompte);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AdresseDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Rue == _objetcommun.Rue));
        }

        [TestMethod]
        public async Task NotFoundGetAdresseByCompteIDTest()
        {
            // Act
            var result = await _controller.GetAdressesByCompteId(0);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #endregion

        #region POST
        [TestMethod]
        public async Task PostAdresseTest_Entity()
        {
            // Arrange
            AdresseCreateDTO adresse = new AdresseCreateDTO()
            {
                Nom = "Travail",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 15,
                IdPays = 1,       
                IdCompte = 1
            }
            ;

            // Act
            var actionResult = await _controller.Post(adresse);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdAdresse = (Adresse)created.Value;
            Assert.AreEqual(adresse.Rue, createdAdresse.Rue);
        }

        [TestMethod]
        public async Task BadRequestPostAdresseTest()
        {
            // Arrange
            AdresseCreateDTO adresse = new AdresseCreateDTO
            {
                Nom = null,
            };

            _controller.ModelState.AddModelError("Nom", "Required");

            // Act
            var actionResult = await _controller.Post(adresse);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteAdresseTest()
        {
            // Act
            var result = await _controller.Delete(_objetcommun.IdAdresse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedAdresse = await _manager.GetByIdAsync(_objetcommun.IdAdresse);
            Assert.IsNull(deletedAdresse);
        }

        [TestMethod]
        public async Task NotFoundDeleteAdresseTest()
        {
            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutAdresseTest()
        {
            // Arrange
            AdresseUpdateDTO adresse = new AdresseUpdateDTO()
            {
                IdAdresse = _objetcommun.IdAdresse,
                Nom = "Domicile",
                LibelleVille = "Chavanod",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = 1,
                IdCompte = 1,
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdAdresse, adresse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var adresseput = await _manager.GetByIdAsync(_objetcommun.IdAdresse);
            Assert.AreEqual(adresse.Nom, adresseput.Nom);
        }

        [TestMethod]
        public async Task NotFoundPutAdresseTest()
        {
            // Arrange
            AdresseUpdateDTO adresse = new AdresseUpdateDTO()
            {
                IdAdresse = _objetcommun.IdAdresse,
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = 12,
                IdPays = 1,
                IdCompte = 1,
            };

            // Act
            var result = await _controller.Put(0, adresse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutAdresseTest()
        {
            // Arrange
            AdresseUpdateDTO adresse = new AdresseUpdateDTO()
            {
                IdAdresse = _objetcommun.IdAdresse,
                Nom = "Domicile",
                LibelleVille = "Annecy",
                CodePostal = "74000",
                Rue = "Route de test",
                Numero = -12,
                IdPays = 1,
                IdCompte = 1,
            };
            _controller.ModelState.AddModelError("Numero", "Le Numero doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdAdresse, adresse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion   
    }
}