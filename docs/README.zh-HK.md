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

## 設定參考

將 `appsettings.template.json` 複製到 `appsettings.json` 並替換所有 `{{PLACEHOLDER}}` 值。將機密存儲在 **.NET User Secrets**（本機）或 Azure App Settings / Key Vault References（生產環境）中，切勿存入原始碼。

---

## 安全性注意事項

- **切勿將機密提交到版本控制系統。**
- OCSP 驗證實作為 **Stub**，會拒絕所有憑證。在生產環境啟用 `EnableOcspValidation` 之前，請替換 `PerformOcspValidationAsync`。
- Nonce 值**永遠不會記錄**在日誌中。
- `Server` 回應標頭被遮蔽為 `webserver`。
