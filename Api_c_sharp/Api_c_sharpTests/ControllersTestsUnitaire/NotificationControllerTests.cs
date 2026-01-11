using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class NotificationControllerTests
    {
        private NotificationController _controller;
        private AutoPulseBdContext _context;
        private NotificationManager _manager;
        private CompteManager _compteManager;
        private IMapper _mapper;
        private Notification _objetcommun;
        private Compte _compteTest;
        private Compte _compteTest2;
        private Annonce _annonceTest;

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

            _manager = new NotificationManager(_context);
            _compteManager = new CompteManager(_context);
            _controller = new NotificationController(_manager, _mapper, _compteManager);

            _context.Notifications.RemoveRange(_context.Notifications);
            await _context.SaveChangesAsync();

            var typeCompte = new TypeCompte
            {
                Libelle = "Utilisateur"
            };

            var compte = new Compte
            {
                Pseudo = "testuser",
                MotDePasse = "Password123!",
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@gmail.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = new DateTime(2000, 1, 1),
                IdTypeCompte = 1
            };

            var compte2 = new Compte
            {
                Pseudo = "testuser2",
                MotDePasse = "Password123!",
                Nom = "Martin",
                Prenom = "Marie",
                Email = "test2@gmail.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = new DateTime(1995, 5, 15),
                IdTypeCompte = 1
            };

            var annonce = new Annonce
            {
                Libelle = "Voiture de test",
                Description = "Description test",
                Prix = 15000,
                DatePublication = DateTime.Now,
                IdCompte = 1
            };

            var notification = new Notification
            {
                Titre = "Nouvelle annonce",
                Message = "Une nouvelle annonce correspond à vos critères",
                Type = "info",
                DateCreation = DateTime.Now,
                EstLue = false,
                IdCompte = 1,
                IdAnnonce = null
            };

            _context.TypesCompte.Add(typeCompte);
            _context.Comptes.Add(compte);
            _context.Comptes.Add(compte2);
            _context.Annonces.Add(annonce);
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _objetcommun = notification;
            _compteTest = compte;
            _compteTest2 = compte2;
            _annonceTest = annonce;
        }

        #region GET

            #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetByID(_objetcommun.IdNotification);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(NotificationDTO));
            Assert.AreEqual(_objetcommun.Titre, result.Value.Titre);
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
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<NotificationDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(n => n.Titre == _objetcommun.Titre));
        }
        #endregion

            #region GetByCompte
        [TestMethod]
        public async Task GetNotificationByCompteIDTest()
        {
            // Act
            var result = await _controller.GetNotificationByCompteID(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<NotificationDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(n => n.Titre == _objetcommun.Titre));
        }

        [TestMethod]
        public async Task NotFoundGetNotificationByCompteIDTest()
        {
            // Act
            var result = await _controller.GetNotificationByCompteID(999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetUnreadNotificationByCompteTest()
        {
            // Act
            var result = await _controller.GetUnreadNotificationByCompte(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<NotificationDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.All(n => !n.EstLue));
        }

        [TestMethod]
        public async Task NotFoundGetUnreadNotificationByCompteTest()
        {
            // Act
            await _manager.MarkAllAsReadAsync(_objetcommun.IdCompte);

            var result = await _controller.GetUnreadNotificationByCompte(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetUnreadCountByCompteTest()
        {
            // Act
            var result = await _controller.GetUnreadCountByCompte(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value > 0);
        }

        [TestMethod]
        public async Task GetUnreadCountByCompteWithNoUnreadTest()
        {
            // Act
            await _manager.MarkAllAsReadAsync(_compteTest.IdCompte);

            var result = await _controller.GetUnreadCountByCompte(_compteTest.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Value);
        }
        #endregion

        #endregion

        #region POST
        [TestMethod]
        public async Task PostNotificationTest_Entity()
        {
            // Arrange
            NotificationCreateDTO notification = new NotificationCreateDTO()
            {
                Titre = "Test notification",
                Message = "Message de test",
                Type = "info",
                IdCompte = 1,
                IdAnnonce = 1,
                UrlNavigation = null,
                AncienPrix = null,
                NouveauPrix = null
            };

            // Act
            var actionResult = await _controller.Post(notification);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdNotification = (Notification)created.Value;
            Assert.AreEqual(notification.Titre, createdNotification.Titre);
            Assert.AreEqual(notification.Message, createdNotification.Message);
        }

        [TestMethod]
        public async Task PostNotificationWithAnnonceTest()
        {
            // Arrange
            NotificationCreateDTO notification = new NotificationCreateDTO()
            {
                Titre = "Baisse de prix",
                Message = "Le prix a baissé !",
                Type = "pricedrop",
                IdCompte = 1,
                IdAnnonce = _annonceTest.IdAnnonce,
                AncienPrix = 15000,
                NouveauPrix = 12000,
                UrlNavigation = $"/annonce/{_annonceTest.IdAnnonce}"
            };

            // Act
            var actionResult = await _controller.Post(notification);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdNotification = (Notification)created.Value;
            Assert.AreEqual(notification.Titre, createdNotification.Titre);
            Assert.AreEqual(_annonceTest.IdAnnonce, createdNotification.IdAnnonce);
        }

        [TestMethod]
        public async Task BadRequestPostNotificationTest()
        {
            // Arrange
            NotificationCreateDTO notification = new NotificationCreateDTO
            {
                Titre = null,
            };

            _controller.ModelState.AddModelError("Titre", "Required");

            // Act
            var actionResult = await _controller.Post(notification);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteNotificationTest()
        {
            // Act
            var result = await _controller.Delete(_objetcommun.IdNotification);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedNotification = await _manager.GetByIdAsync(_objetcommun.IdNotification);
            Assert.IsNull(deletedNotification);
        }

        [TestMethod]
        public async Task NotFoundDeleteNotificationTest()
        {
            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutNotificationTest()
        {
            // Arrange
            NotificationUpdateDTO notification = new NotificationUpdateDTO()
            {
                IdNotification = _objetcommun.IdNotification,
                Titre = "Titre modifié",
                Message = "Message modifié",
                Type = "success",
                IdCompte = 1,
                IdAnnonce = 1,
                UrlNavigation = null,
                AncienPrix = null,
                NouveauPrix = null
            };

            // Act
            var result = await _controller.Put(_objetcommun.IdNotification, notification);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var notificationPut = await _manager.GetByIdAsync(_objetcommun.IdNotification);
            Assert.AreEqual(notification.Titre, notificationPut.Titre);
            Assert.AreEqual(notification.Message, notificationPut.Message);
        }

        [TestMethod]
        public async Task NotFoundPutNotificationTest()
        {
            // Arrange
            NotificationUpdateDTO notification = new NotificationUpdateDTO()
            {
                IdNotification = 1,
                Titre = "Test",
                Message = "Test message",
                Type = "info",
                IdCompte = 1,
                IdAnnonce = null,
                UrlNavigation = null,
                AncienPrix = null,
                NouveauPrix = null
            };

            // Act
            var result = await _controller.Put(0, notification);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutNotificationTest()
        {
            // Arrange
            NotificationUpdateDTO notification = new NotificationUpdateDTO()
            {
                IdNotification = _objetcommun.IdNotification,
                Titre = null,
                Message = "Test message",
                Type = "info",
                IdCompte = 1,
                IdAnnonce = null,
                UrlNavigation = null,
                AncienPrix = null,
                NouveauPrix = null
            };

            _controller.ModelState.AddModelError("Titre", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdNotification, notification);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

        #region MarkAsRead

        [TestMethod]
        public async Task MarkAsReadTest()
        {
            // Act
            var result = await _controller.MarkAsRead(_objetcommun.IdNotification);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var notification = await _manager.GetByIdAsync(_objetcommun.IdNotification);
            Assert.IsTrue(notification.EstLue);
        }

        [TestMethod]
        public async Task NotFoundMarkAsReadTest()
        {
            // Act
            var result = await _controller.MarkAsRead(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task MarkAllAsReadTest()
        {
            // Arrange
            var notification2 = new Notification
            {
                Titre = "Deuxième notification",
                Message = "Message 2",
                Type = "info",
                DateCreation = DateTime.Now,
                EstLue = false,
                IdCompte = _compteTest.IdCompte,
                IdAnnonce = null
            };
            _context.Notifications.Add(notification2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.MarkAllAsRead(_compteTest.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var notifications = await _manager.GetNotificationsByCompteAsync(_compteTest.IdCompte);
            Assert.IsTrue(notifications.All(n => n.EstLue));
        }

        [TestMethod]
        public async Task NotFoundMarkAllAsReadTest()
        {
            // Act
            var result = await _controller.MarkAllAsRead(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task MarkAllAsReadOnlyAffectsTargetCompteTest()
        {
            // Arrange
            var notification2 = new Notification
            {
                Titre = "Notification compte 2",
                Message = "Message compte 2",
                Type = "info",
                DateCreation = DateTime.Now,
                EstLue = false,
                IdCompte = _compteTest2.IdCompte,
                IdAnnonce = null
            };

            // Act
            _context.Notifications.Add(notification2);
            await _context.SaveChangesAsync();

            await _controller.MarkAllAsRead(_compteTest.IdCompte);

            var notifCompte1 = await _manager.GetNotificationsByCompteAsync(_compteTest.IdCompte);
            var notifCompte2 = await _manager.GetNotificationsByCompteAsync(_compteTest2.IdCompte);

            // Assert
            Assert.IsTrue(notifCompte1.All(n => n.EstLue));
            Assert.IsTrue(notifCompte2.All(n => !n.EstLue));
        }

        #endregion

        #region DELETE Old

        [TestMethod]
        public async Task DeleteOldNotificationTest()
        {
            // Arrange
            var oldNotification = new Notification
            {
                Titre = "Ancienne notification",
                Message = "Message ancien",
                Type = "info",
                DateCreation = DateTime.UtcNow.AddDays(-40),
                EstLue = true,
                IdCompte = _compteTest.IdCompte,
                IdAnnonce = null
            };
            _context.Notifications.Add(oldNotification);
            await _context.SaveChangesAsync();

            var oldId = oldNotification.IdNotification;

            // Act
            var result = await _controller.DeleteOldNotification(30);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var notification = await _manager.GetByIdAsync(oldId);
            Assert.IsNull(notification);
        }

        [TestMethod]
        public async Task DeleteOldNotificationKeepsUnreadTest()
        {
            // Arrange
            var oldUnreadNotification = new Notification
            {
                Titre = "Ancienne notification non lue",
                Message = "Message ancien non lu",
                Type = "info",
                DateCreation = DateTime.UtcNow.AddDays(-40),
                EstLue = false,
                IdCompte = _compteTest.IdCompte,
                IdAnnonce = null
            };
            _context.Notifications.Add(oldUnreadNotification);
            await _context.SaveChangesAsync();

            var oldId = oldUnreadNotification.IdNotification;

            // Act
            var result = await _controller.DeleteOldNotification(30);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var notification = await _manager.GetByIdAsync(oldId);
            Assert.IsNotNull(notification);
            Assert.IsFalse(notification.EstLue);
        }

        [TestMethod]
        public async Task DeleteOldNotificationKeepsRecentTest()
        {
            // Arrange
            var recentNotification = new Notification
            {
                Titre = "Notification récente",
                Message = "Message récent",
                Type = "info",
                DateCreation = DateTime.UtcNow.AddDays(-10),
                EstLue = true,
                IdCompte = _compteTest.IdCompte,
                IdAnnonce = null
            };
            _context.Notifications.Add(recentNotification);
            await _context.SaveChangesAsync();

            var recentId = recentNotification.IdNotification;

            // Act
            await _controller.DeleteOldNotification(30);

            // Assert
            var notification = await _manager.GetByIdAsync(recentId);
            Assert.IsNotNull(notification);
        }

        #endregion

        #region NotifCreate

        [TestMethod]
        public async Task NotifCreationAutoAsyncTest()
        {
            // Arrange
            var idComptes = new List<int> { _compteTest.IdCompte, _compteTest2.IdCompte };
            string url = "/annonce/1";
            string titre = "Test notification auto";
            string message = "Message de test automatique";
            string type = "info";
            int idAnnonce = _annonceTest.IdAnnonce;

            // Act
            await _manager.NotifCreationAutoAsync(idComptes, url, titre, message, idAnnonce, type);

            // Assert
            var notifications = await _context.Notifications.Where(n => n.Titre == titre).ToListAsync();
            Assert.AreEqual(2, notifications.Count);
            Assert.IsTrue(notifications.All(n => n.Message == message));
            Assert.IsTrue(notifications.All(n => n.Type == type));
            Assert.IsTrue(notifications.All(n => !n.EstLue));
            Assert.IsTrue(notifications.Any(n => n.IdCompte == _compteTest.IdCompte));
            Assert.IsTrue(notifications.Any(n => n.IdCompte == _compteTest2.IdCompte));
        }

        [TestMethod]
        public async Task NotifCreationAutoAsyncWithPriceTest()
        {
            // Arrange
            var idComptes = new List<int> { _compteTest.IdCompte };
            string url = "/annonce/1";
            string titre = "Baisse de prix";
            string message = "Le prix a baissé";
            string type = "pricedrop";
            int idAnnonce = _annonceTest.IdAnnonce;
            double ancienPrix = 15000;
            double nouveauPrix = 12000;

            // Act
            await _manager.NotifCreationAutoAsync(idComptes, url, titre, message, idAnnonce, type, ancienPrix, nouveauPrix);

            // Assert
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Titre == titre);
            Assert.IsNotNull(notification);
            Assert.AreEqual(ancienPrix, notification.AncienPrix);
            Assert.AreEqual(nouveauPrix, notification.NouveauPrix);
            Assert.AreEqual(url, notification.UrlNavigation);
        }
        #endregion

        #region NotifAnnonce
        [TestMethod]
        public async Task NotifAnnonceTest()
        {
            // Arrange
            var favori1 = new Favori
            {
                IdCompte = _compteTest.IdCompte,
                IdAnnonce = _annonceTest.IdAnnonce,
            };

            var favori2 = new Favori
            {
                IdCompte = _compteTest2.IdCompte,
                IdAnnonce = _annonceTest.IdAnnonce,
            };

            _context.Favoris.Add(favori1);
            _context.Favoris.Add(favori2);
            await _context.SaveChangesAsync();

            double ancienPrix = 15000;
            double nouveauPrix = 12000;

            // Act
            await _manager.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix);

            var notifications = await _context.Notifications
                .Where(n => n.Type == "pricedrop" && n.IdAnnonce == _annonceTest.IdAnnonce)
                .ToListAsync();

            // Assert
            Assert.AreEqual(2, notifications.Count);
            Assert.IsTrue(notifications.All(n => n.Titre == "Mise à jour de l'annonce"));
            Assert.IsTrue(notifications.All(n => n.Message.Contains(_annonceTest.Libelle)));
            Assert.IsTrue(notifications.All(n => n.Message.Contains(ancienPrix.ToString())));
            Assert.IsTrue(notifications.All(n => n.Message.Contains(nouveauPrix.ToString())));
            Assert.IsTrue(notifications.All(n => n.AncienPrix == ancienPrix));
            Assert.IsTrue(notifications.All(n => n.NouveauPrix == nouveauPrix));
            Assert.IsTrue(notifications.All(n => n.UrlNavigation == $"/annonce/{_annonceTest.IdAnnonce}"));
            Assert.IsTrue(notifications.Any(n => n.IdCompte == _compteTest.IdCompte));
            Assert.IsTrue(notifications.Any(n => n.IdCompte == _compteTest2.IdCompte));
        }

        [TestMethod]
        public async Task NotifAnnonceWithNoFavorisTest()
        {
            // Arrange
            double ancienPrix = 15000;
            double nouveauPrix = 12000;

            // Act
            await _manager.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix);

            var notifications = await _context.Notifications
                .Where(n => n.Type == "pricedrop" && n.IdAnnonce == _annonceTest.IdAnnonce)
                .ToListAsync();

            // Assert
            Assert.AreEqual(0, notifications.Count);
        }

        [TestMethod]
        public async Task NotifSuppressionAnnonceTest()
        {
            // Arrange
            var signalement = new Signalement
            {
                IdAnnonceSignale = _annonceTest.IdAnnonce,
                IdCompteSignalant = _compteTest2.IdCompte,
                DescriptionSignalement = "Contenu inapproprié",
                DateCreationSignalement = DateTime.Now,
            };

            _context.Signalements.Add(signalement);
            await _context.SaveChangesAsync();

            // Act
            await _manager.NotifSuppressionAnnonce(_annonceTest.IdAnnonce);

            var notifications = await _context.Notifications
                .Where(n => n.Type == "error" && n.IdAnnonce == _annonceTest.IdAnnonce)
                .ToListAsync();

            // Assert
            Assert.AreEqual(1, notifications.Count);
            var notification = notifications.First();
            Assert.AreEqual("Annonce supprimée", notification.Titre);
            Assert.IsTrue(notification.Message.Contains(_annonceTest.Libelle));
            Assert.IsTrue(notification.Message.Contains(signalement.DescriptionSignalement));
            Assert.AreEqual("/annonces", notification.UrlNavigation);
            Assert.AreEqual(_compteTest.IdCompte, notification.IdCompte);
            Assert.IsFalse(notification.EstLue);
        }

        [TestMethod]
        public async Task NotifSuppressionAnnonceWithoutSignalementTest()
        {
            // Arrange
            var nouvelleAnnonce = new Annonce
            {
                Libelle = "Annonce sans signalement",
                Description = "Description",
                Prix = 10000,
                DatePublication = DateTime.Now,
                IdCompte = _compteTest.IdCompte
            };

            _context.Annonces.Add(nouvelleAnnonce);
            await _context.SaveChangesAsync();

            // Act
            await _manager.NotifSuppressionAnnonce(nouvelleAnnonce.IdAnnonce);

            var notifications = await _context.Notifications
                .Where(n => n.Type == "error" && n.IdAnnonce == nouvelleAnnonce.IdAnnonce)
                .ToListAsync();

            // Assert
            Assert.AreEqual(1, notifications.Count);
            var notification = notifications.First();
            Assert.AreEqual("Annonce supprimée", notification.Titre);
            Assert.IsTrue(notification.Message.Contains(nouvelleAnnonce.Libelle));
        }

        [TestMethod]
        public async Task NotifAnnonceMultipleFavorisTest()
        {
            // Arrange
            var compte3 = new Compte
            {
                Pseudo = "testuser3",
                MotDePasse = "Password123!",
                Nom = "Durand",
                Prenom = "Pierre",
                Email = "test3@gmail.com",
                DateCreation = DateTime.Now,
                DateDerniereConnexion = DateTime.Now,
                DateNaissance = new DateTime(1990, 3, 20),
                IdTypeCompte = 1
            };

            _context.Comptes.Add(compte3);
            await _context.SaveChangesAsync();

            var favoris = new List<Favori>
            {
                new Favori { IdCompte = _compteTest.IdCompte, IdAnnonce = _annonceTest.IdAnnonce },
                new Favori { IdCompte = _compteTest2.IdCompte, IdAnnonce = _annonceTest.IdAnnonce },
                new Favori { IdCompte = compte3.IdCompte, IdAnnonce = _annonceTest.IdAnnonce }
            };

            
            _context.Favoris.AddRange(favoris);
            await _context.SaveChangesAsync();

            double ancienPrix = 15000;
            double nouveauPrix = 11000;

            // Act
            await _manager.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix);

            var notifications = await _context.Notifications
                .Where(n => n.Type == "pricedrop" && n.IdAnnonce == _annonceTest.IdAnnonce)
                .ToListAsync();

            // Assert
            Assert.AreEqual(3, notifications.Count);
            Assert.IsTrue(notifications.All(n => !n.EstLue));
            Assert.IsTrue(notifications.Any(n => n.IdCompte == _compteTest.IdCompte));
            Assert.IsTrue(notifications.Any(n => n.IdCompte == _compteTest2.IdCompte));
            Assert.IsTrue(notifications.Any(n => n.IdCompte == compte3.IdCompte));
        }

        #endregion

        // Ajoutez cette région à la fin de la classe NotificationControllerTests (fichier d'intégration)
        // Juste avant la fermeture #endregion NotifAnnonce

        #region NotifPaiementMiseEnAvant

        [TestMethod]
        public async Task NotifPaiementMiseEnAvantTest()
        {
            // Arrange
            var miseEnAvant = new MiseEnAvant
            {
                LibelleMiseEnAvant = "Premium",
                PrixSemaine = (decimal)9.99,
            };

            _context.MisesEnAvant.Add(miseEnAvant);
            await _context.SaveChangesAsync();

            // Act
            await _manager.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                miseEnAvant.IdMiseEnAvant
            );

            var notifications = await _context.Notifications
                .Where(n => n.Type == "paiement" && n.IdAnnonce == _annonceTest.IdAnnonce)
                .ToListAsync();

            // Assert
            Assert.AreEqual(1, notifications.Count);
            var notification = notifications.First();
            Assert.AreEqual("Paiemement", notification.Titre);
            Assert.IsTrue(notification.Message.Contains(miseEnAvant.LibelleMiseEnAvant));
            Assert.IsTrue(notification.Message.Contains(miseEnAvant.PrixSemaine.ToString()));
            Assert.IsTrue(notification.Message.Contains(_annonceTest.Libelle));
            Assert.AreEqual($"/annonce/{_annonceTest.IdAnnonce}", notification.UrlNavigation);
            Assert.AreEqual(_compteTest.IdCompte, notification.IdCompte);
            Assert.IsFalse(notification.EstLue);
        }

        [TestMethod]
        public async Task NotifPaiementMiseEnAvantWithDifferentGradesTest()
        {
            // Arrange
            var miseEnAvantBasic = new MiseEnAvant
            {
                LibelleMiseEnAvant = "Basic",
                PrixSemaine = (decimal)4.99,
            };

            var miseEnAvantPremium = new MiseEnAvant
            {
                LibelleMiseEnAvant = "Premium",
                PrixSemaine = (decimal)9.99,
            };

            _context.MisesEnAvant.AddRange(miseEnAvantBasic, miseEnAvantPremium);
            await _context.SaveChangesAsync();

            // Act - Premier paiement avec Basic
            await _manager.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                miseEnAvantBasic.IdMiseEnAvant
            );

            // Act - Second paiement avec Premium
            await _manager.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                miseEnAvantPremium.IdMiseEnAvant
            );

            var notifications = await _context.Notifications
                .Where(n => n.Type == "paiement" && n.IdAnnonce == _annonceTest.IdAnnonce)
                .OrderBy(n => n.DateCreation)
                .ToListAsync();

            // Assert
            Assert.AreEqual(2, notifications.Count);

            Assert.IsTrue(notifications[0].Message.Contains("Basic"));
            Assert.IsTrue(notifications[0].Message.Contains("4,99"));

            Assert.IsTrue(notifications[1].Message.Contains("Premium"));
            Assert.IsTrue(notifications[1].Message.Contains("9,99"));
        }

        [TestMethod]
        public async Task NotifPaiementMiseEnAvantMessageContentTest()
        {
            // Arrange
            var miseEnAvant = new MiseEnAvant
            {
                LibelleMiseEnAvant = "Gold",
                PrixSemaine = (decimal)14.99,
            };

            _context.MisesEnAvant.Add(miseEnAvant);
            await _context.SaveChangesAsync();

            // Act
            await _manager.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                miseEnAvant.IdMiseEnAvant
            );

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Type == "paiement" && n.IdAnnonce == _annonceTest.IdAnnonce);

            // Assert
            Assert.IsNotNull(notification);
            Assert.IsTrue(notification.Message.Contains("Votre mise en avant"));
            Assert.IsTrue(notification.Message.Contains("renouvellé"));
            Assert.IsTrue(notification.Message.Contains("changement de grade"));
            Assert.IsTrue(notification.Message.Contains("modification d'annonce"));
        }

        [TestMethod]
        public async Task NotifPaiementMiseEnAvantMultipleAnnoncesTest()
        {
            // Arrange
            var annonce2 = new Annonce
            {
                Libelle = "Deuxième voiture",
                Description = "Description 2",
                Prix = 20000,
                DatePublication = DateTime.Now,
                IdCompte = _compteTest.IdCompte
            };

            var miseEnAvant = new MiseEnAvant
            {
                LibelleMiseEnAvant = "Premium",
                PrixSemaine = (decimal)9.99,
            };

            _context.Annonces.Add(annonce2);
            _context.MisesEnAvant.Add(miseEnAvant);
            await _context.SaveChangesAsync();

            // Act - Paiement pour annonce 1
            await _manager.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                miseEnAvant.IdMiseEnAvant
            );

            // Act - Paiement pour annonce 2
            await _manager.NotifPaiementMiseEnAvant(
                annonce2.IdAnnonce,
                _compteTest.IdCompte,
                miseEnAvant.IdMiseEnAvant
            );

            var notificationsAnnonce1 = await _context.Notifications
                .Where(n => n.Type == "paiement" && n.IdAnnonce == _annonceTest.IdAnnonce)
                .ToListAsync();

            var notificationsAnnonce2 = await _context.Notifications
                .Where(n => n.Type == "paiement" && n.IdAnnonce == annonce2.IdAnnonce)
                .ToListAsync();

            // Assert
            Assert.AreEqual(1, notificationsAnnonce1.Count);
            Assert.AreEqual(1, notificationsAnnonce2.Count);

            Assert.IsTrue(notificationsAnnonce1[0].Message.Contains(_annonceTest.Libelle));
            Assert.IsTrue(notificationsAnnonce2[0].Message.Contains(annonce2.Libelle));

            Assert.AreEqual($"/annonce/{_annonceTest.IdAnnonce}", notificationsAnnonce1[0].UrlNavigation);
            Assert.AreEqual($"/annonce/{annonce2.IdAnnonce}", notificationsAnnonce2[0].UrlNavigation);
        }

        [TestMethod]
        public async Task NotifPaiementMiseEnAvantNotificationPropertiesTest()
        {
            // Arrange
            var miseEnAvant = new MiseEnAvant
            {
                LibelleMiseEnAvant = "Standard",
                PrixSemaine = (decimal)6.99,
            };

            _context.MisesEnAvant.Add(miseEnAvant);
            await _context.SaveChangesAsync();

            var beforeCreation = DateTime.UtcNow;

            // Act
            await _manager.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                miseEnAvant.IdMiseEnAvant
            );

            var afterCreation = DateTime.UtcNow;

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Type == "paiement" && n.IdAnnonce == _annonceTest.IdAnnonce);

            // Assert - Vérifier toutes les propriétés de la notification
            Assert.IsNotNull(notification);
            Assert.AreEqual(_compteTest.IdCompte, notification.IdCompte);
            Assert.AreEqual(_annonceTest.IdAnnonce, notification.IdAnnonce);
            Assert.AreEqual("paiement", notification.Type);
            Assert.AreEqual("Paiemement", notification.Titre);
            Assert.IsFalse(notification.EstLue);
            Assert.IsTrue(notification.DateCreation >= beforeCreation && notification.DateCreation <= afterCreation);
            Assert.IsNull(notification.AncienPrix);
            Assert.IsNull(notification.NouveauPrix);
        }

        #endregion
    }
}