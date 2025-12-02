using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using App.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Api_c_sharp.Models.Entity;
using Microsoft.Extensions.Configuration;
using Api_c_sharp.Models.Authentification;

namespace App.Controllers.Tests
{
    [TestClass()]
    public class CompteControllerTests
    {
        private CompteController _controller;
        private AutoPulseBdContext _context;
        private CompteManager _manager;
        private IMapper _mapper;
        private Compte _objetcommun;
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            var mapperconfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = mapperconfig.CreateMapper();

            _manager = new CompteManager(_context);
            _controller = new CompteController(_manager, _mapper, config);

            _context.Comptes.RemoveRange(_context.Comptes);
            await _context.SaveChangesAsync();

            _context.Marques.Add(new Marque { IdMarque = 1, LibelleMarque = "TestMarque" });
            _context.Motricites.Add(new Motricite { IdMotricite = 1, LibelleMotricite = "4x4" });
            _context.Carburants.Add(new Carburant { IdCarburant = 1, LibelleCarburant = "Essence" });
            _context.BoitesDeVitesses.Add(new BoiteDeVitesse { IdBoiteDeVitesse = 1, LibelleBoite = "Manuelle" });
            _context.Categories.Add(new Categorie { IdCategorie = 1, LibelleCategorie = "SUV" });
            _context.Modeles.Add(new Modele { IdModele = 1, LibelleModele = "Modele Test" });

            TypeCompte typeCompte = new TypeCompte
            {
                IdTypeCompte = 1,
                Libelle = "Standard"
            };

            TypeSignalement typeSignalement = new TypeSignalement
            {
                IdTypeSignalement = 1,
                LibelleTypeSignalement = "Spam"
            };

            Signalement signalement = new Signalement
            {
                IdSignalement = 1,
                DateCreationSignalement = DateTime.UtcNow,
                DescriptionSignalement = "This is a spam report.",
                IdTypeSignalement = typeSignalement.IdTypeSignalement,
                IdCompteSignalant = 1,
                IdCompteSignale = 1
            };

            Compte compte = new Compte
            {
                IdCompte = 1,
                Nom = "Doe",
                Prenom = "John",
                Email = "john@gmail.com",
                MotDePasse = "728b252625ebcddcea74d61760866080a10196087c340a57a88ba511bd387921",
                Pseudo = "johndoe",
                DateCreation = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = typeCompte.IdTypeCompte,
                DateDerniereConnexion = DateTime.UtcNow,
                SignalementsFaits = new List<Signalement> { signalement }

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

            await _context.Pays.AddAsync(pays);
            await _context.Adresses.AddAsync(adresse);
            await _context.MisesEnAvant.AddAsync(miseEnAvant);
            await _context.EtatAnnonces.AddAsync(etatAnnonce);
            await _context.Voitures.AddAsync(voiture);
            await _context.TypesCompte.AddAsync(typeCompte);
            await _context.Comptes.AddAsync(compte);
            await _context.Annonces.AddAsync(annonce);
            await _context.Favoris.AddAsync(favori);
            await _context.SaveChangesAsync();

            _objetcommun = compte;
        }

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CompteDetailDTO));
            Assert.AreEqual(_objetcommun.Nom, result.Value.Nom);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CompteGetDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Pseudo == _objetcommun.Pseudo));
        }

        [TestMethod]
        public async Task PostAdresseTest_Entity()
        {
            CompteCreateDTO compteCreateDTO = new CompteCreateDTO
            {
                Nom = "Smith",
                Prenom = "Jane",
                Email = "jane.smith@gmail.com",
                MotDePasse = "anotherhashedpassword",
                Pseudo = "janesmith",
                DateNaissance = new DateTime(1992, 2, 2),
                IdTypeCompte = 1,
            };

            var actionResult = await _controller.Post(compteCreateDTO);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdcompte = (Compte)created.Value;
            Assert.AreEqual(compteCreateDTO.Email, createdcompte.Email);
        }


        [TestMethod]
        public async Task DeleteAdresseTest()
        {
            var result = await _controller.Delete(_objetcommun.IdCompte);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedAdresse = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.IsNull(deletedAdresse);
        }

        [TestMethod]
        public async Task NotFoundDeleteAdresseTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutAdresseTest()
        {
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = "JohnUpdated",
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
            };

            var result = await _controller.Put(_objetcommun.IdCompte, compteUpdateDTO);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var compteput = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(compteUpdateDTO.Nom, compteput.Nom);
        }

        [TestMethod]
        public async Task NotFoundPutAdresseTest()
        {
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = "JohnUpdated",
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
            };

            var result = await _controller.Put(0, compteUpdateDTO);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutAdresseTest()
        {
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = null,
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("Prenom", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdCompte, compteUpdateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task BadRequestPostAdresseTest()
        {
            CompteCreateDTO compteUpdateDTO = new CompteCreateDTO
            {
                Nom = "DoeUpdated",
                Prenom = "john",
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
                MotDePasse = "hashedpassword",
                Pseudo = "johndoe",
                NumeroSiret = null,
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("NumeroSiret", "Required");

            var actionResult = await _controller.Post(compteUpdateDTO);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task PutAnonymiseTest()
        {
            var result = await _controller.PutAnonymise(_objetcommun.IdCompte);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var compteanonymise = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual("ANONYME", compteanonymise.Nom);
            Assert.AreEqual("Utilisateur", compteanonymise.Prenom);
        }

        [TestMethod]
        public async Task NotFoundPutAnonymiseTest()
        {
            var result = await _controller.PutAnonymise(0);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetByStringTest()
        {
            // Act
            var result = await _controller.GetByString(_objetcommun.Email);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CompteDetailDTO));
            Assert.AreEqual(_objetcommun.Nom, result.Value.Nom);
        }

        [TestMethod]
        public async Task NotFoundGetByStringTest()
        {
            // Act
            var result = await _controller.GetByString("NonExistentMail");
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetByTypeCompteTest()
        {
            // Act
            var result = await _controller.GetByTypeCompte(_objetcommun.IdTypeCompte);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CompteGetDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Pseudo == _objetcommun.Pseudo));
        }

        [TestMethod]
        public async Task NotFoundGetByTypeCompteTest()
        {
            // Act
            var result = await _controller.GetByTypeCompte(999);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetCompteByAnnonceFavoriTest()
        {
            var result = await _controller.GetCompteByAnnonceFavori(1);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CompteGetDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Pseudo == _objetcommun.Pseudo));
        }

        [TestMethod]
        public async Task NotFoundGetCompteByAnnonceFavoriTest()
        {
            // Act
            var result = await _controller.GetCompteByAnnonceFavori(999);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }


        [TestMethod]
        public async Task ModifMotDePasseTest()
        {
            ChangementMdpDTO changementMdpDTO = new ChangementMdpDTO
            {
                IdCompte = _objetcommun.IdCompte,
                MotDePasse = "ouioui",
                Email = _objetcommun.Email
            };
            string Hashpassword = "728b252625ebcddcea74d61760866080a10196087c340a57a88ba511bd387921";
            var result = await _controller.ModifMdp(changementMdpDTO);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(Hashpassword, compteModifie.MotDePasse);
        }

        [TestMethod]
        public async Task NotFoundModifMotDePasseTest()
        {
            ChangementMdpDTO changementMdpDTO = new ChangementMdpDTO
            {
                IdCompte = 0,
                MotDePasse = "ouioui",
                Email = _objetcommun.Email
            };
            var result = await _controller.ModifMdp(changementMdpDTO);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task VerifUserTest()
        {
            ChangementMdpDTO changementMdpDTO = new ChangementMdpDTO
            {
                IdCompte = _objetcommun.IdCompte,
                MotDePasse = "ouioui",
                Email = _objetcommun.Email
            };
            bool result = _controller.VerifUser(changementMdpDTO);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task NotVerifUserTest()
        {
            ChangementMdpDTO changementMdpDTO = new ChangementMdpDTO
            {
                IdCompte = _objetcommun.IdCompte,
                MotDePasse = "nonnon",
                Email = _objetcommun.Email
            };
            bool result = _controller.VerifUser(changementMdpDTO);
            Assert.IsFalse(result);
        }
    }
}