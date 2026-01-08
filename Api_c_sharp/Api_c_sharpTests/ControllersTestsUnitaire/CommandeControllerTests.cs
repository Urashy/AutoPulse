using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Api_c_sharp.Controllers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass]
    public class CommandeControllerTests
    {
        private CommandeController _controller;
        private AutoPulseBdContext _context;
        private CommandeManager _manager;
        private IMapper _mapper;
        private Commande _commandeCommun;
        private IJournalService _journalService;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            // AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _journalService = new JournalManager(_context, NullLogger<JournalManager>.Instance);
            _manager = new CommandeManager(_context);
            _controller = new CommandeController(_manager, _mapper, _journalService);

            // Reset DB
            _context.Commandes.RemoveRange(_context.Commandes);
            await _context.SaveChangesAsync();

            // ----- ENTITÉS -----

            // 1. TypeCompte (must be saved first)
            var typeCompte = new TypeCompte()
            {
                IdTypeCompte = 1,
                Libelle = "Standard",
                Cherchable = true
            };
            await _context.TypesCompte.AddAsync(typeCompte);
            await _context.SaveChangesAsync(); // Save before creating Comptes

            // 2. EtatCompte
            var etatCompte = new EtatCompte()
            {
                IdEtatCompte = 1,
                Libelle = "Actif"
            };
            await _context.EtatComptes.AddAsync(etatCompte);
            await _context.SaveChangesAsync();

            // 3. Comptes (Fixed: acheteur = 1, vendeur = 2)
            var acheteur = new Compte
            {
                IdCompte = 1,
                Pseudo = "john",
                MotDePasse = "hashedpassword",
                Nom = "Doe",
                Prenom = "John",
                Email = "john@doe.com",
                DateCreation = DateTime.UtcNow,
                DateDerniereConnexion = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = 1,
                IdEtatCompte = 1
            };
            await _context.Comptes.AddAsync(acheteur);

            var vendeur = new Compte
            {
                IdCompte = 2,
                Pseudo = "johny",
                MotDePasse = "hashedpassword",
                Nom = "Doe",
                Prenom = "Johnny",
                Email = "johny@doe.com",
                DateCreation = DateTime.UtcNow,
                DateDerniereConnexion = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = 1,
                IdEtatCompte = 1
            };
            await _context.Comptes.AddAsync(vendeur);
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
                IdCompte = 2, // Vendeur's address
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

            // 10. Annonce
            var annonce = new Annonce()
            {
                IdAnnonce = 1,
                Libelle = "Super produit",
                IdCompte = 2, // Vendeur
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                IdVoiture = 1,
                IdMiseEnAvant = 1,
                DatePublication = DateTime.UtcNow,
                Prix = 15000
            };
            await _context.Annonces.AddAsync(annonce);
            await _context.SaveChangesAsync();

            // 11. MoyenPaiement
            var moyenPaiement = new MoyenPaiement()
            {
                IdMoyenPaiement = 1,
                TypePaiement = "Carte"
            };
            await _context.MoyensPaiements.AddAsync(moyenPaiement);

            // 12. EtatCommande
            var etatCommande = new EtatCommande()
            {
                IdEtatCommande = 1,
                Libelle = "En cours"
            };
            await _context.EtatCommandes.AddAsync(etatCommande);
            await _context.SaveChangesAsync();

            // 13. Conversation (needed for Message)
            var conversation = new Conversation()
            {
                IdConversation = 1,
                IdAnnonce = 1,
                DateDernierMessage = DateTime.UtcNow
            };
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            // 14. Message (needed for Offre)
            var message = new Message()
            {
                IdMessage = 1,
                ContenuMessage = "Je suis intéressé",
                DateEnvoiMessage = DateTime.UtcNow,
                IdConversation = 1,
                IdCompte = 1, // Acheteur
                EstLu = false
            };
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            // 15. Offre (CRITICAL - was missing!)
            var offre = new Offre()
            {
                IdOffre = 1,
                IdAnnonce = 1,
                IdMessage = 1,
                Valeur = 14000,
                DateOffre = DateTime.UtcNow,
                EstAccepte = true
            };
            await _context.Offres.AddAsync(offre);
            await _context.SaveChangesAsync();

            // 16. Finally, create the Commande
            _commandeCommun = new Commande()
            {
                IdCommande = 1,
                IdAnnonce = 1,
                IdAcheteur = 1, // john
                IdVendeur = 2,  // johny
                IdOffre = 1,
                IdMoyenPaiement = 1,
                IdEtatCommande = 1,
                Date = DateTime.UtcNow
            };

            await _context.Commandes.AddAsync(_commandeCommun);
            await _context.SaveChangesAsync();
        }

        #region GET
        #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            var id = _commandeCommun.IdCommande;

            // Act
            var result = await _controller.GetByID(id);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CommandeDetailDTO));
            Assert.AreEqual(id, result.Value.IdCommande);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            var idInexistant = 0;

            // Act
            var result = await _controller.GetByID(idInexistant);

            // Assert
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
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }
        #endregion

            #region GetCommandeByCompteID
        [TestMethod]
         public async Task GetCommandeByCompteIDTest()
         {
            // Act
            var result = await _controller.GetCommandeByCompteID(_commandeCommun.IdAcheteur);
            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
         }

        [TestMethod]
        public async Task GetCommandeByCompteIDNotFoundTest()
        {
            // Act
            var result = await _controller.GetCommandeByCompteID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion 
        #endregion

        #region POST
        [TestMethod]
        public async Task PostCommandeTest()
        {
            // Arrange
            CommandeCreateDTO commandeCreateDTO = new CommandeCreateDTO
            {
                IdVendeur = _commandeCommun.IdVendeur,
                IdAcheteur = _commandeCommun.IdAcheteur,
                IdAnnonce = _commandeCommun.IdAnnonce,
                IdMoyenPaiement = _commandeCommun.IdMoyenPaiement,
                Date = DateTime.UtcNow
            };

            // Act
            var result = await _controller.Post(commandeCreateDTO);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task BadRequestPostCommandeTest()
        {
            // Arrange
            CommandeCreateDTO dto = new CommandeCreateDTO();

            _controller.ModelState.AddModelError("Erreur", "Required");

            // Act
            var result = await _controller.Post(dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutCommandeTest()
        {
            // Arrange
            CommandeUpdateDTO dto = new CommandeUpdateDTO
            {
                IdCommande = _commandeCommun.IdCommande,
                IdVendeur = _commandeCommun.IdVendeur,
                IdAcheteur = _commandeCommun.IdAcheteur,
                IdAnnonce = _commandeCommun.IdAnnonce,
                IdMoyenPaiement = _commandeCommun.IdMoyenPaiement,
                Date = DateTime.UtcNow
            };

            // Act
            var result = await _controller.Put(_commandeCommun.IdCommande, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task PutBadRequestTest()
        {
            // Arrange
            var dto = new CommandeUpdateDTO { IdCommande = 999 };

            _controller.ModelState.AddModelError("Test", "Invalid Model");

            // Act
            var result = await _controller.Put(1, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task PutNotFoundTest()
        {
            // Arrange
            var dto = new CommandeUpdateDTO() { IdCommande = 10 };

            // Act
            var result = await _controller.Put(10, dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteCommandeTest()
        {
            // Arrange
            var id = _commandeCommun.IdCommande;

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            Assert.IsNull(await _manager.GetByIdAsync(id));
        }

        [TestMethod]
        public async Task NotFoundDeleteCommandeTest()
        {
            // Arrange
            var id = 0;

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion
        
    }
}
