# WebAppExperimental26

Une application web ASP.NET Core 9 Razor Pages avec authentification Azure AD, TLS mutuel (mTLS), gestion des certificats via Azure Key Vault, Azure Cosmos DB, Azure Blob Storage et une couche de sécurité HTTP renforcée avec une politique de sécurité du contenu basée sur des nonces.

---

## Table des matières

- [Fonctionnalités](#fonctionnalités)
- [Feature Flags](#feature-flags)
- [Prérequis](#prérequis)
- [Installation – Windows Azure (App Service)](#installation--windows-azure-app-service)
- [Installation – Serveur OpenBSD avec services Azure](#installation--serveur-openbsd-avec-services-azure)
- [Référence de configuration](#référence-de-configuration)
- [Scripts utilitaires](#scripts-utilitaires)
- [Notes de sécurité](#notes-de-sécurité)

---

## Fonctionnalités

### Authentification Azure AD (OpenID Connect)
L'application authentifie les utilisateurs via la **Plateforme d'identité Microsoft** en utilisant le protocole OpenID Connect (via `Microsoft.Identity.Web`). Toutes les routes sous `/Experimental` nécessitent une identité Azure AD authentifiée. Les pages `/Privacy`, `/Error` et `/About` sont accessibles publiquement.

### Authentification mTLS par certificat client
Lorsqu'il est activé, les clients doivent présenter un certificat X.509 valide. Les paramètres dans `MtlsSettings` contrôlent si les certificats chaînés, auto-signés ou les deux sont autorisés, la vérification de révocation des certificats et les émetteurs de certificats autorisés.

### Intégration Azure Key Vault
L'application récupère le **certificat serveur** TLS depuis Azure Key Vault au démarrage. Le `X509Certificate2` chargé est injecté directement dans la configuration HTTPS de Kestrel, sans nécessiter de fichier PFX sur le disque.

### Politique de sécurité du contenu avec nonces par requête
Lorsqu'il est activé, chaque réponse HTTP porte un en-tête `Content-Security-Policy` dont la directive `script-src` inclut un **nonce aléatoire cryptographiquement sécurisé** par requête. La CSP prend également en charge les listes d'autorisation basées sur des hachages SHA-256 pour les scripts en ligne.

### En-têtes de sécurité HTTP standard
`UseStandardSecurityHeaders` ajoute à chaque réponse : `X-Frame-Options`, `X-Content-Type-Options`, `Strict-Transport-Security`, `Referrer-Policy`, `Cross-Origin-Opener-Policy`, `Cross-Origin-Resource-Policy`, `Permissions-Policy`, ainsi que la suppression des en-têtes `Server`, `X-Powered-By` et `X-AspNetMvc-Version`.

### Azure Blob Storage
Lorsqu'il est activé, `BlobSettingsService` fournit un service Scoped alimenté par une chaîne de connexion et un nombre maximum configurable de pièces jointes.

### Azure Cosmos DB
Lorsqu'il est activé, l'application vérifie la connexion à Cosmos DB au démarrage en appelant `database.ReadAsync()`.

### Gestion de session sécurisée
Les sessions utilisent un cache mémoire distribué en processus avec un **délai d'inactivité de 30 minutes**. Les cookies de session sont configurés avec `HttpOnly`, `Secure = Always` et `SameSite = Strict`.

### Localisation
L'application prend en charge **11 langues** : en-US, de-DE, es-ES, fr-FR, pt-PT, it-IT, zh-HK, ko-KR, hi-IN, ru-RU et ar-SA. L'arabe inclut un basculement automatique de la mise en page RTL.

### Journalisation sécurisée pour les données PII
`LoggingHelper` hache les informations personnelles identifiables dans la sortie de journalisation à l'aide de HMAC-SHA256. Une clé stable de 32 octets peut être fournie via `Logging:PiiHmacKey`.

---

## Feature Flags

Tous les sous-systèmes majeurs sont contrôlés par des flags booléens dans `appsettings.json`.

| Flag | Valeur par défaut | Description |
|---|---|---|
| `EnableSession` | `true` | Session côté serveur et cookie de session |
| `EnableLocalization` | `true` | Support multilingue (11 langues) |
| `EnableAzureAd` | `true` | Authentification Azure AD / OpenID Connect |
| `EnableAuthorization` | `true` | Politiques d'autorisation au niveau des routes |
| `EnableKeyVault` | `false` | Charger le certificat TLS du serveur depuis Azure Key Vault |
| `EnableNonceServices` | `false` | Génération de nonce CSP par requête |
| `EnableCSP` | `false` | Ajouter l'en-tête `Content-Security-Policy` |
| `EnableSecurityHeaders` | `true` | Ajouter les en-têtes de sécurité HTTP standard |
| `EnableBlobStorage` | `false` | Service Azure Blob Storage |
| `EnableCosmosDb` | `false` | Service Azure Cosmos DB |
| `EnableMtls` | `false` | Exiger des certificats TLS clients |
| `EnableOcspValidation` | `false` | Vérification de révocation OCSP (stub) |

---

## Prérequis

1. **Enregistrement d'application Azure AD** – avec URI de redirection, secret client ou identifiant de certificat.
2. **Azure Key Vault** – contenant le certificat serveur PFX en tant que secret.
3. **Compte Azure Cosmos DB** (optionnel).
4. **Compte Azure Blob Storage** (optionnel).
5. **.NET 9 SDK / Runtime** – version 9.0 ou ultérieure.

---

## Référence de configuration

Copiez `appsettings.template.json` vers `appsettings.json` et remplacez toutes les valeurs `{{PLACEHOLDER}}`. Stockez les secrets dans **.NET User Secrets** (local) ou dans Azure App Settings / Key Vault References (production) — jamais dans le code source.

---

## Notes de sécurité

- **Ne jamais valider des secrets dans le contrôle de code source.**
- L'implémentation de validation OCSP est un **stub** qui rejette tous les certificats. Remplacez `PerformOcspValidationAsync` avant d'activer `EnableOcspValidation` en production.
- Les valeurs nonce ne sont **jamais journalisées**.
- L'en-tête de réponse `Server` est masqué par `webserver`.
