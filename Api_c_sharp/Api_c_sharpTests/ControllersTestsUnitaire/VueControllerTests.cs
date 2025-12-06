using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.ControllersUnitaires.Tests
{
    [TestClass]
    public class VueControllerTests
    {
        private VueController _controller;
        private AutoPulseBdContext _context;
        private VueManager _manager;
        private IMapper _mapper;
        private Vue _objetcommun;

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

            _manager = new VueManager(_context);
            _controller = new VueController(_manager, _mapper);

            // Nettoyage
            _context.APourConversations.RemoveRange(_context.APourConversations);
            await _context.SaveChangesAsync();


            _context.Marques.Add(new Marque { IdMarque = 1, LibelleMarque = "TestMarque" });
            _context.Motricites.Add(new Motricite { IdMotricite = 1, LibelleMotricite = "4x4" });
            _context.Carburants.Add(new Carburant { IdCarburant = 1, LibelleCarburant = "Essence" });
            _context.BoitesDeVitesses.Add(new BoiteDeVitesse { IdBoiteDeVitesse = 1, LibelleBoite = "Manuelle" });
            _context.Categories.Add(new Categorie { IdCategorie = 1, LibelleCategorie = "SUV" });
            _context.Modeles.Add(new Modele { IdModele = 1, LibelleModele = "Modele Test" });

            await _context.SaveChangesAsync();

            TypeCompte typeCompte = new TypeCompte()
            {
                IdTypeCompte = 1,
                Libelle = "Particulier"
            };

            Compte compte = new Compte()
            {
                IdCompte = 1,
                IdTypeCompte = typeCompte.IdTypeCompte,
                Email = "test@gmail.com",
                Pseudo = "TestUser",
                MotDePasse = "Password123",
                Nom = "Doe",
                Prenom = "John",
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = new DateTime(1990, 1, 1),
                DateCreation = DateTime.Now
            };

            Voiture voiture = new Voiture()
            {
                IdVoiture = 1,
                IdMarque = 1,
                IdMotricite = 1,
                IdCarburant = 1,
                IdBoiteDeVitesse = 1,
                IdCategorie = 1,
                Kilometrage = 10000,
                Annee = 2020,
                Puissance = 150,
                MiseEnCirculation = DateTime.Now,
                IdModele = 1,
                NbPlace = 5,
                NbPorte = 5
            };

            EtatAnnonce etatAnnonce = new EtatAnnonce()
            {
                IdEtatAnnonce = 1,
                LibelleEtatAnnonce = "Disponible"
            };

            Pays pays = new Pays()
            {
                IdPays = 1,
                Libelle = "Testland"
            };

            Adresse adresse = new Adresse()
            {
                IdAdresse = 1,
                Nom = "Domicile",
                Rue = "123 Rue de Test",
                LibelleVille = "Testville",
                CodePostal = "12345",
                IdPays = pays.IdPays
            };

            MiseEnAvant miseEnAvant = new MiseEnAvant()
            {
                IdMiseEnAvant = 1,
                LibelleMiseEnAvant = "Standard",
                PrixSemaine = 9,
            };

            Annonce annonce = new Annonce()
            {
                IdAnnonce = 1,
                Libelle = "Annonce Test",
                IdCompte = compte.IdCompte,
                IdEtatAnnonce = etatAnnonce.IdEtatAnnonce,
                IdAdresse = adresse.IdAdresse,
                Prix = 20000,
                Description = "Description de l'annonce",
                IdMiseEnAvant = miseEnAvant.IdMiseEnAvant,
                IdVoiture = voiture.IdVoiture,
            };

            Annonce annonce2 = new Annonce()
            {
                IdAnnonce = 2,
                Libelle = "Annonce Test",
                IdCompte = compte.IdCompte,
                IdEtatAnnonce = etatAnnonce.IdEtatAnnonce,
                IdAdresse = adresse.IdAdresse,
                Prix = 20000,
                Description = "Description de l'annonce",
                IdMiseEnAvant = miseEnAvant.IdMiseEnAvant,
                IdVoiture = voiture.IdVoiture,
            };

            Vue vue = new Vue()
            {
                IdAnnonce = annonce.IdAnnonce,
                IdCompte = compte.IdCompte
            };

            await _context.MisesEnAvant.AddAsync(miseEnAvant);
            await _context.Pays.AddAsync(pays);
            await _context.Adresses.AddAsync(adresse);
            await _context.EtatAnnonces.AddAsync(etatAnnonce);
            await _context.TypesCompte.AddAsync(typeCompte);
            await _context.Comptes.AddAsync(compte);
            await _context.Voitures.AddAsync(voiture);
            await _context.Annonces.AddAsync(annonce);
            await _context.Annonces.AddAsync(annonce2);
            await _context.Vues.AddAsync(vue);
            await _context.SaveChangesAsync();

            _objetcommun = vue;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {

            var result = await _controller.GetByIDs(_objetcommun.IdCompte, _objetcommun.IdAnnonce);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(VueDTO));
            Assert.AreEqual(_objetcommun.IdAnnonce, result.Value.IdAnnonce);
            Assert.AreEqual(_objetcommun.IdCompte, result.Value.IdCompte);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {

            var result = await _controller.GetByIDs(9999, _objetcommun.IdCompte);

            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {

            var result = await _controller.GetAll();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);

            var list = result.Value.ToList();
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<VueDTO>));
            Assert.IsTrue(list.Any());
            Assert.IsTrue(list.Any(x => x.IdAnnonce == _objetcommun.IdAnnonce));
        }

        [TestMethod]
        public async Task PostTest()
        {

            VueDTO dto = new VueDTO
            {
                IdCompte = 1,
                IdAnnonce = 2
            };

            var actionResult = await _controller.Post(dto);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)actionResult.Result;
            var createdEntity = (VueDTO)created.Value;

            Assert.AreEqual(dto.IdAnnonce, createdEntity.IdAnnonce);
            Assert.AreEqual(dto.IdCompte, createdEntity.IdCompte);
        }

        [TestMethod]
        public async Task BadRequestPostTest()
        {
            VueDTO dto = new VueDTO();
            _controller.ModelState.AddModelError("IdConversation", "Required");

            var actionResult = await _controller.Post(dto);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task PutTest()
        {
            VueDTO dto = new VueDTO
            {
                IdCompte = _objetcommun.IdCompte,
                IdAnnonce = _objetcommun.IdAnnonce
            };

            var result = await _controller.Put(_objetcommun.IdCompte, _objetcommun.IdAnnonce, dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updated = await _manager.GetVueByIdsAsync(_objetcommun.IdCompte, _objetcommun.IdAnnonce);
            Assert.AreEqual(dto.IdCompte, updated.IdCompte);
        }

        [TestMethod]
        public async Task NotFoundPutTest()
        {
            VueDTO dto = new VueDTO
            {
                IdCompte = _objetcommun.IdCompte,
                IdAnnonce = _objetcommun.IdAnnonce
            };

            var result = await _controller.Put(9999, 1, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutTest()
        {
            VueDTO dto = new VueDTO
            {
                IdCompte = _objetcommun.IdCompte,
            };
            _controller.ModelState.AddModelError("IdAnnonce", "Required");

            var result = await _controller.Put(_objetcommun.IdCompte, _objetcommun.IdAnnonce, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task DeleteTest()
        {
            var result = await _controller.Delete(_objetcommun.IdCompte, _objetcommun.IdAnnonce);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deleted = await _manager.GetVueByIdsAsync(_objetcommun.IdCompte, _objetcommun.IdAnnonce);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task NotFoundDeleteTest()
        {
            var result = await _controller.Delete(9999, _objetcommun.IdCompte);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
