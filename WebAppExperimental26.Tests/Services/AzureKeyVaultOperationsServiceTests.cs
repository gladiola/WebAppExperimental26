using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using WebAppExperimental26.Services;
using WebAppExperimental26.AzureKeyVaultOperations;
using WebAppExperimental26.Models.Settings;

namespace WebAppExperimental26.Tests.Services
{
    public class AzureKeyVaultOperationsServiceTests
    {
        private readonly Mock<ILogger<AzureKeyVaultOperationsService>> _mockLogger;
        private readonly Mock<IKeyVaultSettingsService> _mockKvSettings;
        private readonly Mock<IAzureADSettingsService> _mockAadSettings;
        private readonly Mock<INonceEncryptionSettingsService> _mockNonceSettings;
        private readonly Mock<IAzureKeyVaultCertificateOperations> _mockCertOps;
        private readonly AzureKeyVaultOperationsService _service;

        public AzureKeyVaultOperationsServiceTests()
        {
            _mockLogger = new Mock<ILogger<AzureKeyVaultOperationsService>>();
            _mockKvSettings = new Mock<IKeyVaultSettingsService>();
            _mockAadSettings = new Mock<IAzureADSettingsService>();
            _mockNonceSettings = new Mock<INonceEncryptionSettingsService>();
            _mockCertOps = new Mock<IAzureKeyVaultCertificateOperations>();

            _service = new AzureKeyVaultOperationsService(
                _mockLogger.Object,
                _mockKvSettings.Object,
                _mockAadSettings.Object,
                _mockNonceSettings.Object,
                _mockCertOps.Object);
        }

        [Fact]
        public void Constructor_ShouldAcceptAllDependencies()
        {
            // Assert
            _service.Should().NotBeNull();
        }

        [Fact]
        public async Task FetchCertificateServer_ShouldCallCertificateOperations()
        {
            // Arrange
            var kvSettings = new KeyVaultSettings
            {
                KeyVaultURL = "https://test-vault.vault.azure.net/",
                KeyVaultSecret = "test-cert",
                KeyVaultPassName = "test-pass"
            };
            var aadSettings = new AzureADSettings
            {
                TenantId = "test-tenant",
                ClientId = "test-client"
            };

            _mockKvSettings.Setup(x => x.GetSettings()).Returns(kvSettings);
            _mockAadSettings.Setup(x => x.GetSettings()).Returns(aadSettings);

            // Act
            await _service.FetchCertificateServer();

            // Assert
            _mockCertOps.Verify(
                x => x.GetCertificateFromKeyVault(
                    aadSettings.TenantId,
                    aadSettings.ClientId,
                    kvSettings.KeyVaultURL,
                    kvSettings.KeyVaultSecret,
                    kvSettings.KeyVaultPassName),
                Times.Once);
        }

        [Fact]
        public async Task FetchSecretIVSecret_ShouldCallGetSecret()
        {
            // Arrange
            var aadSettings = new AzureADSettings
            {
                TenantId = "test-tenant",
                ClientId = "test-client",
                ClientCredentials = new List<ClientCredential>
                {
                    new ClientCredential
                    {
                        SourceType = "ApplicationSecret",
                        ClientSecret = "test-secret"
                    }
                }
            };
            var nonceSettings = new NonceEncryptionSettings
            {
                KeyVaultURL = "https://test-vault.vault.azure.net/",
                IVSecret = "iv-secret"
            };

            _mockAadSettings.Setup(x => x.GetSettings()).Returns(aadSettings);
            _mockNonceSettings.Setup(x => x.GetSettings()).Returns(nonceSettings);

            var expectedSecret = new KeyVaultSecret("iv-secret", "test-value");
            _mockCertOps.Setup(x => x.GetSecretFromKeyVault(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(expectedSecret);

            // Act
            var result = await _service.FetchSecretIVSecret();

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("iv-secret");
        }

        [Fact]
        public async Task FetchSecretNonceKeySecret_ShouldCallGetSecret()
        {
            // Arrange
            var aadSettings = new AzureADSettings
            {
                TenantId = "test-tenant",
                ClientId = "test-client",
                ClientCredentials = new List<ClientCredential>
                {
                    new ClientCredential
                    {
                        SourceType = "ApplicationSecret",
                        ClientSecret = "test-secret"
                    }
                }
            };
            var nonceSettings = new NonceEncryptionSettings
            {
                KeyVaultURL = "https://test-vault.vault.azure.net/",
                NonceKeySecret = "nonce-key-secret"
            };

            _mockAadSettings.Setup(x => x.GetSettings()).Returns(aadSettings);
            _mockNonceSettings.Setup(x => x.GetSettings()).Returns(nonceSettings);

            var expectedSecret = new KeyVaultSecret("nonce-key-secret", "test-value");
            _mockCertOps.Setup(x => x.GetSecretFromKeyVault(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(expectedSecret);

            // Act
            var result = await _service.FetchSecretNonceKeySecret();

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("nonce-key-secret");
        }

        [Fact]
        public void FetchSecret_ShouldThrowNotImplementedException()
        {
            // Act
            Func<Task> act = async () => await _service.FetchSecret();

            // Assert
            act.Should().ThrowAsync<NotImplementedException>();
        }

        [Fact]
        public void FetchCertificate_ShouldThrowNotImplementedException()
        {
            // Act
            Func<Task> act = async () => await _service.FetchCertificate();

            // Assert
            act.Should().ThrowAsync<NotImplementedException>();
        }
    }
}
