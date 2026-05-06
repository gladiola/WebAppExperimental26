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

## Konfigurationsreferenz

Kopieren Sie `appsettings.template.json` nach `appsettings.json` und ersetzen Sie alle `{{PLACEHOLDER}}`-Werte. Bewahren Sie Geheimnisse in **.NET User Secrets** (lokal) oder Azure App Settings / Key Vault References (Produktion) auf – niemals im Quellcode.

---

## Sicherheitshinweise

- **Niemals Geheimnisse in das Quell-Repository übertragen.**
- Die OCSP-Validierungsimplementierung ist ein **Stub**, der alle Zertifikate ablehnt. Ersetzen Sie `PerformOcspValidationAsync` vor dem Aktivieren von `EnableOcspValidation` in der Produktion.
- Nonce-Werte werden **niemals protokolliert**.
- Der `Server`-Antwortheader wird auf `webserver` maskiert.
