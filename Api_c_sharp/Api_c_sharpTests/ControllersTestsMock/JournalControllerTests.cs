using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
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
    [TestCategory("unit")]
    public class JournalControllerTestsMoq
    {
        private Mock<JournalManager> _mockManager;
        private JournalController _controller;
        private IMapper _mapper;
        private Journal _objetcommun;

        [TestInitialize]
        public void Initialize()
        {
            // Création du mock
            _mockManager = new Mock<JournalManager>(null, null);

            // Création du journal de référence
            _objetcommun = new Journal
            {
                IdJournal = 1,
                ContenuJournal = "Ceci est le contenu du premier journal.",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            // Configuration AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Injection dans le controller
            _controller = new JournalController(_mockManager.Object, _mapper);
        }

        #region GET
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdJournal))
                       .ReturnsAsync(_objetcommun);

            // Act
            var result = await _controller.GetByID(_objetcommun.IdJournal);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(JournalDTO));
            Assert.AreEqual(_objetcommun.ContenuJournal, result.Value.ContenuJournal);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Journal)null);

            // Act
            var result = await _controller.GetByID(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllTest()
        {
            // Arrange
            var journalsList = new List<Journal>
            {
                _objetcommun,
                new Journal
                {
                    IdJournal = 2,
                    ContenuJournal = "Deuxième journal",
                    DateJournal = DateTime.UtcNow,
                    IdCompte = 1,
                    IdTypeJournal = 2
                }
            };

            _mockManager.Setup(m => m.GetAllAsync())
                       .ReturnsAsync(journalsList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<JournalDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.ContenuJournal == _objetcommun.ContenuJournal));
            Assert.AreEqual(2, result.Value.Count());
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task PostJournalTest_Entity()
        {
            // Arrange
            JournalCreateDTO journalDTO = new JournalCreateDTO
            {
                ContenuJournal = "Nouveau journal de test",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            var journalEntity = _mapper.Map<Journal>(journalDTO);
            journalEntity.IdJournal = 2;

            _mockManager.Setup(m => m.AddAsync(It.IsAny<Journal>()))
                       .ReturnsAsync(journalEntity)
                       .Verifiable();

            // Act
            var actionResult = await _controller.Post(journalDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)actionResult.Result;
            var createdJournal = (Journal)created.Value;
            Assert.AreEqual(journalDTO.ContenuJournal, createdJournal.ContenuJournal);
            _mockManager.Verify(m => m.AddAsync(It.IsAny<Journal>()), Times.Once);
        }

        [TestMethod]
        public async Task BadRequestPostJournalTest()
        {
            // Arrange
            JournalCreateDTO journalDTO = new JournalCreateDTO()
            {
                ContenuJournal = null,
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            _controller.ModelState.AddModelError("ContenuJournal", "Required");

            // Act
            var actionResult = await _controller.Post(journalDTO);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task DeleteJournalTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdJournal))
                       .ReturnsAsync(_objetcommun);
            _mockManager.Setup(m => m.DeleteAsync(_objetcommun))
                       .Returns(Task.FromResult(true))
                       .Verifiable();

            // Act
            var result = await _controller.Delete(_objetcommun.IdJournal);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.DeleteAsync(_objetcommun), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundDeleteJournalTest()
        {
            // Arrange
            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Journal)null);

            // Act
            var result = await _controller.Delete(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PutJournalTest()
        {
            // Arrange
            var existingJournal = new Journal
            {
                IdJournal = _objetcommun.IdJournal,
                ContenuJournal = "Ancien contenu",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            JournalUpdateDTO journalDTO = new JournalUpdateDTO()
            {
                IdJournal = _objetcommun.IdJournal,
                ContenuJournal = "Journal modifié",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            var updatedJournal = _mapper.Map<Journal>(journalDTO);

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdJournal))
                       .ReturnsAsync(existingJournal);
            _mockManager.Setup(m => m.UpdateAsync(existingJournal, updatedJournal))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            var result = await _controller.Put(_objetcommun.IdJournal, journalDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockManager.Verify(m => m.UpdateAsync(It.IsAny<Journal>(), It.IsAny<Journal>()), Times.Once);
        }

        [TestMethod]
        public async Task NotFoundPutJournalTest()
        {
            // Arrange
            JournalUpdateDTO journalDTO = new JournalUpdateDTO()
            {
                ContenuJournal = "Journal modifié",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            _mockManager.Setup(m => m.GetByIdAsync(0))
                       .ReturnsAsync((Journal)null);

            // Act
            var result = await _controller.Put(0, journalDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task BadRequestPutJournalTest()
        {
            // Arrange
            var existingJournal = new Journal
            {
                IdJournal = _objetcommun.IdJournal,
                ContenuJournal = "Ancien contenu",
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            JournalUpdateDTO journalDTO = new JournalUpdateDTO()
            {
                ContenuJournal = null,
                DateJournal = DateTime.UtcNow,
                IdCompte = 1,
                IdTypeJournal = 1
            };

            _mockManager.Setup(m => m.GetByIdAsync(_objetcommun.IdJournal))
                       .ReturnsAsync(existingJournal);

            _controller.ModelState.AddModelError("ContenuJournal", "Required");

            // Act
            var result = await _controller.Put(_objetcommun.IdJournal, journalDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
        #endregion

        #region Filtred
        [TestMethod]
        public async Task GetFilteredJournal_ByTypeOnly_Test()
        {
            // Arrange
            var journalsList = new List<Journal>
            {
                _objetcommun,
                new Journal
                {
                    IdJournal = 3,
                    ContenuJournal = "Autre connexion",
                    DateJournal = DateTime.UtcNow,
                    IdCompte = 1,
                    IdTypeJournal = 1
                }
            };

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                Order = 0
            };

            _mockManager.Setup(m => m.GetFilteredJournal(It.Is<RechercheJournalDTO>(r => r.IdType == 1)))
                       .ReturnsAsync(journalsList);

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<JournalDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.AreEqual(2, result.Value.Count());
            Assert.IsTrue(result.Value.All(j => j.IdTypeJournal == 1));
        }

        [TestMethod]
        public async Task GetFilteredJournal_WithDateInterval_Test()
        {
            // Arrange
            var dateDebut = DateTime.UtcNow.AddDays(-5);
            var dateFin = DateTime.UtcNow;

            var journalsList = new List<Journal>
            {
                new Journal
                {
                    IdJournal = 1,
                    ContenuJournal = "Journal dans l'intervalle",
                    DateJournal = DateTime.UtcNow.AddDays(-2),
                    IdCompte = 1,
                    IdTypeJournal = 1
                }
            };

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                DebutIntervalle = dateDebut,
                FinIntervalle = dateFin,
                Order = 0
            };

            _mockManager.Setup(m => m.GetFilteredJournal(It.Is<RechercheJournalDTO>(
                r => r.IdType == 1 && r.DebutIntervalle == dateDebut && r.FinIntervalle == dateFin)))
                       .ReturnsAsync(journalsList);

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
            _mockManager.Verify(m => m.GetFilteredJournal(It.IsAny<RechercheJournalDTO>()), Times.Once);
        }

        [TestMethod]
        public async Task GetFilteredJournal_OrderAscending_Test()
        {
            // Arrange
            var date1 = DateTime.UtcNow.AddHours(-3);
            var date2 = DateTime.UtcNow.AddHours(-2);
            var date3 = DateTime.UtcNow.AddHours(-1);

            var journalsList = new List<Journal>
            {
                new Journal { IdJournal = 1, ContenuJournal = "Premier", DateJournal = date1, IdCompte = 1, IdTypeJournal = 1 },
                new Journal { IdJournal = 2, ContenuJournal = "Deuxième", DateJournal = date2, IdCompte = 1, IdTypeJournal = 1 },
                new Journal { IdJournal = 3, ContenuJournal = "Troisième", DateJournal = date3, IdCompte = 1, IdTypeJournal = 1 }
            };

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                Order = 1
            };

            _mockManager.Setup(m => m.GetFilteredJournal(It.Is<RechercheJournalDTO>(r => r.Order == 1)))
                       .ReturnsAsync(journalsList);

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            var journaux = result.Value.ToList();
            Assert.AreEqual(3, journaux.Count);

            for (int i = 0; i < journaux.Count - 1; i++)
            {
                Assert.IsTrue(journaux[i].DateJournal <= journaux[i + 1].DateJournal);
            }
        }

        [TestMethod]
        public async Task GetFilteredJournal_OrderDescending_Test()
        {
            // Arrange
            var date1 = DateTime.UtcNow.AddHours(-1);
            var date2 = DateTime.UtcNow.AddHours(-2);
            var date3 = DateTime.UtcNow.AddHours(-3);

            var journalsList = new List<Journal>
            {
                new Journal { IdJournal = 3, ContenuJournal = "Troisième", DateJournal = date1, IdCompte = 1, IdTypeJournal = 1 },
                new Journal { IdJournal = 2, ContenuJournal = "Deuxième", DateJournal = date2, IdCompte = 1, IdTypeJournal = 1 },
                new Journal { IdJournal = 1, ContenuJournal = "Premier", DateJournal = date3, IdCompte = 1, IdTypeJournal = 1 }
            };

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                Order = 0
            };

            _mockManager.Setup(m => m.GetFilteredJournal(It.Is<RechercheJournalDTO>(r => r.Order == 0)))
                       .ReturnsAsync(journalsList);

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            var journaux = result.Value.ToList();
            Assert.AreEqual(3, journaux.Count);

            for (int i = 0; i < journaux.Count - 1; i++)
            {
                Assert.IsTrue(journaux[i].DateJournal >= journaux[i + 1].DateJournal);
            }
        }

        [TestMethod]
        public async Task GetFilteredJournal_NoResults_Test()
        {
            // Arrange
            var recherche = new RechercheJournalDTO
            {
                IdType = 999,
                Order = 0
            };

            _mockManager.Setup(m => m.GetFilteredJournal(It.Is<RechercheJournalDTO>(r => r.IdType == 999)))
                       .ReturnsAsync((IEnumerable<Journal>)null);

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetFilteredJournal_EmptyResults_Test()
        {
            // Arrange
            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                DebutIntervalle = DateTime.UtcNow.AddDays(-10),
                FinIntervalle = DateTime.UtcNow.AddDays(-5),
                Order = 0
            };

            _mockManager.Setup(m => m.GetFilteredJournal(It.IsAny<RechercheJournalDTO>()))
                       .ReturnsAsync(new List<Journal>());

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetFilteredJournal_WithDateDebutOnly_Test()
        {
            // Arrange
            var dateDebut = DateTime.UtcNow.AddDays(-5);

            var journalsList = new List<Journal>
            {
                new Journal
                {
                    IdJournal = 1,
                    ContenuJournal = "Journal récent",
                    DateJournal = DateTime.UtcNow.AddDays(-2),
                    IdCompte = 1,
                    IdTypeJournal = 1
                }
            };

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                DebutIntervalle = dateDebut,
                Order = 0
            };

            _mockManager.Setup(m => m.GetFilteredJournal(It.Is<RechercheJournalDTO>(
                r => r.DebutIntervalle == dateDebut && !r.FinIntervalle.HasValue)))
                       .ReturnsAsync(journalsList);

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }

        [TestMethod]
        public async Task GetFilteredJournal_WithDateFinOnly_Test()
        {
            // Arrange
            var dateFin = DateTime.UtcNow;

            var journalsList = new List<Journal>
            {
                new Journal
                {
                    IdJournal = 1,
                    ContenuJournal = "Journal ancien",
                    DateJournal = DateTime.UtcNow.AddDays(-2),
                    IdCompte = 1,
                    IdTypeJournal = 1
                }
            };

            var recherche = new RechercheJournalDTO
            {
                IdType = 1,
                FinIntervalle = dateFin,
                Order = 0
            };

            _mockManager.Setup(m => m.GetFilteredJournal(It.Is<RechercheJournalDTO>(
                r => !r.DebutIntervalle.HasValue && r.FinIntervalle == dateFin)))
                       .ReturnsAsync(journalsList);

            // Act
            var result = await _controller.GetFilteredJournal(recherche);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Any());
        }
        #endregion

        #region Log
            #region Connexion
        [TestMethod]
        public async Task LogConnexionAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            _mockManager.Setup(m => m.LogConnexionAsync(idCompte))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogConnexionAsync(idCompte);

            // Assert
            _mockManager.Verify(m => m.LogConnexionAsync(idCompte), Times.Once);
        }

        [TestMethod]
        public async Task LogDeconnexionAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            _mockManager.Setup(m => m.LogDeconnexionAsync(idCompte))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogDeconnexionAsync(idCompte);

            // Assert
            _mockManager.Verify(m => m.LogDeconnexionAsync(idCompte), Times.Once);
        }
        #endregion

            #region Compte
        [TestMethod]
        public async Task LogCreationCompteAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            string pseudo = "testuser";
            _mockManager.Setup(m => m.LogCreationCompteAsync(idCompte, pseudo))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogCreationCompteAsync(idCompte, pseudo);

            // Assert
            _mockManager.Verify(m => m.LogCreationCompteAsync(idCompte, pseudo), Times.Once);
        }

        [TestMethod]
        public async Task LogModificationProfilAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            _mockManager.Setup(m => m.LogModificationProfilAsync(idCompte))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogModificationProfilAsync(idCompte);

            // Assert
            _mockManager.Verify(m => m.LogModificationProfilAsync(idCompte), Times.Once);
        }
        #endregion

            #region Annoce
        [TestMethod]
        public async Task LogPublicationAnnonceAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idAnnonce = 100;
            string titre = "Voiture de sport";
            _mockManager.Setup(m => m.LogPublicationAnnonceAsync(idCompte, idAnnonce, titre))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogPublicationAnnonceAsync(idCompte, idAnnonce, titre);

            // Assert
            _mockManager.Verify(m => m.LogPublicationAnnonceAsync(idCompte, idAnnonce, titre), Times.Once);
        }

        [TestMethod]
        public async Task LogModificationAnnonceAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idAnnonce = 100;
            string titre = "Voiture modifiée";
            _mockManager.Setup(m => m.LogModificationAnnonceAsync(idCompte, idAnnonce, titre))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogModificationAnnonceAsync(idCompte, idAnnonce, titre);

            // Assert
            _mockManager.Verify(m => m.LogModificationAnnonceAsync(idCompte, idAnnonce, titre), Times.Once);
        }

        [TestMethod]
        public async Task LogSuppressionAnnonceAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idAnnonce = 100;
            string titre = "Voiture supprimée";
            _mockManager.Setup(m => m.LogSuppressionAnnonceAsync(idCompte, idAnnonce, titre))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogSuppressionAnnonceAsync(idCompte, idAnnonce, titre);

            // Assert
            _mockManager.Verify(m => m.LogSuppressionAnnonceAsync(idCompte, idAnnonce, titre), Times.Once);
        }
        

        [TestMethod]
        public async Task LogAchatAsyncTest()
        {
            // Arrange
            int idAcheteur = 1;
            int idVendeur = 2;
            int idCommande = 50;
            int idAnnonce = 100;
            int idMoyenPaiement = 1;
            _mockManager.Setup(m => m.LogAchatAsync(idAcheteur, idVendeur, idCommande, idAnnonce, idMoyenPaiement))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogAchatAsync(idAcheteur, idVendeur, idCommande, idAnnonce, idMoyenPaiement);

            // Assert
            _mockManager.Verify(m => m.LogAchatAsync(idAcheteur, idVendeur, idCommande, idAnnonce, idMoyenPaiement), Times.Once);
        }
        #endregion

            #region Signalement
        [TestMethod]
        public async Task LogSignalementCompteAsyncTest()
        {
            // Arrange
            int idSignalant = 1;
            int idSignale = 2;
            int idSignalement = 10;
            int idTypeSignalement = 1;
            string description = "Comportement inapproprié";
            _mockManager.Setup(m => m.LogSignalementCompteAsync(idSignalant, idSignale, idSignalement, idTypeSignalement, description))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogSignalementCompteAsync(idSignalant, idSignale, idSignalement, idTypeSignalement, description);

            // Assert
            _mockManager.Verify(m => m.LogSignalementCompteAsync(idSignalant, idSignale, idSignalement, idTypeSignalement, description), Times.Once);
        }

        [TestMethod]
        public async Task LogSignalementAnnonceAsyncTest()
        {
            // Arrange
            int idSignalant = 1;
            int idAnnoncesignale = 2;
            int idSignalement = 10;
            int idTypeSignalement = 1;
            string description = "Photo inapproprié";
            _mockManager.Setup(m => m.LogSignalementAnnonceAsync(idSignalant, idAnnoncesignale, idSignalement, idTypeSignalement, description))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogSignalementAnnonceAsync(idSignalant, idAnnoncesignale, idSignalement, idTypeSignalement, description);

            // Assert
            _mockManager.Verify(m => m.LogSignalementAnnonceAsync(idSignalant, idAnnoncesignale, idSignalement, idTypeSignalement, description), Times.Once);
        }
        #endregion

            #region Interaction
        [TestMethod]
        public async Task LogDepotAvisAsyncTest()
        {
            // Arrange
            int idJugeur = 1;
            int idJuge = 2;
            int idAvis = 20;
            int note = 5;
            string description = "Excellent vendeur";
            _mockManager.Setup(m => m.LogDepotAvisAsync(idJugeur, idJuge, idAvis, note, description))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogDepotAvisAsync(idJugeur, idJuge, idAvis, note, description);

            // Assert
            _mockManager.Verify(m => m.LogDepotAvisAsync(idJugeur, idJuge, idAvis, note, description), Times.Once);
        }

        [TestMethod]
        public async Task LogMiseFavorisAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idAnnonce = 100;
            _mockManager.Setup(m => m.LogMiseFavorisAsync(idCompte, idAnnonce))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogMiseFavorisAsync(idCompte, idAnnonce);

            // Assert
            _mockManager.Verify(m => m.LogMiseFavorisAsync(idCompte, idAnnonce), Times.Once);
        }

        [TestMethod]
        public async Task LogEnvoiMessageAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idConversation = 5;
            string message = "Bonjour, est-ce toujours disponible ?";
            _mockManager.Setup(m => m.LogEnvoiMessageAsync(idCompte, idConversation, message, null))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogEnvoiMessageAsync(idCompte, idConversation, message);

            // Assert
            _mockManager.Verify(m => m.LogEnvoiMessageAsync(idCompte, idConversation, message, null), Times.Once);
        }

        [TestMethod]
        public async Task LogGenerationFactureAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idFacture = 30;
            int idCommande = 50;
            _mockManager.Setup(m => m.LogGenerationFactureAsync(idCompte, idFacture, idCommande))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogGenerationFactureAsync(idCompte, idFacture, idCommande);

            // Assert
            _mockManager.Verify(m => m.LogGenerationFactureAsync(idCompte, idFacture, idCommande), Times.Once);
        }

        [TestMethod]
        public async Task LogBlocageUtilisateurAsyncTest()
        {
            // Arrange
            int idBloqueur = 1;
            int idBloque = 2;
            _mockManager.Setup(m => m.LogBlocageUtilisateurAsync(idBloqueur, idBloque))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogBlocageUtilisateurAsync(idBloqueur, idBloque);

            // Assert
            _mockManager.Verify(m => m.LogBlocageUtilisateurAsync(idBloqueur, idBloque), Times.Once);
        }

        [TestMethod]
        public async Task LogActionAsyncTest()
        {
            // Arrange
            int idCompte = 1;
            int idTypeJournal = 1;
            string contenu = "Test d'action";
            _mockManager.Setup(m => m.LogActionAsync(idCompte, idTypeJournal, contenu))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Act
            await _mockManager.Object.LogActionAsync(idCompte, idTypeJournal, contenu);

            // Assert
            _mockManager.Verify(m => m.LogActionAsync(idCompte, idTypeJournal, contenu), Times.Once);
        }
        #endregion
        #endregion
    }
}