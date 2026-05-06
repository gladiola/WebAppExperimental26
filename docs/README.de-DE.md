# WebAppExperimental26

Eine ASP.NET Core 9 Razor Pages-Webanwendung mit Azure AD-Authentifizierung, Mutual TLS (mTLS), Azure Key Vault-Zertifikatsverwaltung, Azure Cosmos DB, Azure Blob Storage und einer gehärteten HTTP-Sicherheitsschicht mit nonce-basierter Content Security Policy.

---

## Inhaltsverzeichnis

- [Funktionen](#funktionen)
- [Feature-Flags](#feature-flags)
- [Voraussetzungen](#voraussetzungen)
- [Installation – Windows Azure (App Service)](#installation--windows-azure-app-service)
- [Installation – OpenBSD-Server mit Azure-Diensten](#installation--openbsd-server-mit-azure-diensten)
- [Konfigurationsreferenz](#konfigurationsreferenz)
- [Hilfsskripte](#hilfsskripte)
- [Sicherheitshinweise](#sicherheitshinweise)

---

## Funktionen

### Azure AD-Authentifizierung (OpenID Connect)
Die Anwendung authentifiziert Benutzer über die **Microsoft Identity Platform** mithilfe des OpenID Connect-Protokolls (über `Microsoft.Identity.Web`). Alle Routen unter `/Experimental` erfordern eine authentifizierte Azure AD-Identität. Die Seiten `/Privacy`, `/Error` und `/About` sind öffentlich zugänglich.

### Mutual TLS (mTLS) – Client-Zertifikat-Authentifizierung
Wenn aktiviert, müssen sich Clients mit einem gültigen X.509-Zertifikat ausweisen. Einstellungen in `MtlsSettings` steuern, ob verkettete, selbstsignierte oder beide Zertifikattypen zugelassen werden, sowie die Zertifikatssperrprüfung und erlaubte Zertifikatsaussteller.

### Azure Key Vault-Integration
Die Anwendung ruft beim Start das TLS-**Serverzertifikat** aus Azure Key Vault ab. Das geladene `X509Certificate2` wird direkt in die Kestrel-HTTPS-Konfiguration injiziert, sodass keine PFX-Datei auf der Festplatte vorhanden sein muss.

### Content Security Policy mit Nonces pro Anfrage
Wenn aktiviert, enthält jede HTTP-Antwort einen `Content-Security-Policy`-Header, dessen `script-src`-Direktive eine **kryptografisch zufällige Nonce** pro Anfrage enthält. Die CSP unterstützt auch SHA-256-Hash-basierte Freigabelisten für Inline-Skripte.

### Standardmäßige HTTP-Sicherheitsheader
`UseStandardSecurityHeaders` fügt jeder Antwort folgende Header hinzu: `X-Frame-Options`, `X-Content-Type-Options`, `Strict-Transport-Security`, `Referrer-Policy`, `Cross-Origin-Opener-Policy`, `Cross-Origin-Resource-Policy`, `Permissions-Policy` sowie das Entfernen von `Server`-, `X-Powered-By`- und `X-AspNetMvc-Version`-Headern.

### Azure Blob Storage
Wenn aktiviert, stellt `BlobSettingsService` einen Scoped-Dienst bereit, der über eine Verbindungszeichenfolge und eine konfigurierbare maximale Anzahl von Anhängen betrieben wird.

### Azure Cosmos DB
Wenn aktiviert, überprüft die Anwendung beim Start die Cosmos DB-Verbindung durch Aufruf von `database.ReadAsync()`.

### Sichere Sitzungsverwaltung
Sitzungen verwenden einen In-Prozess-Distributed-Memory-Cache mit einem **30-minütigen Leerlauf-Timeout**. Sitzungs-Cookies sind als `HttpOnly`, `Secure = Always` und `SameSite = Strict` konfiguriert.

### Lokalisierung
Die Anwendung unterstützt **11 Sprachen**: en-US, de-DE, es-ES, fr-FR, pt-PT, it-IT, zh-HK, ko-KR, hi-IN, ru-RU und ar-SA. Arabisch wird mit automatischer RTL-Layoutumschaltung unterstützt.

### PII-sicheres Logging
`LoggingHelper` hasht personenbezogene Daten in der Protokollausgabe mit HMAC-SHA256. Ein stabiler 32-Byte-Schlüssel kann über `Logging:PiiHmacKey` bereitgestellt werden.

---

## Feature-Flags

Alle wichtigen Subsysteme werden durch boolesche Feature-Flags in `appsettings.json` gesteuert.

| Flag | Standard | Beschreibung |
|---|---|---|
| `EnableSession` | `true` | Server-seitige Sitzung und Sitzungs-Cookie |
| `EnableLocalization` | `true` | Mehrsprachige Unterstützung (11 Sprachen) |
| `EnableAzureAd` | `true` | Azure AD / OpenID Connect-Authentifizierung |
| `EnableAuthorization` | `true` | Routenbasierte Autorisierungsrichtlinien |
| `EnableKeyVault` | `false` | TLS-Serverzertifikat aus Azure Key Vault laden |
| `EnableNonceServices` | `false` | CSP-Nonce-Generierung pro Anfrage |
| `EnableCSP` | `false` | `Content-Security-Policy`-Header anhängen |
| `EnableSecurityHeaders` | `true` | Standard-HTTP-Sicherheitsheader anhängen |
| `EnableBlobStorage` | `false` | Azure Blob Storage-Dienst |
| `EnableCosmosDb` | `false` | Azure Cosmos DB-Dienst |
| `EnableMtls` | `false` | Client-TLS-Zertifikate erforderlich |
| `EnableOcspValidation` | `false` | OCSP-Zertifikatsperrprüfung (Stub) |

---

## Voraussetzungen

1. **Azure AD-App-Registrierung** – mit Umleitungs-URI, Client-Secret oder Zertifikat-Credential.
2. **Azure Key Vault** – mit dem PFX-Serverzertifikat als Secret.
3. **Azure Cosmos DB-Konto** (optional).
4. **Azure Blob Storage-Konto** (optional).
5. **.NET 9 SDK / Runtime** – Version 9.0 oder höher.

---

## Installation – Windows Azure (App Service)

### 1. Azure-Ressourcen erstellen

```powershell
# Log in
az login

# Create a resource group
az group create --name MyResourceGroup --location eastus

# Create an App Service plan (Linux or Windows)
az appservice plan create --name MyPlan --resource-group MyResourceGroup --sku B1 --is-linux

# Create the web app (.NET 9)
az webapp create --name MyWebApp26 --resource-group MyResourceGroup \
  --plan MyPlan --runtime "DOTNETCORE:9.0"
```

### 2. Azure AD-Anwendung registrieren

Im [Azure Portal](https://portal.azure.com):
1. Navigieren Sie zu **Microsoft Entra ID → App-Registrierungen → Neue Registrierung**.
2. Setzen Sie die Umleitungs-URI auf `https://<your-app>.azurewebsites.net/signin-oidc`.
3. Erstellen Sie unter **Zertifikate & Geheimnisse** ein Client-Secret und kopieren Sie den Wert.
4. Notieren Sie die **Mandanten-ID** und **Client-ID** aus dem Übersichtsblatt.

### 3. Azure Key Vault erstellen und Serverzertifikat hochladen

```powershell
az keyvault create --name MyKeyVault26 --resource-group MyResourceGroup --location eastus

# Upload your PFX as a Key Vault secret (base64-encoded)
$pfxBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes("server.pfx"))
az keyvault secret set --vault-name MyKeyVault26 --name "ServerCert" --value $pfxBase64

# Grant the App Service Managed Identity access
az keyvault set-policy --name MyKeyVault26 \
  --object-id <managed-identity-object-id> \
  --secret-permissions get list
```

### 4. Anwendungseinstellungen konfigurieren

Kopieren Sie `appsettings.template.json` nach `appsettings.json` und füllen Sie die Platzhalterwerte aus. Geheimnisse dürfen **nicht** in der Quellcodeverwaltung gespeichert werden — legen Sie sie als App Service-Anwendungseinstellungen oder über User Secrets lokal fest:

```powershell
# In Azure App Service, set secrets as app settings:
az webapp config appsettings set --name MyWebApp26 --resource-group MyResourceGroup --settings \
  "AzureAd__TenantId=<TENANT_ID>" \
  "AzureAd__ClientId=<CLIENT_ID>" \
  "AzureAd__ClientSecret=<CLIENT_SECRET>" \
  "AzureKeyVault__KeyVaultURL=https://MyKeyVault26.vault.azure.net/" \
  "AzureKeyVault__KeyVaultSecret=<KV_SECRET>" \
  "AzureKeyVault__KeyVaultPassName=ServerCert" \
  "FeatureFlags__EnableKeyVault=true" \
  "FeatureFlags__EnableAzureAd=true"
```

### 5. Anwendung bereitstellen

```bash
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
az webapp deployment source config-zip \
  --name MyWebApp26 --resource-group MyResourceGroup --src ../app.zip
```

### 6. HTTPS und benutzerdefinierte Domain aktivieren (empfohlen)

```powershell
# Force HTTPS
az webapp update --name MyWebApp26 --resource-group MyResourceGroup --https-only true

# Bind a custom domain and managed TLS certificate
az webapp config hostname add --webapp-name MyWebApp26 --resource-group MyResourceGroup \
  --hostname www.example.com
az webapp config ssl bind --certificate-thumbprint <THUMBPRINT> \
  --name MyWebApp26 --resource-group MyResourceGroup --ssl-type SNI
```

### 7. mTLS auf Azure App Service aktivieren (optional)

Azure App Service unterstützt Client-Zertifikate über das Portal:
1. Gehen Sie zu **App Service → TLS/SSL-Einstellungen → Client-Zertifikate**.
2. Setzen Sie **Eingehende Client-Zertifikate** auf **Erforderlich**.

Setzen Sie dann `FeatureFlags__EnableMtls=true` in den Anwendungseinstellungen.

---

## Installation – OpenBSD-Server mit Azure-Diensten

> **Wichtig:** .NET 9 hat **keinen** offiziellen Microsoft-Build für OpenBSD. Die folgenden Anweisungen verwenden einen **Linux-kompatiblen Container** (über [Podman](https://podman.io/), der im OpenBSD-Paketbaum verfügbar ist), um die ASP.NET Core 9-Anwendung auf OpenBSD auszuführen und dabei über HTTPS mit Azure-Diensten zu kommunizieren.

### 1. Voraussetzungen auf OpenBSD installieren

```sh
# As root
pkg_add podman
pkg_add curl git
```

Wenn weder Podman noch Docker für Ihre OpenBSD-Version verfügbar ist, erwägen Sie die Ausführung der App in einer **Linux-VM** (z. B. vmm(4) mit einem Debian/Ubuntu-Gast) und folgen Sie dem Standard-Linux-Bereitstellungspfad innerhalb dieses Gastsystems.

### 2. ASP.NET Core 9 Runtime-Image herunterladen

```sh
podman pull mcr.microsoft.com/dotnet/aspnet:9.0
```

### 3. Anwendung erstellen (auf einem Linux- oder Windows-Build-Rechner)

Veröffentlichen Sie auf einem Rechner mit installiertem .NET 9 SDK einen eigenständigen Build für Linux x64:

```bash
dotnet publish WebAppExperimental26/WebAppExperimental26.csproj \
  -c Release -r linux-x64 --self-contained true -o ./publish
```

Übertragen Sie das Verzeichnis `publish/` auf den OpenBSD-Host (z. B. über `scp` oder ein freigegebenes Volume).

### 4. Konfigurationsdatei erstellen

Erstellen Sie auf dem OpenBSD-Host `/etc/webappexp26/appsettings.json` mit Ihren Produktionswerten (keine Geheimnisse in der Datei; verwenden Sie stattdessen Umgebungsvariablen):

```json
{
  "AllowedHosts": "your.hostname.example.com",
  "FeatureFlags": {
    "EnableAzureAd": true,
    "EnableKeyVault": true,
    "EnableSecurityHeaders": true,
    "EnableMtls": false
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "CallbackPath": "/signin-oidc"
  },
  "AzureKeyVault": {
    "KeyVaultURL": "https://YOUR_KEYVAULT_NAME.vault.azure.net/",
    "KeyVaultPassName": "ServerCert"
  }
}
```

Geheimnisse werden als Umgebungsvariablen im nächsten Schritt injiziert.

### 5. Container ausführen

```sh
podman run -d \
  --name webappexp26 \
  -p 443:8443 \
  -v /etc/webappexp26:/app/config:ro \
  -v /path/to/publish:/app:ro \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS="https://+:8443" \
  -e AzureAd__ClientSecret="YOUR_CLIENT_SECRET" \
  -e AzureKeyVault__KeyVaultSecret="YOUR_KV_SECRET" \
  -e Logging__PiiHmacKey="YOUR_32_BYTE_BASE64_KEY" \
  mcr.microsoft.com/dotnet/aspnet:9.0 \
  dotnet /app/WebAppExperimental26.dll \
    --contentRoot /app \
    --configDir /app/config
```

### 6. OpenBSD Packet Filter (pf) Firewall konfigurieren

Fügen Sie zu `/etc/pf.conf` hinzu, um eingehenden HTTPS-Verkehr zu erlauben und ausgehende Verbindungen zu Azure-Endpunkten zu gestatten:

```
# Allow inbound HTTPS
pass in on egress proto tcp to port 443

# Allow outbound to Azure AD, Key Vault, Cosmos DB, Blob Storage
pass out on egress proto tcp to port { 443 }
```

Regelsatz neu laden:

```sh
pfctl -f /etc/pf.conf
```

### 7. DNS und TLS-Zertifikate konfigurieren

Stellen Sie sicher, dass der Hostname in `AllowedHosts` auf die öffentliche IP des OpenBSD-Servers auflöst. Azure AD erfordert, dass die Umleitungs-URI (`/signin-oidc`) über HTTPS erreichbar ist, daher muss das Serverzertifikat vertrauenswürdig sein. Verwenden Sie ein Zertifikat einer öffentlichen CA (z. B. Let's Encrypt über `acme-client(1)`) oder laden Sie ein CA-signiertes Zertifikat in Azure Key Vault hoch und aktivieren Sie `EnableKeyVault`.

### 8. Ausgehende Konnektivität zu Azure-Diensten

Die folgenden Azure-Dienstendpunkte müssen vom OpenBSD-Host über TCP 443 erreichbar sein:

| Service | Endpoint |
|---|---|
| Azure AD / Microsoft Identity | `login.microsoftonline.com` |
| Azure Key Vault | `<vault-name>.vault.azure.net` |
| Azure Cosmos DB | `<account>.documents.azure.com` |
| Azure Blob Storage | `<account>.blob.core.windows.net` |

Testen Sie die Konnektivität vor dem Start des Containers:

```sh
curl -I https://login.microsoftonline.com
curl -I https://YOUR_KEYVAULT_NAME.vault.azure.net
```

---

## Konfigurationsreferenz

Kopieren Sie `appsettings.template.json` nach `appsettings.json` und ersetzen Sie alle `{{PLACEHOLDER}}`-Werte.

| Section | Key | Description |
|---|---|---|
| `AzureAd` | `TenantId`, `ClientId`, `ClientSecret` | Azure AD app registration |
| `AzureKeyVault` | `KeyVaultURL`, `KeyVaultSecret`, `KeyVaultPassName` | Key Vault and certificate name |
| `MtlsSettings` | `RequireClientCertificate`, `AllowedIssuers` | mTLS client cert policy |
| `NonceEncryption` | `Key`, `IV` | 32-byte key and 16-byte IV for nonce encryption (base64) |
| `BlobSettings` | `BlobConnectionString`, `MaxAttachments` | Blob Storage connection |
| `CosmosDb` | `CosmosConnectionString`, `DatabaseName`, `ContainerName` | Cosmos DB connection |
| `OcspSettings` | `OcspServerUrl`, `CacheDurationMinutes` | OCSP validation (stub) |
| `Logging` | `PiiHmacKey` | 32-byte base64 HMAC key for PII hashing in logs |

Generieren Sie Verschlüsselungsschlüssel und IVs mit dem enthaltenen PowerShell-Skript:

```powershell
.\WebAppExperimental26\SupportingScripts\IVandKeySampleGenerator.ps1
```

Speichern Sie alle Geheimnisse in **.NET User Secrets** für die lokale Entwicklung:

```bash
dotnet user-secrets set "AzureAd:ClientSecret" "YOUR_SECRET"
dotnet user-secrets set "AzureKeyVault:KeyVaultSecret" "YOUR_KV_SECRET"
dotnet user-secrets set "NonceEncryption:Key" "YOUR_BASE64_KEY"
dotnet user-secrets set "NonceEncryption:IV" "YOUR_BASE64_IV"
```

---

## Hilfsskripte

Das Verzeichnis `SupportingScripts/` enthält PowerShell-Hilfsprogramme:

| Script | Purpose |
|---|---|
| `IVandKeySampleGenerator.ps1` | Generate a random 32-byte AES key and 16-byte IV (base64) |
| `HashInlineScriptPowerShell.ps1` | Compute SHA-256 hashes for inline scripts (for CSP allow-listing) |
| `HashInlineScriptPowerShellBase64Output.ps1` | Same as above, outputs hashes in base64 format |
| `CertificateUploaderToAzureExample.ps1` | Upload a PFX certificate to Azure Key Vault |
| `CheckRoles.ps1` | Verify Azure RBAC role assignments for the app |
| `ExportResourceGroups.ps1` | Export Azure resource group configurations |
| `TroubleshootingCosmosDBInfo.ps1` | Diagnose Cosmos DB connectivity |
| `SetupFromTemplate.ps1` | Automate initial configuration from `appsettings.template.json` |

---

## Sicherheitshinweise

- **Niemals Geheimnisse in das Quell-Repository übertragen.**
- Die OCSP-Validierungsimplementierung ist ein **Stub**, der alle Zertifikate ablehnt. Ersetzen Sie `PerformOcspValidationAsync` vor dem Aktivieren von `EnableOcspValidation` in der Produktion.
- Nonce-Werte werden **niemals protokolliert**.
- Der `Server`-Antwortheader wird auf `webserver` maskiert.
