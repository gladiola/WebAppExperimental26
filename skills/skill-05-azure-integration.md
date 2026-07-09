# Skill 5 — Azure Integration

## What This Skill Covers

Connecting an ASP.NET Core application to the three core Azure services used in this project: **Azure Key Vault** (secrets and certificates), **Azure Cosmos DB** (NoSQL document database), and **Azure Blob Storage** (object storage). Each service has its own SDK, authentication model, and startup verification pattern. This skill also covers the Azure AD service principal model that underpins authentication to all Azure services.

---

## Key Concepts

### Azure Fundamentals
- Azure subscriptions, resource groups, and regions
- Azure portal vs. Azure CLI (`az`) vs. ARM/Bicep templates for provisioning
- Service principals: an application identity in Azure AD used by code running outside Azure
- Managed Identity: a service principal automatically managed by Azure for services running inside Azure (no secret to rotate)
- Role-Based Access Control (RBAC): assigning roles (e.g., `Key Vault Secrets User`) to a service principal or managed identity

### Azure Key Vault
- Two types of Key Vault objects:
  - **Secrets** — arbitrary key/value pairs (connection strings, encryption keys, PFX bytes)
  - **Certificates** — managed X.509 certificates with optional auto-renewal policies
- `Azure.Security.KeyVault.Secrets` NuGet package and `SecretClient`
- Authentication: `ClientSecretCredential(tenantId, clientId, clientSecret)` for service principals; `DefaultAzureCredential` for Managed Identity
- `SecretClient.GetSecretAsync(name)` — retrieving a secret by name
- Soft delete and purge protection — understanding the lifecycle of deleted secrets
- Using Key Vault to store nonce encryption keys, connection strings, and PFX certificates

### Azure Cosmos DB
- NoSQL document model: databases, containers, items (JSON documents)
- `Microsoft.Azure.Cosmos` NuGet package and `CosmosClient`
- Connection string authentication vs. account key authentication (both are secrets stored in Key Vault or User Secrets, never in source)
- Startup connectivity verification: `database.ReadAsync()` to confirm the connection before the app accepts requests
- `CosmosDbService` pattern: wrapping `CosmosClient` in a singleton service that exposes `GetContainer()` and related helpers
- Configurable database and container names via `CosmosDbSettings`

### Azure Blob Storage
- Object storage model: storage accounts, containers, blobs
- `Azure.Storage.Blobs` NuGet package and `BlobServiceClient` / `BlobContainerClient`
- Connection string format and how to retrieve it from Key Vault
- Scoped `BlobSettingsService` providing connection string and maximum attachment count to callers
- Data-plane operations: upload, download, list, delete blobs
- Access tiers and retention policies (awareness-level)

### Feature-Flag-Gated Service Registration
- Only registering Azure services when the corresponding feature flag is enabled
- Async service registration at startup: `await builder.Services.AddCosmosDbServicesAsync(...)` — pattern for services that need to verify connectivity before the app runs
- Graceful degradation: the application starts in a reduced-capability mode when cloud services are unavailable

### Secrets Management Best Practices
- **Never** commit connection strings, account keys, or client secrets to source control
- Development: `dotnet user-secrets set "CosmosDb:ConnectionString" "..."` stores secrets in the OS user profile, not in the repository
- Production: fetch secrets from Azure Key Vault at startup using `SecretClient`
- Template pattern: `appsettings.template.json` documents every configuration key without real values; `appsettings.json` (gitignored) holds real values

---

## Prerequisites

- Skill 1 (ASP.NET Core & Razor Pages)
- An active Azure subscription (free tier is sufficient for development)
- Azure CLI installed (`az login` works)
- Basic familiarity with JSON document databases

---

## How It Applies to This Project

| Concept | Location in Codebase |
|---------|----------------------|
| Key Vault secret retrieval | `AzureKeyVaultOperations/AzureKeyVaultCertificateOperations.cs` |
| Key Vault service wiring | `Extensions/ServiceCollectionExtensions.cs` → `AddKeyVaultServicesAsync` |
| Key Vault settings | `Models/Settings/KeyVaultSettings.cs` |
| Cosmos DB client service | `Services/CosmosDbService.cs` |
| Cosmos DB settings | `Models/Settings/CosmosDbSettings.cs` |
| Cosmos DB service wiring | `Extensions/ServiceCollectionExtensions.cs` → `AddCosmosDbServicesAsync` |
| Blob Storage settings service | `Services/BlobSettingsService.cs` |
| Blob Storage settings | `Models/Settings/BlobSettings.cs` |
| Blob Storage service wiring | `Extensions/ServiceCollectionExtensions.cs` → `AddBlobStorageServices` |
| Configuration template | `appsettings.template.json` |
| Key Vault PFX guide | `docs/en-US/AZURE_KEYVAULT_PFX_GUIDE.md` |

---

## Learning Path

1. Create an Azure resource group using the Azure CLI: `az group create --name rg-demo --location eastus`.
2. Create an Azure Key Vault and store a test secret: `az keyvault secret set --vault-name myvault --name MySecret --value hello`.
3. Register an app in Azure AD; create a client secret; grant the app `Key Vault Secrets User` on the vault.
4. Write a console app that uses `SecretClient` with `ClientSecretCredential` to retrieve the secret.
5. Create a Cosmos DB account (free tier), a database, and a container; verify connectivity with `database.ReadAsync()`.
6. Create a `CosmosDbService` class that wraps `CosmosClient` and exposes the container; register it as a singleton.
7. Create an Azure Storage account and a blob container; upload and download a file using `BlobContainerClient`.
8. Move all secrets and connection strings out of `appsettings.json` and into User Secrets for local development.
9. Integrate Key Vault retrieval into an ASP.NET Core startup; load the Cosmos DB connection string from Key Vault.
10. Add feature-flag gating so Cosmos DB and Blob Storage only register if their flags are `true`.

---

## Suggested Resources

- [Azure Key Vault secrets documentation](https://learn.microsoft.com/azure/key-vault/secrets/quick-create-dotnet)
- [Azure Cosmos DB .NET SDK](https://learn.microsoft.com/azure/cosmos-db/nosql/sdk-dotnet-v3)
- [Azure Blob Storage .NET SDK](https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet)
- [DefaultAzureCredential documentation](https://learn.microsoft.com/dotnet/azure/sdk/authentication/credential-chains#defaultazurecredential-overview)
- [ASP.NET Core User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
- [Azure CLI reference](https://learn.microsoft.com/cli/azure/reference-index)
- `docs/en-US/AZURE_KEYVAULT_PFX_GUIDE.md` — Key Vault PFX import/retrieval walkthrough
- `appsettings.template.json` — documents every configuration key in this project
