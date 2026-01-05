using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Api_c_sharp.Models.Entity;
using System.Drawing;


namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass]
    public class APourCouleurControllerTests
    {
        private APourCouleurController _controller;
        private AutoPulseBdContext _context;
        private APourCouleurManager _manager;
        private IMapper _mapper;
        private APourCouleur _objetCommun;

        [TestInitialize]
        public async Task Initialize()
        {
            // Configuration d'une base de données en mémoire pour les tests
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            // Configuration d'AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Initialisation du manager et du contrôleur
            _manager = new APourCouleurManager(_context);
            _controller = new APourCouleurController(_manager, _mapper);

            // Nettoyage de la base de données
            _context.APourCouleurs.RemoveRange(_context.APourCouleurs);
            await _context.SaveChangesAsync();

            _context.Marques.Add(new Marque { IdMarque = 1, LibelleMarque = "TestMarque" });
            _context.Motricites.Add(new Motricite { IdMotricite = 1, LibelleMotricite = "4x4" });
            _context.Carburants.Add(new Carburant { IdCarburant = 1, LibelleCarburant = "Essence" });
            _context.BoitesDeVitesses.Add(new BoiteDeVitesse { IdBoiteDeVitesse = 1, LibelleBoite = "Manuelle" });
            _context.Categories.Add(new Categorie { IdCategorie = 1, LibelleCategorie = "SUV" });
            _context.Modeles.Add(new Modele { IdModele = 1, LibelleModele = "Modele Test" });

            Couleur couleur = new Couleur
            {
                IdCouleur = 1,
                LibelleCouleur = "Rouge",
                CodeHexaCouleur = "#FF0000"
            };

            Voiture voiture = new Voiture
            {
                IdVoiture = 1,
                IdModele = 1,
                IdMarque = 1,
                IdCategorie = 1,
                IdCarburant = 1,
                IdBoiteDeVitesse = 1,
                IdMotricite = 1,
                Annee = 2020,
                Kilometrage = 10000
            };

            APourCouleur entry = new APourCouleur
            {
                IdVoiture = voiture.IdVoiture,
                IdCouleur = couleur.IdCouleur
            };


            await _context.Couleurs.AddAsync(couleur);
            await _context.Voitures.AddAsync(voiture);
            await _context.APourCouleurs.AddAsync(entry);
            await _context.SaveChangesAsync();

            _objetCommun = entry;
        }
        #region GET
            #region GetById
        [TestMethod]
        public async Task GetByIdsTest()
        {
            // Act
            var result = await _controller.GetByIDs(_objetCommun.IdVoiture,_objetCommun.IdCouleur);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(_objetCommun.IdCouleur, result.Value.IdCouleur);
            Assert.AreEqual(_objetCommun.IdVoiture, result.Value.IdVoiture);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Act
            var result = await _controller.GetByIDs(0,0);  

            // Assert
            Assert.IsNotNull(result.Result);
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

            var list = result.Value.ToList();
            Assert.IsTrue(list.Any());
            Assert.IsTrue(list.Any(x => x.IdCouleur == _objetCommun.IdCouleur));  // Vérification sur IdCouleur
        }
        #endregion

        #endregion

        #region POST
        [TestMethod]
        public async Task PostTest()
        {
            // Arrange
            var dto = new APourCouleurDTO
            {
                IdVoiture = 5,
                IdCouleur = 50
            };

            // Act
            var actionResult = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            var createdEntity = (APourCouleur)created.Value;

            Assert.AreEqual(dto.IdCouleur, createdEntity.IdCouleur); 
            Assert.AreEqual(dto.IdVoiture, createdEntity.IdVoiture); 
        }

        [TestMethod]
        public async Task BadRequestPostTest()
        {
            // Arrange
            var dto = new APourCouleurDTO();
            _controller.ModelState.AddModelError("IdCouleur", "Required");

            // Act
            var actionResult = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutTest()
        {
            // Arrange
            var dto = new APourCouleurDTO
            {
                IdVoiture = 1,  // Modification de IdVoiture
                IdCouleur = _objetCommun.IdCouleur  // IdCouleur reste le même
            };

            // Act
            var result = await _controller.Put(_objetCommun.IdVoiture, _objetCommun.IdCouleur, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updated = await _manager.GetAPourCouleursByIDS(_objetCommun.IdVoiture,_objetCommun.IdCouleur);  // Utilisation de IdCouleur
            Assert.AreEqual(dto.IdVoiture, updated.IdVoiture);  // Vérification sur IdVoiture
        }

        [TestMethod]
        public async Task NotFoundPutTest()
        {
            // Arrange
            var dto = new APourCouleurDTO
            {
                IdVoiture = 1,
                IdCouleur = 9999
            };

            // Act
            var result = await _controller.Put(0,0, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutTest()
        {
            // Arrange
            var dto = new APourCouleurDTO
            {
                IdVoiture = 1,  // IdVoiture
                IdCouleur = _objetCommun.IdCouleur  // IdCouleur
            };
            _controller.ModelState.AddModelError("IdCouleur", "Invalid");

            // Act
            var result = await _controller.Put(_objetCommun.IdVoiture, _objetCommun.IdCouleur, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteTest()
        {
            // Act
            var result = await _controller.Delete(_objetCommun.IdVoiture, _objetCommun.IdCouleur);

            // Then : la réponse est NoContent et l'objet est supprimé
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deleted = await _manager.GetAPourCouleursByIDS(_objetCommun.IdVoiture,_objetCommun.IdCouleur);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task NotFoundDeleteTest()
        {
            // Act
            var result = await _controller.Delete(0,0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion
    }
}

