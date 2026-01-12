using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

using AutoPulse.Shared.DTO;
using Api_c_sharp.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class FactureControllerTests
    {
        private FactureController _controller;
        private AutoPulseBdContext _context;
        private FactureManager _manager;
        private CommandeManager _commandeManager;
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
            _commandeManager = new CommandeManager(_context);
            _controller = new FactureController(_manager, _mapper, _commandeManager);

            // Entités de base
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
                IdTypeCompte = 1,
                Libelle = "Acheteur"
            };

            // AJOUT: Pays et Adresse AVANT les comptes
            var pays = new Pays()
            {
                IdPays = 1,
                Libelle = "France"
            };
            await _context.Pays.AddAsync(pays);
            await _context.SaveChangesAsync();

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
                IdTypeCompte = typeCompte.IdTypeCompte
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
                IdTypeCompte = typeCompte.IdTypeCompte,
                // AJOUT: Optionnel pour vendeur pro
                NumeroSiret = "12345678901234",
                RaisonSociale = "Test SARL"
            };

            await _context.TypesCompte.AddAsync(typeCompte);
            await _context.Comptes.AddAsync(acheteur);
            await _context.Comptes.AddAsync(vendeur);
            await _context.SaveChangesAsync();

            // AJOUT: Adresse associée au vendeur
            var adresse = new Adresse()
            {
                IdAdresse = 1,
                Nom = "Domicile",
                Numero = 10,
                Rue = "Rue de la Paix",
                LibelleVille = "Paris",
                CodePostal = "75001",
                IdCompte = vendeur.IdCompte, // Important: liée au vendeur
                IdPays = pays.IdPays
            };
            await _context.Adresses.AddAsync(adresse);
            await _context.SaveChangesAsync();

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
            await _context.Voitures.AddAsync(voiture);
            await _context.SaveChangesAsync();

            EtatAnnonce etatAnnonce = new EtatAnnonce
            {
                IdEtatAnnonce = 1,
                LibelleEtatAnnonce = "Active"
            };
            await _context.EtatAnnonces.AddAsync(etatAnnonce);
            await _context.SaveChangesAsync();

            Annonce annonce = new Annonce
            {
                IdAnnonce = 1,
                Libelle = "Annonce Test",
                Description = "Description de l'annonce test",
                Prix = 10000,
                DatePublication = DateTime.Now,
                IdCompte = vendeur.IdCompte,
                IdVoiture = voiture.IdVoiture,
                IdEtatAnnonce = etatAnnonce.IdEtatAnnonce,
                IdAdresse = adresse.IdAdresse // IMPORTANT: Lier l'adresse à l'annonce
            };
            await _context.Annonces.AddAsync(annonce);
            await _context.SaveChangesAsync();

            Conversation conversation = new Conversation()
            {
                IdConversation = 1,
                IdAnnonce = annonce.IdAnnonce,
                DateDernierMessage = DateTime.Now
            };
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            Message message1 = new Message()
            {
                IdMessage = 1,
                ContenuMessage = "Bonjour, je suis intéressé par votre annonce.",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = conversation.IdConversation,
                IdCompte = acheteur.IdCompte,
                EstLu = false,
            };

            Message message2 = new Message()
            {
                IdMessage = 2,
                ContenuMessage = "Bonjour, message 2",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = conversation.IdConversation,
                IdCompte = vendeur.IdCompte,
                EstLu = false,
            };
            await _context.Messages.AddRangeAsync(message1, message2);
            await _context.SaveChangesAsync();

            var etatCommande = new EtatCommande()
            {
                IdEtatCommande = 1,
                Libelle = "En cours"
            };
            await _context.EtatCommandes.AddAsync(etatCommande);
            await _context.SaveChangesAsync();

            Offre offre = new Offre()
            {
                IdOffre = 1,
                Valeur = 9500,
                DateOffre = DateTime.Now,
                IdMessage = message1.IdMessage,
                IdAnnonce = annonce.IdAnnonce,
                EstAccepte = true // IMPORTANT: Acceptée pour la commande
            };
            await _context.Offres.AddAsync(offre);
            await _context.MoyensPaiements.AddAsync(moyenPaiement);
            await _context.SaveChangesAsync();

            Commande commande = new Commande
            {
                IdCommande = 1,
                Date = DateTime.Now,
                IdAcheteur = acheteur.IdCompte,
                IdAnnonce = annonce.IdAnnonce,
                IdMoyenPaiement = moyenPaiement.IdMoyenPaiement,
                IdVendeur = vendeur.IdCompte,
                IdEtatCommande = etatCommande.IdEtatCommande,
                IdOffre = offre.IdOffre
            };
            await _context.Commandes.AddAsync(commande);
            await _context.SaveChangesAsync();

            Facture objet = new Facture()
            {
                IdFacture = 1,
                IdCommande = commande.IdCommande,
            };
            await _context.Factures.AddAsync(objet);
            await _context.SaveChangesAsync();

            _objetcommun = objet;
        }

        #region GET

        #region GetById
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<FactureDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.IdFacture == _objetcommun.IdFacture));
        }
        #endregion

        #endregion

        #region POST
        [TestMethod]
        public async Task PostVoitureTest_Entity()
        {
            // Arrange
            var voiture = new FactureDTO
            {
                IdCommande = 1,
            };

            // Act
            var actionResult = await _controller.Post(voiture);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdVoiture = (Facture)created.Value;
            Assert.AreEqual(voiture.IdCommande, createdVoiture.IdCommande);
        }

        [TestMethod]
        public async Task BadRequestPostVoitureTest()
        {
            // Arrange
            var voiture = new FactureDTO
            {
                IdFacture = _objetcommun.IdFacture,
                IdCommande = -20,
            };

            _controller.ModelState.AddModelError("IdCommande", "Le IdCommande doit être supérieur à 0");

            // Act
            var actionResult = await _controller.Post(voiture);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteVoitureTest()
        {
            // Act
            var result = await _controller.Delete(_objetcommun.IdFacture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedVoiture = await _manager.GetByIdAsync(_objetcommun.IdFacture);
            Assert.IsNull(deletedVoiture);
        }

        [TestMethod]
        public async Task NotFoundDeleteVoitureTest()
        {
            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutVoitureTest()
        {
            // Arrange
            var voiture = new FactureDTO()
            {
                IdFacture = _objetcommun.IdFacture,
                IdCommande = _objetcommun.IdCommande,

            };

            // Act
            var result = await _controller.Put(_objetcommun.IdFacture, voiture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var fetchedVoiture = await _manager.GetByIdAsync(_objetcommun.IdFacture);
            Assert.AreEqual(voiture.IdCommande, fetchedVoiture.IdCommande);
        }

        [TestMethod]
        public async Task NotFoundPutVoitureTest()
        {
            // Arrange
            var voiture = new FactureDTO()
            {
                IdCommande = 9999,
            };

            // Act
            var result = await _controller.Put(0, voiture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task BadRequestPutVoitureTest()
        {
            // Arrange
            var voiture = new FactureDTO()
            {
                IdCommande = -20,
            };

            // Force l'erreur de validation
            _controller.ModelState.AddModelError("IdCommande", "Le IdCommande doit être supérieur à 0");

            // Act
            var result = await _controller.Put(_objetcommun.IdFacture, voiture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

        // Ajoutez ces tests à la fin de votre classe FactureControllerTests (fichier non-mock)
        // Juste avant la dernière accolade fermante de la classe

        #region GetFacturePdf

        [TestMethod]
        public async Task GetFacturePdf_ReturnsFileResult_WhenCommandeExists()
        {
            // Act
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FileContentResult));

            var fileResult = result as FileContentResult;
            Assert.AreEqual("application/pdf", fileResult.ContentType);
            Assert.AreEqual($"Facture_Commande_{_objetcommun.IdCommande}.pdf", fileResult.FileDownloadName);
            Assert.IsTrue(fileResult.FileContents.Length > 0);
        }

        [TestMethod]
        public async Task GetFacturePdf_ReturnsFileResult_WithoutDownload()
        {
            // Act
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FileContentResult));

            var fileResult = result as FileContentResult;
            Assert.AreEqual("application/pdf", fileResult.ContentType);
            Assert.AreEqual(fileResult.FileDownloadName, ""); // Pas de nom de fichier en mode preview
            Assert.IsTrue(fileResult.FileContents.Length > 0);
        }

        [TestMethod]
        public async Task GetFacturePdf_ReturnsNotFound_WhenCommandeDoesNotExist()
        {
            // Act
            var result = await _controller.GetFacturePdf(9999, download: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Commande introuvable", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetFacturePdf_ValidatesPdfContent()
        {
            // Act
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: true);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));

            var fileResult = result as FileContentResult;
            var pdfContent = fileResult.FileContents;

            // Vérifier que c'est bien un PDF (commence par %PDF)
            var pdfHeader = System.Text.Encoding.ASCII.GetString(pdfContent.Take(4).ToArray());
            Assert.AreEqual("%PDF", pdfHeader);
        }

        #endregion

    }
}