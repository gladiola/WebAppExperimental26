# Skill 4 — Certificate & PKI Management

## What This Skill Covers

Working with X.509 certificates for TLS server identity and mTLS client authentication: generating certificates, storing them securely in a cloud vault, loading them at runtime into a web server, and validating revocation status with OCSP. This skill bridges the theory of public-key infrastructure with practical ASP.NET Core and Azure Key Vault implementation.

---

## Key Concepts

### X.509 Certificate Fundamentals
- Certificate structure: subject, issuer, public key, validity period, extensions, signature
- Certificate chain: leaf → intermediate CA(s) → root CA
- Distinguished Name (DN) fields: CN (Common Name), O (Organization), OU (Organizational Unit), C (Country)
- Self-signed certificates vs. CA-signed certificates — when each is appropriate
- PFX / PKCS #12 format: a container that bundles the certificate and its private key into a single file, typically password-protected
- PEM format: Base64-encoded DER with `-----BEGIN CERTIFICATE-----` headers

### Generating Certificates for Development
- `openssl req -x509 ...` — generating a self-signed certificate with a private key
- `mkcert` — a developer tool that creates a local CA and issues trusted leaf certificates automatically
- Common parameters: key length (RSA 2048/4096 or ECDSA P-256), validity period, Subject Alternative Names (SAN)

### Loading a Certificate into Kestrel
- `KestrelServerOptions.ConfigureHttpsDefaults` and `ListenOptions.UseHttps`
- Loading from a PFX file on disk: `X509Certificate2(path, password)`
- Loading from Azure Key Vault at startup: retrieve the secret/certificate object, construct `X509Certificate2`, inject into Kestrel before the app starts
- Avoiding disk storage of private keys in production environments

### Azure Key Vault Certificate Operations
- Two storage models in Key Vault: **Secrets** (arbitrary data, including PFX bytes) vs. **Certificates** (managed lifecycle with auto-renewal)
- Authenticating to Key Vault: `ClientId` + `ClientSecret` (service principal) or Managed Identity
- `SecretClient.GetSecretAsync(name)` → decode Base64 → `new X509Certificate2(bytes, password, X509KeyStorageFlags.EphemeralKeySet)`
- Nonce encryption material (IV, key) stored as Key Vault secrets and fetched at startup

### Mutual TLS (mTLS)
- What mutual authentication means: the client presents its own certificate during the TLS handshake; the server validates it
- `ClientCertificateMode.RequireCertificate` vs. `AllowCertificate` in Kestrel's `HttpsConnectionAdapterOptions`
- Reading `HttpContext.Connection.ClientCertificate` in middleware
- Validation logic:
  - Chain building (`X509Chain.Build`) — validates the certificate path to a trusted root
  - Self-signed allowance — bypassing chain validation for known self-signed certs
  - Revocation checking — `X509RevocationMode.Online` vs. `NoCheck`
  - Issuer DN matching — comparing the certificate's `Issuer` property against an allow-list

### OCSP — Online Certificate Status Protocol
- What OCSP is: a real-time protocol for checking whether a specific certificate has been revoked, as an alternative to downloading a full CRL (Certificate Revocation List)
- OCSP request/response structure at a conceptual level
- OCSP stapling: the server pre-fetches and caches its own OCSP response to reduce client round-trips
- Configurable behaviour when the OCSP server is unreachable: fail-closed (reject the cert), fail-open (accept the cert), or warn-only
- In-memory caching of OCSP responses to avoid per-request network calls

---

## Prerequisites

- Skill 1 (ASP.NET Core & Razor Pages)
- Skill 2 (Authentication & Authorization) — especially the mTLS section
- Conceptual understanding of public-key cryptography (asymmetric keys, digital signatures)

---

## How It Applies to This Project

| Concept | Location in Codebase |
|---------|----------------------|
| Azure Key Vault PFX loading at startup | `AzureKeyVaultOperations/AzureKeyVaultCertificateOperations.cs` |
| Key Vault service wiring | `Extensions/ServiceCollectionExtensions.cs` → `AddKeyVaultServicesAsync` |
| Key Vault settings model | `Models/Settings/KeyVaultSettings.cs` |
| mTLS Kestrel configuration | `Extensions/ServiceCollectionExtensions.cs` → `AddMtlsAuthentication` |
| mTLS settings model | `Models/Settings/MtlsSettings.cs` |
| OCSP validation service | `Services/OcspValidationService.cs` |
| OCSP settings model | `Models/Settings/OcspSettings.cs` |
| Key Vault PFX guide (prose) | `docs/en-US/AZURE_KEYVAULT_PFX_GUIDE.md` |
| mTLS setup guide (prose) | `docs/en-US/MTLS_GUIDE.md` |
| OCSP guide (prose) | `docs/en-US/OCSP_GUIDE.md` |

---

## Learning Path

1. Read `docs/en-US/AZURE_KEYVAULT_PFX_GUIDE.md` for the end-to-end Key Vault PFX workflow.
2. Generate a self-signed server certificate with `mkcert` or `openssl`; convert it to PFX format.
3. Configure a Kestrel HTTPS endpoint to load a certificate from a PFX file.
4. Create an Azure Key Vault instance; upload the PFX as a Key Vault secret.
5. Write code to fetch the secret at startup using `SecretClient`, reconstruct the `X509Certificate2`, and inject it into Kestrel.
6. Read `docs/en-US/MTLS_GUIDE.md`; generate a client certificate and configure Kestrel to require it.
7. Write middleware that reads `HttpContext.Connection.ClientCertificate`, builds the chain, and rejects clients whose issuer is not in an allow-list.
8. Read `docs/en-US/OCSP_GUIDE.md`; stub out an `OcspValidationService` that logs a warning and returns a configurable fail-open/fail-closed result.
9. Add in-memory caching to the OCSP service using `IMemoryCache`.

---

## Suggested Resources

- [Azure Key Vault certificates documentation](https://learn.microsoft.com/azure/key-vault/certificates/)
- [Azure Key Vault secrets documentation](https://learn.microsoft.com/azure/key-vault/secrets/)
- `docs/en-US/AZURE_KEYVAULT_PFX_GUIDE.md` — step-by-step PFX import guide for this project
- `docs/en-US/MTLS_GUIDE.md` — mTLS configuration, cert generation, environment-specific notes
- `docs/en-US/OCSP_GUIDE.md` — OCSP template config, server options, cache/timeout tradeoffs
- [Kestrel HTTPS configuration](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/endpoints#configure-https)
- [mkcert](https://github.com/FiloSottile/mkcert) — trusted local certificates for development
- [OpenSSL cookbook](https://www.feistyduck.com/library/openssl-cookbook/) — practical certificate operations
- [RFC 6960 — OCSP](https://datatracker.ietf.org/doc/html/rfc6960)
- [X.509 certificate overview (Microsoft)](https://learn.microsoft.com/dotnet/standard/security/certificate-based-auth)
