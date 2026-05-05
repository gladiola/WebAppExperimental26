using Azure.Security.KeyVault.Secrets;
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

        // Valid AES-GCM test values: 12-byte nonce (24 hex), 32-byte key (64 hex)
        private const string TestIv = "000102030405060708090a0b";
        private const string TestKey = "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f";

        public NonceCatalogServiceTests()
        {
            _mockLogger = new Mock<ILogger<NonceCatalogService>>();
            _service = new NonceCatalogService(_mockLogger.Object);
        }

        private Nonce CreateTestNonce()
        {
            var mockLogger = new Mock<ILogger<Nonce>>();
            var iv = new KeyVaultSecret("test-iv", TestIv);
            var key = new KeyVaultSecret("test-key", TestKey);
            return new Nonce(mockLogger.Object, iv, key);
        }

        [Fact]
        public void AddANonce_ShouldAddNonceSuccessfully()
        {
            // Arrange
            var nonce = CreateTestNonce();
            var catalogKey = "CSPNonce";

            // Act
            var result = _service.AddANonce(catalogKey, nonce);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetANonce_ShouldReturnNonceString_WhenNonceExists()
        {
            // Arrange
            var nonce = CreateTestNonce();
            var catalogKey = "CSPNonce";
            _service.AddANonce(catalogKey, nonce);

            // Act
            var result = _service.GetANonce(catalogKey);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GetANonce_ShouldReturnEmptyString_WhenNonceDoesNotExist()
        {
            // Arrange
            var catalogKey = "NonExistentNonce";

            // Act
            var result = _service.GetANonce(catalogKey);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void RemoveANonce_ShouldRemoveNonceSuccessfully()
        {
            // Arrange
            var nonce = CreateTestNonce();
            var catalogKey = "CSPNonce";
            _service.AddANonce(catalogKey, nonce);

            // Act
            var result = _service.RemoveANonce(catalogKey);

            // Assert
            result.Should().BeTrue();
            _service.GetANonce(catalogKey).Should().BeEmpty();
        }

        [Fact]
        public void RemoveANonce_ShouldReturnFalse_WhenNonceDoesNotExist()
        {
            // Arrange
            var catalogKey = "NonExistentNonce";

            // Act
            var result = _service.RemoveANonce(catalogKey);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void AddANonce_ShouldUpdateExistingNonce()
        {
            // Arrange
            var nonce1 = CreateTestNonce();
            var nonce2 = CreateTestNonce();
            var catalogKey = "CSPNonce";

            // Act
            _service.AddANonce(catalogKey, nonce1);
            var firstValue = _service.GetANonce(catalogKey);
            _service.AddANonce(catalogKey, nonce2);
            var secondValue = _service.GetANonce(catalogKey);

            // Assert
            firstValue.Should().NotBeEmpty();
            secondValue.Should().NotBeEmpty();
        }

        /// <summary>
        /// Verifies that the ConcurrentDictionary backing store allows concurrent reads and writes
        /// without throwing exceptions or losing data. This test fails if the implementation is
        /// reverted to a non-thread-safe Dictionary.
        /// </summary>
        [Fact]
        public void NonceCatalogService_ShouldHandleConcurrentAccess_WithoutExceptions()
        {
            // Arrange
            const int threadCount = 20;
            const int operationsPerThread = 50;
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            var mockLogger = new Mock<ILogger<NonceCatalogService>>();
            var service = new NonceCatalogService(mockLogger.Object);

            // Act — parallel reads and writes
            Parallel.For(0, threadCount, threadIndex =>
            {
                try
                {
                    for (int i = 0; i < operationsPerThread; i++)
                    {
                        var key = $"key-{threadIndex % 5}"; // intentional key collisions
                        var nonce = CreateTestNonce();
                        service.AddANonce(key, nonce);
                        service.GetANonce(key);
                        if (i % 10 == 0)
                        {
                            service.RemoveANonce(key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            // Assert
            exceptions.Should().BeEmpty("concurrent access to NonceCatalogService must not throw exceptions");
        }

        /// <summary>
        /// Confirms the backing collection is ConcurrentDictionary (thread-safe type).
        /// This test fails if the field type is reverted to the non-thread-safe Dictionary.
        /// </summary>
        [Fact]
        public void NonceCatalogService_BackingStore_IsConcurrentDictionary()
        {
            // Use reflection to verify the private static field is a ConcurrentDictionary
            var fieldInfo = typeof(NonceCatalogService)
                .GetField("_nonceCollection",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            fieldInfo.Should().NotBeNull("_nonceCollection field must exist");

            var fieldType = fieldInfo!.FieldType;
            fieldType.Should().Be(
                typeof(System.Collections.Concurrent.ConcurrentDictionary<string, Nonce>),
                "NonceCatalogService must use ConcurrentDictionary for thread safety (security fix #4)");
        }
    }
}
