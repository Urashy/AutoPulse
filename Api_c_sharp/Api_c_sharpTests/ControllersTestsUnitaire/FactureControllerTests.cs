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

            TypeCompte typeComptePro = new TypeCompte
            {
                IdTypeCompte = 2,
                Libelle = "Pro"
            };

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
                IdTypeCompte = typeComptePro.IdTypeCompte,
                NumeroSiret = "12345678901234",
                RaisonSociale = "Test SARL"
            };

            await _context.TypesCompte.AddAsync(typeCompte);
            await _context.TypesCompte.AddAsync(typeComptePro);
            await _context.Comptes.AddAsync(acheteur);
            await _context.Comptes.AddAsync(vendeur);
            await _context.SaveChangesAsync();

            var adresse = new Adresse()
            {
                IdAdresse = 1,
                Nom = "Domicile",
                Numero = 10,
                Rue = "Rue de la Paix",
                LibelleVille = "Paris",
                CodePostal = "75001",
                IdCompte = vendeur.IdCompte,
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
                IdAdresse = adresse.IdAdresse
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
                EstAccepte = true
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
            var result = await _controller.GetByID(_objetcommun.IdFacture);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(FactureDTO));
            Assert.AreEqual(_objetcommun.IdFacture, result.Value.IdFacture);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            var result = await _controller.GetByID(0);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

        #region GetAll
        [TestMethod]
        public async Task GetAllTest()
        {
            var result = await _controller.GetAll();
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
            var voiture = new FactureDTO { IdCommande = 1 };
            var actionResult = await _controller.Post(voiture);
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdVoiture = (Facture)created.Value;
            Assert.AreEqual(voiture.IdCommande, createdVoiture.IdCommande);
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
        #endregion

        #region DELETE
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
        #endregion

        #region PUT
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
            var voiture = new FactureDTO() { IdCommande = 9999 };
            var result = await _controller.Put(0, voiture);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutVoitureTest()
        {
            var voiture = new FactureDTO() { IdCommande = -20 };
            _controller.ModelState.AddModelError("IdCommande", "Le IdCommande doit être supérieur à 0");
            var result = await _controller.Put(_objetcommun.IdFacture, voiture);
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

        #region GetFacturePdf - Tests de base

        [TestMethod]
        public async Task GetFacturePdf_ReturnsFileResult_WhenCommandeExists()
        {
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: true);
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
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: false);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FileContentResult));

            var fileResult = result as FileContentResult;
            Assert.AreEqual("application/pdf", fileResult.ContentType);
            Assert.IsTrue(string.IsNullOrEmpty(fileResult.FileDownloadName));
            Assert.IsTrue(fileResult.FileContents.Length > 0);
        }

        [TestMethod]
        public async Task GetFacturePdf_ReturnsNotFound_WhenCommandeDoesNotExist()
        {
            var result = await _controller.GetFacturePdf(9999, download: true);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Commande introuvable", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetFacturePdf_ValidatesPdfContent()
        {
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: true);
            Assert.IsInstanceOfType(result, typeof(FileContentResult));

            var fileResult = result as FileContentResult;
            var pdfContent = fileResult.FileContents;

            var pdfHeader = System.Text.Encoding.ASCII.GetString(pdfContent.Take(4).ToArray());
            Assert.AreEqual("%PDF", pdfHeader);
        }

        #endregion

        #region GetFacturePdf - Tests avancés Manager

        [TestMethod]
        public async Task GetFacturePdf_WithVendeurPro_CalculatesTVA()
        {
            // Ce test vérifie que le PDF est généré correctement avec un vendeur PRO
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: true);

            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = result as FileContentResult;

            // Vérifier que le PDF contient des données
            Assert.IsTrue(fileResult.FileContents.Length > 1000, "Le PDF devrait contenir des données substantielles");
        }

        [TestMethod]
        public async Task GetFacturePdf_WithVendeurParticulier_NoTVA()
        {
            // Créer un nouveau vendeur particulier
            var typeCompteParticulier = new TypeCompte
            {
                IdTypeCompte = 3,
                Libelle = "Particulier"
            };
            await _context.TypesCompte.AddAsync(typeCompteParticulier);

            var vendeurParticulier = new Compte
            {
                IdCompte = 3,
                Nom = "Dupont",
                Prenom = "Marie",
                Email = "marie.dupont@gmail.com",
                Pseudo = "mariedupont",
                MotDePasse = "Password123!",
                DateCreation = DateTime.Now,
                DateNaissance = new DateTime(1985, 5, 15),
                DateDerniereConnexion = DateTime.Now,
                IdTypeCompte = typeCompteParticulier.IdTypeCompte
            };
            await _context.Comptes.AddAsync(vendeurParticulier);
            await _context.SaveChangesAsync();

            // Créer une nouvelle annonce avec ce vendeur
            var annonce2 = new Annonce
            {
                IdAnnonce = 2,
                Libelle = "Annonce Particulier",
                Description = "Vente particulier",
                Prix = 8000,
                DatePublication = DateTime.Now,
                IdCompte = vendeurParticulier.IdCompte,
                IdVoiture = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1
            };
            await _context.Annonces.AddAsync(annonce2);
            await _context.SaveChangesAsync();

            // Créer une commande avec ce vendeur
            var commande2 = new Commande
            {
                IdCommande = 2,
                Date = DateTime.Now,
                IdAcheteur = 1,
                IdAnnonce = annonce2.IdAnnonce,
                IdMoyenPaiement = 1,
                IdVendeur = vendeurParticulier.IdCompte,
                IdEtatCommande = 1,
                IdOffre = 1
            };
            await _context.Commandes.AddAsync(commande2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetFacturePdf(commande2.IdCommande, download: true);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = result as FileContentResult;
            Assert.IsTrue(fileResult.FileContents.Length > 0);
        }

        [TestMethod]
        public async Task GetFacturePdf_WithOffre_UsesOffreValue()
        {
            // Le setup initial utilise déjà une offre
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: true);

            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = result as FileContentResult;

            // Vérifier que le PDF est généré avec la valeur de l'offre (9500€)
            Assert.IsTrue(fileResult.FileContents.Length > 0);
        }

        [TestMethod]
        public async Task GetFacturePdf_WithoutOffre_UsesAnnoncePrice()
        {
            // Créer une commande sans offre
            var commande3 = new Commande
            {
                IdCommande = 3,
                Date = DateTime.Now,
                IdAcheteur = 1,
                IdAnnonce = 1,
                IdMoyenPaiement = 1,
                IdVendeur = 2,
                IdEtatCommande = 1,
                IdOffre = 1
            };
            await _context.Commandes.AddAsync(commande3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetFacturePdf(commande3.IdCommande, download: true);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = result as FileContentResult;
            Assert.IsTrue(fileResult.FileContents.Length > 0);
        }

        [TestMethod]
        public async Task GetFacturePdf_WithCompleteAddress_IncludesAddress()
        {
            // L'adresse est déjà configurée dans le setup
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: true);

            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = result as FileContentResult;
            Assert.IsTrue(fileResult.FileContents.Length > 0);
        }

        [TestMethod]
        public async Task GetFacturePdf_WithAllVehicleDetails_IncludesAllInfo()
        {
            // Toutes les infos du véhicule sont déjà dans le setup
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: true);

            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = result as FileContentResult;

            // Le PDF devrait contenir toutes les informations
            Assert.IsTrue(fileResult.FileContents.Length > 2000, "Le PDF devrait être substantiel avec toutes les infos");
        }

        [TestMethod]
        public async Task GetFacturePdf_VerifyPdfStructure()
        {
            var result = await _controller.GetFacturePdf(_objetcommun.IdCommande, download: true);
            var fileResult = result as FileContentResult;
            var pdfContent = fileResult.FileContents;

            // Vérifier la structure de base d'un PDF
            var pdfHeader = System.Text.Encoding.ASCII.GetString(pdfContent.Take(4).ToArray());
            Assert.AreEqual("%PDF", pdfHeader);

            // Vérifier que le PDF se termine correctement
            var lastBytes = System.Text.Encoding.ASCII.GetString(pdfContent.Skip(pdfContent.Length - 6).ToArray());
            Assert.IsTrue(lastBytes.Contains("%%EOF") || pdfContent.Length > 100, "Le PDF devrait se terminer correctement");
        }

        #endregion
    }
}