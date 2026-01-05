using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace Api_c_sharp.ControllersUnitaires.Tests
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
        private INotificationService _notificationService;

        // Objets communs pour les tests de suppression
        private Signalement _signalement;
        private Conversation _conversation;
        private APourConversation _aPourConversation;
        private Message _message1;
        private Message _message2;
        private Commande _commande;
        private TypeSignalement _typeSignalement;
        private EtatSignalementPlainte _etatSignalement;

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
            _notificationService = new NotificationManager(_context);
            _manager = new AnnonceManager(_context);
            _controller = new AnnonceController(_manager, _mapper, _journalService, _notificationService);

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
                new TypeJournal { IdTypeJournaux = 14, LibelleTypeJournaux = "Utilisateur bloque un autre utilisateur" }
            );

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

            EtatAnnonce etatAnnonceMasque = new EtatAnnonce()
            {
                IdEtatAnnonce = 4,
                LibelleEtatAnnonce = "Masque"
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
                DatePublication = DateTime.Now
            };

            Favori favori = new Favori()
            {
                IdAnnonce = annonce.IdAnnonce,
                IdCompte = compte.IdCompte
            };

            // Initialisation des objets pour les tests de suppression
            _typeSignalement = new TypeSignalement()
            {
                IdTypeSignalement = 1,
                LibelleTypeSignalement = "Contenu inapproprié"
            };

            _etatSignalement = new EtatSignalementPlainte()
            {
                IdEtatSignalement = 1,
                LibelleEtatSignalement = "En cours"
            };

            _signalement = new Signalement()
            {
                IdSignalement = 1,
                IdAnnonceSignale = annonce.IdAnnonce,
                IdTypeSignalement = _typeSignalement.IdTypeSignalement,
                IdEtatSignalement = _etatSignalement.IdEtatSignalement,
                DescriptionSignalement = "Cette annonce contient du contenu inapproprié",
                IdCompteSignalant = 1
            };

            _conversation = new Conversation()
            {
                IdConversation = 1,
                IdAnnonce = annonce.IdAnnonce,
                DateDernierMessage = DateTime.Now
            };

            _aPourConversation = new APourConversation()
            {
                IdConversation = _conversation.IdConversation,
                IdCompte = 1
            };

            _message1 = new Message()
            {
                IdMessage = 1,
                IdConversation = _conversation.IdConversation,
                IdCompte = 1,
                ContenuMessage = "Bonjour, est-ce que le véhicule est toujours disponible?",
                DateEnvoiMessage = DateTime.Now
            };

            _message2 = new Message()
            {
                IdMessage = 2,
                IdConversation = _conversation.IdConversation,
                IdCompte = 1,
                ContenuMessage = "Oui, il est disponible",
                DateEnvoiMessage = DateTime.Now
            };

            _commande = new Commande()
            {
                IdCommande = 1,
                IdAnnonce = annonce.IdAnnonce,
                IdAcheteur = 2,
            };

            await _context.MisesEnAvant.AddAsync(miseEnAvant);
            await _context.Pays.AddAsync(pays);
            await _context.Adresses.AddAsync(adresse);
            await _context.EtatAnnonces.AddAsync(etatAnnonce);
            await _context.EtatAnnonces.AddAsync(etatAnnonceMasque);
            await _context.TypesCompte.AddAsync(typeCompte);
            await _context.Comptes.AddAsync(compte);
            await _context.Voitures.AddAsync(voiture);
            await _context.Annonces.AddAsync(annonce);
            await _context.Favoris.AddAsync(favori);
            await _context.TypesSignalement.AddAsync(_typeSignalement);
            await _context.EtatSignalementsPlaintes.AddAsync(_etatSignalement);
            await _context.Signalements.AddAsync(_signalement);
            await _context.Conversations.AddAsync(_conversation);
            await _context.APourConversations.AddAsync(_aPourConversation);
            await _context.Messages.AddRangeAsync(_message1, _message2);

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
            AnnonceCreateDTO annonce = new AnnonceCreateDTO()
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
            _context.Messages.RemoveRange(_message1, _message2);
            _context.APourConversations.Remove(_aPourConversation);
            _context.Conversations.Remove(_conversation);
            await _context.SaveChangesAsync();

            SetupUserContext("2");
            // Act
            var result = await _controller.Delete(_objetcommun.IdAnnonce);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedAnnonce = await _manager.GetByIdAsync(_objetcommun.IdAnnonce);
            Assert.IsNull(deletedAnnonce);
        }

        [TestMethod]
        public async Task DeleteAnnonceWithSignalementsTest()
        {
            // Arrange - Retirer les autres dépendances
            _context.Messages.RemoveRange(_message1, _message2);
            _context.APourConversations.Remove(_aPourConversation);
            _context.Conversations.Remove(_conversation);
            await _context.SaveChangesAsync();

            SetupUserContext("2");
            // Act
            var result = await _controller.Delete(_objetcommun.IdAnnonce);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deletedAnnonce = await _manager.GetByIdAsync(_objetcommun.IdAnnonce);
            Assert.IsNull(deletedAnnonce);

            var updatedSignalement = await _context.Signalements
                .Include(s => s.TypeSignalementSignalementNav)
                .FirstOrDefaultAsync(s => s.IdSignalement == _signalement.IdSignalement);

            Assert.IsNotNull(updatedSignalement);
            Assert.IsNull(updatedSignalement.IdAnnonceSignale);
            Assert.AreEqual(_objetcommun.IdCompte, updatedSignalement.IdCompteSignale);
            Assert.AreEqual(2, updatedSignalement.IdEtatSignalement);
            Assert.IsTrue(updatedSignalement.DescriptionSignalement.Contains("Annonce supprimée par l'administrateur"));
        }

        [TestMethod]
        public async Task DeleteAnnonceWithConversationsTest()
        {
            // Arrange - Retirer les signalements
            _context.Signalements.Remove(_signalement);
            await _context.SaveChangesAsync();

            SetupUserContext("2");
            // Act
            var result = await _controller.Delete(_objetcommun.IdAnnonce);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deletedAnnonce = await _manager.GetByIdAsync(_objetcommun.IdAnnonce);
            Assert.IsNull(deletedAnnonce);

            var messages = await _context.Messages
                .Where(m => m.IdConversation == _conversation.IdConversation)
                .ToListAsync();
            Assert.AreEqual(0, messages.Count);

            var aPourConversations = await _context.APourConversations
                .Where(a => a.IdConversation == _conversation.IdConversation)
                .ToListAsync();
            Assert.AreEqual(0, aPourConversations.Count);

            var deletedConversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.IdConversation == _conversation.IdConversation);
            Assert.IsNull(deletedConversation);
        }

        [TestMethod]
        public async Task DeleteAnnonceWithSignalementsAndConversationsTest()
        {
            // Act - Tous les objets nécessaires sont déjà créés dans Initialize
            SetupUserContext("2");
            var result = await _controller.Delete(_objetcommun.IdAnnonce);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deletedAnnonce = await _manager.GetByIdAsync(_objetcommun.IdAnnonce);
            Assert.IsNull(deletedAnnonce);

            var updatedSignalement = await _context.Signalements
                .FirstOrDefaultAsync(s => s.IdSignalement == _signalement.IdSignalement);
            Assert.IsNotNull(updatedSignalement);
            Assert.IsNull(updatedSignalement.IdAnnonceSignale);
            Assert.AreEqual(2, updatedSignalement.IdEtatSignalement);

            var deletedConversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.IdConversation == _conversation.IdConversation);
            Assert.IsNull(deletedConversation);

            var deletedMessages = await _context.Messages
                .Where(m => m.IdMessage == _message1.IdMessage || m.IdMessage == _message2.IdMessage)
                .ToListAsync();
            Assert.AreEqual(0, deletedMessages.Count);
        }

        [TestMethod]
        public async Task DeleteAnnonceWithMultipleSignalementsTest()
        {
            // Arrange - Ajouter un deuxième signalement
            _context.Messages.RemoveRange(_message1, _message2);
            _context.APourConversations.Remove(_aPourConversation);
            _context.Conversations.Remove(_conversation);
            await _context.SaveChangesAsync();
            SetupUserContext("2");

            var signalement2 = new Signalement()
            {
                IdSignalement = 2,
                IdAnnonceSignale = _objetcommun.IdAnnonce,
                IdTypeSignalement = _typeSignalement.IdTypeSignalement,
                IdEtatSignalement = _etatSignalement.IdEtatSignalement,
                DescriptionSignalement = "Deuxième signalement",

            };

            await _context.Signalements.AddAsync(signalement2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Delete(_objetcommun.IdAnnonce);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updatedSignalements = await _context.Signalements
                .Include(s => s.TypeSignalementSignalementNav)
                .Where(s => s.IdSignalement == _signalement.IdSignalement || s.IdSignalement == signalement2.IdSignalement)
                .ToListAsync();

            Assert.AreEqual(2, updatedSignalements.Count);

            foreach (var sig in updatedSignalements)
            {
                Assert.IsNull(sig.IdAnnonceSignale);
                Assert.AreEqual(_objetcommun.IdCompte, sig.IdCompteSignale);
                Assert.AreEqual(2, sig.IdEtatSignalement);
                Assert.IsTrue(sig.DescriptionSignalement.Contains("Annonce supprimée par l'administrateur"));
            }
        }

        [TestMethod]
        public async Task DeleteAnnonceWithCommandeAndSignalementTest()
        {
            // Arrange - Ajouter une commande
            _context.Messages.RemoveRange(_message1, _message2);
            _context.APourConversations.Remove(_aPourConversation);
            _context.Conversations.Remove(_conversation);
            await _context.Commandes.AddAsync(_commande);
            await _context.SaveChangesAsync();

            SetupUserContext("2");
            // Act
            var result = await _controller.Delete(_objetcommun.IdAnnonce);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var updatedSignalement = await _context.Signalements
                .Include(s => s.TypeSignalementSignalementNav)
                .FirstOrDefaultAsync(s => s.IdSignalement == _signalement.IdSignalement);

            Assert.IsNotNull(updatedSignalement);
            Assert.IsNull(updatedSignalement.IdAnnonceSignale);
            Assert.AreEqual(2, updatedSignalement.IdEtatSignalement);

            var annonce = await _context.Annonces.FirstOrDefaultAsync(a => a.IdAnnonce == _objetcommun.IdAnnonce);
            Assert.IsNotNull(annonce);
            Assert.AreEqual(6, annonce.IdEtatAnnonce);
        }

        [TestMethod]
        public async Task BadRequestDeleteAnnonceTest()
        {
            SetupUserContext("2");

            Commande commande = new Commande()
            {
                IdCommande = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                IdAcheteur = 2,
            };

            await _context.Commandes.AddAsync(commande);
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(_objetcommun.IdAnnonce);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task NotFoundDeleteAnnonceTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task DeleteAnnonceTest_AdminCallsNotification()
        {
            SetupUserContext("1"); // Admin (userId = "1")

            _context.Messages.RemoveRange(_message1, _message2);
            _context.APourConversations.Remove(_aPourConversation);
            _context.Conversations.Remove(_conversation);
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(_objetcommun.IdAnnonce);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task PutAnnonceTest()
        {
            AnnonceUpdateDTO annonce = new AnnonceUpdateDTO()
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
            AnnonceUpdateDTO annonce = new AnnonceUpdateDTO()
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
            AnnonceUpdateDTO annonce = new AnnonceUpdateDTO()
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
            AnnonceCreateDTO annonce = new AnnonceCreateDTO()
            {
                Libelle = "Nouvelle Annonce",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = null,
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
            var result = await _controller.GetByIdMiseEnAvant(_objetcommun.IdMiseEnAvant);
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
                KmMax = 15000,
                Order = 5,
                IdBoitedevitesse = 1,
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
        public async Task GetFilteredTriDateCroissantTest()
        {
            Annonce annonce = new Annonce()
            {
                IdAnnonce = 2,
                Libelle = "Annonce Test",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = "Description de l'annonce",
                IdMiseEnAvant = 1,
                IdVoiture = 1,
                DatePublication = DateTime.Now.AddDays(-1)
            };
            await _context.Annonces.AddAsync(annonce);
            await _context.SaveChangesAsync();
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
                KmMax = 15000,
                PageNumber = 1,
                PageSize = 21,
                Order = 3
            };
            // Act
            var result = await _controller.GetFiltered(parametreRecherche);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            var annonces = result.Value.ToList();

            Assert.IsTrue(annonces.Any());
            Assert.IsTrue(annonces[1].DatePublication == _objetcommun.DatePublication);
        }

        [TestMethod]
        public async Task GetFilteredTriDateDecroissantTest()
        {
            Annonce annonce = new Annonce()
            {
                IdAnnonce = 2,
                Libelle = "Annonce Test",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = "Description de l'annonce",
                IdMiseEnAvant = 1,
                IdVoiture = 1,
                DatePublication = DateTime.Now.AddDays(-1)
            };
            await _context.Annonces.AddAsync(annonce);
            await _context.SaveChangesAsync();
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
                KmMax = 15000,
                PageNumber = 1,
                PageSize = 21,
                Order = 4
            };
            // Act
            var result = await _controller.GetFiltered(parametreRecherche);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            var annonces = result.Value.ToList();

            Assert.IsTrue(annonces.Any());
            Assert.IsTrue(annonces[0].DatePublication == _objetcommun.DatePublication);
        }

        [TestMethod]
        public async Task GetFilteredTriCroissantTest()
        {
            Annonce annonce = new Annonce()
            {
                IdAnnonce = 2,
                Libelle = "Annonce Test",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = "Description de l'annonce",
                IdMiseEnAvant = 1,
                IdVoiture = 1,
            };
            await _context.Annonces.AddAsync(annonce);
            await _context.SaveChangesAsync();
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
                KmMax = 15000,
                PageNumber = 1,
                PageSize = 21,
                Order = 1
            };
            // Act
            var result = await _controller.GetFiltered(parametreRecherche);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            var annonces = result.Value.ToList();

            Assert.IsTrue(annonces.Any());
            Assert.IsTrue(annonces[0].Prix == _objetcommun.Prix);
        }

        [TestMethod]
        public async Task GetFilteredTriDecroissantTest()
        {
            Annonce annonce = new Annonce()
            {
                IdAnnonce = 2,
                Libelle = "Annonce Test",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = "Description de l'annonce",
                IdMiseEnAvant = 1,
                IdVoiture = 1,
            };
            await _context.Annonces.AddAsync(annonce);
            await _context.SaveChangesAsync();
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
                KmMax = 15000,
                PageNumber = 1,
                PageSize = 21,
                Order = 2
            };
            // Act
            var result = await _controller.GetFiltered(parametreRecherche);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            var annonces = result.Value.ToList();

            Assert.IsTrue(annonces.Any());
            Assert.IsTrue(annonces[1].Prix == _objetcommun.Prix);
        }

        [TestMethod]
        public async Task GetAnnonceByCompteIDTest()
        {
            // Act
            var result = await _controller.GetAnnoncesByCompteId(_objetcommun.IdCompte);
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
            var result = await _controller.GetAnnoncesByCompteId(0);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task EstMasqueFalseTests()
        {
            var result = await _controller.EstMasque(_objetcommun.IdAnnonce);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsFalse(result.Value);
        }

        [TestMethod]
        public async Task EstMasqueTrueTests()
        {
            _objetcommun.IdEtatAnnonce = 4; // Etat "Masqué"
            await _context.SaveChangesAsync();

            var result = await _controller.EstMasque(_objetcommun.IdAnnonce);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value);
        }

        [TestMethod]
        public async Task GetAnnonceSimilaireTests()
        {
            Voiture voiture = new Voiture()
            {
                IdVoiture = 2,
                IdMarque = 1,
                IdMotricite = 1,
                IdCarburant = 1,
                IdBoiteDeVitesse = 1,
                IdCategorie = 1,
                Kilometrage = 10000,
                Annee = 2021,
                Puissance = 150,
                MiseEnCirculation = DateTime.Now,
                IdModele = 1,
                NbPlace = 5,
                NbPorte = 5
            };

            Annonce annonce = new Annonce()
            {
                IdAnnonce = 2,
                Libelle = "Annonce Test similaire",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 20000,
                Description = "Description de l'annonce similaire",
                IdMiseEnAvant = 1,
                IdVoiture = 2,
                DatePublication = DateTime.Now
            };
            await _context.Voitures.AddAsync(voiture);
            await _context.Annonces.AddAsync(annonce);
            await _context.SaveChangesAsync();

            var result = await _controller.GetSimilaires(_objetcommun.IdAnnonce);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<AnnonceDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Libelle == annonce.Libelle));
        }

        [TestMethod]
        public async Task NotFoundGetAnnonceSimilaireTests()
        {
            var result = await _controller.GetSimilaires(0);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }


        private void SetupUserContext(string userId = "2")
        {
            var claims = new List<Claim>
            {
                new Claim("idUser", userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }
    }
}