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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using AutoPulse.Shared.DTO.Authentification;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class CompteControllerTests
    {
        private CompteController _controller = null!;
        private AutoPulseBdContext _context = null!;
        private CompteManager _manager = null!;
        private RefreshTokenManager _tokenrefreshManager = null!;
        private IConfiguration _config = null!;
        private IMapper _mapper = null!;
        private Compte _objetcommun = null!;
        private IJournalService _journalService = null!;

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


            Dictionary<string, string> inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:SecretKey", "UneSuperCleSecreteTresLonguePourLeTestJWT123456789"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Authentication:Google:ClientId", "test-client-id"},
                {"Authentication:Google:ClientSecret", "test-client-secret"},
                {"Authentication:Google:RedirectUri", "http://localhost:5000/api/compte/googlecallback"},
                {"Email:GmailUser", "sae.autopulse@gmail.com"},
                {"Email:GmailPass", "hywx fzpn sxgq kkvy"},
                {"App:FrontendUrl", "http://localhost:5296"}
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _mapper = mapperconfig.CreateMapper();
            _journalService = new JournalManager(_context, NullLogger<JournalManager>.Instance);
            _manager = new CompteManager(_context);
            _tokenrefreshManager = new RefreshTokenManager(_context);

            
            _controller = new CompteController(_manager, _mapper, _config, _journalService, _tokenrefreshManager);

            
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

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

            EtatCompte etatCompte = new EtatCompte
            {
                IdEtatCompte = 1,
                Libelle = "Actif"
            };

            EtatCompte etatCompteInactif = new EtatCompte
            {
                IdEtatCompte = 2,
                Libelle = "Suspendu"
            };

            TypeCompte typeComptepro = new TypeCompte
            {
                IdTypeCompte = 2,
                Libelle = "Professionnel"
            };

            TypeCompte typeCompteAnonyme = new TypeCompte
            {
                IdTypeCompte = 4,
                Libelle = "Anonyme"
            };

            _context.TypesCompte.AddRange(typeCompte, typeCompteAnonyme,typeComptepro);
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
                Email = "test@test.com",
                MotDePasse = "b2b8804d428bb1129711f32ce77b9d3dde5b063c02ae62fcbc73988ae84d7c76",
                Pseudo = "johndoe",
                DateCreation = DateTime.UtcNow,
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = typeCompte.IdTypeCompte,
                DateDerniereConnexion = DateTime.UtcNow,
                IdEtatCompte = etatCompte.IdEtatCompte,
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

            await _context.EtatComptes.AddAsync(etatCompte);
            await _context.EtatComptes.AddAsync(etatCompteInactif);
            await _context.TypesSignalement.AddAsync(typeSignalement);
            await _context.Signalements.AddAsync(signalement);
            await _context.Pays.AddAsync(pays);
            await _context.Adresses.AddAsync(adresse);
            await _context.MisesEnAvant.AddAsync(miseEnAvant);
            await _context.EtatAnnonces.AddAsync(etatAnnonce);
            await _context.Voitures.AddAsync(voiture);
            await _context.Comptes.AddAsync(compte);
            await _context.Annonces.AddAsync(annonce);
            await _context.Favoris.AddAsync(favori);
            await _context.SaveChangesAsync();

            _objetcommun = compte;
        }

        #region GET
            #region GetById

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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CompteGetDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Pseudo == _objetcommun.Pseudo));
        }

        #endregion

            #region GetProfilPublic
        [TestMethod]
        public async Task GetProfilPublicTest()
        {
            // Act
            var result = await _controller.GetProfilPublic(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CompteProfilPublicDTO));
            Assert.AreEqual(_objetcommun.Pseudo, result.Value.Pseudo);
        }

        [TestMethod]
        public async Task NotFoundGetProfilPublicTest()
        {
            // Act
            var result = await _controller.GetProfilPublic(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        #endregion

            #region GetByString

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

        #endregion

            #region GetByTypeCompte

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
        #endregion

            #region GetCompteByAnnonceFavori
        [TestMethod]
        public async Task GetCompteByAnnonceFavoriTest()
        {
            // Act
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

        #endregion

            #region GetMeTests

        [TestMethod]
        public async Task GetMeTest()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("idUser", _objetcommun.IdCompte.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.GetMe();

            // Assert
            Assert.IsNotNull(result);
            var compteDto = (CompteDetailDTO)result.Value;
            Assert.IsInstanceOfType(compteDto, typeof(CompteDetailDTO));
            Assert.AreEqual(_objetcommun.Email, compteDto.Email);
            Assert.AreEqual(_objetcommun.Pseudo, compteDto.Pseudo);
        }

        [TestMethod]
        public async Task GetMeTest_Unauthorized_NoUserIdClaim()
        {
            // Arrange
            var claims = new List<Claim>();
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.GetMe();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task GetMeTest_NotFound_UserDoesNotExist()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("idUser", "999")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.GetMe();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        #endregion

        #endregion

        #region POST
        [TestMethod]
        public async Task PostCompteTest_Entity()
        {
            // Arrange
            CompteCreateDTO compteCreateDTO = new CompteCreateDTO
            {
                Nom = "Smith",
                Prenom = "Jane",
                Email = "jane.smith@gmail.com",
                MotDePasse = "anotherhashedpassword",
                Pseudo = "janesmith",
                DateNaissance = new DateTime(1992, 2, 2),
                IdTypeCompte = 1,
                EmailVerif = false, // ✅ Ajout explicite
            };

            // Act
            var actionResult = await _controller.Post(compteCreateDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdcompte = (Compte)created.Value;
            Assert.AreEqual(compteCreateDTO.Email, createdcompte.Email);
            Assert.IsFalse(createdcompte.EmailVerif); // ✅ Vérifie que l'email n'est pas vérifié par défaut

            // ✅ Vérifie qu'un token a été créé
            var token = await _context.TokenEmails
                .Where(t => t.IdCompte == createdcompte.IdCompte && t.TypeToken == "EMAIL_VERIFICATION")
                .FirstOrDefaultAsync();
            Assert.IsNotNull(token, "Un token de vérification d'email devrait être créé");
            Assert.IsFalse(token.Utilise);
        }

        [TestMethod]
        public async Task BadRequestPostCompteTest()
        {
            // Arrange
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

            _controller.ModelState.AddModelError("NumeroSiret", "Required");
            
            // Act
            var actionResult = await _controller.Post(compteUpdateDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

        #endregion

        #region DELETE

        [TestMethod]
        public async Task DeleteCompteTest()
        {
            // Act
            var result = await _controller.Delete(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedCompte = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.IsNull(deletedCompte);
        }

        [TestMethod]
        public async Task NotFoundDeleteCompteTest()
        {
            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        #endregion

        #region Put

            #region PutCompte
        [TestMethod]
        public async Task PutCompteTest()
        {
            // Arrange
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = "JohnUpdated",
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdCompte, compteUpdateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var compteput = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(compteUpdateDTO.Nom, compteput.Nom);
        }

        [TestMethod]
        public async Task NotFoundPutCompteTest()
        {
            // Arrange
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = "JohnUpdated",
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
            };

            // Act
            var result = await _controller.Put(0, compteUpdateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutCompteTest()
        {
            // Arrange
            CompteUpdateDTO compteUpdateDTO = new CompteUpdateDTO
            {
                IdCompte = _objetcommun.IdCompte,
                Nom = "DoeUpdated",
                Prenom = null,
                Email = "johnmodif@gmail.com",
                DateNaissance = new DateTime(1991, 1, 1),
                IdTypeCompte = 1,
            };

            _controller.ModelState.AddModelError("Prenom", "Required");
            
            // Act
            var result = await _controller.Put(_objetcommun.IdCompte, compteUpdateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

            #region PutAnonymise
        [TestMethod]
        public async Task PutAnonymiseTest()
        {
            // Act
            var result = await _controller.PutAnonymise(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var compteanonymise = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.IsNotNull(compteanonymise, "Le compte ne devrait pas être null");
            Assert.AreEqual("ANONYME", compteanonymise.Nom);
            Assert.AreEqual("Utilisateur", compteanonymise.Prenom);
        }

        [TestMethod]
        public async Task NotFoundPutAnonymiseTest()
        {
            // Act
            var result = await _controller.PutAnonymise(0);
            
            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutAnonymiseAvecCompteJournauxFavoriTest()
        {
            // Arrange
            Compte compte2 = new Compte
            {
                IdCompte = 2,
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@gmail.com",
                MotDePasse = "anotherhashedpassword",
                Pseudo = "jeandupont",
                DateCreation = DateTime.UtcNow,
                DateNaissance = new DateTime(1985, 5, 5),
                IdTypeCompte = 1,
                DateDerniereConnexion = DateTime.UtcNow,
            };
            _context.Comptes.Add(compte2);
            Adresse adresse = new Adresse
            {
                IdAdresse = 2,
                Nom = "Adresse Test",
                Rue = "456 Rue de Test",
                LibelleVille = "Testville",
                CodePostal = "67890",
                IdPays = 1,
                IdCompte = compte2.IdCompte
            };
            _context.Adresses.Add(adresse);
            _context.Journaux.Add(new Journal
            {
                IdJournal = 1,
                IdCompte = compte2.IdCompte,
                ContenuJournal = "Journal Test",
            });
            _context.Favoris.Add(new Favori
            {
                IdAnnonce = 1,
                IdCompte = compte2.IdCompte
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.PutAnonymise(compte2.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var compteanonymise = await _manager.GetByIdAsync(compte2.IdCompte);
            Assert.IsNull(compteanonymise);
            var Comptes = await _context.Comptes.Where(j => j.IdCompte == compte2.IdCompte).ToListAsync();
            Assert.IsFalse(Comptes.Any(), "Les journaux associés au compte devraient être supprimés.");
            var journaux = await _context.Journaux.Where(j => j.IdCompte == compte2.IdCompte).ToListAsync();
            Assert.IsFalse(journaux.Any(), "Les journaux associés au compte devraient être supprimés.");
            var favoris = await _context.Favoris.Where(f => f.IdCompte == compte2.IdCompte).ToListAsync();
            Assert.IsFalse(favoris.Any(), "Les favoris associés au compte devraient être supprimés.");
        }
        #endregion

            #region PutTypeCompte

        [TestMethod]
        public async Task PutTypeCompteProTest()
        {
            // Arrange
            CompteModifTypeCompteDTO compteModifTypeCompteDTO = new CompteModifTypeCompteDTO
            {
                RaisonSociale = "test",
                NumeroSiret = "12345678912345"
            };

            // Act
            var result = await _controller.PutTypeCompte(_objetcommun.IdCompte, compteModifTypeCompteDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(compteModifTypeCompteDTO.RaisonSociale, compteModifie.RaisonSociale);
            Assert.AreEqual(2, compteModifie.IdTypeCompte);
        }

        [TestMethod]
        public async Task PutTypeComptePersoTest()
        {
            // Arrange
            _objetcommun.IdTypeCompte = 2;
            CompteModifTypeCompteDTO compteModifTypeCompteDTO = new CompteModifTypeCompteDTO
            {
                RaisonSociale = null,
                NumeroSiret = null
            };

            // Act
            var result = await _controller.PutTypeCompte(_objetcommun.IdCompte, compteModifTypeCompteDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(compteModifTypeCompteDTO.RaisonSociale, compteModifie.RaisonSociale);
            Assert.AreEqual(1, compteModifie.IdTypeCompte);
        }

        [TestMethod]
        public async Task NotFoundPutTypeCompteTest()
        {
            // Arrange
            CompteModifTypeCompteDTO compteModifTypeCompteDTO = new CompteModifTypeCompteDTO
            {
                RaisonSociale = "test",
                NumeroSiret = "12345678912345"
            };

            // Act
            var result = await _controller.PutTypeCompte(0, compteModifTypeCompteDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        #endregion

        #endregion

        #region MDP

        [TestMethod]
        public async Task ModifMotDePasseTest()
        {
            // Arrange
            ChangementMdpDTO changementMdpDTO = new ChangementMdpDTO
            {
                IdCompte = _objetcommun.IdCompte,
                MotDePasse = "ouioui",
                Email = _objetcommun.Email
            };
            string Hashpassword = "728b252625ebcddcea74d61760866080a10196087c340a57a88ba511bd387921";

            // Act
            var result = await _controller.ModifMdp(changementMdpDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(Hashpassword, compteModifie.MotDePasse);
        }

        [TestMethod]
        public async Task NotFoundModifMotDePasseTest()
        {
            // Arrange
            ChangementMdpDTO changementMdpDTO = new ChangementMdpDTO
            {
                IdCompte = 0,
                MotDePasse = "ouioui",
                Email = _objetcommun.Email
            };

            // Act
            var result = await _controller.ModifMdp(changementMdpDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }
        #endregion

        #region VerifUser
        [TestMethod]
        public async Task VerifUserTest()
        {
            // Arrange
            ChangementMdpDTO changementMdpDTO = new ChangementMdpDTO
            {
                IdCompte = _objetcommun.IdCompte,
                MotDePasse = "Testmdp1!",
                Email = _objetcommun.Email
            };

            // Act
            bool result = await _controller.VerifUser(changementMdpDTO);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task NotVerifUserTest()
        {
            // Arrange
            ChangementMdpDTO changementMdpDTO = new ChangementMdpDTO
            {
                IdCompte = _objetcommun.IdCompte,
                MotDePasse = "nonnon",
                Email = _objetcommun.Email
            };
            // Act
            bool result = await _controller.VerifUser(changementMdpDTO);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region Login

        [TestMethod]
        public async Task Login_InvalidEmail_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "wrong@test.com",
                MotDePasse = "Testmdp1!"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "john@gmail.com",
                MotDePasse = "WrongPassword"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Login_EmptyEmail_ReturnsBadRequest()
        {
            // arrange
            var loginRequest = new LoginRequest
            {
                Email = "",
                MotDePasse = "Testmdp1!"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_EmptyPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "john@gmail.com",
                MotDePasse = ""
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_NullCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = null,
                MotDePasse = null
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_CaseInsensitiveEmail_ReturnsOk()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "JOHN@GMAIL.COM",
                MotDePasse = "Testmdp1!"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
        }

        #endregion

        #region Logout

        [TestMethod]
        public async Task Logout_AuthenticatedUser_ReturnsOk()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("idUser", _objetcommun.IdCompte.ToString()),
                new Claim(ClaimTypes.NameIdentifier, _objetcommun.Email)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext.User = claimsPrincipal;

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Logout_DeletesCookie()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("idUser", _objetcommun.IdCompte.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext.User = claimsPrincipal;

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Logout_InvalidUserId_ReturnsInternalServerError()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("idUser", "invalid_user_id") // -> FormatException lors du Parse
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        #endregion

        #region ToggleEtatCompte

        [TestMethod]
        public async Task ToggleEtatCompteTest()
        {
            // Act
            var result = await _controller.ToggleEtatCompte(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            Compte compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(2, compteModifie.IdEtatCompte);
        }


        [TestMethod]
        public async Task NotFoundToggleEtatCompteTest()
        {
            // act
            var result = await _controller.ToggleEtatCompte(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }


        [TestMethod]
        public async Task ToggleEtatCompteEstRetirerTest()
        {
            // Arrange
            _objetcommun.IdEtatCompte = 2;
            _context.SaveChanges();

            // Act
            var result = await _controller.ToggleEtatCompte(_objetcommun.IdCompte,true);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            Compte compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(1, compteModifie.IdEtatCompte);
        }

        #endregion

        #region Google Login

        [TestMethod]
        public void GoogleLogin_ReturnsOkResult()
        {
            // Act
            var result = _controller.GoogleLogin();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void GoogleLogin_ReturnsUrlInResponse()
        {
            // Act
            var result = _controller.GoogleLogin();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Value);

            // Extrai l'URL de la réponse ano
            var responseType = okResult.Value.GetType();
            var urlProperty = responseType.GetProperty("url");
            Assert.IsNotNull(urlProperty);
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsGoogleAuthEndpoint()
        {
            // Act
            var result = _controller.GoogleLogin();

            // Assert
            var okResult = result as OkObjectResult;
            var urlProperty = okResult.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult.Value).ToString();

            Assert.IsTrue(url.Contains("accounts.google.com/o/oauth2/v2/auth"),
                "L'URL devrait contenir l'endpoint d'authentification Google");
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsClientId()
        {
            // Act
            var result = _controller.GoogleLogin();

            // Assert
            var okResult = result as OkObjectResult;
            var urlProperty = okResult.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult.Value).ToString();

            Assert.IsTrue(url.Contains("client_id="),
                "L'URL devrait contenir le paramètre client_id");
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsRedirectUri()
        {
            // Act
            var result = _controller.GoogleLogin();

            // Assert
            var okResult = result as OkObjectResult;
            var urlProperty = okResult.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult.Value).ToString();

            Assert.IsTrue(url.Contains("redirect_uri="),
                "L'URL devrait contenir le paramètre redirect_uri");
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsResponseType()
        {
            // Act
            var result = _controller.GoogleLogin();

            // Assert
            var okResult = result as OkObjectResult;
            var urlProperty = okResult.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult.Value).ToString();

            Assert.IsTrue(url.Contains("response_type=code"),
                "L'URL devrait contenir response_type=code");
        }

        [TestMethod]
        public void GoogleLogin_UrlContainsRequiredScopes()
        {
            // Act
            var result = _controller.GoogleLogin();

            // Assert
            var okResult = result as OkObjectResult;
            var urlProperty = okResult.Value.GetType().GetProperty("url");
            string url = urlProperty.GetValue(okResult.Value).ToString();

            Assert.IsTrue(url.Contains("scope="),
                "L'URL devrait contenir le paramètre scope");
            Assert.IsTrue(url.Contains("openid"),
                "L'URL devrait contenir le scope openid");
            Assert.IsTrue(url.Contains("profile"),
                "L'URL devrait contenir le scope profile");
            Assert.IsTrue(url.Contains("email"),
                "L'URL devrait contenir le scope email");
        }

        [TestMethod]
        public async Task GoogleCallback_WithoutCode_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GoogleCallback(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task GoogleCallback_WithEmptyCode_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GoogleCallback("");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("Code manquant", badRequest.Value);
        }
        #endregion

        #region A2F

        [TestMethod]
        public async Task GetStatutA2f_A2fActif_ReturnsStatutCorrect()
        {
            // Arrange
            _objetcommun.A2fActif = true;
            _objetcommun.DateDerniereActivationA2f = DateTime.UtcNow.AddDays(-15);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetStatutA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            var response = okResult.Value;
            var responseType = response.GetType();
            var a2fActifProperty = responseType.GetProperty("A2fActif");
            var dateDerniereActivationProperty = responseType.GetProperty("DateDerniereActivation");
            var doitReactiverProperty = responseType.GetProperty("DoitReactiver");

            Assert.AreEqual(true, a2fActifProperty.GetValue(response));
            Assert.IsNotNull(dateDerniereActivationProperty.GetValue(response));
            Assert.AreEqual(false, doitReactiverProperty.GetValue(response));
        }

        [TestMethod]
        public async Task GetStatutA2f_A2fInactif_ReturnsStatutCorrect()
        {
            // Arrange
            _objetcommun.A2fActif = false;
            _objetcommun.DateDerniereActivationA2f = null;
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetStatutA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            var response = okResult.Value;
            var responseType = response.GetType();
            var a2fActifProperty = responseType.GetProperty("A2fActif");

            Assert.AreEqual(false, a2fActifProperty.GetValue(response));
        }

        [TestMethod]
        public async Task GetStatutA2f_CompteInexistant_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetStatutA2f(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task VerifActivA2f_DoitReactiver_ReturnsTrue()
        {
            // Arrange
            _objetcommun.A2fActif = true;
            _objetcommun.DateDerniereActivationA2f = DateTime.UtcNow.AddDays(-35); // Plus de 30 jours
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.VerifActivA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(true, okResult.Value);
        }

        [TestMethod]
        public async Task VerifActivA2f_NePasDevoirReactiver_ReturnsFalse()
        {
            // Arrange
            _objetcommun.A2fActif = true;
            _objetcommun.DateDerniereActivationA2f = DateTime.UtcNow.AddDays(-15); // Moins de 30 jours
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.VerifActivA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(false, okResult.Value);
        }

        [TestMethod]
        public async Task VerifActivA2f_A2fInactif_ReturnsFalse()
        {
            // Arrange
            _objetcommun.A2fActif = false;
            _objetcommun.DateDerniereActivationA2f = null;
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.VerifActivA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(false, okResult.Value);
        }

        [TestMethod]
        public async Task VerifActivA2f_A2fActifSansDate_ReturnsTrue()
        {
            // Arrange
            _objetcommun.A2fActif = true;
            _objetcommun.DateDerniereActivationA2f = null; // Jamais activé
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.VerifActivA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(true, okResult.Value);
        }

        [TestMethod]
        public async Task VerifActivA2f_CompteInexistant_ReturnsNotFound()
        {
            // Act
            var result = await _controller.VerifActivA2f(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ActiverA2f_ValidData_ActiveA2fEtEnregistreDansJournal()
        {
            // Arrange
            _context.TypesJournal.Add(new TypeJournal
            {
                IdTypeJournaux = 15,
                LibelleTypeJournaux = "Activation A2F"
            });
            await _context.SaveChangesAsync();

            var dto = new A2fActivationDTO
            {
                IdCompte = _objetcommun.IdCompte,
                CodeValidation = "1234567"
            };

            // Act
            var result = await _controller.ActiverA2f(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.IsTrue(compteModifie.A2fActif);
            Assert.IsNotNull(compteModifie.DateDerniereActivationA2f);
            Assert.IsTrue((DateTime.UtcNow - compteModifie.DateDerniereActivationA2f.Value).TotalMinutes < 1);

            var journal = await _context.Journaux
                .Where(j => j.IdCompte == _objetcommun.IdCompte && j.IdTypeJournal == 15)
                .FirstOrDefaultAsync();
            Assert.IsNotNull(journal);
        }

        [TestMethod]
        public async Task ActiverA2f_CompteInexistant_ReturnsNotFound()
        {
            // Arrange
            var dto = new A2fActivationDTO
            {
                IdCompte = 999,
                CodeValidation = "1234567"
            };

            // Act
            var result = await _controller.ActiverA2f(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ActiverA2f_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var dto = new A2fActivationDTO
            {
                IdCompte = _objetcommun.IdCompte,
                CodeValidation = null
            };

            _controller.ModelState.AddModelError("CodeValidation", "Required");

            // Act
            var result = await _controller.ActiverA2f(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task DesactiverA2f_ValidId_DesactiveA2fEtEnregistreDansJournal()
        {
            // Arrange
            _context.TypesJournal.Add(new TypeJournal
            {
                IdTypeJournaux = 16,
                LibelleTypeJournaux = "Désactivation A2F"
            });
            _objetcommun.A2fActif = true;
            _objetcommun.DateDerniereActivationA2f = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DesactiverA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.IsFalse(compteModifie.A2fActif);

            var journal = await _context.Journaux
                .Where(j => j.IdCompte == _objetcommun.IdCompte && j.IdTypeJournal == 16)
                .FirstOrDefaultAsync();
            Assert.IsNotNull(journal);
        }

        [TestMethod]
        public async Task DesactiverA2f_CompteInexistant_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DesactiverA2f(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task DemanderActivationA2f_ValidId_ReturnsOkWithMessage()
        {
            // Act
            var result = await _controller.DemanderActivationA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            var response = okResult.Value;
            var responseType = response.GetType();
            var messageProperty = responseType.GetProperty("Message");

            Assert.IsNotNull(messageProperty);
            var message = messageProperty.GetValue(response).ToString();
            Assert.IsTrue(message.Contains("code d'activation"));
        }

        [TestMethod]
        public async Task DemanderActivationA2f_CompteInexistant_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DemanderActivationA2f(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ValidateA2fLogin_ValidCodeConnexion_ReturnsOkWithToken()
        {
            // Arrange
            var dto = new TokenEmailVerifDTO
            {
                Email = _objetcommun.Email,
                Code = "1234567",
                TypeToken = "A2F_CONNEXION"
            };

            // Act
            var result = await _controller.ValidateA2fLogin(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            var response = okResult.Value;
            var responseType = response.GetType();
            var messageProperty = responseType.GetProperty("message");
            var userIdProperty = responseType.GetProperty("userId");
            var pseudoProperty = responseType.GetProperty("pseudo");

            Assert.AreEqual("Login OK", messageProperty.GetValue(response));
            Assert.AreEqual(_objetcommun.IdCompte, userIdProperty.GetValue(response));
            Assert.AreEqual(_objetcommun.Pseudo, pseudoProperty.GetValue(response));

            var cookies = _controller.Response.Headers["Set-Cookie"];
            Assert.IsTrue(cookies.Count > 0);
            Assert.IsTrue(cookies.ToString().Contains("access_token"));
        }

        [TestMethod]
        public async Task ValidateA2fLogin_ValidCodeActivation_ActiveA2fAndReturnsOk()
        {
            // Arrange
            var dto = new TokenEmailVerifDTO
            {
                Email = _objetcommun.Email,
                Code = "1234567",
                TypeToken = "A2F_ACTIVATION"
            };

            // Act
            var result = await _controller.ValidateA2fLogin(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.IsTrue(compteModifie.A2fActif);
            Assert.IsNotNull(compteModifie.DateDerniereActivationA2f);
        }

        [TestMethod]
        public async Task ValidateA2fLogin_CompteInexistant_ReturnsBadRequest()
        {
            // Arrange
            var dto = new TokenEmailVerifDTO
            {
                Email = "nonexistent@gmail.com",
                Code = "1234567",
                TypeToken = "A2F_CONNEXION"
            };

            // Act
            var result = await _controller.ValidateA2fLogin(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_AvecA2fInactif_ReturnsOkWithToken()
        {
            // Arrange
            _objetcommun.A2fActif = false;
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Email = _objetcommun.Email,
                MotDePasse = "Testmdp1!"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var cookies = _controller.Response.Headers["Set-Cookie"];
            Assert.IsTrue(cookies.Count > 0);
            Assert.IsTrue(cookies.ToString().Contains("access_token"));
        }

        [TestMethod]
        public async Task GetStatutA2f_A2fActifDepuis25Jours_DoitReactiverFalse()
        {
            // Arrange
            _objetcommun.A2fActif = true;
            _objetcommun.DateDerniereActivationA2f = DateTime.UtcNow.AddDays(-25);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetStatutA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            var response = okResult.Value;
            var responseType = response.GetType();
            var doitReactiverProperty = responseType.GetProperty("DoitReactiver");

            Assert.AreEqual(false, doitReactiverProperty.GetValue(response));
        }

        [TestMethod]
        public async Task GetStatutA2f_A2fActifExactement30Jours_DoitReactiverFalse()
        {
            // Arrange
            _objetcommun.A2fActif = true;
            _objetcommun.DateDerniereActivationA2f = DateTime.UtcNow.AddDays(-30);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetStatutA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            var response = okResult.Value;
            var responseType = response.GetType();
            var doitReactiverProperty = responseType.GetProperty("DoitReactiver");

            // Exactement 30 jours = pas besoin de réactiver
            Assert.AreEqual(false, doitReactiverProperty.GetValue(response));
        }

        [TestMethod]
        public async Task GetStatutA2f_A2fActifDepuis31Jours_DoitReactiverTrue()
        {
            // Arrange
            _objetcommun.A2fActif = true;
            _objetcommun.DateDerniereActivationA2f = DateTime.UtcNow.AddDays(-31);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetStatutA2f(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            var response = okResult.Value;
            var responseType = response.GetType();
            var doitReactiverProperty = responseType.GetProperty("DoitReactiver");

            Assert.AreEqual(true, doitReactiverProperty.GetValue(response));
        }

        #endregion

        #region Vérification Email Tests

        [TestMethod]
        public async Task EnvoyerEmailVerification_ValidCompte_ReturnsOk()
        {
            // Act
            var result = await _controller.EnvoyerEmailVerification(_objetcommun.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            var response = okResult.Value;
            var responseType = response.GetType();
            var messageProperty = responseType.GetProperty("message");

            Assert.IsNotNull(messageProperty);
            Assert.AreEqual("Email de vérification envoyé", messageProperty.GetValue(response));

            // Vérifie qu'un token a été créé
            var token = await _context.TokenEmails
                .Where(t => t.IdCompte == _objetcommun.IdCompte && t.TypeToken == "EMAIL_VERIFICATION")
                .OrderByDescending(t => t.Expiration)
                .FirstOrDefaultAsync();

            Assert.IsNotNull(token);
            Assert.IsFalse(token.Utilise);
            Assert.IsTrue(token.Expiration > DateTime.UtcNow);
        }

        [TestMethod]
        public async Task EnvoyerEmailVerification_CompteInexistant_ReturnsNotFound()
        {
            // Act
            var result = await _controller.EnvoyerEmailVerification(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task VerifierEmail_ValidToken_MarksEmailAsVerified()
        {
            // Arrange
            var token = "test-token-12345";
            var tokenEmail = new TokenEmail
            {
                IdCompte = _objetcommun.IdCompte,
                Email = _objetcommun.Email,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                Utilise = false,
                TypeToken = "EMAIL_VERIFICATION"
            };

            await _context.TokenEmails.AddAsync(tokenEmail);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.VerifierEmail(token);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            // Vérifie que l'email est marqué comme vérifié
            var compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.IsTrue(compteModifie.EmailVerif);

            // Vérifie que le token est marqué comme utilisé
            var tokenUtilise = await _context.TokenEmails
                .FirstOrDefaultAsync(t => t.Token == token);
            Assert.IsTrue(tokenUtilise.Utilise);
        }

        [TestMethod]
        public async Task VerifierEmail_TokenInvalide_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.VerifierEmail("token-inexistant");

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;

            var response = badRequest.Value;
            var responseType = response.GetType();
            var messageProperty = responseType.GetProperty("message");

            Assert.AreEqual("Token invalide ou déjà utilisé", messageProperty.GetValue(response));
        }

        [TestMethod]
        public async Task VerifierEmail_TokenExpire_ReturnsBadRequest()
        {
            // Arrange
            var token = "expired-token-12345";
            var tokenEmail = new TokenEmail
            {
                IdCompte = _objetcommun.IdCompte,
                Email = _objetcommun.Email,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(-1), // Token expiré
                Utilise = false,
                TypeToken = "EMAIL_VERIFICATION"
            };

            await _context.TokenEmails.AddAsync(tokenEmail);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.VerifierEmail(token);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;

            var response = badRequest.Value;
            var responseType = response.GetType();
            var messageProperty = responseType.GetProperty("message");

            Assert.AreEqual("Le token a expiré", messageProperty.GetValue(response));
        }

        [TestMethod]
        public async Task VerifierEmail_TokenDejaUtilise_ReturnsBadRequest()
        {
            // Arrange
            var token = "used-token-12345";
            var tokenEmail = new TokenEmail
            {
                IdCompte = _objetcommun.IdCompte,
                Email = _objetcommun.Email,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                Utilise = true, // Déjà utilisé
                TypeToken = "EMAIL_VERIFICATION"
            };

            await _context.TokenEmails.AddAsync(tokenEmail);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.VerifierEmail(token);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;

            var response = badRequest.Value;
            var responseType = response.GetType();
            var messageProperty = responseType.GetProperty("message");

            Assert.AreEqual("Token invalide ou déjà utilisé", messageProperty.GetValue(response));
        }

        [TestMethod]
        public async Task VerifierEmail_CompteInexistant_ReturnsNotFound()
        {
            // Arrange
            var token = "token-compte-inexistant";
            var tokenEmail = new TokenEmail
            {
                IdCompte = 999, // Compte qui n'existe pas
                Email = "inexistant@test.com",
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                Utilise = false,
                TypeToken = "EMAIL_VERIFICATION"
            };

            await _context.TokenEmails.AddAsync(tokenEmail);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.VerifierEmail(token);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        #endregion
    }
}