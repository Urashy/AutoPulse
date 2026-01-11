using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoMapper;
using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    public class NotificationControllerTests
    {
        private NotificationController _controller;
        private Mock<NotificationManager> _mockManager;
        private Mock<CompteManager> _mockCompteManager;
        private Mock<IMapper> _mockMapper;
        private Notification _objetcommun;
        private Compte _compteTest;
        private Compte _compteTest2;
        private Annonce _annonceTest;

        [TestInitialize]
        public void Initialize()
        {
            _mockManager = new Mock<NotificationManager>(null);
            _mockCompteManager = new Mock<CompteManager>(null);
            _mockMapper = new Mock<IMapper>();

            _controller = new NotificationController(
                _mockManager.Object,
                _mockMapper.Object,
                _mockCompteManager.Object
            );

            _compteTest = new Compte
            {
                IdCompte = 1,
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

            _compteTest2 = new Compte
            {
                IdCompte = 2,
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

            _annonceTest = new Annonce
            {
                IdAnnonce = 1,
                Libelle = "Voiture de test",
                Description = "Description test",
                Prix = 15000,
                DatePublication = DateTime.Now,
                IdCompte = 1
            };

            _objetcommun = new Notification
            {
                IdNotification = 1,
                Titre = "Nouvelle annonce",
                Message = "Une nouvelle annonce correspond à vos critères",
                Type = "info",
                DateCreation = DateTime.Now,
                EstLue = false,
                IdCompte = 1,
                IdAnnonce = null
            };
        }

        #region Tests CRUD de base
            #region GET
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            var notificationDto = new NotificationDTO
            {
                IdNotification = _objetcommun.IdNotification,
                Titre = _objetcommun.Titre,
                Message = _objetcommun.Message
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdNotification))
                .ReturnsAsync(_objetcommun);
            _mockMapper.Setup(m => m.Map<NotificationDTO>(_objetcommun))
                .Returns(notificationDto);

            // Act
            var result = await _controller.GetByID(_objetcommun.IdNotification);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(NotificationDTO));
            Assert.AreEqual(_objetcommun.Titre, result.Value.Titre);
            _mockManager.Verify(m => m.GetByIdAsync(_objetcommun.IdNotification), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                .ReturnsAsync((Notification)null);

            // Act
            var result = await _controller.GetByID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockManager.Verify(m => m.GetByIdAsync(0), Times.Once);
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange
            var notifications = new List<Notification> { _objetcommun };
            var notificationsDto = new List<NotificationDTO>
            {
                new NotificationDTO
                {
                    IdNotification = _objetcommun.IdNotification,
                    Titre = _objetcommun.Titre
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                .ReturnsAsync(notifications);
            _mockMapper.Setup(m => m.Map<IEnumerable<NotificationDTO>>(notifications))
                .Returns(notificationsDto);

            // Act
            var result = await _controller.GetAll();

            // Arrange
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<NotificationDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(n => n.Titre == _objetcommun.Titre));
            _mockManager.Verify(m => m.GetAllAsync(), Times.Once);
        }
        #endregion

            #region POST
        [TestMethod]
        public async Task PostNotificationTest_Entity()
        {
            // Arrange
            var notificationDto = new NotificationCreateDTO
            {
                Titre = "Test notification",
                Message = "Message de test",
                Type = "info",
                IdCompte = 1,
                IdAnnonce = 1
            };

            var notification = new Notification
            {
                IdNotification = 2,
                Titre = notificationDto.Titre,
                Message = notificationDto.Message,
                Type = notificationDto.Type,
                IdCompte = notificationDto.IdCompte,
                IdAnnonce = notificationDto.IdAnnonce,
                DateCreation = DateTime.Now,
                EstLue = false
            };

            _mockMapper.Setup(m => m.Map<Notification>(notificationDto))
                .Returns(notification);
            _mockManager.Setup(m => m.AddAsync(It.IsAny<Notification>()))
                .ReturnsAsync(notification);

            // Act
            var actionResult = await _controller.Post(notificationDto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdNotification = (Notification)created.Value;
            Assert.AreEqual(notificationDto.Titre, createdNotification.Titre);
            Assert.AreEqual(notificationDto.Message, createdNotification.Message);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Notification>()), Times.Once);
        }

        [TestMethod]
        public async Task PostNotificationWithAnnonceTest()
        {
            // Arrange
            var notificationDto = new NotificationCreateDTO
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

            var notification = new Notification
            {
                IdNotification = 2,
                Titre = notificationDto.Titre,
                Message = notificationDto.Message,
                Type = notificationDto.Type,
                IdCompte = notificationDto.IdCompte,
                IdAnnonce = notificationDto.IdAnnonce,
                AncienPrix = notificationDto.AncienPrix,
                NouveauPrix = notificationDto.NouveauPrix,
                UrlNavigation = notificationDto.UrlNavigation,
                DateCreation = DateTime.Now,
                EstLue = false
            };

            _mockMapper.Setup(m => m.Map<Notification>(notificationDto))
                .Returns(notification);
            _mockManager.Setup(m => m.AddAsync(It.IsAny<Notification>()))
                .ReturnsAsync(notification);

            // Act
            var actionResult = await _controller.Post(notificationDto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdNotification = (Notification)created.Value;
            Assert.AreEqual(notificationDto.Titre, createdNotification.Titre);
            Assert.AreEqual(_annonceTest.IdAnnonce, createdNotification.IdAnnonce);
        }
        
        [TestMethod]
        public async Task BadRequestPostNotificationTest()
        {
            // Arrange
            var notificationDto = new NotificationCreateDTO
            {
                Titre = null
            };

            _controller.ModelState.AddModelError("Titre", "Required");

            // Act
            var actionResult = await _controller.Post(notificationDto);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Notification>()), Times.Never);

        }
        #endregion

            #region DELETE
        [TestMethod]
        public async Task DeleteNotificationTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdNotification))
                .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.DeleteAsync(_objetcommun))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(_objetcommun.IdNotification);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.GetByIdAsync(_objetcommun.IdNotification), Times.Once);
            _mockManager.Verify(m => m.DeleteAsync(_objetcommun), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundDeleteNotificationTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                .ReturnsAsync((Notification)null);

            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockManager.Verify(m => m.GetByIdAsync(0), Times.Once);
        }
        #endregion

            #region PUT
        [TestMethod]
        public async Task PutNotificationTest()
        {
            // Arrange
            var notificationDto = new NotificationUpdateDTO
            {
                IdNotification = _objetcommun.IdNotification,
                Titre = "Titre modifié",
                Message = "Message modifié",
                Type = "success",
                IdCompte = 1,
                IdAnnonce = 1
            };

            var notificationUpdated = new Notification
            {
                IdNotification = _objetcommun.IdNotification,
                Titre = notificationDto.Titre,
                Message = notificationDto.Message,
                Type = notificationDto.Type,
                IdCompte = notificationDto.IdCompte,
                IdAnnonce = notificationDto.IdAnnonce,
                DateCreation = _objetcommun.DateCreation,
                EstLue = _objetcommun.EstLue
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdNotification))
                .ReturnsAsync(_objetcommun);
            _mockMapper.Setup(m => m.Map<Notification>(notificationDto))
                .Returns(notificationUpdated);
            _mockManager.Setup(m => m.UpdateAsync(_objetcommun, notificationUpdated))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(_objetcommun.IdNotification, notificationDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(_objetcommun, notificationUpdated), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundPutNotificationTest()
        {
            // Arrange
            var notificationDto = new NotificationUpdateDTO
            {
                IdNotification = 1,
                Titre = "Test",
                Message = "Test message",
                Type = "info",
                IdCompte = 1
            };

            _mockManager.Setup(m => m.GetByIdAsync(0))
                .ReturnsAsync((Notification)null);

            // Act
            var result = await _controller.Put(0, notificationDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutNotificationTest()
        {
            // Arrange
            var notificationDto = new NotificationUpdateDTO
            {
                IdNotification = _objetcommun.IdNotification,
                Titre = null,
                Message = "Test message",
                Type = "info",
                IdCompte = 1
            };

            _controller.ModelState.AddModelError("Titre", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdNotification, notificationDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockManager.Verify(m => m.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Notification>(), It.IsAny<Notification>()), Times.Never);
        }
        #endregion

        #endregion

        #region Tests récupération par compte

        [TestMethod]
        public async Task GetNotificationByCompteIDTest()
        {
            // Arrange
            var notifications = new List<Notification> { _objetcommun };
            var notificationsDto = new List<NotificationDTO>
            {
                new NotificationDTO
                {
                    IdNotification = _objetcommun.IdNotification,
                    Titre = _objetcommun.Titre
                }
            };

            _mockManager.Setup(m => m.GetNotificationsByCompteAsync(_objetcommun.IdCompte))
                .ReturnsAsync(notifications);
            _mockMapper.Setup(m => m.Map<IEnumerable<NotificationDTO>>(notifications))
                .Returns(notificationsDto);

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
            // Arrange
            _mockManager.Setup(m => m.GetNotificationsByCompteAsync(999))
                .ReturnsAsync(new List<Notification>());

            // Act
            var result = await _controller.GetNotificationByCompteID(999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetUnreadNotificationByCompteTest()
        {
            // Arrange
            var notifications = new List<Notification> { _objetcommun };
            var notificationsDto = new List<NotificationDTO>
            {
                new NotificationDTO
                {
                    IdNotification = _objetcommun.IdNotification,
                    Titre = _objetcommun.Titre,
                    EstLue = false
                }
            };

            _mockManager.Setup(m => m.GetUnreadNotificationsByCompteAsync(_objetcommun.IdCompte))
                .ReturnsAsync(notifications);
            _mockMapper.Setup(m => m.Map<IEnumerable<NotificationDTO>>(notifications))
                .Returns(notificationsDto);

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
            // Arrange
            _mockManager.Setup(m => m.GetUnreadNotificationsByCompteAsync(_objetcommun.IdCompte))
                .ReturnsAsync(new List<Notification>());

            // Act
            var result = await _controller.GetUnreadNotificationByCompte(_objetcommun.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetUnreadCountByCompteTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetUnreadCountAsync(_objetcommun.IdCompte))
                .ReturnsAsync(1);

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
            // Arrange
            _mockManager.Setup(m => m.GetUnreadCountAsync(_compteTest.IdCompte))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.GetUnreadCountByCompte(_compteTest.IdCompte);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Value);
        }

        #endregion

        #region Tests marquage comme lu

        [TestMethod]
        public async Task MarkAsReadTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdNotification))
                .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.MarkAsReadAsync(_objetcommun.IdNotification))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.MarkAsRead(_objetcommun.IdNotification);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.MarkAsReadAsync(_objetcommun.IdNotification), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundMarkAsReadTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                .ReturnsAsync((Notification)null);

            // Act
            var result = await _controller.MarkAsRead(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task MarkAllAsReadTest()
        {
            // Arrange
            _mockCompteManager.Setup(m => m.GetByIdAsync(_compteTest.IdCompte))
                .ReturnsAsync(_compteTest);
            _mockManager.Setup(m => m.MarkAllAsReadAsync(_compteTest.IdCompte))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.MarkAllAsRead(_compteTest.IdCompte);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.MarkAllAsReadAsync(_compteTest.IdCompte), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundMarkAllAsReadTest()
        {
            // Arrange
            _mockCompteManager.Setup(m => m.GetByIdAsync(999))
                .ReturnsAsync((Compte)null);

            // Act
            var result = await _controller.MarkAllAsRead(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task MarkAllAsReadOnlyAffectsTargetCompteTest()
        {
            // Arrange
            var notifCompte1 = new List<Notification>
            {
                new Notification { IdNotification = 1, IdCompte = _compteTest.IdCompte, EstLue = true }
            };

            var notifCompte2 = new List<Notification>
            {
                new Notification { IdNotification = 2, IdCompte = _compteTest2.IdCompte, EstLue = false }
            };

            _mockCompteManager.Setup(m => m.GetByIdAsync(_compteTest.IdCompte))
                .ReturnsAsync(_compteTest);
            _mockManager.Setup(m => m.MarkAllAsReadAsync(_compteTest.IdCompte))
                .Returns(Task.CompletedTask);
            _mockManager.Setup(m => m.GetNotificationsByCompteAsync(_compteTest.IdCompte))
                .ReturnsAsync(notifCompte1);
            _mockManager.Setup(m => m.GetNotificationsByCompteAsync(_compteTest2.IdCompte))
                .ReturnsAsync(notifCompte2);

            await _controller.MarkAllAsRead(_compteTest.IdCompte);

            // Act
            var resultCompte1 = await _mockManager.Object.GetNotificationsByCompteAsync(_compteTest.IdCompte);
            var resultCompte2 = await _mockManager.Object.GetNotificationsByCompteAsync(_compteTest2.IdCompte);

            // Assert
            Assert.IsTrue(resultCompte1.All(n => n.EstLue));
            Assert.IsTrue(resultCompte2.All(n => !n.EstLue));
        }

        #endregion

        #region Tests suppression anciennes notifications

        [TestMethod]
        public async Task DeleteOldNotificationTest()
        {
            // Arrange
            _mockManager.Setup(m => m.DeleteOldNotificationsAsync(30))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteOldNotification(30);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteOldNotificationsAsync(30), Times.Once);
        }

        [TestMethod]
        public async Task DeleteOldNotificationKeepsUnreadTest()
        {
            // Arrange
            var oldUnreadNotification = new Notification
            {
                IdNotification = 5,
                Titre = "Ancienne notification non lue",
                Message = "Message ancien non lu",
                Type = "info",
                DateCreation = DateTime.UtcNow.AddDays(-40),
                EstLue = false,
                IdCompte = _compteTest.IdCompte
            };

            _mockManager.Setup(m => m.DeleteOldNotificationsAsync(30))
                .Returns(Task.CompletedTask);
            _mockManager.Setup(m => m.GetByIdAsync(oldUnreadNotification.IdNotification))
                .ReturnsAsync(oldUnreadNotification);

            // Act
            var result = await _controller.DeleteOldNotification(30);
            
            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var notification = await _mockManager.Object.GetByIdAsync(oldUnreadNotification.IdNotification);
            Assert.IsNotNull(notification);
            Assert.IsFalse(notification.EstLue);
        }

        [TestMethod]
        public async Task DeleteOldNotificationKeepsRecentTest()
        {
            // Arrange
            var recentNotification = new Notification
            {
                IdNotification = 6,
                Titre = "Notification récente",
                Message = "Message récent",
                Type = "info",
                DateCreation = DateTime.UtcNow.AddDays(-10),
                EstLue = true,
                IdCompte = _compteTest.IdCompte
            };

            _mockManager.Setup(m => m.DeleteOldNotificationsAsync(30))
                .Returns(Task.CompletedTask);
            _mockManager.Setup(m => m.GetByIdAsync(recentNotification.IdNotification))
                .ReturnsAsync(recentNotification);

            // Act
            await _controller.DeleteOldNotification(30);

            var notification = await _mockManager.Object.GetByIdAsync(recentNotification.IdNotification);
            
            // Assert
            Assert.IsNotNull(notification);
        }

        #endregion

        #region Tests méthodes de notification automatique

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

            _mockManager.Setup(m => m.NotifCreationAutoAsync(
                idComptes, url, titre, message, idAnnonce, type,0,0,"",0))
                .Returns(Task.CompletedTask);

            // Act
            await _mockManager.Object.NotifCreationAutoAsync(
                idComptes, url, titre, message, idAnnonce, type);

            // Assert
            _mockManager.Verify(m => m.NotifCreationAutoAsync(
                idComptes, url, titre, message, idAnnonce, type, 0, 0, "", 0), Times.Once);
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

            _mockManager.Setup(m => m.NotifCreationAutoAsync(
                idComptes, url, titre, message, idAnnonce, type, ancienPrix, nouveauPrix, "", 0))
                .Returns(Task.CompletedTask);

            // Act
            await _mockManager.Object.NotifCreationAutoAsync(
                idComptes, url, titre, message, idAnnonce, type, ancienPrix, nouveauPrix);

            // Assert
            _mockManager.Verify(m => m.NotifCreationAutoAsync(
                idComptes, url, titre, message, idAnnonce, type, ancienPrix, nouveauPrix, "", 0), Times.Once);
        }

        [TestMethod]
        public async Task NotifAnnonceTest()
        {
            // Arrange
            double ancienPrix = 15000;
            double nouveauPrix = 12000;

            _mockManager.Setup(m => m.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix))
                .Returns(Task.CompletedTask);

            // Act
            await _mockManager.Object.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix);

            // Assert
            _mockManager.Verify(m => m.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix), Times.Once);
        }

        [TestMethod]
        public async Task NotifAnnonceWithNoFavorisTest()
        {
            // Arrange
            double ancienPrix = 15000;
            double nouveauPrix = 12000;

            _mockManager.Setup(m => m.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix))
                .Returns(Task.CompletedTask);

            // Act
            await _mockManager.Object.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix);

            // Assert
            _mockManager.Verify(m => m.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix), Times.Once);
        }

        [TestMethod]
        public async Task NotifSuppressionAnnonceTest()
        {
            // Arrange
            _mockManager.Setup(m => m.NotifSuppressionAnnonce(_annonceTest.IdAnnonce))
                .Returns(Task.CompletedTask);

            // Act
            await _mockManager.Object.NotifSuppressionAnnonce(_annonceTest.IdAnnonce);

            // Assert
            _mockManager.Verify(m => m.NotifSuppressionAnnonce(_annonceTest.IdAnnonce), Times.Once);
        }

        [TestMethod]
        public async Task NotifSuppressionAnnonceWithoutSignalementTest()
        {
            // Arrange
            var nouvelleAnnonce = new Annonce
            {
                IdAnnonce = 2,
                Libelle = "Annonce sans signalement",
                Description = "Description",
                Prix = 10000,
                DatePublication = DateTime.Now,
                IdCompte = _compteTest.IdCompte
            };

            _mockManager.Setup(m => m.NotifSuppressionAnnonce(nouvelleAnnonce.IdAnnonce))
                .Returns(Task.CompletedTask);

            // Act
            await _mockManager.Object.NotifSuppressionAnnonce(nouvelleAnnonce.IdAnnonce);

            // Asssert
            _mockManager.Verify(m => m.NotifSuppressionAnnonce(nouvelleAnnonce.IdAnnonce), Times.Once);
        }

        [TestMethod]
        public async Task NotifAnnonceMultipleFavorisTest()
        {
            // Arrange
            double ancienPrix = 15000;
            double nouveauPrix = 11000;

            _mockManager.Setup(m => m.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix))
                .Returns(Task.CompletedTask);

            // Act
            await _mockManager.Object.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix);

            // Assert
            _mockManager.Verify(m => m.NotifAnnonce(_annonceTest.IdAnnonce, ancienPrix, nouveauPrix), Times.Once);
        }

        #endregion

        #region Tests NotifPaiementMiseEnAvant

        [TestMethod]
        public async Task NotifPaiementMiseEnAvantTest()
        {
            // Arrange
            int idMiseEnAvant = 1;

            _mockManager.Setup(m => m.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant))
                .Returns(Task.CompletedTask);

            // Act
            await _mockManager.Object.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant);

            // Assert
            _mockManager.Verify(m => m.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant), Times.Once);
        }

        [TestMethod]
        public async Task NotifPaiementMiseEnAvantWithDifferentGradesTest()
        {
            // Arrange
            int idMiseEnAvantBasic = 1;
            int idMiseEnAvantPremium = 2;

            _mockManager.Setup(m => m.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockManager.Object.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvantBasic);

            await _mockManager.Object.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvantPremium);

            // Assert
            _mockManager.Verify(m => m.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvantBasic), Times.Once);

            _mockManager.Verify(m => m.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvantPremium), Times.Once);
        }

        [TestMethod]
        public async Task NotifPaiementMiseEnAvantMultipleAnnoncesTest()
        {
            // Arrange
            var annonce2 = new Annonce
            {
                IdAnnonce = 2,
                Libelle = "Deuxième voiture",
                Description = "Description 2",
                Prix = 20000,
                DatePublication = DateTime.Now,
                IdCompte = _compteTest.IdCompte
            };

            int idMiseEnAvant = 1;

            _mockManager.Setup(m => m.NotifPaiementMiseEnAvant(
                It.IsAny<int>(),
                _compteTest.IdCompte,
                idMiseEnAvant))
                .Returns(Task.CompletedTask);

            // Act
            await _mockManager.Object.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant);

            await _mockManager.Object.NotifPaiementMiseEnAvant(
                annonce2.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant);

            // Assert
            _mockManager.Verify(m => m.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant), Times.Once);

            _mockManager.Verify(m => m.NotifPaiementMiseEnAvant(
                annonce2.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant), Times.Once);

            _mockManager.Verify(m => m.NotifPaiementMiseEnAvant(
                It.IsAny<int>(),
                _compteTest.IdCompte,
                idMiseEnAvant), Times.Exactly(2));
        }

        [TestMethod]
        public async Task NotifPaiementMiseEnAvantVerifyParametersTest()
        {
            // Arrange
            int idAnnonce = _annonceTest.IdAnnonce;
            int idCompte = _compteTest.IdCompte;
            int idMiseEnAvant = 5;

            bool correctParametersCalled = false;

            _mockManager.Setup(m => m.NotifPaiementMiseEnAvant(
                It.Is<int>(id => id == idAnnonce),
                It.Is<int>(id => id == idCompte),
                It.Is<int>(id => id == idMiseEnAvant)))
                .Returns(Task.CompletedTask)
                .Callback(() => correctParametersCalled = true);

            // Act
            await _mockManager.Object.NotifPaiementMiseEnAvant(idAnnonce, idCompte, idMiseEnAvant);

            // Assert
            Assert.IsTrue(correctParametersCalled);
            _mockManager.Verify(m => m.NotifPaiementMiseEnAvant(idAnnonce, idCompte, idMiseEnAvant), Times.Once);
        }

        [TestMethod]
        public async Task NotifPaiementMiseEnAvantMultipleCallsTest()
        {
            // Arrange
            int idMiseEnAvant = 1;

            _mockManager.Setup(m => m.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant))
                .Returns(Task.CompletedTask);

            // Act - Appel multiple pour simuler des renouvellements
            await _mockManager.Object.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant);

            await _mockManager.Object.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant);

            await _mockManager.Object.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant);

            // Assert
            _mockManager.Verify(m => m.NotifPaiementMiseEnAvant(
                _annonceTest.IdAnnonce,
                _compteTest.IdCompte,
                idMiseEnAvant), Times.Exactly(3));
        }

        #endregion
    }
}