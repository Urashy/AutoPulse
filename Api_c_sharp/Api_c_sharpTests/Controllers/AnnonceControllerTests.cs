using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using App.Controllers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Google.Apis.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Controllers.Tests
{
    [TestClass()]
    public class AnnonceControllerTests
    {
        private AnnonceController _controller;
        private AutoPulseBdContext _context;
        private AnnonceManager _manager;
        private IMapper _mapper;
        private Annonce _objetcommun;
        private IJournalService _journalService;

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

            _journalService = new JournalManager(_context, NullLogger<JournalManager>.Instance);
            _manager = new AnnonceManager(_context);
            _controller = new AnnonceController(_manager, _mapper, _journalService);

            _context.Annonces.RemoveRange(_context.Annonces);
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
            _context.TypesJournal.AddRange(
            new TypeJournal { IdTypeJournaux = 1, LibelleTypeJournaux = "Connexion" },
            new TypeJournal { IdTypeJournaux = 2, LibelleTypeJournaux = "Déconnexion" },
            new TypeJournal { IdTypeJournaux = 3, LibelleTypeJournaux = "Création de compte" },
            new TypeJournal { IdTypeJournaux = 4, LibelleTypeJournaux = "Modification de profil" },
            new TypeJournal { IdTypeJournaux = 5, LibelleTypeJournaux = "Publication d'annonce" },
            new TypeJournal { IdTypeJournaux = 6, LibelleTypeJournaux = "Modification d'annonce" },
            new TypeJournal { IdTypeJournaux = 7, LibelleTypeJournaux = "Suppression d'annonce" },
            new TypeJournal { IdTypeJournaux = 8, LibelleTypeJournaux = "Achat" },
            new TypeJournal { IdTypeJournaux = 9, LibelleTypeJournaux = "Signalement" },
            new TypeJournal { IdTypeJournaux = 10, LibelleTypeJournaux = "Dépôt avis" },
            new TypeJournal { IdTypeJournaux = 11, LibelleTypeJournaux = "Mise en favoris" },
            new TypeJournal { IdTypeJournaux = 12, LibelleTypeJournaux = "Envoyer un message/offre" },
            new TypeJournal { IdTypeJournaux = 13, LibelleTypeJournaux = "Génération de facture" },
            new TypeJournal { IdTypeJournaux = 14, LibelleTypeJournaux = "Utilisateur bloque un autre utilisateur" });

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

            Favori favori = new Favori()
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
            await _context.Favoris.AddAsync(favori);
            await _context.SaveChangesAsync();

            _objetcommun = annonce;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(AnnonceDetailDTO));
            Assert.AreEqual(_objetcommun.Libelle, result.Value.Libelle);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
        }

        [TestMethod]
        public async Task PostAnnonceTest_Entity()
        {
            AnnonceCreateUpdateDTO annonce = new AnnonceCreateUpdateDTO()
            {
                Libelle = "Nouvelle Annonce",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = "Description de la nouvelle annonce",
                IdVoiture = _objetcommun.IdVoiture
            };

            var actionResult = await _controller.Post(annonce);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            Annonce annonceCree = (Annonce)created.Value;
            Assert.AreEqual(annonce.Description, annonceCree.Description);
        }


        [TestMethod]
        public async Task DeleteAnnonceTest()
        {
            var result = await _controller.Delete(_objetcommun.IdAnnonce);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedAnnonce = await _manager.GetByIdAsync(_objetcommun.IdAnnonce);
            Assert.IsNull(deletedAnnonce);
        }

        [TestMethod]
        public async Task NotFoundDeleteAnnonceTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutAnnonceTest()
        {
            AnnonceCreateUpdateDTO annonce = new AnnonceCreateUpdateDTO()
            {
                IdAnnonce = _objetcommun.IdAnnonce,
                Libelle = "Nouvelle Annonce",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = "Description de la nouvelle annonce",
                IdVoiture = _objetcommun.IdVoiture
            };

            var result = await _controller.Put(_objetcommun.IdAnnonce, annonce);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            Annonce fetchannonce = await _manager.GetByIdAsync(_objetcommun.IdAnnonce);
            Assert.AreEqual(annonce.Description, fetchannonce.Description);
        }

        [TestMethod]
        public async Task NotFoundPutAnnonceTest()
        {
            AnnonceCreateUpdateDTO annonce = new AnnonceCreateUpdateDTO()
            {
                IdAnnonce = _objetcommun.IdAnnonce,
                Libelle = "Nouvelle Annonce",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = "Description de la nouvelle annonce",
                IdVoiture = _objetcommun.IdVoiture
            };

            var result = await _controller.Put(0, annonce);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutAnnonceTest()
        {
            AnnonceCreateUpdateDTO annonce = new AnnonceCreateUpdateDTO()
            {
                IdAnnonce = _objetcommun.IdAnnonce,
                Libelle = "Nouvelle Annonce",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = "Description de la nouvelle annonce",
                IdVoiture = _objetcommun.IdVoiture
            };

            _controller.ModelState.AddModelError("Description", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdAnnonce, annonce);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task BadRequestPostAnnonceTest()
        {
            AnnonceCreateUpdateDTO annonce = new AnnonceCreateUpdateDTO()
            {
                IdAnnonce = _objetcommun.IdAnnonce,
                Libelle = "Nouvelle Annonce",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = "Description de la nouvelle annonce",
                IdVoiture = _objetcommun.IdVoiture
            };

            _controller.ModelState.AddModelError("Description", "Required");

            var actionResult = await _controller.Post(annonce);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task GetbystringTest()
        {
            // Act
            var result = await _controller.GetByString("Annonce Test");
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(AnnonceDetailDTO));
            Assert.AreEqual(_objetcommun.Libelle, result.Value.Libelle);
        }

        [TestMethod]
        public async Task NotFoundGetbystringTest()
        {
            // Act
            var result = await _controller.GetByString("Annonce Inexistante");
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));

        }

        [TestMethod]
        public async Task GetByMiseEnavant()
        {
            // Act
            var result = await _controller.GetByIdMiseEnAvant((int)_objetcommun.IdMiseEnAvant);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
        }

        [TestMethod]
        public async Task NotFoundGetByMiseEnAvant()
        {
            // Act
            var result = await _controller.GetByIdMiseEnAvant(0);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetByCompteFavoris()
        {
            // Act
            var result = await _controller.GetByCompteFavoris(_objetcommun.IdCompte);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
        }

        [TestMethod]
        public async Task NotFoundGetByCompteFavoris()
        {
            // Act
            var result = await _controller.GetByCompteFavoris(0);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetByFiltersTest()
        {
            ParametreRecherche parametreRecherche = new ParametreRecherche()
            {
                Departement = "12345",
                IdCarburant = 1,
                IdMarque = 1,
                IdModele = 1,
                PrixMin = 10000,
                PrixMax = 30000,
                IdTypeVoiture = 1,
                IdTypeVendeur = 1,
                Nom = "Annonce",
                KmMin = 5000,
                KmMax = 15000
            };
            // Act
            var result = await _controller.GetFiltered(parametreRecherche);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
        }

        [TestMethod]
        public async Task NotFoundGetByFiltersTest()
        {
            ParametreRecherche parametreRecherche = new ParametreRecherche()
            {
                Departement = "99999",
                IdCarburant = 99,
                IdMarque = 99,
                IdModele = 99,
                PrixMin = 999999,
                PrixMax = 999999,
                IdTypeVoiture = 99,
                IdTypeVendeur = 99,
                Nom = "Inexistant",
                KmMin = 999999,
                KmMax = 999999
            };
            // Act
            var result = await _controller.GetFiltered(parametreRecherche); ;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetByFiltersNoPaginationTest()
        {
            ParametreRecherche parametreRecherche = new ParametreRecherche()
            {
                Departement = "12345",
                IdCarburant = 1,
                IdMarque = 1,
                IdModele = 1,
                PrixMin = 10000,
                PrixMax = 30000,
                IdTypeVoiture = 1,
                IdTypeVendeur = 1,
                Nom = "Annonce",
                KmMin = 5000,
                KmMax = 15000
            };
            // Act
            var result = await _controller.GetFiltered(parametreRecherche);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
        }

        [TestMethod]
        public async Task NotFoundGetByFiltersNoPaginationTest()
        {
            ParametreRecherche parametreRecherche = new ParametreRecherche()
            {
                Departement = "99999",
                IdCarburant = 99,
                IdMarque = 99,
                IdModele = 99,
                PrixMin = 999999,
                PrixMax = 999999,
                IdTypeVoiture = 99,
                IdTypeVendeur = 99,
                Nom = "Inexistant",
                KmMin = 999999,
                KmMax = 999999
            };
            // Act
            var result = await _controller.GetFiltered(parametreRecherche);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAnnonceByCompteIDTest()
        {
            // Act
            var result = await _controller.GetAnnoncesByCompteID(_objetcommun.IdCompte);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == _objetcommun.Libelle));
        }

        [TestMethod]
        public async Task NotFoundGetAnnonceByCompteIDTest()
        {
            // Act
            var result = await _controller.GetAnnoncesByCompteID(0);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}