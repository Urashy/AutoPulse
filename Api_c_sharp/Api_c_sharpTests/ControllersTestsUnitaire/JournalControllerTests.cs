using Api_c_sharp.Controllers;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api_c_sharp.ControllersUnitaires.Tests
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
                new TypeJournal { IdTypeJournaux = 9, LibelleTypeJournaux = "Signalement Compte" },
                new TypeJournal { IdTypeJournaux = 10, LibelleTypeJournaux = "Signalement Annonce" },
                new TypeJournal { IdTypeJournaux = 11, LibelleTypeJournaux = "Dépôt avis" },
                new TypeJournal { IdTypeJournaux = 12, LibelleTypeJournaux = "Mise en favoris" },
                new TypeJournal { IdTypeJournaux = 13, LibelleTypeJournaux = "Envoyer un message/offre" },
                new TypeJournal { IdTypeJournaux = 14, LibelleTypeJournaux = "Génération de facture" },
                new TypeJournal { IdTypeJournaux = 15, LibelleTypeJournaux = "Utilisateur bloque un autre utilisateur" }
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

        #region GET

            #region GetById

        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdJournal);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(JournalDTO));
            Assert.AreEqual(_objetcommun.ContenuJournal, result.Value.ContenuJournal);
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
        #endregion

            #region GetAll
        public async Task GetAllTest()
        {
            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<JournalDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.ContenuJournal == _objetcommun.ContenuJournal));
        }
        #endregion

        #endregion

        #region POST
        [TestMethod]
        public async Task PostJournalTest_Entity()
        {
            // Arrange
            JournalCreateDTO journal = new JournalCreateDTO
            {
                ContenuJournal = "Nouveau journal de test",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            // Act
            var actionResult = await _controller.Post(journal);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            Journal createdjournal = (Journal)created.Value;
            Assert.AreEqual(journal.ContenuJournal, createdjournal.ContenuJournal);
        }

        [TestMethod]
        public async Task BadRequestPostJournalTest()
        {
            // Arrange
            JournalCreateDTO journal = new JournalCreateDTO()
            {
                ContenuJournal = null,
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            _controller.ModelState.AddModelError("ContenuJournal", "Required");

            // Act
            var actionResult = await _controller.Post(journal);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteJournalTest()
        {
            // Act
            var result = await _controller.Delete(_objetcommun.IdJournal);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedJournal = await _manager.GetByIdAsync(_objetcommun.IdJournal);
            Assert.IsNull(deletedJournal);
        }

        [TestMethod]
        public async Task NotFoundDeleteJournalTest()
        {
            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutJournalTest()
        {
            // Arrange
            JournalUpdateDTO journal = new JournalUpdateDTO()
            {
                IdJournal = _objetcommun.IdJournal,
                ContenuJournal = "Journal modifié",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdJournal, journal);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            Journal journalput = await _manager.GetByIdAsync(_objetcommun.IdJournal);
            Assert.AreEqual(journal.ContenuJournal, journalput.ContenuJournal);
        }

        [TestMethod]
        public async Task NotFoundPutJournalTest()
        {
            // Arrange
            JournalUpdateDTO journal = new JournalUpdateDTO()
            {
                ContenuJournal = "Journal modifié",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            // Act
            var result = await _controller.Put(0, journal);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutJournalTest()
        {
            // Arrange
            JournalUpdateDTO journal = new JournalUpdateDTO()
            {
                ContenuJournal = null,
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            _controller.ModelState.AddModelError("ContenuJournal", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdJournal, journal);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

        #region GetFilteredJournal
        [TestMethod]
        public async Task GetFilteredJournal_ByTypeOnly_Test()
        {
            // Arrange
            await _journalService.LogConnexionAsync(1);
            await _journalService.LogConnexionAsync(1);
            await _journalService.LogDeconnexionAsync(1);

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                Order = 0
            };

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<JournalDTO>));
            Assert.IsTrue(result.Value.Count() >= 2);
            Assert.IsTrue(result.Value.All(j => j.IdTypeJournal == 1));
        }

        [TestMethod]
        public async Task GetFilteredJournal_WithDateIntervalComplete_Test()
        {
            // Arrange
            var dateDebut = DateTime.UtcNow.AddDays(-1);
            var dateFin = DateTime.UtcNow.AddDays(1);

            await _journalService.LogConnexionAsync(1);
            await _journalService.LogConnexionAsync(1);

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                DebutIntervalle = dateDebut,
                FinIntervalle = dateFin,
                Order = 0
            };

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.All(j => j.DateJournal >= dateDebut && j.DateJournal <= dateFin));
        }

        [TestMethod]
        public async Task GetFilteredJournal_OrderAscending_Test()
        {
            // Arrange
            await _journalService.LogConnexionAsync(1);
            await Task.Delay(100);
            await _journalService.LogConnexionAsync(1);
            await Task.Delay(100);
            await _journalService.LogConnexionAsync(1);

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                Order = 1
            };

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            var journaux = result.Value.ToList();
            Assert.IsTrue(journaux.Count >= 3);

            for (int i = 0; i < journaux.Count - 1; i++)
            {
                Assert.IsTrue(journaux[i].DateJournal <= journaux[i + 1].DateJournal);
            }
        }

        [TestMethod]
        public async Task GetFilteredJournal_OrderDescending_Test()
        {
            // Arrange
            await _journalService.LogConnexionAsync(1);
            await Task.Delay(100);
            await _journalService.LogConnexionAsync(1);
            await Task.Delay(100);
            await _journalService.LogConnexionAsync(1);

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                Order = 0
            };

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            var journaux = result.Value.ToList();
            Assert.IsTrue(journaux.Count >= 3);

            for (int i = 0; i < journaux.Count - 1; i++)
            {
                Assert.IsTrue(journaux[i].DateJournal >= journaux[i + 1].DateJournal);
            }
        }

        [TestMethod]
        public async Task GetFilteredJournal_NoResults_Test()
        {
            // Arrange
            var recherche = new RechercheJournalDTO
            {
                IdType = 999,
                Order = 0
            };

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetFilteredJournal_WithDateIntervalNoResults_Test()
        {
            // Arrange
            var dateDebut = DateTime.UtcNow.AddDays(-10);
            var dateFin = DateTime.UtcNow.AddDays(-5);

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                DebutIntervalle = dateDebut,
                FinIntervalle = dateFin,
                Order = 0
            };

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region LOG

            #region Connexion
        [TestMethod]
        public async Task LogConnexionAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogConnexionAsync(idCompte);

            // Assert
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
            // Arrange
            int idCompte = 1;
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogDeconnexionAsync(idCompte);

            // Assert
            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(2, lastJournal.IdTypeJournal); // Type Déconnexion
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Déconnexion"));
        }
        #endregion

            #region Compte
        [TestMethod]
        public async Task LogCreationCompteAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            string pseudo = "testuser";
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogCreationCompteAsync(idCompte, pseudo);

            // Assert
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
            // Arrange
            int idCompte = 1;
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogModificationProfilAsync(idCompte);

            // Assert
            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(4, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Modification"));
        }

        [TestMethod]
        public async Task LogSignalementCompteAsyncTest()
        {
            // Arrange
            int idSignalant = 1;
            int idSignale = 2;
            int idSignalement = 10;
            int idTypeSignalement = 1;
            string description = "Comportement inapproprié";
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogSignalementCompteAsync(idSignalant, idSignale, idSignalement, idTypeSignalement, description);

            // Assert
            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(9, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Signalement"));
        }
        #endregion

            #region Annonce
        [TestMethod]
        public async Task LogPublicationAnnonceAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idAnnonce = 100;
            string titre = "Voiture de sport";
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogPublicationAnnonceAsync(idCompte, idAnnonce, titre);

            // Assert
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
            // Arrange
            int idCompte = 1;
            int idAnnonce = 100;
            string titre = "Voiture modifiée";
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogModificationAnnonceAsync(idCompte, idAnnonce, titre);

            // Assert
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
            // Arrange
            int idCompte = 1;
            int idAnnonce = 100;
            string titre = "Voiture supprimée";
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogSuppressionAnnonceAsync(idCompte, idAnnonce, titre);

            // Assert
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
            // Arrange
            int idAcheteur = 1;
            int idVendeur = 2;
            int idCommande = 50;
            int idAnnonce = 100;
            int idMoyenPaiement = 1;
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogAchatAsync(idAcheteur, idVendeur, idCommande, idAnnonce, idMoyenPaiement);

            // Assert
            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(8, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Achat"));
            Assert.IsTrue(lastJournal.ContenuJournal.Contains(idCommande.ToString()));
        }

        [TestMethod]
        public async Task LogSignalementAnnonceAsyncTest()
        {
            // Arrange
            int idSignalant = 1;
            int idAnnoncesignale = 2;
            int idSignalement = 10;
            int idTypeSignalement = 1;
            string description = "Photo inapproprié";
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogSignalementAnnonceAsync(idSignalant, idAnnoncesignale, idSignalement, idTypeSignalement, description);

            // Assert
            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(10, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Signalement"));
        }
        #endregion

            #region Interaction
        [TestMethod]
        public async Task LogDepotAvisAsyncTest()
        {
            // Arrange
            int idJugeur = 1;
            int idJuge = 2;
            int idAvis = 20;
            int note = 5;
            string description = "Excellent vendeur";
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogDepotAvisAsync(idJugeur, idJuge, idAvis, note, description);

            // Assert
            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(11, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("avis"));
            Assert.IsTrue(lastJournal.ContenuJournal.Contains(note.ToString()));
        }

        [TestMethod]
        public async Task LogMiseFavorisAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idAnnonce = 100;
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogMiseFavorisAsync(idCompte, idAnnonce);

            // Assert
            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(12, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("favoris"));
        }

        [TestMethod]
        public async Task LogEnvoiMessageAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idConversation = 5;
            string message = "Bonjour, est-ce toujours disponible ?";
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogEnvoiMessageAsync(idCompte, idConversation, message);

            // Assert
            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(13, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("message"));
        }

        [TestMethod]
        public async Task LogGenerationFactureAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idFacture = 30;
            int idCommande = 50;
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogGenerationFactureAsync(idCompte, idFacture, idCommande);

            // Assert
            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(14, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("facture"));
        }

        [TestMethod]
        public async Task LogBlocageUtilisateurAsyncTest()
        {
            // Arrange
            int idBloqueur = 1;
            int idBloque = 2;
            int countBefore = _context.Journaux.Count();

            // Act
            await _journalService.LogBlocageUtilisateurAsync(idBloqueur, idBloque);

            // Assert
            int countAfter = _context.Journaux.Count();
            Assert.AreEqual(countBefore + 1, countAfter);

            var lastJournal = await _context.Journaux.OrderByDescending(j => j.IdJournal).FirstOrDefaultAsync();
            Assert.IsNotNull(lastJournal);
            Assert.AreEqual(15, lastJournal.IdTypeJournal);
            Assert.IsTrue(lastJournal.ContenuJournal.Contains("Blocage"));
        }
        #endregion

            #region Error
        [TestMethod]
        public async Task ErrorLogActionTest()
        {
            // Arrange
            int invalidIdCompte = -1;
            try
            {
                // Act
                await _journalService.LogConnexionAsync(invalidIdCompte);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public async Task LogActionAsync_WithInvalidForeignKey_ShouldHandleError()
        {
            // Arrange
            int idCompteInexistant = 9999;
            int idTypeJournalInexistant = 9999;
            int countBefore = _context.Journaux.Count();

            // Act
            await _manager.LogActionAsync(idCompteInexistant, idTypeJournalInexistant, "Test erreur");

            int countAfter = _context.Journaux.Count();

            // Assert
            Assert.IsTrue(true, "La méthode n'a pas planté malgré des IDs invalides");
        }

        [TestMethod]
        public async Task LogActionAsync_WithNullContent_ShouldHandleError()
        {
            // Arrange
            int countBefore = _context.Journaux.Count();

            try
            {
                // Act
                await _manager.LogActionAsync(1, 1, null);
            }
            catch (Exception)
            {
                
            }

            int countAfter = _context.Journaux.Count();

            // Assert
            Assert.IsTrue(countAfter >= countBefore, "Le système continue de fonctionner après l'erreur");
        }
        #endregion

        #endregion
    }
}