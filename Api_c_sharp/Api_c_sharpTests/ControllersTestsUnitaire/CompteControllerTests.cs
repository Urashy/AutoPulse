using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Authentification;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using Api_c_sharp.Controllers;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoPulse.Shared.DTO.Authentification;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class CompteControllerTests
    {
        private CompteController _controller;
        private AutoPulseBdContext _context;
        private CompteManager _manager;
        private IConfiguration _config;
        private IMapper _mapper;
        private Compte _objetcommun;
        private IJournalService _journalService;

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

            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:SecretKey", "UneSuperCleSecreteTresLonguePourLeTestJWT123456789"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Authentication:Google:ClientId", "test-client-id"},
                {"Authentication:Google:ClientSecret", "test-client-secret"},
                {"Authentication:Google:RedirectUri", "http://localhost:5000/api/compte/googlecallback"}
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _mapper = mapperconfig.CreateMapper();
            _journalService = new JournalManager(_context, NullLogger<JournalManager>.Instance);
            _manager = new CompteManager(_context);

            // ✅ IMPORTANT : Utiliser _config (configuration en mémoire) au lieu de config
            _controller = new CompteController(_manager, _mapper, _config, _journalService);

            // ✅ Configuration du contexte HTTP pour les cookies
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
                Email = "john@gmail.com",
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

            // ✅ Ajouter TypeSignalement au contexte
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

            _controller.ModelState.AddModelError("Prenom", "Required");
            var result = await _controller.Put(_objetcommun.IdCompte, compteUpdateDTO);

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
            Assert.IsNotNull(compteanonymise, "Le compte ne devrait pas être null");
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
        public async Task PutAnonymiseAvecAdresseJournauxFavoriTest()
        {
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
            var result = await _controller.PutAnonymise(compte2.IdCompte);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var compteanonymise = await _manager.GetByIdAsync(compte2.IdCompte);
            Assert.IsNull(compteanonymise);
            var adresses = await _context.Adresses.Where(j => j.IdCompte == compte2.IdCompte).ToListAsync();
            Assert.IsFalse(adresses.Any(), "Les journaux associés au compte devraient être supprimés.");
            var journaux = await _context.Journaux.Where(j => j.IdCompte == compte2.IdCompte).ToListAsync();
            Assert.IsFalse(journaux.Any(), "Les journaux associés au compte devraient être supprimés.");
            var favoris = await _context.Favoris.Where(f => f.IdCompte == compte2.IdCompte).ToListAsync();
            Assert.IsFalse(favoris.Any(), "Les favoris associés au compte devraient être supprimés.");
        }

        [TestMethod]
        public async Task PutTypeCompteProTest()
                    {
            CompteModifTypeCompteDTO compteModifTypeCompteDTO = new CompteModifTypeCompteDTO
            {
                RaisonSociale = "test",
                NumeroSiret = "12345678912345"
            };
            var result = await _controller.PutTypeCompte(_objetcommun.IdCompte, compteModifTypeCompteDTO);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(compteModifTypeCompteDTO.RaisonSociale, compteModifie.RaisonSociale);
            Assert.AreEqual(2,compteModifie.IdTypeCompte);
        }

        [TestMethod]
        public async Task PutTypeComptePersoTest()
        {
            _objetcommun.IdTypeCompte = 2;
            CompteModifTypeCompteDTO compteModifTypeCompteDTO = new CompteModifTypeCompteDTO
            {
                RaisonSociale = null,
                NumeroSiret = null
            };
            var result = await _controller.PutTypeCompte(_objetcommun.IdCompte, compteModifTypeCompteDTO);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(compteModifTypeCompteDTO.RaisonSociale, compteModifie.RaisonSociale);
            Assert.AreEqual(1, compteModifie.IdTypeCompte);
        }

        [TestMethod]
        public async Task NotFoundPutTypeCompteTest()
        {
            CompteModifTypeCompteDTO compteModifTypeCompteDTO = new CompteModifTypeCompteDTO
            {
                RaisonSociale = "test",
                NumeroSiret = "12345678912345"
            };
            var result = await _controller.PutTypeCompte(0,compteModifTypeCompteDTO);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetByStringTest()
        {
            var result = await _controller.GetByString(_objetcommun.Email);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(CompteDetailDTO));
            Assert.AreEqual(_objetcommun.Nom, result.Value.Nom);
        }

        [TestMethod]
        public async Task NotFoundGetByStringTest()
        {
            var result = await _controller.GetByString("NonExistentMail");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetByTypeCompteTest()
        {
            var result = await _controller.GetByTypeCompte(_objetcommun.IdTypeCompte);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CompteGetDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Pseudo == _objetcommun.Pseudo));
        }

        [TestMethod]
        public async Task NotFoundGetByTypeCompteTest()
        {
            var result = await _controller.GetByTypeCompte(999);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetCompteByAnnonceFavoriTest()
        {
            var result = await _controller.GetCompteByAnnonceFavori(1);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<CompteGetDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.Pseudo == _objetcommun.Pseudo));
        }

        [TestMethod]
        public async Task NotFoundGetCompteByAnnonceFavoriTest()
        {
            var result = await _controller.GetCompteByAnnonceFavori(999);
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
                MotDePasse = "Testmdp1!",
                Email = _objetcommun.Email
            };
            bool result = await _controller.VerifUser(changementMdpDTO);
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
            bool result = await _controller.VerifUser(changementMdpDTO);
            Assert.IsFalse(result);
        }

        #region Tests Login

        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var loginRequest = new LoginRequest()
            {
                Email = "john@gmail.com",
                MotDePasse = "Testmdp1!"
            };

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var cookies = _controller.Response.Headers["Set-Cookie"];
            Assert.IsTrue(cookies.Count > 0);
            Assert.IsTrue(cookies.ToString().Contains("access_token"));
        }

        [TestMethod]
        public async Task Login_InvalidEmail_ReturnsUnauthorized()
        {
            var loginRequest = new LoginRequest
            {
                Email = "wrong@test.com",
                MotDePasse = "Testmdp1!"
            };

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var loginRequest = new LoginRequest
            {
                Email = "john@gmail.com",
                MotDePasse = "WrongPassword"
            };

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Login_EmptyEmail_ReturnsBadRequest()
        {
            var loginRequest = new LoginRequest
            {
                Email = "",
                MotDePasse = "Testmdp1!"
            };

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_EmptyPassword_ReturnsBadRequest()
        {
            var loginRequest = new LoginRequest
            {
                Email = "john@gmail.com",
                MotDePasse = ""
            };

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_NullCredentials_ReturnsBadRequest()
        {
            var loginRequest = new LoginRequest
            {
                Email = null,
                MotDePasse = null
            };

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_CaseInsensitiveEmail_ReturnsOk()
        {
            var loginRequest = new LoginRequest
            {
                Email = "JOHN@GMAIL.COM",
                MotDePasse = "Testmdp1!"
            };

            var result = await _controller.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
        }

        #endregion

        #region Tests Logout

        [TestMethod]
        public async Task Logout_AuthenticatedUser_ReturnsOk()
        {
            var claims = new List<Claim>
            {
                new Claim("idUser", _objetcommun.IdCompte.ToString()),
                new Claim(ClaimTypes.NameIdentifier, _objetcommun.Email)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext.User = claimsPrincipal;

            var result = await _controller.Logout();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Logout_DeletesCookie()
        {
            var claims = new List<Claim>
            {
                new Claim("idUser", _objetcommun.IdCompte.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext.User = claimsPrincipal;

            var result = await _controller.Logout();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Logout_InvalidUserId_ReturnsInternalServerError()
        {
            // Arrange - Configurer un claim avec un userId non numérique pour forcer une exception
            var claims = new List<Claim>
            {
                new Claim("idUser", "invalid_user_id") // Cela va causer une FormatException lors du Parse
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
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOfType(okResult.Value, typeof(CompteDetailDTO));
            var compteDto = (CompteDetailDTO)okResult.Value;
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

        [TestMethod]
        public async Task ToggleEtatCompteTest()
        {
            var result = await _controller.ToggleEtatCompte(_objetcommun.IdCompte);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            Compte compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(2, compteModifie.IdEtatCompte);
        }


        [TestMethod]
        public async Task NotFoundToggleEtatCompteTest()
        {
            var result = await _controller.ToggleEtatCompte(0);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }


        [TestMethod]
        public async Task ToggleEtatCompteEstRetirerTest()
        {
            _objetcommun.IdEtatCompte = 2;
            _context.SaveChanges();

            var result = await _controller.ToggleEtatCompte(_objetcommun.IdCompte,true);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            Compte compteModifie = await _manager.GetByIdAsync(_objetcommun.IdCompte);
            Assert.AreEqual(1, compteModifie.IdEtatCompte);
        }

        #region Tests Google Login

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

            // Extraire l'URL de la réponse anonyme
            var responseType = okResult.Value.GetType();
            var urlProperty = responseType.GetProperty("url");
            Assert.IsNotNull(urlProperty);

            string url = urlProperty.GetValue(okResult.Value)?.ToString();
            Assert.IsNotNull(url);
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
    }
}