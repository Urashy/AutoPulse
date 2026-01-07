using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class TypeCompteControllerTests
    {
        private TypeCompteController _controller;
        private AutoPulseBdContext _context;
        private TypeCompteManager _manager;
        private IMapper _mapper;
        private TypeCompte _objetcommun;

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

            _manager = new TypeCompteManager(_context);
            _controller = new TypeCompteController(_manager, _mapper);

            _context.TypesCompte.RemoveRange(_context.TypesCompte);
            await _context.SaveChangesAsync();

            TypeCompte objet = new TypeCompte()
            {
                IdTypeCompte = 1,
                Libelle = "TestTypeCompte",
                Cherchable = true
            };

            Compte compte = new Compte()
            {
                IdCompte = 1,
                Pseudo = "TestPseudo",
                MotDePasse = "TestMotDePasse",
                Nom = "TestNom",
                Prenom = "TestPrenom",
                Email = "test@gmail.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = objet.IdTypeCompte
            };

            await _context.TypesCompte.AddAsync(objet);
            await _context.Comptes.AddAsync(compte);
            await _context.SaveChangesAsync();

            // Initialisation de l'objet commun après l'avoir sauvegardé
            _objetcommun = objet;
        }
        #region GET
            #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetcommun.IdTypeCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(TypeCompteDTO));
            Assert.AreEqual(_objetcommun.Libelle, result.Value.Libelle);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<TypeCompteDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
        }
        #endregion

            #region GetType
        [TestMethod]
        public async Task GetTypeByCherchable()
        {
            // Act
            var result = await _controller.GetTypeComptesPourChercher();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<TypeCompteDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
        }

        [TestMethod]
        public async Task GetTypeCompteByCompteIdTests()
        {
            // Act
            var result = await _controller.GetTypeCompteByCompteId(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(TypeCompteDTO));
            Assert.AreEqual(_objetcommun.Libelle, result.Value.Libelle);
        }

        [TestMethod]
        public async Task NotFoundGetTypeCompteByCompteIdTests()
        {
            // Act
            var result = await _controller.GetTypeCompteByCompteId(0);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #endregion
    }
}