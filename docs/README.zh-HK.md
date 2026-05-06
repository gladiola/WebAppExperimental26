# WebAppExperimental26

一個 ASP.NET Core 9 Razor Pages 網頁應用程式，具備 Azure AD 身份驗證、雙向 TLS（mTLS）、Azure Key Vault 憑證管理、Azure Cosmos DB、Azure Blob Storage，以及基於 nonce 的 Content Security Policy 強化 HTTP 安全層。

---

## 目錄

- [功能](#功能)
- [功能標誌](#功能標誌)
- [前提條件](#前提條件)
- [安裝 – Windows Azure（App Service）](#安裝--windows-azure-app-service)
- [安裝 – 與 Azure 服務通訊的 OpenBSD 伺服器](#安裝--與-azure-服務通訊的-openbsd-伺服器)
- [設定參考](#設定參考)
- [支援腳本](#支援腳本)
- [安全性注意事項](#安全性注意事項)

---

## 功能

### Azure AD 身份驗證（OpenID Connect）
應用程式透過 **Microsoft 身份平台** 使用 OpenID Connect 協定（透過 `Microsoft.Identity.Web`）驗證用戶。`/Experimental` 下的所有路由都需要已驗證的 Azure AD 身份。`/Privacy`、`/Error` 和 `/About` 頁面可公開訪問。

### mTLS 客戶端憑證驗證
啟用後，客戶端必須提供有效的 X.509 憑證。`MtlsSettings` 中的設定控制是否允許鏈結、自簽或兩者兼用的憑證，以及憑證撤銷檢查和允許的憑證簽發者。

### Azure Key Vault 整合
應用程式在啟動時從 Azure Key Vault 取得 TLS **伺服器憑證**。載入的 `X509Certificate2` 直接注入 Kestrel 的 HTTPS 預設值，無需磁碟上的 PFX 檔案。

### 每次請求使用 Nonce 的 Content Security Policy
啟用後，每個 HTTP 回應都帶有 `Content-Security-Policy` 標頭，其 `script-src` 指令包含每次請求生成的**加密隨機 nonce**。CSP 亦支援針對內聯腳本的 SHA-256 雜湊白名單。

### 標準 HTTP 安全標頭
`UseStandardSecurityHeaders` 為每個回應附加：`X-Frame-Options`、`X-Content-Type-Options`、`Strict-Transport-Security`、`Referrer-Policy`、`Cross-Origin-Opener-Policy`、`Cross-Origin-Resource-Policy`、`Permissions-Policy`，並移除 `Server`、`X-Powered-By` 和 `X-AspNetMvc-Version` 標頭。

### Azure Blob Storage
啟用後，`BlobSettingsService` 提供由連接字串和可設定最大附件數支援的 Scoped 服務。

### Azure Cosmos DB
啟用後，應用程式在啟動時透過呼叫 `database.ReadAsync()` 驗證 Cosmos DB 連線。

### 安全的 Session 管理
Session 使用進程內分散式記憶體快取，具有 **30 分鐘閒置逾時**。Session Cookie 設定為 `HttpOnly`、`Secure = Always` 和 `SameSite = Strict`。

### 本地化
應用程式支援 **11 種語言**：en-US、de-DE、es-ES、fr-FR、pt-PT、it-IT、zh-HK、ko-KR、hi-IN、ru-RU 和 ar-SA。阿拉伯語包含自動 RTL 版面切換。

### PII 安全日誌記錄
`LoggingHelper` 使用 HMAC-SHA256 對日誌輸出中的個人可識別資訊進行雜湊處理。可透過 `Logging:PiiHmacKey` 提供穩定的 32 字節密鑰。

---

## 功能標誌

所有主要子系統由 `appsettings.json` 中的布林功能標誌控制。

| 標誌 | 預設值 | 描述 |
|---|---|---|
| `EnableSession` | `true` | 伺服器端 Session 和 Session Cookie |
| `EnableLocalization` | `true` | 多語言支援（11 種語言） |
| `EnableAzureAd` | `true` | Azure AD / OpenID Connect 身份驗證 |
| `EnableAuthorization` | `true` | 路由層級授權政策 |
| `EnableKeyVault` | `false` | 從 Azure Key Vault 載入 TLS 伺服器憑證 |
| `EnableNonceServices` | `false` | 每次請求的 CSP nonce 生成 |
| `EnableCSP` | `false` | 附加 `Content-Security-Policy` 標頭 |
| `EnableSecurityHeaders` | `true` | 附加標準 HTTP 安全標頭 |
| `EnableBlobStorage` | `false` | Azure Blob Storage 服務 |
| `EnableCosmosDb` | `false` | Azure Cosmos DB 服務 |
| `EnableMtls` | `false` | 要求客戶端 TLS 憑證 |
| `EnableOcspValidation` | `false` | OCSP 憑證撤銷檢查（Stub） |

---

## 前提條件

1. **Azure AD 應用程式註冊** – 含重新導向 URI、客戶端密碼或憑證憑據。
2. **Azure Key Vault** – 含 PFX 伺服器憑證作為 Secret。
3. **Azure Cosmos DB 帳戶**（選用）。
4. **Azure Blob Storage 帳戶**（選用）。
5. **.NET 9 SDK / Runtime** – 9.0 版或更高版本。

---

## 安裝 – Windows Azure（App Service）

### 1. 建立 Azure 資源

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

### 2. 註冊 Azure AD 應用程式

在 [Azure 入口網站](https://portal.azure.com)：
1. 前往 **Microsoft Entra ID → 應用程式註冊 → 新增註冊**。
2. 將重新導向 URI 設定為 `https://<your-app>.azurewebsites.net/signin-oidc`。
3. 在 **憑證和密碼** 下，建立用戶端密碼並複製其值。
4. 從概觀刀鋒視窗記下 **租用戶識別碼** 和 **用戶端識別碼**。

### 3. 建立 Azure Key Vault 並上傳伺服器憑證

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

### 4. 設定應用程式設定

將 `appsettings.template.json` 複製到 `appsettings.json` 並填入占位符值。機密**不得**儲存在原始碼控制中 — 將其設定為 App Service 應用程式設定或在本機透過 User Secrets 設定：

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

### 5. 部署應用程式

```bash
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
az webapp deployment source config-zip \
  --name MyWebApp26 --resource-group MyResourceGroup --src ../app.zip
```

### 6. 啟用 HTTPS 和自訂網域（建議）

```powershell
# Force HTTPS
az webapp update --name MyWebApp26 --resource-group MyResourceGroup --https-only true

# Bind a custom domain and managed TLS certificate
az webapp config hostname add --webapp-name MyWebApp26 --resource-group MyResourceGroup \
  --hostname www.example.com
az webapp config ssl bind --certificate-thumbprint <THUMBPRINT> \
  --name MyWebApp26 --resource-group MyResourceGroup --ssl-type SNI
```

### 7. 在 Azure App Service 上啟用 mTLS（選用）

Azure App Service 透過入口網站支援用戶端憑證：
1. 前往 **App Service → TLS/SSL 設定 → 用戶端憑證**。
2. 將 **傳入用戶端憑證** 設定為 **需要**。

然後在應用程式設定中設定 `FeatureFlags__EnableMtls=true`。

---

## 安裝 – 與 Azure 服務通訊的 OpenBSD 伺服器

> **重要：** .NET 9 **沒有**適用於 OpenBSD 的官方 Microsoft 組建。以下指示使用 **Linux 相容容器**（透過 [Podman](https://podman.io/)，可在 OpenBSD 的套件樹中取得）在 OpenBSD 上執行 ASP.NET Core 9 應用程式，同時透過 HTTPS 與 Azure 服務通訊。

### 1. 在 OpenBSD 上安裝必要條件

```sh
# As root
pkg_add podman
pkg_add curl git
```

如果您的 OpenBSD 版本不提供 Podman 或 Docker，請考慮在 **Linux VM**（例如具有 Debian/Ubuntu Guest 的 vmm(4)）中執行應用程式，並從該 Guest 系統中遵循標準 Linux 部署路徑。

### 2. 提取 ASP.NET Core 9 執行階段映像

```sh
podman pull mcr.microsoft.com/dotnet/aspnet:9.0
```

### 3. 建置應用程式（在 Linux 或 Windows 建置機器上）

在安裝了 .NET 9 SDK 的機器上，發佈以 Linux x64 為目標的獨立組建：

```bash
dotnet publish WebAppExperimental26/WebAppExperimental26.csproj \
  -c Release -r linux-x64 --self-contained true -o ./publish
```

將 `publish/` 目錄傳輸到 OpenBSD 主機（例如透過 `scp` 或共享磁碟區）。

### 4. 建立設定檔

在 OpenBSD 主機上，使用您的生產值建立 `/etc/webappexp26/appsettings.json`（檔案中不含機密；改用環境變數）：

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

機密將在下一步中作為環境變數注入。

### 5. 執行容器

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

### 6. 設定 OpenBSD 封包過濾器 (pf) 防火牆

新增至 `/etc/pf.conf` 以允許傳入的 HTTPS 並允許傳出連線至 Azure 端點：

```
# Allow inbound HTTPS
pass in on egress proto tcp to port 443

# Allow outbound to Azure AD, Key Vault, Cosmos DB, Blob Storage
pass out on egress proto tcp to port { 443 }
```

重新載入規則集：

```sh
pfctl -f /etc/pf.conf
```

### 7. 設定 DNS 和 TLS 憑證

確保 `AllowedHosts` 中的主機名稱解析為 OpenBSD 伺服器的公用 IP。Azure AD 要求重新導向 URI（`/signin-oidc`）可透過 HTTPS 存取，因此伺服器憑證必須受信任。使用來自公用 CA 的憑證（例如透過 `acme-client(1)` 使用 Let's Encrypt），或將 CA 簽署的憑證上傳至 Azure Key Vault 並啟用 `EnableKeyVault`。

### 8. 對 Azure 服務的傳出連線

以下 Azure 服務端點必須能從 OpenBSD 主機透過 TCP 443 存取：

| Service | Endpoint |
|---|---|
| Azure AD / Microsoft Identity | `login.microsoftonline.com` |
| Azure Key Vault | `<vault-name>.vault.azure.net` |
| Azure Cosmos DB | `<account>.documents.azure.com` |
| Azure Blob Storage | `<account>.blob.core.windows.net` |

在啟動容器之前測試連線：

```sh
curl -I https://login.microsoftonline.com
curl -I https://YOUR_KEYVAULT_NAME.vault.azure.net
```

---

## 設定參考

將 `appsettings.template.json` 複製到 `appsettings.json` 並替換所有 `{{PLACEHOLDER}}` 值。

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

使用包含的 PowerShell 腳本產生加密金鑰和 IV：

```powershell
.\WebAppExperimental26\SupportingScripts\IVandKeySampleGenerator.ps1
```

將所有機密儲存在 **.NET User Secrets** 中以用於本機開發：

```bash
dotnet user-secrets set "AzureAd:ClientSecret" "YOUR_SECRET"
dotnet user-secrets set "AzureKeyVault:KeyVaultSecret" "YOUR_KV_SECRET"
dotnet user-secrets set "NonceEncryption:Key" "YOUR_BASE64_KEY"
dotnet user-secrets set "NonceEncryption:IV" "YOUR_BASE64_IV"
```

---

## 支援腳本

`SupportingScripts/` 目錄包含 PowerShell 公用程式：

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

## 安全性注意事項

- **切勿將機密提交到版本控制系統。**
- OCSP 驗證實作為 **Stub**，會拒絕所有憑證。在生產環境啟用 `EnableOcspValidation` 之前，請替換 `PerformOcspValidationAsync`。
- Nonce 值**永遠不會記錄**在日誌中。
- `Server` 回應標頭被遮蔽為 `webserver`。
