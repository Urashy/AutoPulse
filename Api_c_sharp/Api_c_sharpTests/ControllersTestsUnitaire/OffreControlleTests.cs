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

            _controller = new OffreController(_manager, _mapper,_messageManager,_commandeManager,_annonceManager,_notificationService);

            _context.Offres.RemoveRange(_context.Offres);
            await _context.SaveChangesAsync();

            TypeCompte typecompte = new TypeCompte()
            {
                IdTypeCompte = 1,
                Libelle = "Particulier"
            };

            // Vendeur
            Compte compte1 = new Compte()
            {
                IdCompte = 1,
                Email = "john@gmail.com",
                MotDePasse = "Password123!",
                Nom = "Doe",
                Prenom = "John",
                DateNaissance = new DateTime(1990, 1, 1),
                IdTypeCompte = typecompte.IdTypeCompte,
                Pseudo = "john_doe",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now
            };

            // Acheteur
            Compte compte2 = new Compte()
            {
                IdCompte = 2,
                Email = "jane@gmail.com",
                MotDePasse = "Password123!",
                Nom = "Smith",
                Prenom = "Jane",
                DateNaissance = new DateTime(1992, 5, 15),
                IdTypeCompte = typecompte.IdTypeCompte,
                Pseudo = "jane_smith",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now
            };

            Annonce annonce = new Annonce()
            {
                Libelle = "Annonce de test",
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdAdresse = 1,
                IdVoiture = 1,
                Prix = 10000,
                Description = "Description de test"
            };

            Conversation conversation = new Conversation()
            {
                IdConversation = 1,
                IdAnnonce = 1
            };

            Message message1 = new Message()
            {
                IdMessage = 1,
                ContenuMessage = "Bonjour, je suis intéressé par votre annonce.",
                DateEnvoiMessage = DateTime.Now,
                IdConversation = 1,
                IdCompte = 1,
                EstLu = false,
            };

            Offre offre = new Offre()
            {
                Valeur = 9500,
                DateOffre = DateTime.Now,
                IdMessage = 1,
            };
            _context.TypesCompte.Add(typecompte);
            _context.Comptes.AddRange(compte1, compte2);
            _context.Annonces.Add(annonce);
            _context.Conversations.Add(conversation);
            _context.Messages.Add(message1);
            _context.Offres.Add(offre);
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
        public async Task GetByMessage()
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
        public async Task NotFoundGetByMessage()
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
            // Arrange
            OffreCreateDTO Offre = new OffreCreateDTO
            {
                Valeur = 9000,
                IdAnnonce = 1,
                IdMessage = _objetcommun.IdMessage,
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
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdOffre, offre);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var offreput = await _manager.GetByIdAsync(_objetcommun.IdOffre);
            Assert.AreEqual(offre.Valeur, offreput.Valeur);
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