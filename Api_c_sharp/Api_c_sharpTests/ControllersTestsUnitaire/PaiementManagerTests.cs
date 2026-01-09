using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass]
    [TestCategory("integration")]
    public class PaiementManagerTests
    {
        private AutoPulseBdContext _context = null!;
        private PaiementManager _manager = null!;
        private Annonce _objetcommun = null!;
        private Compte _compteCommun = null!;
        private CarteBancaire _carteCommune = null!;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Paiement_{Guid.NewGuid()}")
                .Options;
            _context = new AutoPulseBdContext(options);
            _manager = new PaiementManager(_context);

            _context.Paiements.RemoveRange(_context.Paiements);
            await _context.SaveChangesAsync();
            // Données de base
            _context.Marques.Add(new Marque { IdMarque = 1, LibelleMarque = "TestMarque" });
            _context.Motricites.Add(new Motricite { IdMotricite = 1, LibelleMotricite = "4x4" });
            _context.Carburants.Add(new Carburant { IdCarburant = 1, LibelleCarburant = "Essence" });
            _context.BoitesDeVitesses.Add(new BoiteDeVitesse { IdBoiteDeVitesse = 1, LibelleBoite = "Manuelle" });
            _context.Categories.Add(new Categorie { IdCategorie = 1, LibelleCategorie = "SUV" });
            _context.Modeles.Add(new Modele { IdModele = 1, LibelleModele = "Modele Test" });

            TypeCompte typeCompte = new TypeCompte()
            {
                IdTypeCompte = 1,
                Libelle = "Particulier"
            };

            _compteCommun = new Compte()
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

            MiseEnAvant miseEnAvant2 = new MiseEnAvant()
            {
                IdMiseEnAvant = 2,
                LibelleMiseEnAvant = "Or",
                PrixSemaine = 15,
            };

            MiseEnAvant miseEnAvant3 = new MiseEnAvant()
            {
                IdMiseEnAvant = 3,
                LibelleMiseEnAvant = "Platine",
                PrixSemaine = 25,
            };

            MiseEnAvant miseEnAvant4 = new MiseEnAvant()
            {
                IdMiseEnAvant = 4,
                LibelleMiseEnAvant = "Diamant",
                PrixSemaine = 35,
            };

            _objetcommun = new Annonce()
            {
                IdAnnonce = 1,
                Libelle = "Annonce Test",
                IdCompte = _compteCommun.IdCompte,
                IdEtatAnnonce = etatAnnonce.IdEtatAnnonce,
                IdAdresse = adresse.IdAdresse,
                Prix = 20000,
                Description = "Description de l'annonce",
                IdMiseEnAvant = miseEnAvant.IdMiseEnAvant,
                IdVoiture = voiture.IdVoiture,
                DatePublication = DateTime.Now,
                ProchaineMiseEnAvant = 3
            };

            _carteCommune = new CarteBancaire()
            {
                IdCarteBancaire = 1,
                IdCompte = _compteCommun.IdCompte,
                NumeroCarte = "c4sn80fEeFLXETmUBD0XeIsm5Jt57CxBs8kZY718bRM=",
                CodeSecurite = "aUjuu / kOAi3zIU2NIANtgw ==",
                DateExpiration = DateTime.Now.AddYears(2),
                TypeCarte = "Visa"
            };

            await _context.MisesEnAvant.AddAsync(miseEnAvant);
            await _context.MisesEnAvant.AddAsync(miseEnAvant2);
            await _context.MisesEnAvant.AddAsync(miseEnAvant3);
            await _context.MisesEnAvant.AddAsync(miseEnAvant4);
            await _context.Pays.AddAsync(pays);
            await _context.Adresses.AddAsync(adresse);
            await _context.EtatAnnonces.AddAsync(etatAnnonce);
            await _context.EtatAnnonces.AddAsync(etatAnnonceMasque);
            await _context.TypesCompte.AddAsync(typeCompte);
            await _context.Comptes.AddAsync(_compteCommun);
            await _context.Voitures.AddAsync(voiture);
            await _context.Annonces.AddAsync(_objetcommun);
            await _context.CarteBancaires.AddAsync(_carteCommune);
            await _context.SaveChangesAsync();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Tests Liste Vide

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvantListVide()
        {
            
            //assert
            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce + 1,
                DatePaiement = DateTime.UtcNow.AddDays(-7), // Seulement 3 jours
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_AucunPaiement_RetourneListeVide()
        {
            // Arrange - Aucun paiement ajouté

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_PaiementTropRecent_RetourneListeVide()
        {
            // Arrange
            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-3), // Seulement 3 jours
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_PaiementTropAncien_RetourneListeVide()
        {
            // Arrange
            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-10), // 10 jours
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        #endregion

        #region Tests Renouvellement Basique

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvantListNonVide()
        {
            int nextid = (int)_objetcommun.ProchaineMiseEnAvant;
            // Arrange
            Paiement paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7),
                IdMiseEnAvant = _objetcommun.IdMiseEnAvant,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(nextid, result.First().IdMiseEnAvant);
        }

        [TestMethod]
        public async Task VerifPaiementProchaineMiseEnavant1()
        {
            _objetcommun.ProchaineMiseEnAvant = 1;
            await _context.SaveChangesAsync();

            Paiement paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7),
                IdMiseEnAvant = _objetcommun.IdMiseEnAvant,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };

            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_PaiementExactement7Jours_CreeNouveauPaiement()
        {
            // Arrange
            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(_objetcommun.IdAnnonce, result.First().IdAnnonce);
            Assert.AreEqual(_compteCommun.IdCompte, result.First().IdCompte);
            Assert.AreEqual(_carteCommune.IdCarteBancaire, result.First().IdCarteBancaire);
        }

        #endregion

        #region Tests ProchaineMiseEnAvant

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_AvecProchaineMiseEnAvant_CreeNouveauPaiementEtMiseAJourAnnonce()
        {
            // Arrange
            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(1, result.Count());

            var nouveauPaiement = result.First();
            Assert.AreEqual(3, nouveauPaiement.IdMiseEnAvant); // ProchaineMiseEnAvant = 3

            var annonceMAJ = await _context.Annonces.FindAsync(_objetcommun.IdAnnonce);
            Assert.AreEqual(3, annonceMAJ.IdMiseEnAvant);
            Assert.IsNull(annonceMAJ.ProchaineMiseEnAvant);
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_ProchaineMiseEnAvantEst1_PasDePaiementCree()
        {
            _objetcommun.ProchaineMiseEnAvant = 1; 
            await _context.SaveChangesAsync();

            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = _objetcommun.IdMiseEnAvant,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(0, result.Count()); // Pas de nouveau paiement

            var annonceMAJ = await _context.Annonces.FindAsync(_objetcommun.IdAnnonce);
            Assert.AreEqual(1, annonceMAJ.IdMiseEnAvant); // Mise à jour vers Standard
            Assert.IsNull(annonceMAJ.ProchaineMiseEnAvant);
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_ProchaineMiseEnAvantNull_RenouvelleMemeNiveau()
        {
            // Arrange
            _objetcommun.ProchaineMiseEnAvant = null;
            _objetcommun.IdMiseEnAvant = 2; // Or
            _context.Annonces.Update(_objetcommun);
            await _context.SaveChangesAsync();

            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(2, result.First().IdMiseEnAvant); // Renouvelle au même niveau
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_ProchaineMiseEnAvantNullEtStandard_PasDePaiement()
        {
            // Arrange
            _objetcommun.ProchaineMiseEnAvant = null;
            _objetcommun.IdMiseEnAvant = 1; // Standard
            _context.Annonces.Update(_objetcommun);
            await _context.SaveChangesAsync();

            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 1,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(0, result.Count()); // Pas de renouvellement pour Standard
        }

        #endregion

        #region Tests Multiples Annonces

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_MultipleAnnonces_CreeMultiplesPaiements()
        {
            // Arrange
            var annonce2 = new Annonce()
            {
                IdAnnonce = 2,
                Libelle = "Annonce Test 2",
                IdCompte = _compteCommun.IdCompte,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                Prix = 25000,
                Description = "Description 2",
                IdMiseEnAvant = 2,
                IdVoiture = 1,
                DatePublication = DateTime.Now,
                ProchaineMiseEnAvant = 3
            };
            await _context.Annonces.AddAsync(annonce2);
            await _context.SaveChangesAsync();

            var paiement1 = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };

            var paiement2 = new Paiement()
            {
                IdPaiement = 2,
                IdAnnonce = annonce2.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };

            await _context.Paiements.AddAsync(paiement1);
            await _context.Paiements.AddAsync(paiement2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_MultiplePaiementsMemeAnnonce_PrendLePlusRecent()
        {
            // Arrange
            var paiement1 = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-14).Date,
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };

            var paiement2 = new Paiement()
            {
                IdPaiement = 2,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 3,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };

            await _context.Paiements.AddAsync(paiement1);
            await _context.Paiements.AddAsync(paiement2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(1, result.Count());
            // Le nouveau paiement doit utiliser ProchaineMiseEnAvant (3)
            Assert.AreEqual(3, result.First().IdMiseEnAvant);
        }

        #endregion

        #region Tests Différents Niveaux de Mise en Avant

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_MiseEnAvantOr_CreeNouveauPaiement()
        {
            // Arrange
            _objetcommun.IdMiseEnAvant = 2; // Or
            _objetcommun.ProchaineMiseEnAvant = 2;
            _context.Annonces.Update(_objetcommun);

            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(2, result.First().IdMiseEnAvant);
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_MiseEnAvantPlatine_CreeNouveauPaiement()
        {
            // Arrange
            _objetcommun.IdMiseEnAvant = 3; // Platine
            _objetcommun.ProchaineMiseEnAvant = 3;
            _context.Annonces.Update(_objetcommun);

            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 3,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().IdMiseEnAvant);
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_MiseEnAvantDiamant_CreeNouveauPaiement()
        {
            // Arrange
            _objetcommun.IdMiseEnAvant = 4; // Diamant
            _objetcommun.ProchaineMiseEnAvant = 4;
            _context.Annonces.Update(_objetcommun);

            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 4,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(4, result.First().IdMiseEnAvant);
        }

        #endregion

        #region Tests Changement de Niveau

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_PassageDeOrAPlatine_CreeNouveauPaiement()
        {
            // Arrange
            _objetcommun.IdMiseEnAvant = 2; // Or
            _objetcommun.ProchaineMiseEnAvant = 3; // Platine
            _context.Annonces.Update(_objetcommun);
            await _context.SaveChangesAsync();

            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 2,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().IdMiseEnAvant);

            var annonceMAJ = await _context.Annonces.FindAsync(_objetcommun.IdAnnonce);
            Assert.AreEqual(3, annonceMAJ.IdMiseEnAvant);
        }

        [TestMethod]
        public async Task VerifPaiementAutoMiseEnAvant_PassageDePlatineADiamant_CreeNouveauPaiement()
        {
            // Arrange
            _objetcommun.IdMiseEnAvant = 3; // Platine
            _objetcommun.ProchaineMiseEnAvant = 4; // Diamant
            _context.Annonces.Update(_objetcommun);
            await _context.SaveChangesAsync();

            var paiement = new Paiement()
            {
                IdPaiement = 1,
                IdAnnonce = _objetcommun.IdAnnonce,
                DatePaiement = DateTime.UtcNow.AddDays(-7).Date,
                IdMiseEnAvant = 3,
                IdCarteBancaire = _carteCommune.IdCarteBancaire,
                IdCompte = _compteCommun.IdCompte,
                IdCommande = null
            };
            await _context.Paiements.AddAsync(paiement);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.VerifPaiementAutoMiseEnAvant();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(4, result.First().IdMiseEnAvant);

            var annonceMAJ = await _context.Annonces.FindAsync(_objetcommun.IdAnnonce);
            Assert.AreEqual(4, annonceMAJ.IdMiseEnAvant);
            Assert.IsNull(annonceMAJ.ProchaineMiseEnAvant);
        }
        #endregion
    }
}