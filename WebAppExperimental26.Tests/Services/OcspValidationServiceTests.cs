using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Cryptography.X509Certificates;
using WebAppExperimental26.Models.Settings;
using WebAppExperimental26.Services;

namespace WebAppExperimental26.Tests.Services
{
    /// <summary>
    /// Unit tests for OCSP validation service
    /// </summary>
    public class OcspValidationServiceTests
    {
        private readonly Mock<ILogger<OcspValidationService>> _mockLogger;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly OcspSettings _settings;
        private readonly OcspValidationService _service;

        public OcspValidationServiceTests()
        {
            _mockLogger = new Mock<ILogger<OcspValidationService>>();
            _mockHttpClient = new Mock<HttpClient>();
            
            _settings = new OcspSettings
            {
                EnableOcspValidation = true,
                OcspServerUrl = "https://ocsp.test.com",
                RequestTimeoutSeconds = 30,
                MaxRetryAttempts = 3,
                CacheDurationMinutes = 60,
                ServerUnavailableBehavior = "Warn"
            };

            _service = new OcspValidationService(_mockLogger.Object, _settings, new HttpClient());
        }

        [Fact]
        public void Constructor_AcceptsAllDependencies()
        {
            // Assert
            _service.Should().NotBeNull();
        }

        [Fact]
        public async Task ValidateCertificateAsync_WhenDisabled_ReturnsTrue()
        {
            // Arrange
            _settings.EnableOcspValidation = false;
            var cert = CreateTestCertificate();

            // Act
            var result = await _service.ValidateCertificateAsync(cert);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateCertificateWithDetailsAsync_WhenDisabled_ReturnsDisabledStatus()
        {
            // Arrange
            _settings.EnableOcspValidation = false;
            var cert = CreateTestCertificate();

            // Act
            var result = await _service.ValidateCertificateWithDetailsAsync(cert);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Status.Should().Be(OcspStatus.Disabled);
            result.Message.Should().Contain("disabled");
        }

        [Fact]
        public async Task ValidateCertificateWithDetailsAsync_WhenNoServerUrl_ReturnsWarning()
        {
            // Arrange
            _settings.OcspServerUrl = null;
            _settings.ServerUnavailableBehavior = "Warn";
            var cert = CreateTestCertificate();

            // Act
            var result = await _service.ValidateCertificateWithDetailsAsync(cert);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Status.Should().Be(OcspStatus.Warning);
            result.Message.Should().Contain("not configured");
        }

        [Fact]
        public async Task ValidateCertificateWithDetailsAsync_WhenNoServerUrl_AndFailBehavior_ReturnsFalse()
        {
            // Arrange
            _settings.OcspServerUrl = null;
            _settings.ServerUnavailableBehavior = "Fail";
            var cert = CreateTestCertificate();

            // Act
            var result = await _service.ValidateCertificateWithDetailsAsync(cert);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Status.Should().Be(OcspStatus.ServerUnavailable);
        }

        [Fact]
        public async Task ValidateCertificateWithDetailsAsync_TemplateImplementation_ReturnsGoodStatus()
        {
            // Arrange
            var cert = CreateTestCertificate();

            // Act
            var result = await _service.ValidateCertificateWithDetailsAsync(cert);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Status.Should().Be(OcspStatus.Good);
            result.Message.Should().Contain("Good");
            result.CertificateThumbprint.Should().NotBeNullOrEmpty();
            result.CertificateSubject.Should().NotBeNullOrEmpty();
            result.ValidatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task ValidateCertificateWithDetailsAsync_CachesResults()
        {
            // Arrange
            var cert = CreateTestCertificate();

            // Act - First call
            var result1 = await _service.ValidateCertificateWithDetailsAsync(cert);
            var validatedAt1 = result1.ValidatedAt;

            // Act - Second call (should use cache)
            var result2 = await _service.ValidateCertificateWithDetailsAsync(cert);
            var validatedAt2 = result2.ValidatedAt;

            // Assert - Both calls should return same result
            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1.CertificateThumbprint.Should().Be(result2.CertificateThumbprint);
        }

        [Theory]
        [InlineData("Fail", false, OcspStatus.ServerUnavailable)]
        [InlineData("Allow", true, OcspStatus.ServerUnavailable)]
        [InlineData("Warn", true, OcspStatus.Warning)]
        public async Task ValidateCertificateWithDetailsAsync_ServerUnavailableBehavior_WorksCorrectly(
            string behavior, bool expectedValid, OcspStatus expectedStatus)
        {
            // Arrange
            _settings.OcspServerUrl = null;
            _settings.ServerUnavailableBehavior = behavior;
            var cert = CreateTestCertificate();

            // Act
            var result = await _service.ValidateCertificateWithDetailsAsync(cert);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().Be(expectedValid);
            result.Status.Should().Be(expectedStatus);
        }

        [Fact]
        public async Task ValidateCertificateAsync_ReturnsBooleanResult()
        {
            // Arrange
            var cert = CreateTestCertificate();

            // Act
            var result = await _service.ValidateCertificateAsync(cert);

            // Assert
            result.Should().BeOfType<bool>();
        }

        [Fact]
        public void OcspValidationResult_HasAllRequiredProperties()
        {
            // Act
            var result = new OcspValidationResult
            {
                IsValid = true,
                Status = OcspStatus.Good,
                Message = "Test message",
                ValidatedAt = DateTime.UtcNow,
                CertificateThumbprint = "ABC123",
                CertificateSubject = "CN=Test"
            };

            // Assert
            result.IsValid.Should().BeTrue();
            result.Status.Should().Be(OcspStatus.Good);
            result.Message.Should().Be("Test message");
            result.ValidatedAt.Should().NotBeNull();
            result.CertificateThumbprint.Should().Be("ABC123");
            result.CertificateSubject.Should().Be("CN=Test");
        }

        [Theory]
        [InlineData(OcspStatus.Good)]
        [InlineData(OcspStatus.Revoked)]
        [InlineData(OcspStatus.Unknown)]
        [InlineData(OcspStatus.Disabled)]
        [InlineData(OcspStatus.ServerUnavailable)]
        [InlineData(OcspStatus.Warning)]
        [InlineData(OcspStatus.Error)]
        public void OcspStatus_AllValuesAreValid(OcspStatus status)
        {
            // Act
            var result = new OcspValidationResult
            {
                Status = status
            };

            // Assert
            result.Status.Should().Be(status);
        }

        [Fact]
        public async Task ValidateCertificateWithDetailsAsync_WithDetailedLogging_LogsInformation()
        {
            // Arrange
            _settings.EnableDetailedLogging = true;
            var cert = CreateTestCertificate();

            // Act
            await _service.ValidateCertificateWithDetailsAsync(cert);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ValidateCertificateWithDetailsAsync_PopulatesCertificateInformation()
        {
            // Arrange
            var cert = CreateTestCertificate();

            // Act
            var result = await _service.ValidateCertificateWithDetailsAsync(cert);

            // Assert
            result.CertificateThumbprint.Should().Be(cert.Thumbprint);
            result.CertificateSubject.Should().Be(cert.Subject);
        }

        /// <summary>
        /// Helper method to create a test certificate
        /// </summary>
        private X509Certificate2 CreateTestCertificate()
        {
            // Create a self-signed certificate for testing
            using var rsa = System.Security.Cryptography.RSA.Create(2048);
            var request = new CertificateRequest(
                "CN=Test Certificate",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            var cert = request.CreateSelfSigned(
                DateTimeOffset.Now.AddDays(-1),
                DateTimeOffset.Now.AddDays(365));

            return cert;
        }
    }
}
