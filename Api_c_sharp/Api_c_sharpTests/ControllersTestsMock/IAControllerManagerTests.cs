using Api_c_sharp.Controllers;
using Api_c_sharp.Models.Entity;
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
    public class IAManagerMockTests
    {
        private Mock<AutoPulseBdContext> _mockContext;
        private Mock<IMapper> _mockMapper;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger<IAManager>> _mockLogger;
        private IAManager _manager;

        [TestInitialize]
        public void Initialize()
        {
            _mockContext = new Mock<AutoPulseBdContext>();
            _mockMapper = new Mock<IMapper>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<IAManager>>();

            _mockConfiguration.Setup(c => c["PythonAPI:BaseUrl"]).Returns("http://localhost:8000");

            _manager = new IAManager(
                _mockContext.Object,
                _mockMapper.Object,
                _httpClient,
                _mockConfiguration.Object,
                _mockLogger.Object
            );
        }

        [TestCleanup]
        public void Cleanup()
        {
            _httpClient?.Dispose();
        }

        #region PredictAsync Tests
        [TestMethod]
        public async Task PredictAsync_CNN_ValidData_ReturnsSuccessResult()
        {
            // Arrange
            var dataCNN = new DataCNN { ImageBase64 = "validBase64" };
            var expectedResponse = new ResultatCNN
            {
                Success = true,
                Manufacturer = "Toyota",
                Model = "Corolla",
                ConfidenceScore = 0.95f
            };

            SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expectedResponse));

            // Act
            var result = await _manager.PredictAsync(dataCNN);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ResultatCNN));
            var cnnResult = (ResultatCNN)result;
            Assert.IsTrue(cnnResult.Success);
            Assert.AreEqual("Toyota", cnnResult.Manufacturer);
        }

        [TestMethod]
        public async Task PredictAsync_Prediction_ValidData_ReturnsSuccessResult()
        {
            // Arrange
            var dataPrediction = new DataPrediction { ProdYear = 2020, Manufacturer = "Renault" };
            var expectedResponse = new ResultatPrediction
            {
                Success = true,
                PredictedPrice = 12500.50,
                Currency = "EUR"
            };

            SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expectedResponse));

            // Act
            var result = await _manager.PredictAsync(dataPrediction);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ResultatPrediction));
            var predResult = (ResultatPrediction)result;
            Assert.IsTrue(predResult.Success);
            Assert.AreEqual(12500.50, predResult.PredictedPrice);
        }

        [TestMethod]
        public async Task PredictAsync_Ajustement_ValidData_ReturnsSuccessResult()
        {
            // Arrange
            var dataAjustement = new DataAjustement { BasePrice = 15000, Description = "Test" };
            var expectedResponse = new ResultatAjustement
            {
                Success = true,
                AdjustedPrice = 16500,
                BasePrice = 15000
            };

            SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expectedResponse));

            // Act
            var result = await _manager.PredictAsync(dataAjustement);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ResultatAjustement));
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PredictAsync_HttpRequestFails_ThrowsException()
        {
            // Arrange
            var dataCNN = new DataCNN { ImageBase64 = "validBase64" };
            SetupHttpResponse(HttpStatusCode.ServiceUnavailable, "");

            // Act
            await _manager.PredictAsync(dataCNN);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PredictAsync_InvalidJson_ThrowsException()
        {
            // Arrange
            var dataCNN = new DataCNN { ImageBase64 = "valid" };
            SetupHttpResponse(HttpStatusCode.OK, "invalid json {{{");

            // Act
            await _manager.PredictAsync(dataCNN);
        }
        #endregion

        #region HealthCheckAsync Tests
        [TestMethod]
        public async Task HealthCheckAsync_ServiceHealthy_ReturnsTrue()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.OK, "");

            // Act
            var result = await _manager.HealthCheckAsync();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task HealthCheckAsync_ServiceUnhealthy_ReturnsFalse()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.ServiceUnavailable, "");

            // Act
            var result = await _manager.HealthCheckAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HealthCheckAsync_ExceptionThrown_ReturnsFalse()
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

        #region Benchmark Tests
        [TestMethod]
        public async Task GetAllBenchmarksAsync_ReturnsAllBenchmarks()
        {
            // Arrange
            var benchmarks = new List<BenchmarkIA>
            {
                new BenchmarkIA { IdBenchmark = 1, ModelType = "cnn" },
                new BenchmarkIA { IdBenchmark = 2, ModelType = "prediction" }
            };
            var benchmarkDtos = new List<BenchmarkIAListDTO>
            {
                new BenchmarkIAListDTO { IdBenchmark = 1, ModelType = "cnn" },
                new BenchmarkIAListDTO { IdBenchmark = 2, ModelType = "prediction" }
            };

            var mockDbSet = CreateMockDbSet(benchmarks);
            _mockContext.Setup(c => c.BenchmarksIA).Returns(mockDbSet.Object);
            _mockMapper.Setup(m => m.Map<IEnumerable<BenchmarkIAListDTO>>(It.IsAny<List<BenchmarkIA>>()))
                .Returns(benchmarkDtos);

            // Act
            var result = await _manager.GetAllBenchmarksAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetBenchmarkByIdAsync_ExistingId_ReturnsBenchmark()
        {
            // Arrange
            var benchmark = new BenchmarkIA { IdBenchmark = 1, ModelType = "cnn" };
            var benchmarkDto = new BenchmarkIADTO { IdBenchmark = 1, ModelType = "cnn" };

            var mockDbSet = CreateMockDbSet(new List<BenchmarkIA> { benchmark });
            _mockContext.Setup(c => c.BenchmarksIA).Returns(mockDbSet.Object);
            _mockMapper.Setup(m => m.Map<BenchmarkIADTO>(It.IsAny<BenchmarkIA>()))
                .Returns(benchmarkDto);

            // Act
            var result = await _manager.GetBenchmarkByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.IdBenchmark);
        }

        [TestMethod]
        public async Task CreateBenchmarkAsync_ValidData_ReturnsBenchmark()
        {
            // Arrange
            var createDto = new BenchmarkIACreateDTO { BenchmarkId = "bench_123", ModelType = "cnn" };
            var entity = new BenchmarkIA { IdBenchmark = 1, ModelType = "cnn" };
            var resultDto = new BenchmarkIADTO { IdBenchmark = 1, ModelType = "cnn" };

            _mockMapper.Setup(m => m.Map<BenchmarkIA>(createDto)).Returns(entity);
            _mockMapper.Setup(m => m.Map<BenchmarkIADTO>(entity)).Returns(resultDto);

            var mockDbSet = new Mock<DbSet<BenchmarkIA>>();
            _mockContext.Setup(c => c.BenchmarksIA).Returns(mockDbSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _manager.CreateBenchmarkAsync(createDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.IdBenchmark);
        }

        [TestMethod]
        public async Task DeleteBenchmarkAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            var benchmark = new BenchmarkIA { IdBenchmark = 1 };
            var mockDbSet = new Mock<DbSet<BenchmarkIA>>();
            mockDbSet.Setup(d => d.FindAsync(1)).ReturnsAsync(benchmark);

            _mockContext.Setup(c => c.BenchmarksIA).Returns(mockDbSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _manager.DeleteBenchmarkAsync(1);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteBenchmarkAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<BenchmarkIA>>();
            mockDbSet.Setup(d => d.FindAsync(999)).ReturnsAsync((BenchmarkIA)null);

            _mockContext.Setup(c => c.BenchmarksIA).Returns(mockDbSet.Object);

            // Act
            var result = await _manager.DeleteBenchmarkAsync(999);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region Helper Methods
        private void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockSet;
        }
        #endregion
    }

}