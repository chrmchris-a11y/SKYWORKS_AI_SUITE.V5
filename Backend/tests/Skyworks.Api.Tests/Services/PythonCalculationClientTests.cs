using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Skyworks.Api.Services;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Skyworks.Api.Tests.Services
{
    public class PythonCalculationClientTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<PythonCalculationClient>> _mockLogger;
        private readonly PythonCalculationClient _client;

        public PythonCalculationClientTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost:8001")
            };
            
            _mockLogger = new Mock<ILogger<PythonCalculationClient>>();
            var options = Options.Create(new PythonServiceOptions { BaseUrl = "http://localhost:8001" });
            
            _client = new PythonCalculationClient(_httpClient, options, _mockLogger.Object);
        }

        [Fact]
        public async Task CalculateSAIL20Async_ValidRequest_ReturnsCorrectResponse()
        {
            // Arrange
            var expectedResponse = new PythonSAILResponseRaw { Sail = "IV", OsoCount = 18 };
            var responseJson = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
            });

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                });

            // Act
            var result = await _client.CalculateSAIL20Async(5, "b");

            // Assert
            Assert.Equal("IV", result.Sail);
            Assert.Equal(18, result.OsoCount);
            Assert.Null(result.Category);
        }

        [Fact]
        public async Task CalculateSAIL25Async_ValidRequest_ReturnsCorrectResponse()
        {
            // Arrange
            var expectedResponse = new PythonSAILResponseRaw { Sail = "V" };
            var responseJson = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
            });

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                });

            // Act
            var result = await _client.CalculateSAIL25Async(6, 4);

            // Assert
            Assert.Equal("V", result.Sail);
            
            // If soraVersion=="2.5" ⇒ discard any oso_count in Python echo; keep null in .NET
            Assert.Null(result.OsoCount);
            Assert.Null(result.Category);
        }

        [Fact]
        public async Task CalculateSAIL20Async_CategoryCResponse_ReturnsCorrectMapping()
        {
            // Arrange
            var expectedResponse = new PythonSAILResponseRaw { Category = "C" };
            var responseJson = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
            });

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                });

            // Act
            var result = await _client.CalculateSAIL20Async(8, "a");

            // Assert
            // SAIL feeds Annex-E; Category-C short-circuits SAIL
            Assert.Null(result.Sail);
            Assert.Null(result.OsoCount);
            Assert.Equal("C", result.Category);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task CalculateSAIL20Async_RetryableError_RetriesWithExponentialBackoff(HttpStatusCode errorCode)
        {
            // Arrange
            var callCount = 0;
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<CancellationToken>())
                .Returns(() =>
                {
                    callCount++;
                    if (callCount <= 3)
                    {
                        return Task.FromResult(new HttpResponseMessage { StatusCode = errorCode });
                    }
                    
                    var successResponse = new PythonSAILResponseRaw { Sail = "III" };
                    var responseJson = JsonSerializer.Serialize(successResponse, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                    });
                    return Task.FromResult(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseJson)
                    });
                });

            // Act
            var result = await _client.CalculateSAIL20Async(3, "c");

            // Assert
            Assert.Equal("III", result.Sail);
            Assert.Equal(4, callCount); // 3 retries + 1 success
        }

        [Fact]
        public async Task CalculateSAIL20Async_MaxRetriesExceeded_ThrowsHttpRequestException()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadGateway });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _client.CalculateSAIL20Async(3, "a"));
        }

        [Fact]
        public async Task CalculateSAIL20Async_NormalizesArcInput()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var expectedResponse = new PythonSAILResponseRaw { Sail = "I" };
            var responseJson = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
            });

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                });

            // Act
            await _client.CalculateSAIL20Async(1, "ARC_A");

            // Assert
            Assert.NotNull(capturedRequest);
            var requestContent = await capturedRequest.Content!.ReadAsStringAsync();
            Assert.Contains("\"residual_arc\":\"ARC-a\"", requestContent);
        }
    }
}

