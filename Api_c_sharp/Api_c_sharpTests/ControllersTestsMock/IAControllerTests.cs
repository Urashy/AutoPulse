using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Repository.AI;
using AutoMapper;
using AutoPulse.Shared.DTO.IA.Benchmark;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Api_c_sharp.ControllersMock.Tests
{

    [TestClass()]
    [TestCategory("unit")]
    public class IAControllerMockTests
    {
        private Mock<IIAService> _mockIAService;
        private IAController _controller;

        [TestInitialize]
        public void Initialize()
        {
            _mockIAService = new Mock<IIAService>();
            _controller = new IAController(_mockIAService.Object);
        }

        #region Health Tests
        [TestMethod]
        public async Task Health_ServiceHealthy_ReturnsOk()
        {
            // Arrange
            _mockIAService.Setup(s => s.HealthCheckAsync()).ReturnsAsync(true);

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var jsonString = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);
            Assert.AreEqual("healthy", response.GetProperty("status").GetString());
            _mockIAService.Verify(s => s.HealthCheckAsync(), Times.Once);
        }

        [TestMethod]
        public async Task Health_ServiceUnhealthy_Returns503()
        {
            // Arrange
            _mockIAService.Setup(s => s.HealthCheckAsync()).ReturnsAsync(false);

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(503, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task Health_ExceptionThrown_Returns503()
        {
            // Arrange
            _mockIAService.Setup(s => s.HealthCheckAsync()).ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual(503, ((ObjectResult)result).StatusCode);
        }
        #endregion

        #region Predict - CNN Tests
        [TestMethod]
        public async Task Predict_CNN_ValidData_ReturnsOk()
        {
            // Arrange
            var dataCNN = new DataCNN { ImageBase64 = "validBase64Image==" };
            var expectedResult = new ResultatCNN
            {
                Success = true,
                Manufacturer = "Toyota",
                Model = "Corolla",
                ConfidenceScore = 0.95f
            };
            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataCNN>())).ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var resultatCNN = (ResultatCNN)okResult.Value;
            Assert.IsTrue(resultatCNN.Success);
            Assert.AreEqual("Toyota", resultatCNN.Manufacturer);
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataCNN>()), Times.Once);
        }

        [TestMethod]
        public async Task Predict_CNN_MissingImage_ReturnsBadRequest()
        {
            // Arrange
            var dataCNN = new DataCNN { ImageBase64 = "" };

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
        }

        [TestMethod]
        public async Task Predict_CNN_NullImage_ReturnsBadRequest()
        {
            // Arrange
            var dataCNN = new DataCNN { ImageBase64 = null };

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Predict_CNN_ServiceReturnsFailure_ReturnsBadRequest()
        {
            // Arrange
            var dataCNN = new DataCNN { ImageBase64 = "validBase64" };
            var failedResult = new ResultatCNN { Success = false, Error = "Image processing failed" };
            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataCNN>())).ReturnsAsync(failedResult);

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region Predict - Prediction Tests
        [TestMethod]
        public async Task Predict_Prediction_ValidData_ReturnsOk()
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
            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataPrediction>())).ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Predict(dataPrediction);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var resultat = (ResultatPrediction)okResult.Value;
            Assert.IsTrue(resultat.Success);
            Assert.AreEqual(12500.50, resultat.PredictedPrice);
        }

        [TestMethod]
        public async Task Predict_Prediction_MissingProdYear_ReturnsBadRequest()
        {
            // Arrange
            var dataPrediction = new DataPrediction { ProdYear = null, Manufacturer = "Renault" };

            // Act
            var result = await _controller.Predict(dataPrediction);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region Predict - Ajustement Tests
        [TestMethod]
        public async Task Predict_Ajustement_ValidData_ReturnsOk()
        {
            // Arrange
            var dataAjustement = new DataAjustement
            {
                BasePrice = 15000,
                Description = "Excellent état"
            };
            var expectedResult = new ResultatAjustement
            {
                Success = true,
                AdjustedPrice = 16500,
                BasePrice = 15000,
                ReductionPercent = 10.0
            };
            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataAjustement>())).ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Predict(dataAjustement);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var resultat = (ResultatAjustement)okResult.Value;
            Assert.IsTrue(resultat.Success);
            Assert.AreEqual(16500, resultat.AdjustedPrice);
        }

        [TestMethod]
        public async Task Predict_Ajustement_InvalidBasePrice_ReturnsBadRequest()
        {
            // Arrange
            var dataAjustement = new DataAjustement { BasePrice = 0, Description = "Test" };

            // Act
            var result = await _controller.Predict(dataAjustement);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Predict_Ajustement_MissingDescription_ReturnsBadRequest()
        {
            // Arrange
            var dataAjustement = new DataAjustement { BasePrice = 15000, Description = "" };

            // Act
            var result = await _controller.Predict(dataAjustement);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }
        #endregion

        #region Predict - General Tests
        [TestMethod]
        public async Task Predict_ModelStateInvalid_ReturnsBadRequest()
        {
            // Arrange
            var dataCNN = new DataCNN { ImageBase64 = "valid" };
            _controller.ModelState.AddModelError("Type", "Required");

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Predict_ServiceThrowsException_Returns503()
        {
            // Arrange
            var dataCNN = new DataCNN { ImageBase64 = "valid" };
            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataCNN>()))
                .ThrowsAsync(new Exception("Python service unavailable"));

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            Assert.AreEqual(503, ((ObjectResult)result.Result).StatusCode);
        }
        #endregion

        #region Benchmark Tests
        [TestMethod]
        public async Task BenchmarkGetAll_ReturnsOkWithList()
        {
            // Arrange
            var benchmarks = new List<BenchmarkIAListDTO>
            {
                new BenchmarkIAListDTO { IdBenchmark = 1, ModelType = "cnn" },
                new BenchmarkIAListDTO { IdBenchmark = 2, ModelType = "prediction" }
            };
            _mockIAService.Setup(s => s.GetAllBenchmarksAsync()).ReturnsAsync(benchmarks);

            // Act
            var result = await _controller.BenchmarkGetAll();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var data = (IEnumerable<BenchmarkIAListDTO>)okResult.Value;
            Assert.AreEqual(2, data.Count());
        }

        [TestMethod]
        public async Task BenchmarkGetById_ExistingId_ReturnsOk()
        {
            // Arrange
            var benchmark = new BenchmarkIADTO { IdBenchmark = 1, ModelType = "cnn" };
            _mockIAService.Setup(s => s.GetBenchmarkByIdAsync(1)).ReturnsAsync(benchmark);

            // Act
            var result = await _controller.BenchmarkGetById(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var data = (BenchmarkIADTO)okResult.Value;
            Assert.AreEqual(1, data.IdBenchmark);
        }

        [TestMethod]
        public async Task BenchmarkGetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockIAService.Setup(s => s.GetBenchmarkByIdAsync(999)).ReturnsAsync((BenchmarkIADTO)null);

            // Act
            var result = await _controller.BenchmarkGetById(999);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task BenchmarkGetLatestByType_ReturnsOkWithDictionary()
        {
            // Arrange
            var benchmarks = new Dictionary<string, BenchmarkIADTO>
            {
                { "cnn", new BenchmarkIADTO { IdBenchmark = 1, ModelType = "cnn" } },
                { "prediction", new BenchmarkIADTO { IdBenchmark = 2, ModelType = "prediction" } }
            };
            _mockIAService.Setup(s => s.GetLatestBenchmarksByTypeAsync()).ReturnsAsync(benchmarks);

            // Act
            var result = await _controller.BenchmarkGetLatestByType();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var data = (Dictionary<string, BenchmarkIADTO>)okResult.Value;
            Assert.AreEqual(2, data.Count);
        }

        [TestMethod]
        public async Task BenchmarkGetStats_ReturnsOkWithStats()
        {
            // Arrange
            var stats = new BenchmarkIAStatsDTO
            {
                TotalBenchmarks = 10,
                GlobalAvgInferenceTimeMs = 50.5,
                GlobalSuccessRate = 95.0
            };
            _mockIAService.Setup(s => s.GetBenchmarkStatsAsync()).ReturnsAsync(stats);

            // Act
            var result = await _controller.BenchmarkGetStats();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var data = (BenchmarkIAStatsDTO)okResult.Value;
            Assert.AreEqual(10, data.TotalBenchmarks);
        }

        [TestMethod]
        public async Task BenchmarkGetHistoryByType_ReturnsOk()
        {
            // Arrange
            var history = new List<BenchmarkIAListDTO>
            {
                new BenchmarkIAListDTO { IdBenchmark = 1, ModelType = "cnn" }
            };
            _mockIAService.Setup(s => s.GetBenchmarkHistoryByTypeAsync("cnn", 10)).ReturnsAsync(history);

            // Act
            var result = await _controller.BenchmarkGetHistoryByType("cnn");

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task BenchmarkPost_ValidData_ReturnsCreated()
        {
            // Arrange
            var createDto = new BenchmarkIACreateDTO
            {
                BenchmarkId = "bench_123",
                ModelType = "cnn",
                TotalIterations = 100
            };
            var createdDto = new BenchmarkIADTO { IdBenchmark = 1, ModelType = "cnn" };
            _mockIAService.Setup(s => s.CreateBenchmarkAsync(It.IsAny<BenchmarkIACreateDTO>()))
                .ReturnsAsync(createdDto);

            // Act
            var result = await _controller.BenchmarkPost(createDto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task BenchmarkSync_SuccessfulSync_ReturnsOk()
        {
            // Arrange
            var benchmarks = new List<BenchmarkIADTO>
            {
                new BenchmarkIADTO { IdBenchmark = 1, ModelType = "cnn" }
            };
            _mockIAService.Setup(s => s.SyncBenchmarksFromPythonAsync()).ReturnsAsync(benchmarks);

            // Act
            var result = await _controller.BenchmarkSync();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task BenchmarkSync_HttpRequestException_Returns503()
        {
            // Arrange
            _mockIAService.Setup(s => s.SyncBenchmarksFromPythonAsync())
                .ThrowsAsync(new HttpRequestException("Service unavailable"));

            // Act
            var result = await _controller.BenchmarkSync();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            Assert.AreEqual(503, ((ObjectResult)result.Result).StatusCode);
        }

        [TestMethod]
        public async Task BenchmarkDelete_ExistingId_ReturnsNoContent()
        {
            // Arrange
            _mockIAService.Setup(s => s.DeleteBenchmarkAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.BenchmarkDelete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task BenchmarkDelete_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockIAService.Setup(s => s.DeleteBenchmarkAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _controller.BenchmarkDelete(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }
        #endregion
    }
}