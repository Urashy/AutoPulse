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

            // Créer une annonce pour les tests
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
            _context.Annonces.Add(annonce);
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _objetcommun = notification;
            _compteTest = compte;
            _annonceTest = annonce;
        }

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

        [TestMethod]
        public async Task PostNotificationTest_Entity()
        {
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

            var actionResult = await _controller.Post(notification);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdNotification = (Notification)created.Value;
            Assert.AreEqual(notification.Titre, createdNotification.Titre);
            Assert.AreEqual(notification.Message, createdNotification.Message);
        }

        [TestMethod]
        public async Task PostNotificationWithAnnonceTest()
        {
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

            var actionResult = await _controller.Post(notification);

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;

            var createdNotification = (Notification)created.Value;
            Assert.AreEqual(notification.Titre, createdNotification.Titre);
            Assert.AreEqual(_annonceTest.IdAnnonce, createdNotification.IdAnnonce);
        }

        [TestMethod]
        public async Task DeleteNotificationTest()
        {
            var result = await _controller.Delete(_objetcommun.IdNotification);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var deletedNotification = await _manager.GetByIdAsync(_objetcommun.IdNotification);
            Assert.IsNull(deletedNotification);
        }

        [TestMethod]
        public async Task NotFoundDeleteNotificationTest()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutNotificationTest()
        {
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

            var result = await _controller.Put(_objetcommun.IdNotification, notification);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var notificationPut = await _manager.GetByIdAsync(_objetcommun.IdNotification);
            Assert.AreEqual(notification.Titre, notificationPut.Titre);
            Assert.AreEqual(notification.Message, notificationPut.Message);
        }

        [TestMethod]
        public async Task NotFoundPutNotificationTest()
        {
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

            var result = await _controller.Put(0, notification);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutNotificationTest()
        {
            NotificationUpdateDTO notification = new NotificationUpdateDTO()
            {
                IdNotification = _objetcommun.IdNotification,
                Titre = null, // Valeur invalide
                Message = "Test message",
                Type = "info",
                IdCompte = 1,
                IdAnnonce = null,
                UrlNavigation = null,
                AncienPrix = null,
                NouveauPrix = null
            };

            // Forcer l'erreur de validation dans le test
            _controller.ModelState.AddModelError("Titre", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdNotification, notification);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task BadRequestPostNotificationTest()
        {
            NotificationCreateDTO notification = new NotificationCreateDTO
            {
                Titre = null, // Valeur invalide
            };

            _controller.ModelState.AddModelError("Titre", "Required");

            var actionResult = await _controller.Post(notification);

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }

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
            // Marquer toutes les notifications comme lues
            await _manager.MarkAllAsReadAsync(_objetcommun.IdCompte);

            // Act
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
            // Arrange - Ajouter une autre notification non lue
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
        public async Task DeleteOldNotificationTest()
        {
            // Arrange - Créer une ancienne notification lue
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
            // Arrange - Créer une ancienne notification NON lue (doit être conservée)
            var oldUnreadNotification = new Notification
            {
                Titre = "Ancienne notification non lue",
                Message = "Message ancien non lu",
                Type = "info",
                DateCreation = DateTime.UtcNow.AddDays(-40),
                EstLue = false, // Non lue
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

            // La notification non lue doit toujours exister
            var notification = await _manager.GetByIdAsync(oldId);
            Assert.IsNotNull(notification);
            Assert.IsFalse(notification.EstLue);
        }
    }
}