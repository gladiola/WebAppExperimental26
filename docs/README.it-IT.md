# WebAppExperimental26

Un'applicazione web ASP.NET Core 9 Razor Pages con autenticazione Azure AD, TLS mutuale (mTLS), gestione dei certificati tramite Azure Key Vault, Azure Cosmos DB, Azure Blob Storage e un livello di sicurezza HTTP rafforzato con Content Security Policy basata su nonce.

---

## Indice

- [Funzionalità](#funzionalità)
- [Feature Flag](#feature-flag)
- [Prerequisiti](#prerequisiti)
- [Installazione – Windows Azure (App Service)](#installazione--windows-azure-app-service)
- [Installazione – Server OpenBSD con servizi Azure](#installazione--server-openbsd-con-servizi-azure)
- [Riferimento alla configurazione](#riferimento-alla-configurazione)
- [Script di supporto](#script-di-supporto)
- [Note sulla sicurezza](#note-sulla-sicurezza)

---

## Funzionalità

### Autenticazione Azure AD (OpenID Connect)
L'applicazione autentica gli utenti tramite la **Microsoft Identity Platform** usando il protocollo OpenID Connect (tramite `Microsoft.Identity.Web`). Tutte le route sotto `/Experimental` richiedono un'identità Azure AD autenticata. Le pagine `/Privacy`, `/Error` e `/About` sono accessibili pubblicamente.

### Autenticazione mTLS con certificato client
Quando abilitato, i client devono presentare un certificato X.509 valido. Le impostazioni in `MtlsSettings` controllano se sono consentiti certificati concatenati, auto-firmati o entrambi, la verifica della revoca dei certificati e gli emittenti di certificati consentiti.

### Integrazione con Azure Key Vault
L'applicazione recupera il **certificato del server** TLS da Azure Key Vault all'avvio. Il `X509Certificate2` caricato viene iniettato direttamente nella configurazione HTTPS di Kestrel, senza necessità di file PFX sul disco.

### Content Security Policy con nonce per richiesta
Quando abilitato, ogni risposta HTTP include un'intestazione `Content-Security-Policy` la cui direttiva `script-src` contiene un **nonce casuale crittograficamente sicuro** per ogni richiesta. La CSP supporta anche elenchi di autorizzazioni basati su hash SHA-256 per script inline.

### Intestazioni di sicurezza HTTP standard
`UseStandardSecurityHeaders` aggiunge a ogni risposta: `X-Frame-Options`, `X-Content-Type-Options`, `Strict-Transport-Security`, `Referrer-Policy`, `Cross-Origin-Opener-Policy`, `Cross-Origin-Resource-Policy`, `Permissions-Policy` e rimuove le intestazioni `Server`, `X-Powered-By` e `X-AspNetMvc-Version`.

### Azure Blob Storage
Quando abilitato, `BlobSettingsService` fornisce un servizio Scoped supportato da una stringa di connessione e un numero massimo configurabile di allegati.

### Azure Cosmos DB
Quando abilitato, l'applicazione verifica la connessione a Cosmos DB all'avvio chiamando `database.ReadAsync()`.

### Gestione sicura delle sessioni
Le sessioni utilizzano una cache di memoria distribuita in-process con un **timeout di inattività di 30 minuti**. I cookie di sessione sono configurati come `HttpOnly`, `Secure = Always` e `SameSite = Strict`.

### Localizzazione
L'applicazione supporta **11 lingue**: en-US, de-DE, es-ES, fr-FR, pt-PT, it-IT, zh-HK, ko-KR, hi-IN, ru-RU e ar-SA. L'arabo include il cambio automatico del layout RTL.

### Logging sicuro per i dati PII
`LoggingHelper` esegue l'hash delle informazioni di identificazione personale nell'output di log usando HMAC-SHA256. Una chiave stabile di 32 byte può essere fornita tramite `Logging:PiiHmacKey`.

---

## Feature Flag

Tutti i principali sottosistemi sono controllati da flag booleani in `appsettings.json`.

| Flag | Predefinito | Descrizione |
|---|---|---|
| `EnableSession` | `true` | Sessione lato server e cookie di sessione |
| `EnableLocalization` | `true` | Supporto multilingue (11 lingue) |
| `EnableAzureAd` | `true` | Autenticazione Azure AD / OpenID Connect |
| `EnableAuthorization` | `true` | Criteri di autorizzazione a livello di route |
| `EnableKeyVault` | `false` | Carica il certificato TLS del server da Azure Key Vault |
| `EnableNonceServices` | `false` | Generazione di nonce CSP per richiesta |
| `EnableCSP` | `false` | Aggiungere l'intestazione `Content-Security-Policy` |
| `EnableSecurityHeaders` | `true` | Aggiungere le intestazioni di sicurezza HTTP standard |
| `EnableBlobStorage` | `false` | Servizio Azure Blob Storage |
| `EnableCosmosDb` | `false` | Servizio Azure Cosmos DB |
| `EnableMtls` | `false` | Richiedere certificati TLS client |
| `EnableOcspValidation` | `false` | Verifica revoca OCSP (stub) |

---

## Prerequisiti

1. **Registrazione applicazione Azure AD** – con URI di reindirizzamento, segreto client o credenziale certificato.
2. **Azure Key Vault** – con il certificato PFX del server come segreto.
3. **Account Azure Cosmos DB** (opzionale).
4. **Account Azure Blob Storage** (opzionale).
5. **.NET 9 SDK / Runtime** – versione 9.0 o successiva.

---

## Riferimento alla configurazione

Copiare `appsettings.template.json` in `appsettings.json` e sostituire tutti i valori `{{PLACEHOLDER}}`. Conservare i segreti in **.NET User Secrets** (locale) o in Azure App Settings / Key Vault References (produzione) — mai nel codice sorgente.

---

## Note sulla sicurezza

- **Non memorizzare mai i segreti nel controllo del codice sorgente.**
- L'implementazione della validazione OCSP è uno **stub** che rifiuta tutti i certificati. Sostituire `PerformOcspValidationAsync` prima di abilitare `EnableOcspValidation` in produzione.
- I valori nonce non vengono **mai registrati** nei log.
- L'intestazione di risposta `Server` è mascherata con `webserver`.
