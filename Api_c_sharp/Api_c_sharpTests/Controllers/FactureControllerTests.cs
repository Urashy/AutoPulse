using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoPulse.Shared.DTO;
using App.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api_c_sharp.Controllers;

namespace App.Controllers.Tests
{
    [TestClass()]
    public class FactureControllerTests
    {
        private FactureController _controller;
        private AutoPulseBdContext _context;
        private FactureManager _manager;
        private IMapper _mapper;
        private Facture _objetcommun;

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

            _manager = new FactureManager(_context);
            _controller = new FactureController(_manager, _mapper);

            _context.Marques.Add(new Marque { IdMarque = 1, LibelleMarque = "TestMarque" });
            _context.Motricites.Add(new Motricite { IdMotricite = 1, LibelleMotricite = "4x4" });
            _context.Carburants.Add(new Carburant { IdCarburant = 1, LibelleCarburant = "Essence" });
            _context.BoitesDeVitesses.Add(new BoiteDeVitesse { IdBoiteDeVitesse = 1, LibelleBoite = "Manuelle" });
            _context.Categories.Add(new Categorie { IdCategorie = 1, LibelleCategorie = "SUV" });
            _context.Modeles.Add(new Modele { IdModele = 1, LibelleModele = "Modele Test" });

            MoyenPaiement moyenPaiement = new MoyenPaiement
            {
                IdMoyenPaiement = 1,
                TypePaiement = "Carte Bancaire"
            };

            TypeCompte typeCompte = new TypeCompte
            {
                IdTypeCompte= 1,
                Libelle = "Acheteur"
            };

            Compte acheteur = new Compte
            {
                IdCompte = 1,
                Nom = "Doe",
                Prenom = "John",
                Email = "john.doe@gmail.com",
                Pseudo = "johndoe",
                MotDePasse = "Password123!",
                DateCreation = DateTime.Now,
                DateNaissance = new DateTime(1990, 1, 1),
                DateDerniereConnexion = DateTime.Now,
                IdTypeCompte =typeCompte.IdTypeCompte
            };

            Compte vendeur = new Compte
            {
                IdCompte = 2,
                Nom = "Test",
                Prenom = "John",
                Email = "john.Test@gmail.com",
                Pseudo = "testjohn",
                MotDePasse = "Password123!",
                DateCreation = DateTime.Now,
                DateNaissance = new DateTime(1990, 1, 1),
                DateDerniereConnexion = DateTime.Now,
                IdTypeCompte = typeCompte.IdTypeCompte
            };

            Voiture voiture = new Voiture
            {
                IdVoiture = 1,
                IdMarque = 1,
                IdModele = 1,
                IdCategorie = 1,
                IdCarburant = 1,
                IdBoiteDeVitesse = 1,
                IdMotricite = 1,
                Kilometrage = 5000,
                Annee = 2021,
                Puissance = 150,
                MiseEnCirculation = new DateTime(2021, 6, 15)
            };

            Annonce annnonce = new Annonce
            {
                IdAnnonce = 1,
                Libelle = "Annonce Test",
                Description = "Description de l'annonce test",
                Prix = 10000,
                DatePublication = DateTime.Now,
                IdCompte = vendeur.IdCompte,
                IdVoiture = 1,
            };

            Commande commande = new Commande
            {
                IdCommande = 1,
                Date = DateTime.Now,
                IdAcheteur = acheteur.IdCompte,
                IdAnnonce = annnonce.IdAnnonce,
                IdMoyenPaiement = moyenPaiement.IdMoyenPaiement,
                IdVendeur = vendeur.IdCompte
            };

            Facture objet = new Facture()
            {
                IdFacture = 1,
                IdCommande = commande.IdCommande,
            };
            await _context.TypesCompte.AddAsync(typeCompte);
            await _context.MoyensPaiements.AddAsync(moyenPaiement);
            await _context.Comptes.AddAsync(acheteur);
            await _context.Comptes.AddAsync(vendeur);
            await _context.Voitures.AddAsync(voiture);
            await _context.Annonces.AddAsync(annnonce);
            await _context.Commandes.AddAsync(commande);
            await _context.Factures.AddAsync(objet);
            await _context.SaveChangesAsync();

            _objetcommun = objet;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdFacture);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(FactureDTO));
            Assert.AreEqual(_objetcommun.IdFacture, result.Value.IdFacture);
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

        [TestMethod]
        public async Task GetAllTest()
        {
            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<FactureDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.IdFacture == _objetcommun.IdFacture));
        }

        [TestMethod]
        public async Task PostVoitureTest_Entity()
        {
            var voiture = new FactureDTO
            {
                IdCommande = 1,
            };

            var actionResult = await _controller.Post(voiture);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdVoiture = (Facture)created.Value;
            Assert.AreEqual(voiture.IdCommande, createdVoiture.IdCommande);
        }


        [TestMethod]
        public async Task DeleteVoitureTest()
        {
            var result = await _controller.Delete(_objetcommun.IdFacture);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedVoiture = await _manager.GetByIdAsync(_objetcommun.IdFacture);
            Assert.IsNull(deletedVoiture);
        }

        [TestMethod]
        public async Task NotFoundDeleteVoitureTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutVoitureTest()
        {
            var voiture = new FactureDTO()
            {
                IdFacture = _objetcommun.IdFacture,
                IdCommande = _objetcommun.IdCommande,

            };

            var result = await _controller.Put(_objetcommun.IdFacture, voiture);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var fetchedVoiture = await _manager.GetByIdAsync(_objetcommun.IdFacture);
            Assert.AreEqual(voiture.IdCommande, fetchedVoiture.IdCommande);
        }

        [TestMethod]
        public async Task NotFoundPutVoitureTest()
        {
            var voiture = new FactureDTO()
            {
                IdCommande = 9999,
            };

            var result = await _controller.Put(0, voiture);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutVoitureTest()
        {
            // Arrange : voiture avec kilométrage invalide
            var voiture = new FactureDTO()
            {
                IdCommande = -20,
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("IdCommande", "Le IdCommande doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdFacture, voiture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task BadRequestPostVoitureTest()
        {
            var voiture = new FactureDTO
            {
                IdFacture = _objetcommun.IdFacture,
                IdCommande = -20,
            };

            _controller.ModelState.AddModelError("IdCommande", "Le IdCommande doit être supérieur à 0");

            var actionResult = await _controller.Post(voiture);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

    }
}