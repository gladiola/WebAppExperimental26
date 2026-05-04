using Microsoft.Extensions.Logging;
using WebAppExperimental26.Models.Main_Objects;
using WebAppExperimental26.Models.Settings;
using WebAppExperimental26.Services;

namespace WebAppExperimental26.Tests.Services
{
    public class NonceCatalogServiceTests
    {
        private readonly Mock<ILogger<NonceCatalogService>> _mockLogger;
        private readonly NonceCatalogService _service;

        public NonceCatalogServiceTests()
        {
            _mockLogger = new Mock<ILogger<NonceCatalogService>>();
            _service = new NonceCatalogService(_mockLogger.Object);
        }

        [Fact]
        public void AddANonce_ShouldAddNonceSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Nonce>>();
            var nonce = new Nonce(mockLogger.Object, "testIV", "testKey");
            var key = "CSPNonce";

            // Act
            var result = _service.AddANonce(key, nonce);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetANonce_ShouldReturnNonceString_WhenNonceExists()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Nonce>>();
            var nonce = new Nonce(mockLogger.Object, "testIV", "testKey");
            var key = "CSPNonce";
            _service.AddANonce(key, nonce);

            // Act
            var result = _service.GetANonce(key);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GetANonce_ShouldReturnEmptyString_WhenNonceDoesNotExist()
        {
            // Arrange
            var key = "NonExistentNonce";

            // Act
            var result = _service.GetANonce(key);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void RemoveANonce_ShouldRemoveNonceSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Nonce>>();
            var nonce = new Nonce(mockLogger.Object, "testIV", "testKey");
            var key = "CSPNonce";
            _service.AddANonce(key, nonce);

            // Act
            var result = _service.RemoveANonce(key);

            // Assert
            result.Should().BeTrue();
            _service.GetANonce(key).Should().BeEmpty();
        }

        [Fact]
        public void RemoveANonce_ShouldReturnFalse_WhenNonceDoesNotExist()
        {
            // Arrange
            var key = "NonExistentNonce";

            // Act
            var result = _service.RemoveANonce(key);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void AddANonce_ShouldUpdateExistingNonce()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Nonce>>();
            var nonce1 = new Nonce(mockLogger.Object, "testIV1", "testKey1");
            var nonce2 = new Nonce(mockLogger.Object, "testIV2", "testKey2");
            var key = "CSPNonce";

            // Act
            _service.AddANonce(key, nonce1);
            var firstValue = _service.GetANonce(key);
            _service.AddANonce(key, nonce2);
            var secondValue = _service.GetANonce(key);

            // Assert
            firstValue.Should().NotBeEmpty();
            secondValue.Should().NotBeEmpty();
            secondValue.Should().NotBe(firstValue);
        }
    }
}
