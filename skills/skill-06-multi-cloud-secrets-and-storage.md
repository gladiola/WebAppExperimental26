# Skill 6 — Multi-Cloud Secrets & Storage

## What This Skill Covers

Extending the application beyond Azure to support equivalent services from **Amazon Web Services (AWS)** and **Google Cloud Platform (GCP)**. Each cloud has its own secrets manager, NoSQL database, and authentication model. This skill emphasises recognising the structural parallels between providers so that knowledge transfers across clouds.

---

## Key Concepts

### The Multi-Cloud Pattern
- Identifying equivalent services across providers:

| Capability | Azure | AWS | GCP |
|-----------|-------|-----|-----|
| Secrets / cert storage | Key Vault | Secrets Manager | Secret Manager |
| NoSQL document database | Cosmos DB | DynamoDB | Firestore |
| Identity provider | Entra ID (Azure AD) | Cognito | Identity Platform |
| Object storage | Blob Storage | S3 | Cloud Storage |

- Using a shared `IOperationsService` interface so the application can swap providers without changing business logic
- Feature-flag-gated registration: only the enabled provider's services are registered at startup

---

### AWS Secrets Manager
- What it is: a managed service for storing and rotating secrets (API keys, passwords, certificates, connection strings)
- `AWSSDK.SecretsManager` NuGet package and `IAmazonSecretsManager` / `AmazonSecretsManagerClient`
- AWS credentials model: `AccessKeyId` and `SecretAccessKey` (for development); IAM roles (for production)
- Credential storage: never in source control; use environment variables, AWS credentials file, or IAM instance roles
- Key operations: `GetSecretValueRequest` with the secret's ARN or name; decoding the `SecretString` or `SecretBinary` payload
- Fetching nonce encryption material and TLS certificates from Secrets Manager (mirrors the Azure Key Vault pattern)

### Amazon DynamoDB
- What it is: a fully managed key-value and document NoSQL database (AWS equivalent of Azure Cosmos DB)
- `AWSSDK.DynamoDBv2` NuGet package and `IAmazonDynamoDB` / `AmazonDynamoDBClient`
- Core model: tables, items (JSON-like attribute maps), partition key and optional sort key
- Connectivity verification at startup: `DescribeTableRequest` to confirm the table exists and is accessible
- `AwsDynamoDbService` pattern: wrapping `IAmazonDynamoDB` in a singleton service; exposing `GetTableAsync()` and `GetTableName()`
- AWS credentials for DynamoDB: same model as Secrets Manager

### Google Cloud Secret Manager
- What it is: a managed service for storing versioned secrets (GCP equivalent of Azure Key Vault secrets)
- `Google.Cloud.SecretManager.V1` NuGet package and `SecretManagerServiceClient`
- Authentication: **Application Default Credentials (ADC)** — `gcloud auth application-default login` for local development; Workload Identity for GCP-hosted services; an explicit service-account JSON key file when running outside GCP
- Core operations: `AccessSecretVersionRequest` with `projects/{project}/secrets/{secret}/versions/latest`
- Fetching secrets and certificates from Secret Manager (mirrors the Azure Key Vault pattern)

### Google Cloud Firestore
- What it is: a fully managed serverless document database (GCP equivalent of Azure Cosmos DB)
- `Google.Cloud.Firestore` NuGet package and `FirestoreDb`
- Model: projects → collections → documents (JSON-like field maps)
- Authentication: ADC or a service-account JSON key file path supplied via configuration
- Connectivity setup: `FirestoreDb.Create(projectId)` or with explicit credentials; the client is registered as a singleton
- `GcpFirestoreService` pattern: exposing `GetCollection()`, `GetCollectionName()`, and `GetDatabase()`

### Credential Security Across Clouds
- AWS: `AccessKeyId` / `SecretAccessKey` — store in User Secrets or environment variables; prefer IAM roles in production
- GCP: service account JSON key file — store outside the repository; prefer ADC / Workload Identity in production
- Never embed cloud credentials in `appsettings.json` or any file tracked by git
- Template pattern: document every configuration key in `appsettings.template.json` with placeholder values

### Interface-Driven Design for Provider Abstraction
- Define an interface (`IKeyVaultOperationsService` or equivalent) with the operations the app needs: `FetchSecret`, `FetchCertificate`, `FetchSecretIVSecret`, `FetchSecretNonceKeySecret`
- Implement the interface once per provider; register the appropriate implementation based on the active feature flag
- Stub implementations that log a warning and return empty values are acceptable for providers that are not yet wired to a live environment

---

## Prerequisites

- Skill 1 (ASP.NET Core & Razor Pages)
- Skill 5 (Azure Integration) — understanding the Azure Key Vault and Cosmos DB patterns helps you recognise the analogues
- An AWS account (free tier) and the AWS CLI installed (`aws configure`)
- A GCP project and the Google Cloud CLI installed (`gcloud init`)

---

## How It Applies to This Project

| Concept | Location in Codebase |
|---------|----------------------|
| AWS Secrets Manager service | `Services/AwsSecretsManagerOperationsService.cs` |
| AWS Secrets Manager operations | `AwsSecretManagerOperations/AwsSecretManagerOperations.cs` |
| AWS Secrets Manager settings | `Models/Settings/AwsSecretsManagerSettings.cs` |
| DynamoDB service | `Services/AwsDynamoDbService.cs` |
| DynamoDB settings | `Models/Settings/AwsDynamoDbSettings.cs` |
| DynamoDB settings service | `Services/AwsDynamoDbSettingsService.cs` |
| GCP Secret Manager service | `Services/GcpSecretManagerOperationsService.cs` |
| GCP Secret Manager operations | `GcpSecretManagerOperations/GcpSecretManagerOperations.cs` |
| GCP Secret Manager settings | `Models/Settings/GcpSecretManagerSettings.cs` |
| Firestore service | `Services/GcpFirestoreService.cs` |
| Firestore settings | `Models/Settings/GcpFirestoreSettings.cs` |
| Firestore settings service | `Services/GcpFirestoreSettingsService.cs` |
| AWS Cognito settings | `Models/Settings/AwsCognitoSettings.cs` |
| GCP Identity settings | `Models/Settings/GcpIdentitySettings.cs` |
| Service wiring | `Extensions/ServiceCollectionExtensions.cs` → `AddAwsSecretsManagerServices`, `AddAwsDynamoDbServicesAsync`, `AddGcpSecretManagerServices`, `AddGcpFirestoreServicesAsync` |

---

## Learning Path

1. Create an AWS Secrets Manager secret via the AWS console or CLI: `aws secretsmanager create-secret --name MySecret --secret-string hello`.
2. Write a console app that fetches the secret using `AmazonSecretsManagerClient`.
3. Create a DynamoDB table; verify connectivity at startup using `DescribeTableRequest`.
4. Create a GCP project; enable the Secret Manager API; store a secret and retrieve it with `SecretManagerServiceClient`.
5. Create a Firestore database in native mode; write and read a document using `FirestoreDb`.
6. Define a shared interface with `FetchSecret` and `FetchCertificate`; implement it for both AWS and GCP providers.
7. Register each implementation behind its feature flag; verify that only the enabled provider is active.
8. Move all AWS and GCP credentials to User Secrets or environment variables; confirm nothing sensitive remains in tracked files.

---

## Suggested Resources

- [AWS Secrets Manager documentation](https://docs.aws.amazon.com/secretsmanager/latest/userguide/intro.html)
- [AWS SDK for .NET](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/quick-start.html)
- [Amazon DynamoDB developer guide](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Introduction.html)
- [Google Cloud Secret Manager documentation](https://cloud.google.com/secret-manager/docs)
- [Google Cloud Firestore documentation](https://cloud.google.com/firestore/docs)
- [Application Default Credentials](https://cloud.google.com/docs/authentication/application-default-credentials)
- [Google Cloud client libraries for .NET](https://cloud.google.com/dotnet/docs/reference)
- AWS CLI: `aws configure`, `aws secretsmanager`, `aws dynamodb`
- Google Cloud CLI: `gcloud auth application-default login`, `gcloud secrets versions access`
