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
    public class PlainteControllerTests
    {
        private PlainteController _controller;
        private AutoPulseBdContext _context;
        private PlainteManager _manager;
        private IMapper _mapper;
        private Plainte _objetcommun;

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

            _manager = new PlainteManager(_context);
            _controller = new PlainteController(_manager, _mapper);

            _context.Adresses.RemoveRange(_context.Adresses);
            await _context.SaveChangesAsync();

            TypeCompte typeCompte = new TypeCompte
            {
                Libelle = "Utilisateur"
            };

            EtatCompte etatCompte = new EtatCompte
            {
                IdEtatCompte = 1,
                Libelle = "Actif"
            };

            Compte compte = new Compte
            {
                Pseudo = "testuser",
                MotDePasse = "Password123!",
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@gmail.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = new DateTime(2000, 1, 1),
                IdTypeCompte = 1,
                IdEtatCompte = 1
            };

            TypeSignalement typeSignalement = new TypeSignalement
            {
                IdTypeSignalement = 1,
                LibelleTypeSignalement = "Bug"
            };

            Signalement signalement = new Signalement
            {
                DateCreationSignalement = DateTime.Now,
                DescriptionSignalement = "Ceci est un signalement de test.",
                IdCompteSignalant = 1,
                IdCompteSignale = 1,
                IdTypeSignalement = 1
            };

            Plainte plainte = new Plainte
            {
                IdPlainte = 1,
                Description = "Ceci est une plainte de test. ",
                DateCreation = DateTime.Now,
                IdSignalement = 1,
                IdCompte = 1
            };
            _context.TypesCompte.Add(typeCompte);
            _context.EtatComptes.Add(etatCompte);
            _context.Comptes.Add(compte);
            _context.TypesSignalement.Add(typeSignalement);
            _context.Signalements.Add(signalement);
            _context.Plaintes.Add(plainte);
            await _context.SaveChangesAsync();

            _objetcommun = plainte;
        }
        #region GET
            #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdPlainte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(PlainteDTO));
            Assert.AreEqual(_objetcommun.Description, result.Value.Description);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<PlainteDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Description == _objetcommun.Description));
        }
        #endregion

            #region GetPlainteByCompte
        [TestMethod]
        public async Task GetPlainteByCompteIDTest()
        {
            // Act
            var result = await _controller.GetByIDCompte(_objetcommun.IdCompte);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<PlainteDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Description == _objetcommun.Description));
        }

        [TestMethod]
        public async Task NotFoundGetPlainteByCompteIDTest()
        {
            // Act
            var result = await _controller.GetByIDCompte(0);
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
            PlainteCreateDTO dto = new PlainteCreateDTO
            {
                Description = "Nouvelle plainte de test",
                IdSignalement = 1,
                IdCompte = 1
            };

            // Act
            var actionResult = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdAdresse = (Plainte)created.Value;
            Assert.AreEqual(dto.Description, createdAdresse.Description);
        }

        [TestMethod]
        public async Task BadRequestPostAdresseTest()
        {
            // Arrange
            PlainteCreateDTO dto = new PlainteCreateDTO()
            {
                Description = null,
                IdSignalement = 1,
                IdCompte = 1
            };

            _controller.ModelState.AddModelError("Description", "Required");

            // Act
            var actionResult = await _controller.Post(dto);
            
            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteAdresseTest()
        {
            // Act
            var result = await _controller.Delete(_objetcommun.IdPlainte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedAdresse = await _manager.GetByIdAsync(_objetcommun.IdPlainte);
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
            PlainteUpdateDTO dto = new PlainteUpdateDTO()
            {
                IdPlainte = _objetcommun.IdPlainte,
                Description = "Plainte modifiée",
                IdSignalement = 1,
                IdCompte = 1
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdPlainte, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var adresseput = await _manager.GetByIdAsync(_objetcommun.IdPlainte);
            Assert.AreEqual(dto.Description, adresseput.Description);
        }

        [TestMethod]
        public async Task NotFoundPutAdresseTest()
        {
            // Arrange
            PlainteUpdateDTO dto = new PlainteUpdateDTO()
            {
                IdPlainte = _objetcommun.IdPlainte,
                Description = "Plainte modifiée",
                IdSignalement = 1,
                IdCompte = 1
            };

            // Act
            var result = await _controller.Put(0, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutAdresseTest()
        {
            // Arrange
            PlainteUpdateDTO dto = new PlainteUpdateDTO()
            {
                IdPlainte = _objetcommun.IdPlainte,
                Description = null,
                IdSignalement = 1,
                IdCompte = 1
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("Description", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdPlainte, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        #endregion
        

        
    }
}