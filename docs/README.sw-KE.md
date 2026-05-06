# WebAppExperimental26

Programu ya wavuti ya ASP.NET Core 9 Razor Pages yenye uthibitishaji wa Azure AD, TLS ya pande zote (mTLS), usimamizi wa cheti cha Azure Key Vault, Azure Cosmos DB, Azure Blob Storage, **AWS Secrets Manager**, **Amazon DynamoDB**, **Google Cloud Secret Manager**, **Google Cloud Firestore**, na tabaka la usalama la HTTP lililoimarishwa na sera ya usalama wa maudhui inayotumia nonce.

---

## Jedwali la Maudhui

- [Vipengele](#vipengele)
- [Alama za Vipengele](#alama-za-vipengele)
- [Mahitaji ya Awali](#mahitaji-ya-awali)
- [Ufungaji – Windows Azure (App Service)](#ufungaji--windows-azure-app-service)
- [Ufungaji – Seva ya OpenBSD inayowasiliana na Huduma za Azure](#ufungaji--seva-ya-openbsd-inayowasiliana-na-huduma-za-azure)
- [Marejeo ya Usanidi](#marejeo-ya-usanidi)
- [Hati za Msaada](#hati-za-msaada)
- [Maelezo ya Usalama](#maelezo-ya-usalama)

---

## Vipengele

### Uthibitishaji wa Azure AD (OpenID Connect)
Programu inawathibitisha watumiaji kupitia **Jukwaa la Utambulisho la Microsoft** kwa kutumia itifaki ya OpenID Connect (kupitia `Microsoft.Identity.Web`). Njia zote chini ya `/Experimental` zinahitaji utambulisho wa Azure AD uliothibitishwa. Kurasa za `/Privacy`, `/Error`, na `/About` zinapatikana hadharani.

### Uthibitishaji wa mTLS kwa Cheti cha Mteja
Inapowezeshwa, wateja lazima wawasilishe cheti halali cha X.509. Mipangilio katika `MtlsSettings` inadhibiti kama vyeti vilivyounganishwa, vilivyosainiwa na mtumiaji mwenyewe, au vyote viwili vinaruhusiwa, ukaguzi wa kufutwa kwa cheti, na watoa cheti wanaoruhusiwa.

### Ushirikiano wa Azure Key Vault
Programu inachukua **cheti cha seva** cha TLS kutoka Azure Key Vault wakati wa kuanza. `X509Certificate2` iliyopakiwa inawekwa moja kwa moja katika usanidi wa HTTPS wa Kestrel, hivyo faili ya PFX haihitajika kuwepo kwenye diski.

### Sera ya Usalama wa Maudhui yenye Nonce kwa Kila Ombi
Inapowezeshwa, kila jibu la HTTP lina kichwa cha `Content-Security-Policy` ambacho maelekezo ya `script-src` yake yanajumuisha **nonce ya nasibu ya kriptografia** kwa kila ombi. CSP pia inasaidia orodha ya ruhusa kulingana na heshi ya SHA-256 kwa hati za ndani.

### Vichwa vya Usalama vya HTTP vya Kawaida
`UseStandardSecurityHeaders` inaongeza kwenye kila jibu: `X-Frame-Options`, `X-Content-Type-Options`, `Strict-Transport-Security`, `Referrer-Policy`, `Cross-Origin-Opener-Policy`, `Cross-Origin-Resource-Policy`, `Permissions-Policy`, na kuondoa vichwa vya `Server`, `X-Powered-By`, na `X-AspNetMvc-Version`.

### Azure Blob Storage
Inapowezeshwa, `BlobSettingsService` inatoa huduma ya Scoped inayoungwa mkono na mfuatano wa muunganisho na idadi ya juu inayoweza kusanidiwa ya viambatisho.

### Azure Cosmos DB
Inapowezeshwa, programu inakagua muunganisho wa Cosmos DB wakati wa kuanza kwa kuita `database.ReadAsync()`.

### AWS Secrets Manager
Inapowezeshwa, `AwsSecretsManagerOperationsService` inachukua siri na vyeti kutoka AWS Secrets Manager. Usanidi katika sehemu ya `AwsSecretsManager` na vigezo: `Region`, `CertificateSecretName`, `IVSecretName`, `NonceKeySecretName`, na vitambulisho `AccessKeyId`/`SecretAccessKey`.

### Amazon DynamoDB
Inapowezeshwa, `AwsDynamoDbService` inakagua muunganisho wa jedwali la DynamoDB wakati wa kuanza. Usanidi katika sehemu ya `AwsDynamoDb` na vigezo: `Region`, `TableName`, na vitambulisho `AccessKeyId`/`SecretAccessKey`.

### GCP Secret Manager
Inapowezeshwa, `GcpSecretManagerOperationsService` inachukua siri kutoka Google Cloud Secret Manager. Usanidi katika sehemu ya `GcpSecretManager` na vigezo: `ProjectId`, `CertificateSecretId`, `IVSecretId`, `NonceKeySecretId`, na `CredentialFilePath` (si lazima, inatumia ADC ikiwa iko tupu).

### GCP Firestore
Inapowezeshwa, `GcpFirestoreService` inajenga mteja wa Firestore wakati wa kuanza. Usanidi katika sehemu ya `GcpFirestore` na vigezo: `ProjectId`, `DatabaseId` (chaguo-msingi: "(default)"), `CollectionName`, na `CredentialFilePath` (si lazima).

### Usimamizi wa Utambulisho wa AWS Cognito
Inapowezeshwa, `AddAwsCognitoAuthentication` inasanidi uthibitishaji wa OpenID Connect dhidi ya **Amazon Cognito User Pool** — sawa na Microsoft Entra ID / Azure AD kwa AWS. Mwisho wa ugunduzi wa OIDC ni:
`https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}/.well-known/openid-configuration`
Usanidi katika sehemu ya `AwsCognito`: `Region`, `UserPoolId`, `AppClientId`, `AppClientSecret` (hifadhi katika Siri za Mtumiaji), na `Domain`.

### Jukwaa la Utambulisho la GCP
Inapowezeshwa, `AddGcpIdentityAuthentication` inasanidi uthibitishaji wa OpenID Connect kwa kutumia **Google OAuth 2.0 / OIDC** — sawa na Microsoft Entra ID / Azure AD kwa GCP. Mwisho wa ugunduzi wa OIDC ni:
`https://accounts.google.com/.well-known/openid-configuration`
Usanidi katika sehemu ya `GcpIdentity`: `ClientId`, `ClientSecret` (hifadhi katika Siri za Mtumiaji), na `ProjectId` si lazima.

### Usimamizi wa Kipindi Salama
Vipindi vinatumia hifadhi ya kumbukumbu iliyosambazwa ndani ya mchakato na **muda wa kuisha wa dakika 30**. Vidakuzi vya kipindi vinaundwa kwa `HttpOnly`, `Secure = Always`, na `SameSite = Strict`.

### Ujanibishaji
Programu inasaidia **lugha 25**: en-US, de-DE, es-ES, fr-FR, pt-PT, it-IT, zh-HK, ko-KR, hi-IN, ru-RU, ar-SA, sw-KE, ja-JP, ht-HT, haw-US, sm-WS, mi-NZ, af-ZA, nl-NL, ha-NG, am-ET, yo-NG, bn-BD, zh-CN, na ga-IE. Kiarabu kinajumuisha kubadilisha mpangilio wa RTL kiotomatiki.

### Uandishi wa Kumbukumbu Salama kwa Data ya PII
`LoggingHelper` inafanya heshi ya taarifa za kibinafsi zinazotambuliwa katika matokeo ya kumbukumbu kwa kutumia HMAC-SHA256. Ufunguo wa byte 32 unaweza kutolewa kupitia `Logging:PiiHmacKey`.

---

## Alama za Vipengele

Mifumo mikubwa yote inadhibitiwa na alama za boolean katika `appsettings.json`.

| Alama | Chaguo-msingi | Maelezo |
|---|---|---|
| `EnableSession` | `true` | Kipindi cha seva na kidakuzi cha kipindi |
| `EnableLocalization` | `true` | Msaada wa lugha nyingi (lugha 25) |
| `EnableAzureAd` | `true` | Uthibitishaji wa Azure AD / OpenID Connect |
| `EnableAuthorization` | `true` | Sera za idhini katika kiwango cha njia |
| `EnableKeyVault` | `false` | Pakia cheti cha TLS cha seva kutoka Azure Key Vault |
| `EnableNonceServices` | `false` | Uzalishaji wa nonce ya CSP kwa kila ombi |
| `EnableCSP` | `false` | Ongeza kichwa cha `Content-Security-Policy` |
| `EnableSecurityHeaders` | `true` | Ongeza vichwa vya usalama vya HTTP vya kawaida |
| `EnableBlobStorage` | `false` | Huduma ya Azure Blob Storage |
| `EnableCosmosDb` | `false` | Huduma ya Azure Cosmos DB |
| `EnableAwsSecretsManager` | `false` | AWS Secrets Manager (mfano) |
| `EnableAwsDynamoDb` | `false` | Amazon DynamoDB |
| `EnableAwsCognito` | `false` | Usimamizi wa utambulisho wa AWS Cognito (OpenID Connect) |
| `EnableGcpSecretManager` | `false` | GCP Secret Manager (mfano) |
| `EnableGcpFirestore` | `false` | Google Cloud Firestore |
| `EnableGcpIdentity` | `false` | Jukwaa la Utambulisho la GCP (Google OAuth 2.0 / OIDC) |
| `EnableMtls` | `false` | Hitaji vyeti vya TLS vya mteja |
| `EnableOcspValidation` | `false` | Ukaguzi wa kufutwa kwa cheti cha OCSP (mfano) |

---

## Mahitaji ya Awali

1. **Usajili wa Programu ya Azure AD** – na URI ya kuelekezwa, siri ya mteja au kitambulisho cha cheti.
2. **Azure Key Vault** – yenye cheti cha seva cha PFX kama siri.
3. **Akaunti ya Azure Cosmos DB** (si lazima).
4. **Akaunti ya Azure Blob Storage** (si lazima).
5. **.NET 9 SDK / Runtime** – toleo 9.0 au zaidi.
6. **Vitambulisho vya AWS** (mtumiaji au jukumu la IAM lenye ruhusa za `secretsmanager` na `dynamodb`) – zinahitajika wakati `EnableAwsSecretsManager` au `EnableAwsDynamoDb` zimewezeshwa.
7. **Akaunti ya huduma ya GCP au ADC** (yenye ruhusa za `secretmanager` na `datastore`) – zinahitajika wakati `EnableGcpSecretManager` au `EnableGcpFirestore` zimewezeshwa.

---

## Ufungaji – Windows Azure (App Service)

### 1. Unda Rasilimali za Azure

```powershell
# Ingia
az login

# Unda kikundi cha rasilimali
az group create --name MyResourceGroup --location eastus

# Unda mpango wa App Service (Linux au Windows)
az appservice plan create --name MyPlan --resource-group MyResourceGroup --sku B1 --is-linux

# Unda programu ya wavuti (.NET 9)
az webapp create --name MyWebApp26 --resource-group MyResourceGroup \
  --plan MyPlan --runtime "DOTNETCORE:9.0"
```

### 2. Sajili Programu ya Azure AD

Katika [Lango la Azure](https://portal.azure.com):
1. Nenda kwa **Microsoft Entra ID → Usajili wa programu → Usajili mpya**.
2. Weka URI ya kuelekezwa kuwa `https://<your-app>.azurewebsites.net/signin-oidc`.
3. Chini ya **Vyeti na siri**, unda siri ya mteja na nakili thamani.
4. Angalia **Kitambulisho cha Mpangilio** na **Kitambulisho cha Mteja** kutoka kwenye kidirisha cha Muhtasari.

### 3. Unda Azure Key Vault na Pakia Cheti cha Seva

```powershell
az keyvault create --name MyKeyVault26 --resource-group MyResourceGroup --location eastus

# Pakia PFX yako kama siri ya Key Vault (imewekwa kwa base64)
$pfxBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes("server.pfx"))
az keyvault secret set --vault-name MyKeyVault26 --name "ServerCert" --value $pfxBase64

# Toa ruhusa kwa Utambulisho Unaosimamiwa wa App Service
az keyvault set-policy --name MyKeyVault26 \
  --object-id <managed-identity-object-id> \
  --secret-permissions get list
```

### 4. Sanidi Mipangilio ya Programu

Nakili `appsettings.template.json` kwenye `appsettings.json` na jaza thamani za nafasi-weka. Siri **hazipaswi** kuhifadhiwa kwenye udhibiti wa chanzo — ziweke kama Mipangilio ya Programu ya App Service au kupitia Siri za Mtumiaji ndani ya nchi:

```powershell
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

### 5. Weka Programu

```bash
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
az webapp deployment source config-zip \
  --name MyWebApp26 --resource-group MyResourceGroup --src ../app.zip
```

### 6. Wezesha HTTPS na Kikoa cha Kawaida (inapendekezwa)

```powershell
az webapp update --name MyWebApp26 --resource-group MyResourceGroup --https-only true

az webapp config hostname add --webapp-name MyWebApp26 --resource-group MyResourceGroup \
  --hostname www.example.com
az webapp config ssl bind --certificate-thumbprint <THUMBPRINT> \
  --name MyWebApp26 --resource-group MyResourceGroup --ssl-type SNI
```

### 7. Wezesha mTLS kwenye Azure App Service (si lazima)

Azure App Service inasaidia vyeti vya mteja kupitia lango:
1. Nenda kwa **App Service → Mipangilio ya TLS/SSL → Vyeti vya mteja**.
2. Weka **Vyeti vya mteja vinavyoingia** kuwa **Inahitajika**.

Kisha weka `FeatureFlags__EnableMtls=true` katika Mipangilio ya Programu.

---

## Ufungaji – Seva ya OpenBSD inayowasiliana na Huduma za Azure

> **Muhimu:** .NET 9 **haina** toleo rasmi la Microsoft kwa OpenBSD. Maelekezo yafuatayo yanatumia **chombo cha kontena kinachooana na Linux** (kupitia [Podman](https://podman.io/), ambacho kinapatikana katika mti wa pakiti za OpenBSD) kuendesha programu ya ASP.NET Core 9 kwenye OpenBSD huku ikiwasiliana na huduma za Azure kupitia HTTPS.

### 1. Sakinisha Mahitaji ya Awali kwenye OpenBSD

```sh
# Kama mtumiaji wa msingi
pkg_add podman
pkg_add curl git
```

### 2. Pakua Picha ya ASP.NET Core 9 Runtime

```sh
podman pull mcr.microsoft.com/dotnet/aspnet:9.0
```

### 3. Jenga Programu (kwenye mashine ya kujenga ya Linux au Windows)

```bash
dotnet publish WebAppExperimental26/WebAppExperimental26.csproj \
  -c Release -r linux-x64 --self-contained true -o ./publish
```

### 4. Unda Faili ya Usanidi

Kwenye mwenyeji wa OpenBSD, unda `/etc/webappexp26/appsettings.json`:

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

### 5. Anzisha Kontena

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

### 6. Sanidi Firewall ya OpenBSD Packet Filter (pf)

Ongeza kwenye `/etc/pf.conf`:

```
# Ruhusu HTTPS inayoingia
pass in on egress proto tcp to port 443

# Ruhusu nje kuelekea Azure AD, Key Vault, Cosmos DB, Blob Storage
pass out on egress proto tcp to port { 443 }
```

Pakia upya sheria:

```sh
pfctl -f /etc/pf.conf
```

### 7. Sanidi DNS na Vyeti vya TLS

Hakikisha jina la mwenyeji katika `AllowedHosts` linatatua kwa IP ya umma ya seva ya OpenBSD. Azure AD inahitaji URI ya kuelekezwa (`/signin-oidc`) ifikie kupitia HTTPS.

### 8. Muunganisho wa Nje kuelekea Huduma za Azure

| Huduma | Mwisho |
|---|---|
| Azure AD / Microsoft Identity | `login.microsoftonline.com` |
| Azure Key Vault | `<vault-name>.vault.azure.net` |
| Azure Cosmos DB | `<account>.documents.azure.com` |
| Azure Blob Storage | `<account>.blob.core.windows.net` |
| AWS Secrets Manager | `secretsmanager.REGION.amazonaws.com` |
| Amazon DynamoDB | `dynamodb.REGION.amazonaws.com` |
| GCP Secret Manager | `secretmanager.googleapis.com` |
| GCP Firestore | `firestore.googleapis.com` |

---

## Marejeo ya Usanidi

Nakili `appsettings.template.json` kwenye `appsettings.json` na ubadilishe thamani zote za `{{PLACEHOLDER}}`.

| Sehemu | Ufunguo | Maelezo |
|---|---|---|
| `AzureAd` | `TenantId`, `ClientId`, `ClientSecret` | Usajili wa programu ya Azure AD |
| `AzureKeyVault` | `KeyVaultURL`, `KeyVaultSecret`, `KeyVaultPassName` | Key Vault na jina la cheti |
| `MtlsSettings` | `RequireClientCertificate`, `AllowedIssuers` | Sera ya cheti cha mteja cha mTLS |
| `NonceEncryption` | `Key`, `IV` | Ufunguo wa byte 32 na IV ya byte 16 kwa usimbaji wa nonce (base64) |
| `BlobSettings` | `BlobConnectionString`, `MaxAttachments` | Muunganisho wa Blob Storage |
| `CosmosDb` | `CosmosConnectionString`, `DatabaseName`, `ContainerName` | Muunganisho wa Cosmos DB |
| `AwsSecretsManager` | `Region`, `CertificateSecretName`, `IVSecretName`, `NonceKeySecretName`, `AccessKeyId`, `SecretAccessKey` | AWS Secrets Manager |
| `AwsDynamoDb` | `Region`, `TableName`, `AccessKeyId`, `SecretAccessKey` | Amazon DynamoDB |
| `GcpSecretManager` | `ProjectId`, `CertificateSecretId`, `IVSecretId`, `NonceKeySecretId`, `CredentialFilePath` | GCP Secret Manager |
| `GcpFirestore` | `ProjectId`, `DatabaseId`, `CollectionName`, `CredentialFilePath` | Google Cloud Firestore |
| `OcspSettings` | `OcspServerUrl`, `CacheDurationMinutes` | Uthibitishaji wa OCSP (mfano) |
| `Logging` | `PiiHmacKey` | Ufunguo wa HMAC wa base64 wa byte 32 kwa heshi ya PII katika kumbukumbu |

Zalisha funguo za usimbaji na IVs kwa kutumia hati ya PowerShell iliyojumuishwa:

```powershell
.\WebAppExperimental26\SupportingScripts\IVandKeySampleGenerator.ps1
```

Hifadhi siri zote katika **.NET User Secrets** kwa maendeleo ya ndani ya nchi:

```bash
dotnet user-secrets set "AzureAd:ClientSecret" "YOUR_SECRET"
dotnet user-secrets set "AzureKeyVault:KeyVaultSecret" "YOUR_KV_SECRET"
dotnet user-secrets set "NonceEncryption:Key" "YOUR_BASE64_KEY"
dotnet user-secrets set "NonceEncryption:IV" "YOUR_BASE64_IV"
dotnet user-secrets set "AwsSecretsManager:AccessKeyId" "YOUR_AWS_ACCESS_KEY_ID"
dotnet user-secrets set "AwsSecretsManager:SecretAccessKey" "YOUR_AWS_SECRET_ACCESS_KEY"
dotnet user-secrets set "AwsDynamoDb:AccessKeyId" "YOUR_AWS_ACCESS_KEY_ID"
dotnet user-secrets set "AwsDynamoDb:SecretAccessKey" "YOUR_AWS_SECRET_ACCESS_KEY"
```

> Kwa GCP, weka kigezo cha mazingira ya `GOOGLE_APPLICATION_CREDENTIALS` kwenye njia ya faili ya JSON ya akaunti ya huduma au endesha `gcloud auth application-default login` kwa maendeleo ya ndani ya nchi.

---

## Hati za Msaada

Saraka ya `SupportingScripts/` ina zana za PowerShell:

| Hati | Madhumuni |
|---|---|
| `IVandKeySampleGenerator.ps1` | Zalisha ufunguo wa AES wa byte 32 wa nasibu na IV ya byte 16 (base64) |
| `HashInlineScriptPowerShell.ps1` | Hesabu heshi za SHA-256 kwa hati za ndani (kwa orodha ya ruhusa ya CSP) |
| `HashInlineScriptPowerShellBase64Output.ps1` | Kama hapo juu, inazalisha heshi katika muundo wa base64 |
| `CertificateUploaderToAzureExample.ps1` | Pakia cheti cha PFX kwenye Azure Key Vault |
| `CheckRoles.ps1` | Kagua ugawaji wa jukumu la RBAC la Azure kwa programu |
| `ExportResourceGroups.ps1` | Hamisha usanidi wa kikundi cha rasilimali za Azure |
| `TroubleshootingCosmosDBInfo.ps1` | Chunguza muunganisho wa Cosmos DB |
| `SetupFromTemplate.ps1` | Otomatisha usanidi wa awali kutoka `appsettings.template.json` |

---

## Maelezo ya Usalama

- **Usiwahi kuweka siri kwenye udhibiti wa chanzo.**
- Utekelezaji wa uthibitishaji wa OCSP ni **mfano** ambao unakataa vyeti vyote. Badilisha `PerformOcspValidationAsync` kabla ya kuwezesha `EnableOcspValidation` katika uzalishaji.
- Thamani za nonce **haziwahi kuandikwa kwenye kumbukumbu**.
- Kichwa cha jibu cha `Server` kimefichwa kuwa `webserver`.
- **Usihifadhi vitambulisho vya AWS au GCP kwenye udhibiti wa toleo.** Tumia vigeuzi vya mazingira au kidhibiti cha siri.
- Utekelezaji wa AWS na GCP ni **mifano** inayohitaji utekelezaji kamili kabla ya matumizi ya uzalishaji.
