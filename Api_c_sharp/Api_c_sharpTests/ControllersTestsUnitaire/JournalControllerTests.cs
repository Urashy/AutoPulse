using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
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
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace App.ControllersUnitaires.Tests
{
    [TestClass()]
    public class JournalControllerTests
    {
        private JournalController _controller;
        private AutoPulseBdContext _context;
        private JournalManager _manager;
        private IMapper _mapper;
        private Journal _objetcommun;
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

            _manager = new JournalManager(_context, NullLogger<JournalManager>.Instance);
            _journalService = _manager; // JournalManager implémente IJournalService
            _controller = new JournalController(_manager, _mapper);

            _context.Journaux.RemoveRange(_context.Journaux);
            await _context.SaveChangesAsync();

            TypeCompte typeCompte = new TypeCompte
            {
                IdTypeCompte = 1,
                Libelle = "Utilisateur",
                Cherchable = true
            };

            Compte compte = new Compte
            {
                IdCompte = 1,
                Pseudo = "testuser",
                MotDePasse = "Password123!",
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@gmail.com",
                DateCreation = DateTime.UtcNow,
                DateDerniereConnexion = DateTime.UtcNow,
                DateNaissance = new DateTime(2000, 1, 1),
                IdTypeCompte = 1
            };

            // Ajout des types de journaux requis
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
                new TypeJournal { IdTypeJournaux = 14, LibelleTypeJournaux = "Utilisateur bloque un autre utilisateur" }
            );

            Journal journal = new Journal
            {
                IdJournal = 1,
                ContenuJournal = "Ceci est le contenu du premier journal.",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            _context.TypesCompte.Add(typeCompte);
            _context.Comptes.Add(compte);
            _context.Journaux.Add(journal);
            await _context.SaveChangesAsync();

            _objetcommun = journal;
        }

        // ==========================================
        // TESTS CRUD CLASSIQUES
        // ==========================================

        [TestMethod]
        public async Task GetByIdTest()
        {
            var result = await _controller.GetByID(_objetcommun.IdJournal);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(JournalDTO));
            Assert.AreEqual(_objetcommun.ContenuJournal, result.Value.ContenuJournal);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            var result = await _controller.GetByID(0);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            var result = await _controller.GetAll();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<JournalDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.ContenuJournal == _objetcommun.ContenuJournal));
        }

        [TestMethod]
        public async Task PostJournalTest_Entity()
        {
            JournalDTO journal = new JournalDTO
            {
                ContenuJournal = "Nouveau journal de test",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            var actionResult = await _controller.Post(journal);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            Journal createdjournal = (Journal)created.Value;
            Assert.AreEqual(journal.ContenuJournal, createdjournal.ContenuJournal);
        }

        [TestMethod]
        public async Task DeleteJournalTest()
        {
            var result = await _controller.Delete(_objetcommun.IdJournal);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedJournal = await _manager.GetByIdAsync(_objetcommun.IdJournal);
            Assert.IsNull(deletedJournal);
        }

        [TestMethod]
        public async Task NotFoundDeleteJournalTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutJournalTest()
        {
            JournalDTO journal = new JournalDTO()
            {
                IdJournal = _objetcommun.IdJournal,
                ContenuJournal = "Journal modifié",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            var result = await _controller.Put(_objetcommun.IdJournal, journal);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            Journal journalput = await _manager.GetByIdAsync(_objetcommun.IdJournal);
            Assert.AreEqual(journal.ContenuJournal, journalput.ContenuJournal);
        }

        [TestMethod]
        public async Task NotFoundPutJournalTest()
        {
            JournalDTO journal = new JournalDTO()
            {
                ContenuJournal = "Journal modifié",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            var result = await _controller.Put(0, journal);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutJournalTest()
        {
            JournalDTO journal = new JournalDTO()
            {
                ContenuJournal = null,
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            _controller.ModelState.AddModelError("ContenuJournal", "Required");

            var result = await _controller.Put(_objetcommun.IdJournal, journal);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task BadRequestPostJournalTest()
        {
            JournalDTO journal = new JournalDTO()
            {
                ContenuJournal = null,
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            _controller.ModelState.AddModelError("ContenuJournal", "Required");

            var actionResult = await _controller.Post(journal);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task GetJournalByTypeTest()
        {
            var result = await _controller.GetAllByType(1);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<JournalDTO>));
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task NotFoundGetJournalByTypeTest()
        {
            var result = await _controller.GetAllByType(999);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        // ==========================================
        // TESTS IJournalService - Méthodes de logging
        // ==========================================

        [TestMethod]
        public async Task LogConnexionAsyncTest()
        {
            int idCompte = 1;
            int countBefore = _context.Journaux.Count();

            await _journalService.LogConnexionAsync(idCompte);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(1, lastJournal.IdTypeJournal); // Type Connexion
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Connexion"));
        }

        [TestMethod]
        public async Task LogDeconnexionAsyncTest()
        {
            int idCompte = 1;
            int countBefore = _context.Journaux.Count();

            await _journalService.LogDeconnexionAsync(idCompte);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(2, lastJournal.IdTypeJournal); // Type Déconnexion
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Déconnexion"));
        }

        [TestMethod]
        public async Task LogCreationCompteAsyncTest()
        {
            int idCompte = 1;
            string pseudo = "testuser";
            int countBefore = _context.Journaux.Count();

            await _journalService.LogCreationCompteAsync(idCompte, pseudo);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(3, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains(pseudo));
        }

        [TestMethod]
        public async Task LogModificationProfilAsyncTest()
        {
            int idCompte = 1;
            int countBefore = _context.Journaux.Count();

            await _journalService.LogModificationProfilAsync(idCompte);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(4, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Modification"));
        }

        [TestMethod]
        public async Task LogPublicationAnnonceAsyncTest()
        {
            int idCompte = 1;
            int idAnnonce = 100;
            string titre = "Voiture de sport";
            int countBefore = _context.Journaux.Count();

            await _journalService.LogPublicationAnnonceAsync(idCompte, idAnnonce, titre);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(5, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains(titre));
            Assert.IsTrue(lastJournal.ContenuJournal.Contains(idAnnonce.ToString()));
        }

        [TestMethod]
        public async Task LogModificationAnnonceAsyncTest()
        {
            int idCompte = 1;
            int idAnnonce = 100;
            string titre = "Voiture modifiée";
            int countBefore = _context.Journaux.Count();

            await _journalService.LogModificationAnnonceAsync(idCompte, idAnnonce, titre);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(6, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Modification"));
        }

        [TestMethod]
        public async Task LogSuppressionAnnonceAsyncTest()
        {
            int idCompte = 1;
            int idAnnonce = 100;
            string titre = "Voiture supprimée";
            int countBefore = _context.Journaux.Count();

            await _journalService.LogSuppressionAnnonceAsync(idCompte, idAnnonce, titre);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(7, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Suppression"));
        }

        [TestMethod]
        public async Task LogAchatAsyncTest()
        {
            int idAcheteur = 1;
            int idVendeur = 2;
            int idCommande = 50;
            int idAnnonce = 100;
            int idMoyenPaiement = 1;
            int countBefore = _context.Journaux.Count();

            await _journalService.LogAchatAsync(idAcheteur, idVendeur, idCommande, idAnnonce, idMoyenPaiement);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(8, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Achat"));
            Assert.IsTrue(lastJournal.ContenuJournal.Contains(idCommande.ToString()));
        }

        [TestMethod]
        public async Task LogSignalementAsyncTest()
        {
            int idSignalant = 1;
            int idSignale = 2;
            int idSignalement = 10;
            int idTypeSignalement = 1;
            string description = "Comportement inapproprié";
            int countBefore = _context.Journaux.Count();

            await _journalService.LogSignalementAsync(idSignalant, idSignale, idSignalement, idTypeSignalement, description);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(9, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Signalement"));
        }

        [TestMethod]
        public async Task LogDepotAvisAsyncTest()
        {
            int idJugeur = 1;
            int idJuge = 2;
            int idAvis = 20;
            int note = 5;
            string description = "Excellent vendeur";
            int countBefore = _context.Journaux.Count();

            await _journalService.LogDepotAvisAsync(idJugeur, idJuge, idAvis, note, description);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(10, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("avis"));
            Assert.IsTrue(lastJournal.ContenuJournal.Contains(note.ToString()));
        }

        [TestMethod]
        public async Task LogMiseFavorisAsyncTest()
        {
            int idCompte = 1;
            int idAnnonce = 100;
            int countBefore = _context.Journaux.Count();

            await _journalService.LogMiseFavorisAsync(idCompte, idAnnonce);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(11, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("favoris"));
        }

        [TestMethod]
        public async Task LogEnvoiMessageAsyncTest()
        {
            int idCompte = 1;
            int idConversation = 5;
            string message = "Bonjour, est-ce toujours disponible ?";
            int countBefore = _context.Journaux.Count();

            await _journalService.LogEnvoiMessageAsync(idCompte, idConversation, message);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(12, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("message"));
        }

        [TestMethod]
        public async Task LogGenerationFactureAsyncTest()
        {
            int idCompte = 1;
            int idFacture = 30;
            int idCommande = 50;
            int countBefore = _context.Journaux.Count();

            await _journalService.LogGenerationFactureAsync(idCompte, idFacture, idCommande);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(13, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("facture"));
        }

        [TestMethod]
        public async Task LogBlocageUtilisateurAsyncTest()
        {
            int idBloqueur = 1;
            int idBloque = 2;
            int countBefore = _context.Journaux.Count();

            await _journalService.LogBlocageUtilisateurAsync(idBloqueur, idBloque);

            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(14, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Blocage"));
        }

        [TestMethod]
        public async Task GetJournalByTypeFromServiceTest()
        {
            // Créer plusieurs journaux de différents types
            await _journalService.LogConnexionAsync(1);
            await _journalService.LogConnexionAsync(1);
            await _journalService.LogDeconnexionAsync(1);

            var connexionJournaux = await _journalService.GetJournalByType(1);

            Assert.IsNotNull(connexionJournaux);
            Assert.IsTrue(connexionJournaux.Count() >= 2);
            Assert.IsTrue(connexionJournaux.All(j => j.IdTypeJournal == 1));
        }
    }
}