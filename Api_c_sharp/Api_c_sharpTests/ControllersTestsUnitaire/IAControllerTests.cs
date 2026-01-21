using Api_c_sharp.Controllers;
using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.AI;
using AutoMapper;
using AutoPulse.Shared.DTO.IA.Benchmark;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Text.Json;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class IAControllerTests
    {
        private IAController _controller;
        private IAManager _manager;
        private ImageManager _imageManager = null!;
        private AnnonceManager _annonceManager = null!;
        private VoitureManager _voitureManager = null!;
        private AutoPulseBdContext _context;
        private IMapper _mapper;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger<IAManager>> _mockLogger;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_IA_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<IAManager>>();

            _mockConfiguration
                .Setup(x => x["PythonAPI:BaseUrl"])
                .Returns("http://localhost:8000");

            _manager = new IAManager(_context, _mapper, _httpClient, _mockConfiguration.Object, _mockLogger.Object);
            _imageManager = new ImageManager(_context);
            _annonceManager = new AnnonceManager(_context);
            _voitureManager = new VoitureManager(_context);
            _controller = new IAController(_manager, _imageManager, _annonceManager, _voitureManager);

            await _context.SaveChangesAsync();
        }

        #region Health Check Tests

        [TestMethod]
        public async Task HealthCheckAsync_Success()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _manager.HealthCheckAsync();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task HealthCheckAsync_Failure()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _manager.HealthCheckAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HealthCheckAsync_Exception()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            // Act
            var result = await _manager.HealthCheckAsync();

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region Benchmark CRUD Tests

        [TestMethod]
        public async Task CreateBenchmarkAsync_Success()
        {
            // Arrange
            var benchmarkDto = new BenchmarkIACreateDTO
            {
                BenchmarkId = "bench_001",
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

            // Act
            var result = await _manager.CreateBenchmarkAsync(benchmarkDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(benchmarkDto.BenchmarkId, result.BenchmarkId);
            Assert.AreEqual(benchmarkDto.ModelType, result.ModelType);
        }

        [TestMethod]
        public async Task GetBenchmarkByIdAsync_Success()
        {
            // Arrange
            var benchmarkDto = new BenchmarkIACreateDTO
            {
                BenchmarkId = "bench_002",
                ModelType = "prediction",
                Timestamp = DateTime.UtcNow,
                TotalIterations = 50,
                SuccessfulPredictions = 48,
                FailedPredictions = 2,
                SuccessRatePercent = 96.0,
                AvgInferenceTimeMs = 32.5,
                MinInferenceTimeMs = 30.0,
                MaxInferenceTimeMs = 35.0,
                StdInferenceTimeMs = 1.5,
                PredictionsPerSecond = 30.77,
                TotalTimeSeconds = 1.625,
                Platform = "Windows",
                Processor = "Intel Core i5",
                PythonVersion = "3.10.0",
                CpuCount = 4,
                MemoryTotalGb = 8.0,
                MemoryAvailableGb = 4.0
            };

            var created = await _manager.CreateBenchmarkAsync(benchmarkDto);

            // Act
            var result = await _manager.GetBenchmarkByIdAsync(created.IdBenchmark);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(created.IdBenchmark, result.IdBenchmark);
            Assert.AreEqual(benchmarkDto.BenchmarkId, result.BenchmarkId);
        }

        [TestMethod]
        public async Task GetBenchmarkByIdAsync_NotFound()
        {
            // Act
            var result = await _manager.GetBenchmarkByIdAsync(999);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAllBenchmarksAsync_Success()
        {
            // Arrange
            var benchmarks = new[]
            {
                new BenchmarkIACreateDTO
                {
                    BenchmarkId = "bench_003",
                    ModelType = "cnn",
                    Timestamp = DateTime.UtcNow.AddHours(-2),
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
                },
                new BenchmarkIACreateDTO
                {
                    BenchmarkId = "bench_004",
                    ModelType = "prediction",
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    TotalIterations = 50,
                    SuccessfulPredictions = 48,
                    FailedPredictions = 2,
                    SuccessRatePercent = 96.0,
                    AvgInferenceTimeMs = 32.5,
                    MinInferenceTimeMs = 30.0,
                    MaxInferenceTimeMs = 35.0,
                    StdInferenceTimeMs = 1.5,
                    PredictionsPerSecond = 30.77,
                    TotalTimeSeconds = 1.625,
                    Platform = "Windows",
                    Processor = "Intel Core i5",
                    PythonVersion = "3.10.0",
                    CpuCount = 4,
                    MemoryTotalGb = 8.0,
                    MemoryAvailableGb = 4.0
                }
            };

            foreach (var benchmark in benchmarks)
            {
                await _manager.CreateBenchmarkAsync(benchmark);
            }

            // Act
            var result = await _manager.GetAllBenchmarksAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task DeleteBenchmarkAsync_Success()
        {
            // Arrange
            var benchmarkDto = new BenchmarkIACreateDTO
            {
                BenchmarkId = "bench_005",
                ModelType = "ajustement",
                Timestamp = DateTime.UtcNow,
                TotalIterations = 75,
                SuccessfulPredictions = 72,
                FailedPredictions = 3,
                SuccessRatePercent = 96.0,
                AvgInferenceTimeMs = 28.0,
                MinInferenceTimeMs = 25.0,
                MaxInferenceTimeMs = 30.0,
                StdInferenceTimeMs = 1.0,
                PredictionsPerSecond = 35.71,
                TotalTimeSeconds = 2.1,
                Platform = "macOS",
                Processor = "Apple M1",
                PythonVersion = "3.11.0",
                CpuCount = 8,
                MemoryTotalGb = 16.0,
                MemoryAvailableGb = 10.0
            };

            var created = await _manager.CreateBenchmarkAsync(benchmarkDto);

            // Act
            var deleted = await _manager.DeleteBenchmarkAsync(created.IdBenchmark);

            // Assert
            Assert.IsTrue(deleted);
            var result = await _manager.GetBenchmarkByIdAsync(created.IdBenchmark);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task DeleteBenchmarkAsync_NotFound()
        {
            // Act
            var result = await _manager.DeleteBenchmarkAsync(999);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region Benchmark Stats and History Tests

        [TestMethod]
        public async Task GetLatestBenchmarksByTypeAsync_Success()
        {
            // Arrange
            var benchmarkCNN = new BenchmarkIACreateDTO
            {
                BenchmarkId = "bench_cnn_001",
                ModelType = "cnn",
                Timestamp = DateTime.UtcNow.AddHours(-1),
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

            await _manager.CreateBenchmarkAsync(benchmarkCNN);

            // Act
            var result = await _manager.GetLatestBenchmarksByTypeAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey("cnn"));
            Assert.AreEqual("bench_cnn_001", result["cnn"].BenchmarkId);
        }

        [TestMethod]
        public async Task GetBenchmarkHistoryByTypeAsync_Success()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                var benchmark = new BenchmarkIACreateDTO
                {
                    BenchmarkId = $"bench_history_{i}",
                    ModelType = "prediction",
                    Timestamp = DateTime.UtcNow.AddHours(-i),
                    TotalIterations = 50 + i,
                    SuccessfulPredictions = 48 + i,
                    FailedPredictions = 2,
                    SuccessRatePercent = 96.0,
                    AvgInferenceTimeMs = 32.5,
                    MinInferenceTimeMs = 30.0,
                    MaxInferenceTimeMs = 35.0,
                    StdInferenceTimeMs = 1.5,
                    PredictionsPerSecond = 30.77,
                    TotalTimeSeconds = 1.625,
                    Platform = "Windows",
                    Processor = "Intel Core i5",
                    PythonVersion = "3.10.0",
                    CpuCount = 4,
                    MemoryTotalGb = 8.0,
                    MemoryAvailableGb = 4.0
                };
                await _manager.CreateBenchmarkAsync(benchmark);
            }

            // Act
            var result = await _manager.GetBenchmarkHistoryByTypeAsync("prediction", 3);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public async Task GetBenchmarkStatsAsync_Success()
        {
            // Arrange
            var benchmarks = new[]
            {
                new BenchmarkIACreateDTO
                {
                    BenchmarkId = "stats_cnn",
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
                },
                new BenchmarkIACreateDTO
                {
                    BenchmarkId = "stats_pred",
                    ModelType = "prediction",
                    Timestamp = DateTime.UtcNow,
                    TotalIterations = 50,
                    SuccessfulPredictions = 48,
                    FailedPredictions = 2,
                    SuccessRatePercent = 96.0,
                    AvgInferenceTimeMs = 32.5,
                    MinInferenceTimeMs = 30.0,
                    MaxInferenceTimeMs = 35.0,
                    StdInferenceTimeMs = 1.5,
                    PredictionsPerSecond = 30.77,
                    TotalTimeSeconds = 1.625,
                    Platform = "Windows",
                    Processor = "Intel Core i5",
                    PythonVersion = "3.10.0",
                    CpuCount = 4,
                    MemoryTotalGb = 8.0,
                    MemoryAvailableGb = 4.0
                },
                new BenchmarkIACreateDTO
                {
                    BenchmarkId = "stats_adj",
                    ModelType = "ajustement",
                    Timestamp = DateTime.UtcNow,
                    TotalIterations = 75,
                    SuccessfulPredictions = 72,
                    FailedPredictions = 3,
                    SuccessRatePercent = 96.0,
                    AvgInferenceTimeMs = 28.0,
                    MinInferenceTimeMs = 25.0,
                    MaxInferenceTimeMs = 30.0,
                    StdInferenceTimeMs = 1.0,
                    PredictionsPerSecond = 35.71,
                    TotalTimeSeconds = 2.1,
                    Platform = "macOS",
                    Processor = "Apple M1",
                    PythonVersion = "3.11.0",
                    CpuCount = 8,
                    MemoryTotalGb = 16.0,
                    MemoryAvailableGb = 10.0
                }
            };

            foreach (var benchmark in benchmarks)
            {
                await _manager.CreateBenchmarkAsync(benchmark);
            }

            // Act
            var result = await _manager.GetBenchmarkStatsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.TotalBenchmarks);
            Assert.IsTrue(result.GlobalAvgInferenceTimeMs > 0);
            Assert.IsTrue(result.GlobalSuccessRate > 0);
            Assert.IsNotNull(result.BenchmarksByModel);
            Assert.AreEqual(3, result.BenchmarksByModel.Count);
        }

        #endregion

        #region Predict Integration Tests

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
                ConfidenceScore = 0.95
            };

            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(mockResult))
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _manager.PredictAsync(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ResultatCNN));
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task Predict_HttpError()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "invalid_base64"
            };

            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Internal Server Error")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            // Act
            await _manager.PredictAsync(dataCNN);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task Predict_InvalidJSON()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "data:image/jpeg;base64,/9j/4AAQSkZJRgABA..."
            };

            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("Invalid JSON response")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            // Act
            await _manager.PredictAsync(dataCNN);
        }

        #endregion

        #region Controller Integration Tests

        [TestMethod]
        public async Task BenchmarkGetAll_Controller_Success()
        {
            // Arrange
            var benchmarkDto = new BenchmarkIACreateDTO
            {
                BenchmarkId = "ctrl_bench_001",
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

            await _manager.CreateBenchmarkAsync(benchmarkDto);

            // Act
            var result = await _controller.BenchmarkGetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var benchmarks = (IEnumerable<BenchmarkIAListDTO>)okResult.Value;
            Assert.IsTrue(benchmarks.Any());
        }

        [TestMethod]
        public async Task BenchmarkGetById_Controller_Success()
        {
            // Arrange
            var benchmarkDto = new BenchmarkIACreateDTO
            {
                BenchmarkId = "ctrl_bench_002",
                ModelType = "prediction",
                Timestamp = DateTime.UtcNow,
                TotalIterations = 50,
                SuccessfulPredictions = 48,
                FailedPredictions = 2,
                SuccessRatePercent = 96.0,
                AvgInferenceTimeMs = 32.5,
                MinInferenceTimeMs = 30.0,
                MaxInferenceTimeMs = 35.0,
                StdInferenceTimeMs = 1.5,
                PredictionsPerSecond = 30.77,
                TotalTimeSeconds = 1.625,
                Platform = "Windows",
                Processor = "Intel Core i5",
                PythonVersion = "3.10.0",
                CpuCount = 4,
                MemoryTotalGb = 8.0,
                MemoryAvailableGb = 4.0
            };

            var created = await _manager.CreateBenchmarkAsync(benchmarkDto);

            // Act
            var result = await _controller.BenchmarkGetById(created.IdBenchmark);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOfType(okResult.Value, typeof(BenchmarkIADTO));
        }

        [TestMethod]
        public async Task BenchmarkGetById_Controller_NotFound()
        {
            // Act
            var result = await _controller.BenchmarkGetById(999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task BenchmarkGetStats_Controller_Success()
        {
            // Arrange
            var benchmarks = new[]
            {
                new BenchmarkIACreateDTO
                {
                    BenchmarkId = "stats_ctrl_cnn",
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
                },
                new BenchmarkIACreateDTO
                {
                    BenchmarkId = "stats_ctrl_pred",
                    ModelType = "prediction",
                    Timestamp = DateTime.UtcNow,
                    TotalIterations = 50,
                    SuccessfulPredictions = 48,
                    FailedPredictions = 2,
                    SuccessRatePercent = 96.0,
                    AvgInferenceTimeMs = 32.5,
                    MinInferenceTimeMs = 30.0,
                    MaxInferenceTimeMs = 35.0,
                    StdInferenceTimeMs = 1.5,
                    PredictionsPerSecond = 30.77,
                    TotalTimeSeconds = 1.625,
                    Platform = "Windows",
                    Processor = "Intel Core i5",
                    PythonVersion = "3.10.0",
                    CpuCount = 4,
                    MemoryTotalGb = 8.0,
                    MemoryAvailableGb = 4.0
                },
                new BenchmarkIACreateDTO
                {
                    BenchmarkId = "stats_ctrl_adj",
                    ModelType = "ajustement",
                    Timestamp = DateTime.UtcNow,
                    TotalIterations = 75,
                    SuccessfulPredictions = 72,
                    FailedPredictions = 3,
                    SuccessRatePercent = 96.0,
                    AvgInferenceTimeMs = 28.0,
                    MinInferenceTimeMs = 25.0,
                    MaxInferenceTimeMs = 30.0,
                    StdInferenceTimeMs = 1.0,
                    PredictionsPerSecond = 35.71,
                    TotalTimeSeconds = 2.1,
                    Platform = "macOS",
                    Processor = "Apple M1",
                    PythonVersion = "3.11.0",
                    CpuCount = 8,
                    MemoryTotalGb = 16.0,
                    MemoryAvailableGb = 10.0
                }
            };

            foreach (var benchmark in benchmarks)
            {
                await _manager.CreateBenchmarkAsync(benchmark);
            }

            // Act
            var result = await _controller.BenchmarkGetStats();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task BenchmarkDelete_Controller_Success()
        {
            // Arrange
            var benchmarkDto = new BenchmarkIACreateDTO
            {
                BenchmarkId = "del_bench",
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

            var created = await _manager.CreateBenchmarkAsync(benchmarkDto);

            // Act
            var result = await _controller.BenchmarkDelete(created.IdBenchmark);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task BenchmarkDelete_Controller_NotFound()
        {
            // Act
            var result = await _controller.BenchmarkDelete(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task Health_Controller_Success()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Health_Controller_Unavailable()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.Health();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(503, objectResult.StatusCode);
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task Predict_Controller_ValidationError_CNN()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "" // Image vide
            };

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Predict_Controller_ValidationError_Ajustement_InvalidPrice()
        {
            // Arrange
            var dataAdj = new DataAjustement
            {
                BasePrice = -100, // Prix négatif
                Description = "Test"
            };

            // Act
            var result = await _controller.Predict(dataAdj);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Predict_Controller_ValidationError_Ajustement_NoDescription()
        {
            // Arrange
            var dataAdj = new DataAjustement
            {
                BasePrice = 15000,
                Description = "" // Description vide
            };

            // Act
            var result = await _controller.Predict(dataAdj);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Predict_Controller_Service_Unavailable()
        {
            // Arrange
            var dataCNN = new DataCNN
            {
                ImageBase64 = "data:image/jpeg;base64,/9j/4AAQSkZJRgABA..."
            };

            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Service Error")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.Predict(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(503, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task BenchmarkPost_Controller_InvalidData()
        {
            // Arrange
            var benchmarkDto = new BenchmarkIACreateDTO();
            _controller.ModelState.AddModelError("BenchmarkId", "BenchmarkId est requis");

            // Act
            var result = await _controller.BenchmarkPost(benchmarkDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task BenchmarkGetAll_Controller_Error()
        {
            // Arrange - Créer une situation où un problème pourrait survenir
            var benchmarkDto = new BenchmarkIACreateDTO
            {
                BenchmarkId = "error_bench",
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

            await _manager.CreateBenchmarkAsync(benchmarkDto);

            // Act
            var result = await _controller.BenchmarkGetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsNotNull(okResult.Value);
        }

        [TestMethod]
        public async Task BenchmarkGetLatestByType_Controller_EmptyResult()
        {
            // Act - Pas de benchmarks créés
            var result = await _controller.BenchmarkGetLatestByType();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var benchmarks = (Dictionary<string, BenchmarkIADTO>)okResult.Value;
            Assert.AreEqual(0, benchmarks.Count);
        }

        #endregion

        #region Benchmark Sync Integration Tests

        [TestMethod]
        public async Task BenchmarkSync_Success()
        {
            // Arrange - Créer des données de test dans la base
            var image = new Image { IdImage = 1, IdVoiture = 1, Fichier = new byte[] { 1, 2, 3 } };
            var annonce = new Annonce { IdAnnonce = 1, Prix = 15000, Libelle = "Test", Description = "Test" };
            var voiture = new Voiture 
            { 
                IdVoiture = 1, 
                Annee = 2020,
                MarqueVoitureNavigation = new Marque() { LibelleMarque = "Toyota" },
                ModeleVoitureNavigation = new Modele() { LibelleModele = "Corolla" },
                CategorieVoitureNavigation = new Categorie() { LibelleCategorie = "Sedan" },
                InterieurCuire = true,
                CarburantVoitureNavigation = new Carburant() { LibelleCarburant = "Petrol" },
                CylindrerMoteur = 2.0,
                Kilometrage = 50000,
                NbCylindres = 4,
                BoiteVoitureNavigation = new BoiteDeVitesse() { LibelleBoite = "Manual" },
                MotriciteVoitureNavigation = new Motricite() { LibelleMotricite = "Front" },
                NbPorte = 4,
                PositionVolant = true,
                NbAirbag = 6
            };

            await _context.Images.AddAsync(image);
            await _context.Annonces.AddAsync(annonce);
            await _context.Voitures.AddAsync(voiture);
            await _context.SaveChangesAsync();

            // Mock pour les 3 appels de benchmark (CNN, Prediction, Ajustement)
            var cnnBenchmarkResponse = new
            {
                benchmark_id = "sync_cnn_001",
                timestamp = DateTime.UtcNow.ToString("o"),
                stats = new
                {
                    total_iterations = 1,
                    successful_predictions = 1,
                    failed_predictions = 0,
                    success_rate_percent = 100.0,
                    avg_inference_time_ms = 45.5,
                    min_inference_time_ms = 40.0,
                    max_inference_time_ms = 50.0,
                    std_inference_time_ms = 2.5,
                    predictions_per_second = 22.0,
                    total_time_seconds = 0.045
                },
                system_info = new
                {
                    platform = "Linux",
                    processor = "Intel Core i7",
                    python_version = "3.9.0",
                    cpu_count = 8,
                    system_memory_total_gb = 16.0,
                    system_memory_available_gb = 8.0
                }
            };

            var predictionBenchmarkResponse = new
            {
                benchmark_id = "sync_pred_001",
                timestamp = DateTime.UtcNow.ToString("o"),
                stats = new
                {
                    total_iterations = 1,
                    successful_predictions = 1,
                    failed_predictions = 0,
                    success_rate_percent = 100.0,
                    avg_inference_time_ms = 32.5,
                    min_inference_time_ms = 30.0,
                    max_inference_time_ms = 35.0,
                    std_inference_time_ms = 1.5,
                    predictions_per_second = 30.77,
                    total_time_seconds = 0.032
                },
                system_info = new
                {
                    platform = "Windows",
                    processor = "Intel Core i5",
                    python_version = "3.10.0",
                    cpu_count = 4,
                    system_memory_total_gb = 8.0,
                    system_memory_available_gb = 4.0
                }
            };

            var ajustementBenchmarkResponse = new
            {
                benchmark_id = "sync_adj_001",
                timestamp = DateTime.UtcNow.ToString("o"),
                stats = new
                {
                    total_iterations = 1,
                    successful_predictions = 1,
                    failed_predictions = 0,
                    success_rate_percent = 100.0,
                    avg_inference_time_ms = 28.0,
                    min_inference_time_ms = 25.0,
                    max_inference_time_ms = 30.0,
                    std_inference_time_ms = 1.0,
                    predictions_per_second = 35.71,
                    total_time_seconds = 0.028
                },
                system_info = new
                {
                    platform = "macOS",
                    processor = "Apple M1",
                    python_version = "3.11.0",
                    cpu_count = 8,
                    system_memory_total_gb = 16.0,
                    system_memory_available_gb = 10.0
                }
            };

            // Configuration du mock pour répondre différemment selon le type de benchmark
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/benchmark") &&
                        req.Content.ReadAsStringAsync().Result.Contains("\"type\":\"cnn\"")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(cnnBenchmarkResponse))
                });

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/benchmark") &&
                        req.Content.ReadAsStringAsync().Result.Contains("\"type\":\"prediction\"")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(predictionBenchmarkResponse))
                });

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/benchmark") &&
                        req.Content.ReadAsStringAsync().Result.Contains("\"type\":\"ajustement\"")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(ajustementBenchmarkResponse))
                });

            // Act
            var result = await _manager.SyncBenchmarksFromPythonAsync(
                new[] { new DataCNN { ImageBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 }) } },
                new[] { new DataAjustement { BasePrice = 15000, Description = "Test" } },
                new[] { new DataPrediction { ProdYear = 2020, Manufacturer = "Toyota", Model = "Corolla" } }
            );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());

            var resultList = result.ToList();
            Assert.IsTrue(resultList.Any(b => b.ModelType == "cnn"));
            Assert.IsTrue(resultList.Any(b => b.ModelType == "prediction"));
            Assert.IsTrue(resultList.Any(b => b.ModelType == "ajustement"));

            // Vérifier que les benchmarks ont bien été créés dans la base de données
            var dbBenchmarks = await _context.BenchmarksIA.ToListAsync();
            Assert.AreEqual(3, dbBenchmarks.Count);
        }

        [TestMethod]
        public async Task BenchmarkSync_PartialData_Success()
        {
            // Arrange - Créer uniquement les données pour CNN et Prediction
            var image = new Image() { IdImage = 1, IdVoiture = 1, Fichier = new byte[] { 1, 2, 3 } };
            var voiture = new Voiture 
            { 
                IdVoiture = 1, 
                Annee = 2020,
                MarqueVoitureNavigation = new Marque() { LibelleMarque = "Toyota" },
                ModeleVoitureNavigation = new Modele() { LibelleModele = "Corolla" },
                CategorieVoitureNavigation = new Categorie { LibelleCategorie = "Sedan" },
                InterieurCuire = true,
                CarburantVoitureNavigation = new Carburant() { LibelleCarburant = "Petrol" },
                CylindrerMoteur = 2.0,
                Kilometrage = 50000,
                NbCylindres = 4,
                BoiteVoitureNavigation = new BoiteDeVitesse() { LibelleBoite = "Manual" },
                MotriciteVoitureNavigation = new Motricite() { LibelleMotricite = "Front" },
                NbPorte = 4,
                PositionVolant = true,
                NbAirbag = 6
            };

            await _context.Images.AddAsync(image);
            await _context.Voitures.AddAsync(voiture);
            await _context.SaveChangesAsync();

            // Mock uniquement pour CNN et Prediction
            var cnnBenchmarkResponse = new
            {
                benchmark_id = "partial_cnn_001",
                timestamp = DateTime.UtcNow.ToString("o"),
                stats = new
                {
                    total_iterations = 1,
                    successful_predictions = 1,
                    failed_predictions = 0,
                    success_rate_percent = 100.0,
                    avg_inference_time_ms = 45.5,
                    min_inference_time_ms = 40.0,
                    max_inference_time_ms = 50.0,
                    std_inference_time_ms = 2.5,
                    predictions_per_second = 22.0,
                    total_time_seconds = 0.045
                },
                system_info = new
                {
                    platform = "Linux",
                    processor = "Intel Core i7",
                    python_version = "3.9.0",
                    cpu_count = 8,
                    system_memory_total_gb = 16.0,
                    system_memory_available_gb = 8.0
                }
            };

            var predictionBenchmarkResponse = new
            {
                benchmark_id = "partial_pred_001",
                timestamp = DateTime.UtcNow.ToString("o"),
                stats = new
                {
                    total_iterations = 1,
                    successful_predictions = 1,
                    failed_predictions = 0,
                    success_rate_percent = 100.0,
                    avg_inference_time_ms = 32.5,
                    min_inference_time_ms = 30.0,
                    max_inference_time_ms = 35.0,
                    std_inference_time_ms = 1.5,
                    predictions_per_second = 30.77,
                    total_time_seconds = 0.032
                },
                system_info = new
                {
                    platform = "Windows",
                    processor = "Intel Core i5",
                    python_version = "3.10.0",
                    cpu_count = 4,
                    system_memory_total_gb = 8.0,
                    system_memory_available_gb = 4.0
                }
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/benchmark") &&
                        req.Content.ReadAsStringAsync().Result.Contains("\"type\":\"cnn\"")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(cnnBenchmarkResponse))
                });

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/benchmark") &&
                        req.Content.ReadAsStringAsync().Result.Contains("\"type\":\"prediction\"")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(predictionBenchmarkResponse))
                });

            // Act - Ne passer que CNN et Prediction, pas Ajustement
            var result = await _manager.SyncBenchmarksFromPythonAsync(
                new[] { new DataCNN { ImageBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 }) } },
                null, // Pas de données d'ajustement
                new[] { new DataPrediction { ProdYear = 2020, Manufacturer = "Toyota", Model = "Corolla" } }
            );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsFalse(result.Any(b => b.ModelType == "ajustement"));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task BenchmarkSync_ApiUnavailable()
        {
            // Arrange - IMPORTANT: Il faut passer des données pour que la requête HTTP soit faite
            var cnnData = new[] { new DataCNN { ImageBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 }) } };

            // Mock qui retourne une erreur 503
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/benchmark")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("Service Unavailable")
                });

            // Act - Passer des données pour déclencher l'appel HTTP
            await _manager.SyncBenchmarksFromPythonAsync(cnnData, null, null);
        }

        [TestMethod]
        public async Task BenchmarkSync_Controller_Success()
        {
            // Arrange - Créer les données nécessaires dans la base
            for (int i = 1; i <= 9; i++)
            {
                var image = new Image { IdImage = i, IdVoiture = i, Fichier = new byte[] { 1, 2, 3 } };
                var annonce = new Annonce { IdAnnonce = i, Prix = 15000 + i * 1000, Libelle = "Test", Description = $"Test {i}" };
                var voiture = new Voiture 
                { 
                    IdVoiture = i, 
                    Annee = 2020 + i,
                    MarqueVoitureNavigation = new Marque() { LibelleMarque = "Toyota" },
                    ModeleVoitureNavigation = new Modele() { LibelleModele = "Corolla" },
                    CategorieVoitureNavigation = new Categorie() { LibelleCategorie = "Sedan" },
                    InterieurCuire = true,
                    CarburantVoitureNavigation = new Carburant() { LibelleCarburant = "Petrol" },
                    CylindrerMoteur = 2.0,
                    Kilometrage = 50000,
                    NbCylindres = 4,
                    BoiteVoitureNavigation = new BoiteDeVitesse() { LibelleBoite = "Manual" },
                    MotriciteVoitureNavigation = new Motricite() { LibelleMotricite = "Front" },
                    NbPorte = 4,
                    PositionVolant = true,
                    NbAirbag = 6
                };

                await _context.Images.AddAsync(image);
                await _context.Annonces.AddAsync(annonce);
                await _context.Voitures.AddAsync(voiture);
            }
            await _context.SaveChangesAsync();

            // Configuration des mocks comme dans BenchmarkSync_Success
            var benchmarkResponse = new
            {
                benchmark_id = "ctrl_sync",
                timestamp = DateTime.UtcNow.ToString("o"),
                stats = new
                {
                    total_iterations = 9,
                    successful_predictions = 9,
                    failed_predictions = 0,
                    success_rate_percent = 100.0,
                    avg_inference_time_ms = 45.5,
                    min_inference_time_ms = 40.0,
                    max_inference_time_ms = 50.0,
                    std_inference_time_ms = 2.5,
                    predictions_per_second = 22.0,
                    total_time_seconds = 0.41
                },
                system_info = new
                {
                    platform = "Linux",
                    processor = "Intel Core i7",
                    python_version = "3.9.0",
                    cpu_count = 8,
                    system_memory_total_gb = 16.0,
                    system_memory_available_gb = 8.0
                }
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/benchmark")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(benchmarkResponse))
                });

            // Act
            var result = await _controller.BenchmarkSync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var okResult = (ObjectResult)result.Result;
            Assert.IsNotNull(okResult.Value);
        }

        [TestMethod]
        public async Task BenchmarkSync_Controller_ApiError()
        {
            // Arrange - Créer des données minimales
            var image = new Image { IdImage = 1, IdVoiture = 1, Fichier = new byte[] { 1, 2, 3 } };
            await _context.Images.AddAsync(image);
            await _context.SaveChangesAsync();

            // Mock qui retourne une erreur 500
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/benchmark")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Internal Server Error")
                });

            // Act
            var result = await _controller.BenchmarkSync();

            // Assert - Le controller doit attraper l'exception et retourner 500
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(500, objectResult.StatusCode); // Changé de 503 à 500
        }

        #endregion
    }
}