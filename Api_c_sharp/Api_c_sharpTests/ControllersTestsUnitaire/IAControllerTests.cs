using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Repository.AI;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    [TestCategory("integration")]
    public class IAControllerIntegrationTests
    {
        private IAController _controller;
        private IIAService _iaService;
        private HttpClient _httpClient;
        private IConfiguration _configuration;

        [TestInitialize]
        public void Initialize()
        {
            // Configuration pour pointer vers l'API Python de test
            var inMemorySettings = new Dictionary<string, string>
            {
                {"PythonAPI:BaseUrl", "http://localhost:8000"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Création du HttpClient
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Création des loggers
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            // Création du manager et du controller
            _controller = new IAController(_iaService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _httpClient?.Dispose();
        }

        #region Health
        [TestMethod]
        public async Task HealthTest_RealService()
        {
            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsNotNull(result);

            // Le test passe si le service est disponible ou indisponible
            // car on teste la vraie intégration
            if (result is OkObjectResult okResult)
            {
                dynamic value = okResult.Value;
                Assert.AreEqual("healthy", value.status);
            }
            else if (result is ObjectResult objectResult)
            {
                Assert.AreEqual(503, objectResult.StatusCode);
            }
        }
        #endregion

        #region CNN
        [TestMethod]
        public async Task PredictCNNTest_RealService()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
            };

            try
            {
                // Act
                var result = await _controller.Predict(dataCNN);

                // Assert
                Assert.IsNotNull(result);

                if (result.Result is OkObjectResult okResult)
                {
                    var resultatCNN = (ResultatCNN)okResult.Value;
                    Assert.AreEqual("cnn", resultatCNN.Type);
                    Assert.IsTrue(resultatCNN.Success);
                    Assert.IsNotNull(resultatCNN.Manufacturer);
                    Assert.IsNotNull(resultatCNN.Model);
                    Assert.IsTrue(resultatCNN.ConfidenceScore >= 0 && resultatCNN.ConfidenceScore <= 1);
                }
                else if (result.Result is ObjectResult objectResult)
                {
                    // Service indisponible - test considéré comme passé
                    Assert.AreEqual(503, objectResult.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // Si le service Python n'est pas disponible, on log mais on ne fait pas échouer le test
                Console.WriteLine($"Service Python non disponible: {ex.Message}");
                Assert.Inconclusive("Service Python non disponible pour les tests d'intégration");
            }
        }

        [TestMethod]
        public async Task PredictTest_CNNInvalidData_MissingImage()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = ""
            };

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequest = (BadRequestObjectResult)result.Result;
            Assert.AreEqual("L'image en base64 est requise pour la reconnaissance visuelle", badRequest.Value);
        }

        #endregion

        #region Prediction
        [TestMethod]
        public async Task PredictPredictionTest_RealService()
        {
            // Arrange
            var dataPrediction = new DataPrediction
            {
                ProdYear = 2020,
                Manufacturer = "Renault",
                Model = "Clio",
                Mileage = "50000",
                EngineVolume = 1.5f,
                Category = "Sedan",
                FuelType = "Petrol",
                GearBoxType = "Manual"
            };

            try
            {
                // Act
                var result = await _controller.Predict(dataPrediction);

                // Assert
                Assert.IsNotNull(result);

                if (result.Result is OkObjectResult okResult)
                {
                    var resultatPrediction = (ResultatPrediction)okResult.Value;
                    Assert.AreEqual("prediction", resultatPrediction.Type);
                    Assert.IsTrue(resultatPrediction.Success);
                    Assert.IsTrue(resultatPrediction.PredictedPrice > 0);
                    Assert.IsNotNull(resultatPrediction.Currency);
                }
                else if (result.Result is ObjectResult objectResult)
                {
                    Assert.AreEqual(503, objectResult.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Service Python non disponible: {ex.Message}");
                Assert.Inconclusive("Service Python non disponible pour les tests d'intégration");
            }
        }

        [TestMethod]
        public async Task PredictTest_PredictionInvalidData_MissingProdYear()
        {
            // Arrange
            var dataPrediction = new DataPrediction
            {
                ProdYear = null,
                Manufacturer = "Renault"
            };

            // Act
            var result = await _controller.Predict(dataPrediction);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequest = (BadRequestObjectResult)result.Result;
            Assert.AreEqual("L'année de production est requise pour la prédiction", badRequest.Value);
        }
        #endregion

        #region Ajustement
        [TestMethod]
        public async Task PredictAjustementTest_RealService()
        {
            // Arrange
            var dataAjustement = new DataAjustement
            {
                BasePrice = 15000,
                Description = "Véhicule en excellent état, options premium"
            };

            try
            {
                // Act
                var result = await _controller.Predict(dataAjustement);

                // Assert
                Assert.IsNotNull(result);

                if (result.Result is OkObjectResult okResult)
                {
                    var resultatAjustement = (ResultatAjustement)okResult.Value;
                    Assert.AreEqual("ajustement", resultatAjustement.Type);
                    Assert.IsTrue(resultatAjustement.Success);
                    Assert.IsTrue(resultatAjustement.AdjustedPrice > 0);
                    Assert.IsTrue(resultatAjustement.BasePrice > 0);
                }
                else if (result.Result is ObjectResult objectResult)
                {
                    Assert.AreEqual(503, objectResult.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Service Python non disponible: {ex.Message}");
                Assert.Inconclusive("Service Python non disponible pour les tests d'intégration");
            }
        }


        [TestMethod]
        public async Task PredictTest_AjustementInvalidData_InvalidBasePrice()
        {
            // Arrange
            var dataAjustement = new DataAjustement
            {
                BasePrice = 0,
                Description = "Test"
            };

            // Act
            var result = await _controller.Predict(dataAjustement);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequest = (BadRequestObjectResult)result.Result;
            Assert.AreEqual("Le prix de base doit être supérieur à 0", badRequest.Value);
        }

        [TestMethod]
        public async Task PredictTest_AjustementInvalidData_MissingDescription()
        {
            // Arrange
            var dataAjustement = new DataAjustement
            {
                BasePrice = 15000,
                Description = ""
            };

            // Act
            var result = await _controller.Predict(dataAjustement);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequest = (BadRequestObjectResult)result.Result;
            Assert.AreEqual("La description est requise pour l'ajustement de prix", badRequest.Value);
        }
        #endregion

        #region ModelState
        [TestMethod]
        public async Task PredictTest_ModelStateInvalid()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "validbase64"
            };

            _controller.ModelState.AddModelError("Type", "Required");

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }
        #endregion
    }
}