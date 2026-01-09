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

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class OffreControllerTests
    {
        private OffreController _controller;
        private AutoPulseBdContext _context;
        private OffreManager _manager;
        private MessageManager _messageManager;
        private CommandeManager _commandeManager;
        private AnnonceManager _annonceManager;
        private IMapper _mapper;
        private Offre _objetcommun;
        private INotificationService _notificationService;

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

            _manager = new OffreManager(_context);
            _messageManager = new MessageManager(_context);
            _commandeManager = new CommandeManager(_context);
            _annonceManager = new AnnonceManager(_context);
            _notificationService = new NotificationManager(_context);

            _controller = new OffreController(_manager, _mapper, _messageManager, _commandeManager, _annonceManager, _notificationService);

            _context.Offres.RemoveRange(_context.Offres);
            await _context.SaveChangesAsync();

            // 1. TypeCompte
            TypeCompte typecompte = new TypeCompte()
            {
                IdTypeCompte = 1,
                Libelle = "Particulier",
                Cherchable = true
            };
            await _context.TypesCompte.AddAsync(typecompte);
            await _context.SaveChangesAsync();

            // 2. EtatCompte
            var etatCompte = new EtatCompte()
            {
                IdEtatCompte = 1,
                Libelle = "Actif"
            };
            await _context.EtatComptes.AddAsync(etatCompte);
            await _context.SaveChangesAsync();

            // 3. Comptes (Vendeur et Acheteur)
            Compte compte1 = new Compte()
            {
                IdCompte = 1,
                Email = "john@gmail.com",
                MotDePasse = "Password123!",
                Nom = "Doe",
                Prenom = "John",
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = 1,
                IdEtatCompte = 1,
                Pseudo = "john_doe",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now
            };

            Compte compte2 = new Compte()
            {
                IdCompte = 2,
                Email = "jane@gmail.com",
                MotDePasse = "Password123!",
                Nom = "Smith",
                Prenom = "Jane",
                DateNaissance = new DateTime(1992, 5, 15),
                IdTypeCompte = 1,
                IdEtatCompte = 1,
                Pseudo = "jane_smith",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now
            };
            await _context.Comptes.AddRangeAsync(compte1, compte2);
            await _context.SaveChangesAsync();

            // 4. Pays (needed for Adresse)
            var pays = new Pays()
            {
                IdPays = 1,
                Libelle = "France"
            };
            await _context.Pays.AddAsync(pays);
            await _context.SaveChangesAsync();

            // 5. Adresse
            var adresse = new Adresse()
            {
                IdAdresse = 1,
                Nom = "Domicile",
                Numero = 10,
                Rue = "Rue de la Paix",
                LibelleVille = "Paris",
                CodePostal = "75001",
                IdCompte = 1,
                IdPays = 1
            };
            await _context.Adresses.AddAsync(adresse);
            await _context.SaveChangesAsync();

            // 6. Voiture dependencies
            var marque = new Marque()
            {
                IdMarque = 1,
                LibelleMarque = "Peugeot"
            };
            await _context.Marques.AddAsync(marque);

            var modele = new Modele()
            {
                IdModele = 1,
                LibelleModele = "308",
                IdMarque = 1
            };
            await _context.Modeles.AddAsync(modele);

            var carburant = new Carburant()
            {
                IdCarburant = 1,
                LibelleCarburant = "Diesel"
            };
            await _context.Carburants.AddAsync(carburant);

            var boiteDeVitesse = new BoiteDeVitesse()
            {
                IdBoiteDeVitesse = 1,
                LibelleBoite = "Manuelle"
            };
            await _context.BoitesDeVitesses.AddAsync(boiteDeVitesse);

            var categorie = new Categorie()
            {
                IdCategorie = 1,
                LibelleCategorie = "Berline"
            };
            await _context.Categories.AddAsync(categorie);

            var motricite = new Motricite()
            {
                IdMotricite = 1,
                LibelleMotricite = "Traction"
            };
            await _context.Motricites.AddAsync(motricite);
            await _context.SaveChangesAsync();

            // 7. Voiture
            var voiture = new Voiture()
            {
                IdVoiture = 1,
                IdMarque = 1,
                IdModele = 1,
                IdCarburant = 1,
                IdBoiteDeVitesse = 1,
                IdCategorie = 1,
                IdMotricite = 1,
                Kilometrage = 50000,
                Annee = 2020,
                Puissance = 130,
                MiseEnCirculation = new DateTime(2020, 1, 1),
                NbPlace = 5,
                NbPorte = 5,
                Couple = 300,
                NbCylindres = 4,
                NbAirbag = 6,
                InterieurCuire = false,
                CylindrerMoteur = 1.6,
                PositionVolant = false
            };
            await _context.Voitures.AddAsync(voiture);
            await _context.SaveChangesAsync();

            // 8. EtatAnnonce
            var etatAnnonce = new EtatAnnonce()
            {
                IdEtatAnnonce = 1,
                LibelleEtatAnnonce = "Publiée"
            };
            await _context.EtatAnnonces.AddAsync(etatAnnonce);

            // 9. MiseEnAvant
            var miseEnAvant = new MiseEnAvant()
            {
                IdMiseEnAvant = 1,
                LibelleMiseEnAvant = "Standard",
                PrixSemaine = 0
            };
            await _context.MisesEnAvant.AddAsync(miseEnAvant);
            await _context.SaveChangesAsync();

            // 10. Annonce (CRITICAL - needs all dependencies)
            Annonce annonce = new Annonce()
            {
                IdAnnonce = 1,
                Libelle = "Annonce de test",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                IdVoiture = 1,
                IdMiseEnAvant = 1,
                Prix = 10000,
                Description = "Description de test",
                DatePublication = DateTime.Now
            };
            await _context.Annonces.AddAsync(annonce);
            await _context.SaveChangesAsync();

            // 11. Conversation
            Conversation conversation = new Conversation()
            {
                IdConversation = 1,
                IdAnnonce = 1,
                DateDernierMessage = DateTime.Now
            };
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            // 12. Messages
            Message message1 = new Message()
            {
                IdMessage = 1,
                ContenuMessage = "Bonjour, je suis intéressé par votre annonce.",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = 1,
                IdCompte = 2, // Acheteur
                EstLu = false,
            };

            Message message2 = new Message()
            {
                IdMessage = 2,
                ContenuMessage = "Bonjour, message 2",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = 1,
                IdCompte = 1, // Vendeur
                EstLu = false,
            };
            await _context.Messages.AddRangeAsync(message1, message2);
            await _context.SaveChangesAsync();

            // 13. MoyenPaiement (needed for Commande)
            var moyenPaiement = new MoyenPaiement()
            {
                IdMoyenPaiement = 1,
                TypePaiement = "Carte bancaire"
            };
            await _context.MoyensPaiements.AddAsync(moyenPaiement);

            // 14. EtatCommande (needed for Commande)
            var etatCommande = new EtatCommande()
            {
                IdEtatCommande = 1,
                Libelle = "En cours"
            };
            await _context.EtatCommandes.AddAsync(etatCommande);
            await _context.SaveChangesAsync();

            // 15. Offre
            Offre offre = new Offre()
            {
                IdOffre = 1,
                Valeur = 9500,
                DateOffre = DateTime.Now,
                IdMessage = 1,
                IdAnnonce = 1,
                EstAccepte = null // En attente
            };
            await _context.Offres.AddAsync(offre);
            await _context.SaveChangesAsync();

            _objetcommun = offre;
        }

        #region GET
        #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdOffre);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(OffreDTO));
            Assert.AreEqual(_objetcommun.Valeur, result.Value.Valeur);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<OffreDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Valeur == _objetcommun.Valeur));
        }
        #endregion

        #region GetByMessage
        [TestMethod]
        public async Task GetByMessageTest()
        {
            // Act
            var result = await _controller.GetByMessage(_objetcommun.IdMessage);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<OffreDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Valeur == _objetcommun.Valeur));
        }

        [TestMethod]
        public async Task NotFoundGetByMessageTest()
        {
            // Act
            var result = await _controller.GetByMessage(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion
        #endregion

        #region POST
        [TestMethod]
        public async Task PostOffreTest_Entity()
        {
            // Supprimer l'offre existante
            _context.Offres.Remove(_objetcommun);
            await _context.SaveChangesAsync();

            OffreCreateDTO Offre = new OffreCreateDTO
            {
                Valeur = 9000,
                IdAnnonce = 1,
                IdMessage = 1,
            };

            // Act
            var actionResult = await _controller.Post(Offre);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdOffre = (Offre)created.Value;
            Assert.AreEqual(Offre.Valeur, createdOffre.Valeur);
        }

        [TestMethod]
        public async Task PostOffreTest_ConflictWhenPendingOfferExists()
        {
            // L'offre _objetcommun existe déjà et est en attente
            OffreCreateDTO nouvelleOffre = new OffreCreateDTO
            {
                Valeur = 9000,
                IdAnnonce = 1,
                IdMessage = 1,  // Même message/conversation que _objetcommun
            };

            var actionResult = await _controller.Post(nouvelleOffre);

            // ✅ Vérifier qu'on obtient bien un Conflict
            Assert.IsInstanceOfType(actionResult.Result, typeof(ConflictObjectResult));

            var conflict = (ConflictObjectResult)actionResult.Result;
            Assert.AreEqual("Une offre en attente existe déjà dans cette conversation.", conflict.Value);
        }

        [TestMethod]
        public async Task BadRequestPostOffreTest()
        {
            // Arrange
            OffreCreateDTO Offre = new OffreCreateDTO
            {
                Valeur = 0,
                IdAnnonce = 1,
                IdMessage = _objetcommun.IdMessage,
            };

            _controller.ModelState.AddModelError("Valeur", "La Valeur doit être supérieur à 0");

            // Act
            var actionResult = await _controller.Post(Offre);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task PostOffreTest_WithSameCompteIdForMessageAndAnnonce()
        {
            // Créer une nouvelle annonce avec le même compte que le message
            var annonceMemeCpt = new Annonce()
            {
                IdAnnonce = 2,
                Libelle = "Annonce même compte",
                IdCompte = 2, // Même que l'IdCompte du message1
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                IdVoiture = 1,
                IdMiseEnAvant = 1,
                Prix = 8000,
                Description = "Test même compte",
                DatePublication = DateTime.Now
            };
            await _context.Annonces.AddAsync(annonceMemeCpt);
            await _context.SaveChangesAsync();

            // Supprimer l'offre existante pour éviter le conflit
            _context.Offres.Remove(_objetcommun);
            await _context.SaveChangesAsync();

            OffreCreateDTO offre = new OffreCreateDTO
            {
                Valeur = 7500,
                IdAnnonce = 2, // L'annonce avec IdCompte = 2
                IdMessage = 1, // Le message avec IdCompte = 2
            };

            // Act
            var actionResult = await _controller.Post(offre);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdOffre = (Offre)created.Value;
            Assert.AreEqual(offre.Valeur, createdOffre.Valeur);
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteOffreTest()
        {
            // Act
            var result = await _controller.Delete(_objetcommun.IdOffre);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedOffre = await _manager.GetByIdAsync(_objetcommun.IdOffre);
            Assert.IsNull(deletedOffre);
        }

        [TestMethod]
        public async Task NotFoundDeleteOffreTest()
        {
            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutOffreTest()
        {
            // Arrange
            OffreUpdateDTO offre = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 9200,
                DateOffre = DateTime.Now,
                IdMessage = _objetcommun.IdMessage,
                IdAnnonce = 1,
                EstAccepte = null
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, offre);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var offreput = await _manager.GetByIdAsync(_objetcommun.IdOffre);
            Assert.AreEqual(offre.Valeur, offreput.Valeur);
        }

        [TestMethod]
        public async Task PutOffreTest_WhenAccepted_CreatesCommande()
        {
            // Arrange
            OffreUpdateDTO offre = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 9200,
                DateOffre = DateTime.Now,
                IdMessage = _objetcommun.IdMessage,
                IdAnnonce = 1,
                EstAccepte = true // ✅ Offre acceptée
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, offre);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            // Vérifier qu'une commande a été créée
            var commandes = await _context.Commandes.ToListAsync();
            Assert.IsTrue(commandes.Any());

            var commande = commandes.First();
            Assert.AreEqual(2, commande.IdAcheteur); // IdCompte du message
            Assert.AreEqual(1, commande.IdVendeur);  // IdCompte de l'annonce
            Assert.AreEqual(1, commande.IdAnnonce);
            Assert.AreEqual(_objetcommun.IdOffre, commande.IdOffre);
        }

        [TestMethod]
        public async Task PutOffreTest_WhenAccepted_WithSameCompteId()
        {
            // Créer une annonce et un message avec le même IdCompte
            var annonceMemeCpt = new Annonce()
            {
                IdAnnonce = 3,
                Libelle = "Annonce même compte",
                IdCompte = 2, // Même compte que le message
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                IdVoiture = 1,
                IdMiseEnAvant = 1,
                Prix = 8000,
                Description = "Test même compte",
                DatePublication = DateTime.Now
            };
            await _context.Annonces.AddAsync(annonceMemeCpt);
            await _context.SaveChangesAsync();

            var nouvelleOffre = new Offre()
            {
                IdOffre = 2,
                Valeur = 7500,
                DateOffre = DateTime.Now,
                IdMessage = 1, // Message avec IdCompte = 2
                IdAnnonce = 3, // Annonce avec IdCompte = 2
                EstAccepte = null
            };
            await _context.Offres.AddAsync(nouvelleOffre);
            await _context.SaveChangesAsync();

            OffreUpdateDTO offre = new OffreUpdateDTO()
            {
                IdOffre = 2,
                Valeur = 7500,
                DateOffre = DateTime.Now,
                IdMessage = 1,
                IdAnnonce = 3,
                EstAccepte = true
            };

            // Act
            var result = await _controller.Put(2, offre);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            // Vérifier qu'une commande a été créée avec le bon IdAcheteur
            var commandes = await _context.Commandes
                .Where(c => c.IdOffre == 2)
                .ToListAsync();

            Assert.IsTrue(commandes.Any());
            var commande = commandes.First();
            Assert.AreEqual(2, commande.IdAcheteur); // IdCompte de l'annonce
            Assert.AreEqual(2, commande.IdVendeur);  // IdCompte de l'annonce
        }

        [TestMethod]
        public async Task PutOffreTest_WhenNotAccepted_NoCommandeCreated()
        {
            // Arrange
            var commandesAvant = await _context.Commandes.CountAsync();

            OffreUpdateDTO offre = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 9200,
                DateOffre = DateTime.Now,
                IdMessage = _objetcommun.IdMessage,
                IdAnnonce = 1,
                EstAccepte = false // ❌ Offre refusée
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, offre);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            // Vérifier qu'aucune commande n'a été créée
            var commandesApres = await _context.Commandes.CountAsync();
            Assert.AreEqual(commandesAvant, commandesApres);
        }

        [TestMethod]
        public async Task NotFoundPutOffreTest()
        {
            // Arrange
            OffreUpdateDTO offre = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 9200,
                DateOffre = DateTime.Now,
                IdMessage = _objetcommun.IdMessage,
                IdAnnonce = 1,
            };

            // Act
            var result = await _controller.Put(0, offre);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutOffreTest()
        {
            // Arrange
            OffreUpdateDTO offre = new OffreUpdateDTO()
            {
                IdOffre = _objetcommun.IdOffre,
                Valeur = 0,
                DateOffre = DateTime.Now,
                IdMessage = _objetcommun.IdMessage,
                IdAnnonce = 1,
            };

            // Forcer l'erreur de validation
            _controller.ModelState.AddModelError("Valeur", "La Valeur doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, offre);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion
    }
}