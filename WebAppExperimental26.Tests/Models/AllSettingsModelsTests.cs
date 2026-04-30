using WebAppExperimental26.Models.Settings;

namespace WebAppExperimental26.Tests.Models
{
    public class AllSettingsModelsTests
    {
        public class AzureADSettingsTests
        {
            [Fact]
            public void AllProperties_CanBeSet()
            {
                // Act
                var settings = new AzureADSettings
                {
                    Instance = "https://login.microsoftonline.com/",
                    Domain = "test.onmicrosoft.com",
                    TenantId = "test-tenant",
                    ClientId = "test-client",
                    CallbackPath = "/signin-oidc",
                    Authority = "https://login.microsoftonline.com/test-tenant",
                    ClientCredentials = new List<ClientCredential>()
                };

                // Assert
                settings.Instance.Should().Be("https://login.microsoftonline.com/");
                settings.Domain.Should().Be("test.onmicrosoft.com");
                settings.TenantId.Should().Be("test-tenant");
            }

            [Fact]
            public void ClientCredentials_CanBeEmpty()
            {
                // Act
                var settings = new AzureADSettings
                {
                    ClientCredentials = new List<ClientCredential>()
                };

                // Assert
                settings.ClientCredentials.Should().NotBeNull();
                settings.ClientCredentials.Should().BeEmpty();
            }
        }

        public class ClientCredentialTests
        {
            [Fact]
            public void Properties_CanBeSet()
            {
                // Act
                var credential = new ClientCredential
                {
                    SourceType = "ClientSecret",
                    ClientSecret = "test-secret"
                };

                // Assert
                credential.SourceType.Should().Be("ClientSecret");
                credential.ClientSecret.Should().Be("test-secret");
            }
        }

        public class BlobSettingsTests
        {
            [Fact]
            public void AllProperties_CanBeSet()
            {
                // Act
                var settings = new BlobSettings
                {
                    BlobConnectionString = "test-connection",
                    MaxAttachments = 10
                };

                // Assert
                settings.BlobConnectionString.Should().Be("test-connection");
                settings.MaxAttachments.Should().Be(10);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(100)]
            public void MaxAttachments_AcceptsValidValues(int maxAttachments)
            {
                // Act
                var settings = new BlobSettings { MaxAttachments = maxAttachments };

                // Assert
                settings.MaxAttachments.Should().Be(maxAttachments);
            }
        }

        public class CosmosDbSettingsTests
        {
            [Fact]
            public void AllProperties_CanBeSet()
            {
                // Act
                var settings = new CosmosDbSettings
                {
                    AccountEndpoint = "https://test.documents.azure.com:443/",
                    AccountKey = "test-key",
                    DatabaseName = "TestDb",
                    ContainerName = "TestContainer",
                    CosmosConnectionString = "AccountEndpoint=test"
                };

                // Assert
                settings.AccountEndpoint.Should().Be("https://test.documents.azure.com:443/");
                settings.DatabaseName.Should().Be("TestDb");
                settings.ContainerName.Should().Be("TestContainer");
            }
        }

        public class KeyVaultSettingsTests
        {
            [Fact]
            public void AllProperties_CanBeSet()
            {
                // Act
                var settings = new KeyVaultSettings
                {
                    KeyVaultURL = "https://test-vault.vault.azure.net/",
                    KeyVaultSecret = "test-secret",
                    KeyVaultPassName = "test-cert"
                };

                // Assert
                settings.KeyVaultURL.Should().Be("https://test-vault.vault.azure.net/");
                settings.KeyVaultSecret.Should().Be("test-secret");
                settings.KeyVaultPassName.Should().Be("test-cert");
            }
        }

        public class NonceEncryptionSettingsTests
        {
            [Fact]
            public void AllProperties_CanBeSet()
            {
                // Act
                var settings = new NonceEncryptionSettings
                {
                    Key = "test-key",
                    IV = "test-iv",
                    KeyVaultURL = "https://test-vault.vault.azure.net/",
                    IVSecret = "iv-secret",
                    NonceKeySecret = "nonce-key-secret"
                };

                // Assert
                settings.Key.Should().Be("test-key");
                settings.IV.Should().Be("test-iv");
                settings.KeyVaultURL.Should().Be("https://test-vault.vault.azure.net/");
            }
        }

        public class CSPScriptHashSettingsTests
        {
            [Fact]
            public void AllProperties_CanBeSet()
            {
                // Act
                var settings = new CSPScriptHashSettings
                {
                    ManuallyCalculatedInlineHash1 = "sha256-abc123",
                    ManuallyCalculatedInlineHash2 = "sha256-def456"
                };

                // Assert
                settings.ManuallyCalculatedInlineHash1.Should().Be("sha256-abc123");
                settings.ManuallyCalculatedInlineHash2.Should().Be("sha256-def456");
            }

            [Fact]
            public void Hashes_CanBeNull()
            {
                // Act
                var settings = new CSPScriptHashSettings();

                // Assert
                settings.ManuallyCalculatedInlineHash1.Should().BeNull();
                settings.ManuallyCalculatedInlineHash2.Should().BeNull();
            }
        }
    }
}
