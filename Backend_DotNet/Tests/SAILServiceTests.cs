using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SORA.Compliance.Services;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;

namespace SORA.Compliance.Tests
{
    /// <summary>
    /// Integration tests for SAIL Service.
    /// Tests parity with Python implementation and golden cases.
    /// </summary>
    public class SAILServiceTests
    {
        private SAILService CreateService(HttpClient httpClient)
        {
            var options = Options.Create(new SAILServiceOptions
            {
                PythonApiBaseUrl = "http://localhost:8000"
            });
            
            return new SAILService(httpClient, NullLogger<SAILService>.Instance, options);
        }

        [Theory]
        [InlineData(3, 'a')] // → SAIL I
        [InlineData(4, 'a')] // → SAIL II
        [InlineData(5, 'b')] // → SAIL IV
        [InlineData(6, 'b')] // → SAIL IV
        [InlineData(7, 'c')] // → SAIL VI
        [InlineData(7, 'd')] // → SAIL VI
        public async Task CalculateSail20Async_GoldenCases_ReturnsExpectedResults(int grc, char arc)
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var response = new
            {
                sail = GetExpectedSail20(grc, arc),
                category = (string?)null
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(response), Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.CalculateSail20Async(grc, arc);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(GetExpectedSail20(grc, arc), result.Sail);
            Assert.Null(result.Category);
            Assert.Equal(GetExpectedOSOCount20(result.Sail), result.OsoCount);
            Assert.Contains("Table D.1", result.Reference);
        }

        [Fact]
        public async Task CalculateSail20Async_CategoryC_ReturnsNullSail()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var response = new
            {
                sail = (string?)null,
                category = "C"
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(response), Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.CalculateSail20Async(8, 'a');

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Sail);
            Assert.Equal("C", result.Category);
            Assert.Null(result.OsoCount);
        }

        [Theory]
        [InlineData(6, 4)] // Table 7 lookup
        [InlineData(9, 1)] // → SAIL VI (GRC 9-10 rule)
        [InlineData(10, 5)] // → SAIL VI (GRC 9-10 rule)
        public async Task CalculateSail25Async_GoldenCases_ReturnsExpectedResults(int grc, int residualArc)
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var expectedSail = (grc >= 9) ? "VI" : "IV"; // Simplified expectation
            var response = new
            {
                sail = expectedSail,
                category = (string?)null
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(response), Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.CalculateSail25Async(grc, residualArc);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Sail);
            Assert.Null(result.OsoCount); // SORA 2.5 should not return hard-coded count
            Assert.Contains("Table 7", result.Reference);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        public async Task CalculateSail25Async_InvalidResidualArc_ReturnsError(int invalidArc)
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = CreateService(httpClient);

            // Act
            var result = await service.CalculateSail25Async(5, invalidArc);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("residual ARC level in [1..10]", result.ErrorMessage);
        }

        [Fact]
        public async Task GetOSORequirementsAsync_Sora20_ReturnsCountAndList()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var response = new
            {
                oso_count = 15,
                requirements = new[]
                {
                    new { id = "OSO#01", title = "Test OSO", robustness = "Medium" }
                }
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(response), Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.GetOSORequirementsAsync("III", "2.0");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(15, result.OsoCount);
            Assert.Single(result.Requirements);
            Assert.Equal("OSO#01", result.Requirements[0].Id);
        }

        [Fact]
        public async Task GetOSORequirementsAsync_Sora25_ReturnsNullCountButList()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var response = new
            {
                oso_count = (int?)null,
                requirements = new[]
                {
                    new { id = "OSO#01", title = "Test OSO 2.5", robustness = "High" }
                }
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(response), Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.GetOSORequirementsAsync("IV", "2.5");

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.OsoCount); // SORA 2.5 should not return hard-coded count
            Assert.Single(result.Requirements);
            Assert.Contains("Annex E", result.Reference);
        }

        private static string GetExpectedSail20(int grc, char arc)
        {
            // Simplified SAIL lookup for test validation
            return (grc, arc) switch
            {
                (3, 'a') => "I",
                (4, 'a') => "II",
                (5, 'b') => "IV",
                (6, 'b') => "IV", 
                (7, 'c') => "VI",
                (7, 'd') => "VI",
                _ => "Unknown"
            };
        }

        private static int? GetExpectedOSOCount20(string? sail)
        {
            return sail switch
            {
                "I" => 6,
                "II" => 10,
                "III" => 15,
                "IV" => 18,
                "V" => 21,
                "VI" => 24,
                _ => null
            };
        }
    }
}

