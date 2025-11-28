using Api_c_sharp.Controllers;
using Api_c_sharp.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
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


namespace App.Controllers.Tests
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

        [TestMethod]
        public async Task GetByIdsTest()
        {
            // Given : un enregistrement existant en base (_objetCommun)

            // When : on appelle le contrôleur pour récupérer l'objet par son ID
            var result = await _controller.GetByIDs(_objetCommun.IdVoiture,_objetCommun.IdCouleur);

            // Then : l'objet est retrouvé et correspond aux valeurs attendues
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(_objetCommun.IdCouleur, result.Value.IdCouleur);
            Assert.AreEqual(_objetCommun.IdVoiture, result.Value.IdVoiture);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Given : un ID inexistant

            // When : on demande un objet avec cet ID
            var result = await _controller.GetByIDs(0,0);  // ID inexistant

            // Then : la réponse est NotFound
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Given : au moins un enregistrement en base (_objetCommun)

            // When : on récupère toute la liste
            var result = await _controller.GetAll();

            // Then : la liste contient des éléments dont celui inséré en setup
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);

            var list = result.Value.ToList();
            Assert.IsTrue(list.Any());
            Assert.IsTrue(list.Any(x => x.IdCouleur == _objetCommun.IdCouleur));  // Vérification sur IdCouleur
        }

        [TestMethod]
        public async Task PostTest()
        {
            // Given : un DTO valide à insérer
            var dto = new APourCouleurDTO
            {
                IdVoiture = 5,
                IdCouleur = 50
            };

            // When : on appelle le POST
            var actionResult = await _controller.Post(dto);

            // Then : l'objet est créé et renvoyé dans un CreatedAtActionResult
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            var createdEntity = (APourCouleur)created.Value;

            Assert.AreEqual(dto.IdCouleur, createdEntity.IdCouleur); 
            Assert.AreEqual(dto.IdVoiture, createdEntity.IdVoiture); 
        }

        [TestMethod]
        public async Task BadRequestPostTest()
        {
            // Given : un DTO invalide + ModelState en erreur
            var dto = new APourCouleurDTO();
            _controller.ModelState.AddModelError("IdCouleur", "Required");

            // When : on appelle POST avec un modèle invalide
            var actionResult = await _controller.Post(dto);

            // Then : la réponse est BadRequest
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task PutTest()
        {
            // Given : un DTO valide pour mettre à jour un élément existant
            var dto = new APourCouleurDTO
            {
                IdVoiture = 1,  // Modification de IdVoiture
                IdCouleur = _objetCommun.IdCouleur  // IdCouleur reste le même
            };

            // When : on appelle PUT
            var result = await _controller.Put(_objetCommun.IdVoiture, _objetCommun.IdCouleur, dto);

            // Then : la réponse est NoContent et l'objet est mis à jour
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updated = await _manager.GetAPourCouleursByIDS(_objetCommun.IdVoiture,_objetCommun.IdCouleur);  // Utilisation de IdCouleur
            Assert.AreEqual(dto.IdVoiture, updated.IdVoiture);  // Vérification sur IdVoiture
        }

        [TestMethod]
        public async Task NotFoundPutTest()
        {
            // Given : un DTO avec un ID inexistant
            var dto = new APourCouleurDTO
            {
                IdVoiture = 1,
                IdCouleur = 9999
            };

            // When : on tente de mettre à jour cet ID
            var result = await _controller.Put(0,0, dto);

            // Then : le contrôleur retourne NotFound
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutTest()
        {
            // Given : un DTO valide mais ModelState invalide
            var dto = new APourCouleurDTO
            {
                IdVoiture = 1,  // IdVoiture
                IdCouleur = _objetCommun.IdCouleur  // IdCouleur
            };
            _controller.ModelState.AddModelError("IdCouleur", "Invalid");

            // When : on appelle PUT
            var result = await _controller.Put(_objetCommun.IdVoiture, _objetCommun.IdCouleur, dto);

            // Then : le contrôleur retourne BadRequest
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task DeleteTest()
        {
            // Given : un enregistrement existant à supprimer (_objetCommun)

            // When : on appelle DELETE
            var result = await _controller.Delete(_objetCommun.IdVoiture, _objetCommun.IdCouleur);

            // Then : la réponse est NoContent et l'objet est supprimé
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deleted = await _manager.GetAPourCouleursByIDS(_objetCommun.IdVoiture,_objetCommun.IdCouleur);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task NotFoundDeleteTest()
        {
            // Given : un ID inexistant

            // When : on appelle DELETE
            var result = await _controller.Delete(0,0);

            // Then : la réponse est NotFound
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}

