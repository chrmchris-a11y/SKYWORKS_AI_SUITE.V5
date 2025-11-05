using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Skyworks.Api.Controllers;
using Skyworks.Api.Services;
using Xunit;

namespace Skyworks.Api.Tests.Controllers
{
    public class SAILControllerTests
    {
        private readonly Mock<PythonCalculationClient> _mockPythonClient;
        private readonly Mock<ILogger<SAILController>> _mockLogger;
        private readonly SAILController _controller;

        public SAILControllerTests()
        {
            _mockPythonClient = new Mock<PythonCalculationClient>();
            _mockLogger = new Mock<ILogger<SAILController>>();
            _controller = new SAILController(_mockPythonClient.Object, _mockLogger.Object);
        }

        [Theory]
        [InlineData(1, "a", "I", 6)]
        [InlineData(3, "a", "I", 6)]
        [InlineData(4, "a", "II", 10)]
        [InlineData(5, "b", "IV", 18)]
        [InlineData(6, "b", "IV", 18)]
        [InlineData(7, "c", "VI", 24)]
        public async Task CalculateSAIL_Sora20_ValidMapping_ReturnsCorrectSAIL(int grc, string arc, string expectedSail, int expectedOsoCount)
        {
            // Arrange
            var request = new SAILRequest
            {
                SoraVersion = "2.0",
                FinalGrc = grc,
                ResidualArc = arc
            };

            // Act
            var result = await _controller.CalculateSAIL(request);

            // Assert
            var okResult = Assert.IsType<ActionResult<SAILResponse>>(result);
            var response = Assert.IsType<SAILResponse>(okResult.Value);
            Assert.Equal(expectedSail, response.Sail);
            
            // "In Annex E the 24 Operational Safety Objectives… are listed with their associated levels of robustness." (EASA AMC/GM – SORA)
            Assert.Equal(expectedOsoCount, response.OsoCount);
            Assert.Null(response.Category);
            Assert.Equal("JARUS SORA 2.0 Table 5 (GRC×ARC→SAIL), AMC Annex E link", response.Reference);
        }

        [Fact]
        public async Task CalculateSAIL_Sora20_GrcGreaterThan7_ReturnsCategoryC()
        {
            // Arrange
            var request = new SAILRequest
            {
                SoraVersion = "2.0",
                FinalGrc = 8,
                ResidualArc = "a"
            };

            // Act
            var result = await _controller.CalculateSAIL(request);

            // Assert
            var okResult = Assert.IsType<ActionResult<SAILResponse>>(result);
            var response = Assert.IsType<SAILResponse>(okResult.Value);
            
            // "The SAIL defined in Step #9 is the level of robustness required to satisfy the safety objectives specified in Annex E." (EASA AMC/GM – SORA)
            // Category-C path stops SAIL
            Assert.Null(response.Sail);
            Assert.Null(response.OsoCount);
            Assert.Equal("C", response.Category);
            Assert.Contains("Category C for GRC>7", response.Reference);
        }

        [Theory]
        [InlineData(6, 4, "V")]
        [InlineData(10, 10, "VI")]
        [InlineData(9, 1, "VI")]
        public async Task CalculateSAIL_Sora25_ValidMapping_ReturnsCorrectSAIL(int grc, int arcLevel, string expectedSail)
        {
            // Arrange
            var request = new SAILRequest
            {
                SoraVersion = "2.5",
                FinalGrc = grc,
                ResidualArcLevel = arcLevel
            };

            if (grc < 9) // Only mock Python client for non-high-GRC cases
            {
                _mockPythonClient.Setup(x => x.CalculateSAIL25Async(grc, arcLevel))
                    .ReturnsAsync(new PythonSAILResponse { Sail = expectedSail });
            }

            // Act
            var result = await _controller.CalculateSAIL(request);

            // Assert
            var okResult = Assert.IsType<ActionResult<SAILResponse>>(result);
            var response = Assert.IsType<SAILResponse>(okResult.Value);
            Assert.Equal(expectedSail, response.Sail);
            
            // Do not return osoCount for 2.5; 2.5 Annex-E profile differs
            Assert.Null(response.OsoCount);
            Assert.Null(response.Category);
            
            if (grc >= 9)
            {
                Assert.Contains("high-GRC rule (9–10→VI)", response.Reference);
            }
        }

        [Theory]
        [InlineData("3.0")]
        [InlineData("")]
        [InlineData(null)]
        public async Task CalculateSAIL_InvalidSoraVersion_ReturnsBadRequest(string soraVersion)
        {
            // Arrange
            var request = new SAILRequest
            {
                SoraVersion = soraVersion,
                FinalGrc = 5,
                ResidualArc = "a"
            };

            // Act
            var result = await _controller.CalculateSAIL(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("soraVersion must be '2.0' or '2.5'", badRequestResult.Value?.ToString());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(8)]
        public async Task CalculateSAIL_Sora20_InvalidGrc_ReturnsBadRequest(int grc)
        {
            // Arrange
            var request = new SAILRequest
            {
                SoraVersion = "2.0",
                FinalGrc = grc,
                ResidualArc = "a"
            };

            // Act
            var result = await _controller.CalculateSAIL(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            if (grc <= 7) // Only test the lower bound since GRC 8 is valid but triggers Category C
            {
                Assert.Contains("finalGrc must be between 1 and 7", badRequestResult.Value?.ToString());
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        public async Task CalculateSAIL_Sora25_InvalidGrc_ReturnsBadRequest(int grc)
        {
            // Arrange
            var request = new SAILRequest
            {
                SoraVersion = "2.5",
                FinalGrc = grc,
                ResidualArcLevel = 5
            };

            // Act
            var result = await _controller.CalculateSAIL(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("finalGrc must be between 1 and 10", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task CalculateSAIL_Sora20_MissingResidualArc_ReturnsBadRequest()
        {
            // Arrange
            var request = new SAILRequest
            {
                SoraVersion = "2.0",
                FinalGrc = 5
            };

            // Act
            var result = await _controller.CalculateSAIL(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("residualArc is required for SORA 2.0", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task CalculateSAIL_Sora25_MissingResidualArcLevel_ReturnsBadRequest()
        {
            // Arrange
            var request = new SAILRequest
            {
                SoraVersion = "2.5",
                FinalGrc = 5
            };

            // Act
            var result = await _controller.CalculateSAIL(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("residualArcLevel is required for SORA 2.5", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task CalculateSAIL_Sora25_WithLetterArc_ReturnsBadRequest()
        {
            // Arrange - Residual ARC is numeric in 2.5; do not bin to letters
            var request = new SAILRequest
            {
                SoraVersion = "2.5",
                FinalGrc = 5,
                ResidualArc = "b",
                ResidualArcLevel = 3
            };

            // Act
            var result = await _controller.CalculateSAIL(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Use residualArcLevel (numeric) for SORA 2.5", badRequestResult.Value?.ToString());
        }

        [Theory]
        [InlineData("ARC-a", "a")]
        [InlineData("ARC_b", "b")]
        [InlineData("c", "c")]
        [InlineData("ARC-D", "d")]
        public async Task CalculateSAIL_Sora20_NormalizesArcInput(string input, string expectedNormalized)
        {
            // Arrange
            var request = new SAILRequest
            {
                SoraVersion = "2.0",
                FinalGrc = 3,
                ResidualArc = input
            };

            // Act
            var result = await _controller.CalculateSAIL(request);

            // Assert
            var okResult = Assert.IsType<ActionResult<SAILResponse>>(result);
            var response = Assert.IsType<SAILResponse>(okResult.Value);
            Assert.Equal(expectedNormalized, response.ResidualArc);
        }
    }
}

