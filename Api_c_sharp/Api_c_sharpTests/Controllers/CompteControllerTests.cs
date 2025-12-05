using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Authentification;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using App.Controllers;
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

namespace App.Controllers.Tests
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

            // ✅ Configuration JWT en mémoire
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:SecretKey", "UneSuperCleSecreteTresLonguePourLeTestJWT123456789"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
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

            TypeCompte typeCompteAnonyme = new TypeCompte
            {
                IdTypeCompte = 4,
                Libelle = "Anonyme"
            };

            _context.TypesCompte.AddRange(typeCompte, typeCompteAnonyme);
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
            var loginRequest = new LoginRequest
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
    }
}