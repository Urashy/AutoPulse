using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Repository.AI;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class IAControllerUnitTests
    {
        private Mock<IIAService> _mockIAService;
        private IAController _controller;

        [TestInitialize]
        public void Initialize()
        {
            _mockIAService = new Mock<IIAService>();
            _controller = new IAController(_mockIAService.Object);
        }
        #region Health
        [TestMethod]
        public async Task HealthTest_ServiceHealthy_ReturnsOk()
        {
            // Arrange
            _mockIAService.Setup(s => s.HealthCheckAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result;
            var jsonString = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);

            Assert.AreEqual("healthy", response.GetProperty("status").GetString());
            Assert.AreEqual("Service IA opérationnel", response.GetProperty("message").GetString());

            _mockIAService.Verify(s => s.HealthCheckAsync(), Times.Once);
        }

        [TestMethod]
        public async Task HealthTest_ServiceUnhealthy_Returns503()
        {
            // Arrange
            _mockIAService.Setup(s => s.HealthCheckAsync())
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));

            var objectResult = (ObjectResult)result;
            Assert.AreEqual(503, objectResult.StatusCode);

            var jsonString = JsonSerializer.Serialize(objectResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);

            Assert.AreEqual("unhealthy", response.GetProperty("status").GetString());
        }

        [TestMethod]
        public async Task HealthTest_ExceptionThrown_Returns503()
        {
            // Arrange
            _mockIAService.Setup(s => s.HealthCheckAsync())
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));

            var objectResult = (ObjectResult)result;
            Assert.AreEqual(503, objectResult.StatusCode);
        }
        #endregion

        #region Prediction
        [TestMethod]
        public async Task PredictCNNTest_ValidData_ReturnsOk()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
            };

            var expectedResult = new ResultatCNN
            {
                Success = true,
                Manufacturer = "Toyota",
                Model = "Corolla",
                ConfidenceScore = 0.95f
            };

            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataCNN>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var resultatCNN = (ResultatCNN)okResult.Value;

            Assert.AreEqual("cnn", resultatCNN.Type);
            Assert.IsTrue(resultatCNN.Success);
            Assert.AreEqual("Toyota", resultatCNN.Manufacturer);
            Assert.AreEqual("Corolla", resultatCNN.Model);
            Assert.AreEqual(0.95f, resultatCNN.ConfidenceScore);

            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataCNN>()), Times.Once);
        }

        [TestMethod]
        public async Task PredictPredictionTest_ValidData_ReturnsOk()
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

            var expectedResult = new ResultatPrediction
            {
                Success = true,
                PredictedPrice = 12500.50,
                Currency = "EUR"
            };

            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataPrediction>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Predict(dataPrediction);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var resultatPrediction = (ResultatPrediction)okResult.Value;

            Assert.AreEqual("prediction", resultatPrediction.Type);
            Assert.IsTrue(resultatPrediction.Success);
            Assert.AreEqual(12500.50, resultatPrediction.PredictedPrice);
            Assert.AreEqual("EUR", resultatPrediction.Currency);

            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataPrediction>()), Times.Once);
        }

        [TestMethod]
        public async Task PredictAjustementTest_ValidData_ReturnsOk()
        {
            // Arrange
            var dataAjustement = new DataAjustement
            {
                BasePrice = 15000,
                Description = "Véhicule en excellent état, options premium"
            };

            var expectedResult = new ResultatAjustement
            {
                Success = true,
                AdjustedPrice = 16500,
                BasePrice = 15000,
                ReductionPercent = 10.0
            };

            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataAjustement>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Predict(dataAjustement);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result.Result;
            var resultatAjustement = (ResultatAjustement)okResult.Value;

            Assert.AreEqual("ajustement", resultatAjustement.Type);
            Assert.IsTrue(resultatAjustement.Success);
            Assert.AreEqual(16500, resultatAjustement.AdjustedPrice);
            Assert.AreEqual(15000, resultatAjustement.BasePrice);

            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAjustement>()), Times.Once);
        }

        [TestMethod]
        public async Task PredictTest_ServiceReturnsFailure_ReturnsBadRequest()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "validbase64"
            };

            var failedResult = new ResultatCNN
            {
                Success = false,
                Error = "Impossible de traiter l'image"
            };

            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataCNN>()))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

            var badRequest = (BadRequestObjectResult)result.Result;
            var jsonString = JsonSerializer.Serialize(badRequest.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);

            Assert.AreEqual("Impossible de traiter l'image", response.GetProperty("message").GetString());
        }

        [TestMethod]
        public async Task PredictTest_ServiceThrowsException_Returns503()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "validbase64"
            };

            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataCNN>()))
                .ThrowsAsync(new Exception("Service Python indisponible"));

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));

            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(503, objectResult.StatusCode);

            var jsonString = JsonSerializer.Serialize(objectResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);

            Assert.AreEqual("Le service IA est temporairement indisponible", response.GetProperty("message").GetString());
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

            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
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

            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
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

            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
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

            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
        }

        [TestMethod]
        public async Task PredictTest_ModelStateInvalid_ReturnsBadRequest()
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

            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
        }

        [TestMethod]
        public async Task PredictTest_CNNWithNullImage_ReturnsBadRequest()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = null
            };

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
        }

        [TestMethod]
        public async Task PredictTest_AjustementWithNegativePrice_ReturnsBadRequest()
        {
            // Arrange
            var dataAjustement = new DataAjustement
            {
                BasePrice = -100,
                Description = "Test description"
            };

            // Act
            var result = await _controller.Predict(dataAjustement);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
        }
        #endregion
    }
}