using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Repository.AI;
using AutoPulse.Shared.DTO.IA.Benchmark;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Api_c_sharp.ControllersMock.Tests
{
    [TestClass()]
    [TestCategory("unit")]
    public class IAControllerTests
    {
        private Mock<IIAService> _mockIAService;
        private IAController _controller;

        [TestInitialize]
        public void Initialize()
        {
            _mockIAService = new Mock<IIAService>();
            _controller = new IAController(_mockIAService.Object);
        }

        #region Predict Tests

        [TestMethod]
        public async Task Predict_CNN_Success()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "data:image/jpeg;base64,/9j/4AAQSkZJRgABA..."
            };

            var mockResult = new ResultatCNN
            {
                Success = true,
                Manufacturer = "Toyota",
                Model = "Corolla",
                FullName = "Toyota Corolla",
                ConfidenceScore = 0.95,
                Confidence = "95%"
            };

            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataAI>()))
                         .ReturnsAsync(mockResult)
                         .Verifiable();

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOfType(okResult.Value, typeof(ResultatCNN));
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Once);
        }

        [TestMethod]
        public async Task Predict_Prediction_Success()
        {
            // Arrange
            var dataPred = new DataPrediction
            {
                ProdYear = 2020,
                Mileage = "50000",
                EngineVolume = (float)2.0
            };

            var mockResult = new ResultatPrediction
            {
                Success = true,
                PredictedPrice = 15000.00
            };

            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataAI>()))
                         .ReturnsAsync(mockResult)
                         .Verifiable();

            // Act
            var result = await _controller.Predict(dataPred);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOfType(okResult.Value, typeof(ResultatPrediction));
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Once);
        }

        [TestMethod]
        public async Task Predict_Ajustement_Success()
        {
            // Arrange
            var dataAdj = new DataAjustement
            {
                BasePrice = 15000,
                Description = "Bon état, entretien régulier"
            };

            var mockResult = new ResultatAjustement
            {
                Success = true,
                BasePrice = 15000,
                AdjustedPrice = 16500.00,
                ReductionAmount = -1500,
                ReductionPercent = -10.0,
                QualityCoefficient = 1.1,
                Category = "Bon état",
                DescriptionAnalyzed = "Bon état général"
            };

            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataAI>()))
                         .ReturnsAsync(mockResult)
                         .Verifiable();

            // Act
            var result = await _controller.Predict(dataAdj);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOfType(okResult.Value, typeof(ResultatAjustement));
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Once);
        }

        [TestMethod]
        public async Task Predict_Failed()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "invalid_base64"
            };

            var mockResult = new ResultatCNN
            {
                Success = false,
                Error = "Image invalide"
            };

            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataAI>()))
                         .ReturnsAsync(mockResult)
                         .Verifiable();

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Once);
        }

        [TestMethod]
        public async Task Predict_InvalidData_CNN_MissingImage()
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
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
        }

        [TestMethod]
        public async Task Predict_InvalidData_Ajustement_InvalidPrice()
        {
            // Arrange
            var dataAdj = new DataAjustement
            {
                BasePrice = -100,
                Description = "Test"
            };

            // Act
            var result = await _controller.Predict(dataAdj);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
        }

        [TestMethod]
        public async Task Predict_InvalidData_Ajustement_MissingDescription()
        {
            // Arrange
            var dataAdj = new DataAjustement
            {
                BasePrice = 15000,
                Description = ""
            };

            // Act
            var result = await _controller.Predict(dataAdj);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
        }

        [TestMethod]
        public async Task Predict_Service_Unavailable()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "data:image/jpeg;base64,/9j/4AAQSkZJRgABA..."
            };

            _mockIAService.Setup(s => s.PredictAsync(It.IsAny<DataAI>()))
                         .ThrowsAsync(new Exception("Service indisponible"))
                         .Verifiable();

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(503, objectResult.StatusCode);
        }

        #endregion

        #region Health Check Tests

        [TestMethod]
        public async Task Health_Healthy()
        {
            // Arrange
            _mockIAService.Setup(s => s.HealthCheckAsync())
                         .ReturnsAsync(true)
                         .Verifiable();

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockIAService.Verify(s => s.HealthCheckAsync(), Times.Once);
        }

        [TestMethod]
        public async Task Health_Unhealthy()
        {
            // Arrange
            _mockIAService.Setup(s => s.HealthCheckAsync())
                         .ReturnsAsync(false)
                         .Verifiable();

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(503, objectResult.StatusCode);
            _mockIAService.Verify(s => s.HealthCheckAsync(), Times.Once);
        }

        #endregion

        #region Benchmark Get Tests

        [TestMethod]
        public async Task BenchmarkGetAll_Success()
        {
            // Arrange
            var benchmarks = new List<BenchmarkIAListDTO>
            {
                new BenchmarkIAListDTO
                {
                    IdBenchmark = 1,
                    ModelType = "cnn",
                    Timestamp = DateTime.UtcNow,
                    TotalIterations = 100,
                    SuccessRatePercent = 95.0,
                    AvgInferenceTimeMs = 45.5,
                    PredictionsPerSecond = 22.0
                },
                new BenchmarkIAListDTO
                {
                    IdBenchmark = 2,
                    ModelType = "prediction",
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    TotalIterations = 50,
                    SuccessRatePercent = 96.0,
                    AvgInferenceTimeMs = 32.5,
                    PredictionsPerSecond = 30.77
                }
            };

            _mockIAService.Setup(s => s.GetAllBenchmarksAsync())
                         .ReturnsAsync(benchmarks)
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkGetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var returnedBenchmarks = (IEnumerable<BenchmarkIAListDTO>)okResult.Value;
            Assert.AreEqual(2, returnedBenchmarks.Count());
            _mockIAService.Verify(s => s.GetAllBenchmarksAsync(), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkGetById_Success()
        {
            // Arrange
            var benchmark = new BenchmarkIADTO
            {
                IdBenchmark = 1,
                BenchmarkId = "bench_001",
                ModelType = "cnn",
                Timestamp = DateTime.UtcNow,
                SuccessRatePercent = 95.0,
                AvgInferenceTimeMs = 45.5
            };

            _mockIAService.Setup(s => s.GetBenchmarkByIdAsync(1))
                         .ReturnsAsync(benchmark)
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkGetById(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.AreEqual(benchmark.BenchmarkId, ((BenchmarkIADTO)okResult.Value).BenchmarkId);
            _mockIAService.Verify(s => s.GetBenchmarkByIdAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkGetById_NotFound()
        {
            // Arrange
            _mockIAService.Setup(s => s.GetBenchmarkByIdAsync(999))
                         .ReturnsAsync((BenchmarkIADTO)null)
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkGetById(999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            _mockIAService.Verify(s => s.GetBenchmarkByIdAsync(999), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkGetLatestByType_Success()
        {
            // Arrange
            var latestBenchmarks = new Dictionary<string, BenchmarkIADTO>
            {
                { "cnn", new BenchmarkIADTO { IdBenchmark = 1, ModelType = "cnn", BenchmarkId = "bench_cnn_001" } },
                { "prediction", new BenchmarkIADTO { IdBenchmark = 2, ModelType = "prediction", BenchmarkId = "bench_pred_001" } },
                { "ajustement", new BenchmarkIADTO { IdBenchmark = 3, ModelType = "ajustement", BenchmarkId = "bench_adj_001" } }
            };

            _mockIAService.Setup(s => s.GetLatestBenchmarksByTypeAsync())
                         .ReturnsAsync(latestBenchmarks)
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkGetLatestByType();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var returned = (Dictionary<string, BenchmarkIADTO>)okResult.Value;
            Assert.AreEqual(3, returned.Count);
            _mockIAService.Verify(s => s.GetLatestBenchmarksByTypeAsync(), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkGetStats_Success()
        {
            // Arrange
            var stats = new BenchmarkIAStatsDTO
            {
                TotalBenchmarks = 1,
                GlobalAvgInferenceTimeMs = 35.3,
                GlobalSuccessRate = 95.67,
                BenchmarksByModel = new Dictionary<string, int>
                {
                    { "cnn", 1 },
                    { "prediction", 1 },
                    { "ajustement", 1 }
                }
            };

            _mockIAService.Setup(s => s.GetBenchmarkStatsAsync())
                         .ReturnsAsync(stats)
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkGetStats();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOfType(okResult.Value, typeof(BenchmarkIAStatsDTO));
            _mockIAService.Verify(s => s.GetBenchmarkStatsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkGetHistoryByType_Success()
        {
            // Arrange
            var history = new List<BenchmarkIAListDTO>
            {
                new BenchmarkIAListDTO { IdBenchmark = 1, ModelType = "cnn", Timestamp = DateTime.UtcNow, TotalIterations = 100, SuccessRatePercent = 95.0, AvgInferenceTimeMs = 45.5, PredictionsPerSecond = 22.0 },
                new BenchmarkIAListDTO { IdBenchmark = 2, ModelType = "cnn", Timestamp = DateTime.UtcNow.AddHours(-1), TotalIterations = 100, SuccessRatePercent = 95.0, AvgInferenceTimeMs = 45.5, PredictionsPerSecond = 22.0 }
            };

            _mockIAService.Setup(s => s.GetBenchmarkHistoryByTypeAsync("cnn", 10))
                         .ReturnsAsync(history)
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkGetHistoryByType("cnn");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var returned = (IEnumerable<BenchmarkIAListDTO>)okResult.Value;
            Assert.AreEqual(2, returned.Count());
            _mockIAService.Verify(s => s.GetBenchmarkHistoryByTypeAsync("cnn", 10), Times.Once);
        }

        #endregion

        #region Benchmark Create Tests

        [TestMethod]
        public async Task BenchmarkPost_Success()
        {
            // Arrange
            var benchmarkDto = new BenchmarkIACreateDTO
            {
                BenchmarkId = "bench_new",
                ModelType = "cnn",
                Timestamp = DateTime.UtcNow,
                TotalIterations = 100,
                SuccessfulPredictions = 95,
                FailedPredictions = 5,
                SuccessRatePercent = 95.0,
                AvgInferenceTimeMs = 45.5,
                MinInferenceTimeMs = 40.0,
                MaxInferenceTimeMs = 50.0,
                StdInferenceTimeMs = 2.5,
                PredictionsPerSecond = 22.0,
                TotalTimeSeconds = 4.55,
                Platform = "Linux",
                Processor = "Intel Core i7",
                PythonVersion = "3.9.0",
                CpuCount = 8,
                MemoryTotalGb = 16.0,
                MemoryAvailableGb = 8.0
            };

            var createdBenchmark = new BenchmarkIADTO
            {
                IdBenchmark = 1,
                BenchmarkId = benchmarkDto.BenchmarkId,
                ModelType = benchmarkDto.ModelType
            };

            _mockIAService.Setup(s => s.CreateBenchmarkAsync(It.IsAny<BenchmarkIACreateDTO>()))
                         .ReturnsAsync(createdBenchmark)
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkPost(benchmarkDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result;
            Assert.AreEqual(nameof(_controller.BenchmarkGetById), createdResult.ActionName);
            _mockIAService.Verify(s => s.CreateBenchmarkAsync(It.IsAny<BenchmarkIACreateDTO>()), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkPost_InvalidData()
        {
            // Arrange
            var benchmarkDto = new BenchmarkIACreateDTO();
            _controller.ModelState.AddModelError("BenchmarkId", "BenchmarkId est requis");

            // Act
            var result = await _controller.BenchmarkPost(benchmarkDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockIAService.Verify(s => s.CreateBenchmarkAsync(It.IsAny<BenchmarkIACreateDTO>()), Times.Never);
        }

        #endregion

        #region Benchmark Delete Tests

        [TestMethod]
        public async Task BenchmarkDelete_Success()
        {
            // Arrange
            _mockIAService.Setup(s => s.DeleteBenchmarkAsync(1))
                         .ReturnsAsync(true)
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkDelete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockIAService.Verify(s => s.DeleteBenchmarkAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkDelete_NotFound()
        {
            // Arrange
            _mockIAService.Setup(s => s.DeleteBenchmarkAsync(999))
                         .ReturnsAsync(false)
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkDelete(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            _mockIAService.Verify(s => s.DeleteBenchmarkAsync(999), Times.Once);
        }

        #endregion

        #region Benchmark Sync Tests

        [TestMethod]
        public async Task BenchmarkSync_Success()
        {
            // Arrange
            var syncedBenchmarks = new List<BenchmarkIADTO>
            {
                new BenchmarkIADTO { IdBenchmark = 1, BenchmarkId = "bench_sync_001", ModelType = "cnn" },
                new BenchmarkIADTO { IdBenchmark = 2, BenchmarkId = "bench_sync_002", ModelType = "prediction" },
                new BenchmarkIADTO { IdBenchmark = 3, BenchmarkId = "bench_sync_003", ModelType = "ajustement" }
            };

            _mockIAService.Setup(s => s.SyncBenchmarksFromPythonAsync())
                         .ReturnsAsync(syncedBenchmarks)
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkSync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsNotNull(okResult.Value);
            _mockIAService.Verify(s => s.SyncBenchmarksFromPythonAsync(), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkSync_ServiceUnavailable()
        {
            // Arrange
            _mockIAService.Setup(s => s.SyncBenchmarksFromPythonAsync())
                         .ThrowsAsync(new HttpRequestException("Service IA indisponible"))
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkSync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(503, objectResult.StatusCode);
            _mockIAService.Verify(s => s.SyncBenchmarksFromPythonAsync(), Times.Once);
        }

        #endregion

        #region Additional Error Handling Tests

        [TestMethod]
        public async Task Predict_Prediction_MissingYear()
        {
            // Arrange
            var dataPred = new DataPrediction
            {
                ProdYear = null // Année manquante
            };

            // Act
            var result = await _controller.Predict(dataPred);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
        }

        [TestMethod]
        public async Task BenchmarkGetAll_Error()
        {
            // Arrange
            _mockIAService.Setup(s => s.GetAllBenchmarksAsync())
                         .ThrowsAsync(new Exception("Database connection error"))
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkGetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(500, objectResult.StatusCode);
            _mockIAService.Verify(s => s.GetAllBenchmarksAsync(), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkGetById_Error()
        {
            // Arrange
            _mockIAService.Setup(s => s.GetBenchmarkByIdAsync(It.IsAny<int>()))
                         .ThrowsAsync(new Exception("Database error"))
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkGetById(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(500, objectResult.StatusCode);
            _mockIAService.Verify(s => s.GetBenchmarkByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkGetStats_Error()
        {
            // Arrange
            _mockIAService.Setup(s => s.GetBenchmarkStatsAsync())
                         .ThrowsAsync(new Exception("Calculation error"))
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkGetStats();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(500, objectResult.StatusCode);
            _mockIAService.Verify(s => s.GetBenchmarkStatsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkGetHistoryByType_Error()
        {
            // Arrange
            _mockIAService.Setup(s => s.GetBenchmarkHistoryByTypeAsync(It.IsAny<string>(), It.IsAny<int>()))
                         .ThrowsAsync(new Exception("Query error"))
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkGetHistoryByType("cnn");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(500, objectResult.StatusCode);
            _mockIAService.Verify(s => s.GetBenchmarkHistoryByTypeAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkPost_Error()
        {
            // Arrange
            var benchmarkDto = new BenchmarkIACreateDTO
            {
                BenchmarkId = "bench_error",
                ModelType = "cnn",
                Timestamp = DateTime.UtcNow,
                TotalIterations = 100,
                SuccessfulPredictions = 95,
                FailedPredictions = 5,
                SuccessRatePercent = 95.0,
                AvgInferenceTimeMs = 45.5,
                MinInferenceTimeMs = 40.0,
                MaxInferenceTimeMs = 50.0,
                StdInferenceTimeMs = 2.5,
                PredictionsPerSecond = 22.0,
                TotalTimeSeconds = 4.55,
                Platform = "Linux",
                Processor = "Intel Core i7",
                PythonVersion = "3.9.0",
                CpuCount = 8,
                MemoryTotalGb = 16.0,
                MemoryAvailableGb = 8.0
            };

            _mockIAService.Setup(s => s.CreateBenchmarkAsync(It.IsAny<BenchmarkIACreateDTO>()))
                         .ThrowsAsync(new Exception("Database error"))
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkPost(benchmarkDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(500, objectResult.StatusCode);
            _mockIAService.Verify(s => s.CreateBenchmarkAsync(It.IsAny<BenchmarkIACreateDTO>()), Times.Once);
        }

        [TestMethod]
        public async Task BenchmarkDelete_Error()
        {
            // Arrange
            _mockIAService.Setup(s => s.DeleteBenchmarkAsync(It.IsAny<int>()))
                         .ThrowsAsync(new Exception("Delete error"))
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkDelete(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            _mockIAService.Verify(s => s.DeleteBenchmarkAsync(It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public async Task Health_Exception()
        {
            // Arrange
            _mockIAService.Setup(s => s.HealthCheckAsync())
                         .ThrowsAsync(new Exception("Connection error"))
                         .Verifiable();

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(503, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task BenchmarkSync_Error()
        {
            // Arrange
            _mockIAService.Setup(s => s.SyncBenchmarksFromPythonAsync())
                         .ThrowsAsync(new Exception("Sync error"))
                         .Verifiable();

            // Act
            var result = await _controller.BenchmarkSync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(500, objectResult.StatusCode);
            _mockIAService.Verify(s => s.SyncBenchmarksFromPythonAsync(), Times.Once);
        }

        [TestMethod]
        public async Task Predict_ModelState_Invalid()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "data:image/jpeg;base64,test"
            };

            _controller.ModelState.AddModelError("Type", "Type invalide");

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockIAService.Verify(s => s.PredictAsync(It.IsAny<DataAI>()), Times.Never);
        }

        #endregion
    }
}