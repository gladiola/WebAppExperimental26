# WebAppExperimental26

Una aplicación web ASP.NET Core 9 Razor Pages con autenticación Azure AD, TLS mutuo (mTLS), gestión de certificados con Azure Key Vault, Azure Cosmos DB, Azure Blob Storage y una capa de seguridad HTTP reforzada con política de seguridad de contenido basada en nonces.

---

## Tabla de contenidos

- [Características](#características)
- [Feature Flags](#feature-flags)
- [Requisitos previos](#requisitos-previos)
- [Instalación – Windows Azure (App Service)](#instalación--windows-azure-app-service)
- [Instalación – Servidor OpenBSD con servicios de Azure](#instalación--servidor-openbsd-con-servicios-de-azure)
- [Referencia de configuración](#referencia-de-configuración)
- [Scripts de soporte](#scripts-de-soporte)
- [Notas de seguridad](#notas-de-seguridad)

---

## Características

### Autenticación Azure AD (OpenID Connect)
La aplicación autentica a los usuarios a través de la **Plataforma de identidad de Microsoft** usando el protocolo OpenID Connect (mediante `Microsoft.Identity.Web`). Todas las rutas bajo `/Experimental` requieren una identidad Azure AD autenticada. Las páginas `/Privacy`, `/Error` y `/About` son de acceso público.

### Autenticación mTLS con certificados de cliente
Cuando está habilitado, los clientes deben presentar un certificado X.509 válido. La configuración en `MtlsSettings` controla si se permiten certificados encadenados, autofirmados o ambos, la verificación de revocación de certificados y los emisores de certificados permitidos.

### Integración con Azure Key Vault
La aplicación recupera el **certificado del servidor** TLS desde Azure Key Vault al inicio. El `X509Certificate2` cargado se inyecta directamente en la configuración HTTPS de Kestrel, por lo que no es necesario ningún archivo PFX en disco.

### Política de seguridad de contenido con nonces por solicitud
Cuando está habilitado, cada respuesta HTTP lleva un encabezado `Content-Security-Policy` cuya directiva `script-src` incluye un **nonce criptográficamente aleatorio** por solicitud. La CSP también admite listas de permitidos basadas en hash SHA-256 para scripts en línea.

### Encabezados de seguridad HTTP estándar
`UseStandardSecurityHeaders` agrega a cada respuesta: `X-Frame-Options`, `X-Content-Type-Options`, `Strict-Transport-Security`, `Referrer-Policy`, `Cross-Origin-Opener-Policy`, `Cross-Origin-Resource-Policy`, `Permissions-Policy` y elimina los encabezados `Server`, `X-Powered-By` y `X-AspNetMvc-Version`.

### Azure Blob Storage
Cuando está habilitado, `BlobSettingsService` proporciona un servicio Scoped respaldado por una cadena de conexión y un número máximo configurable de archivos adjuntos.

### Azure Cosmos DB
Cuando está habilitado, la aplicación verifica la conexión a Cosmos DB al inicio llamando a `database.ReadAsync()`.

### Gestión de sesiones segura
Las sesiones utilizan una caché de memoria distribuida en proceso con un **tiempo de espera de inactividad de 30 minutos**. Las cookies de sesión se configuran como `HttpOnly`, `Secure = Always` y `SameSite = Strict`.

### Localización
La aplicación admite **11 idiomas**: en-US, de-DE, es-ES, fr-FR, pt-PT, it-IT, zh-HK, ko-KR, hi-IN, ru-RU y ar-SA. El árabe incluye cambio automático de diseño RTL.

### Registro seguro para PII
`LoggingHelper` hashea la información de identificación personal en la salida de registro usando HMAC-SHA256. Se puede proporcionar una clave de 32 bytes estable a través de `Logging:PiiHmacKey`.

---

## Feature Flags

Todos los subsistemas principales están controlados por flags booleanas en `appsettings.json`.

| Flag | Valor predeterminado | Descripción |
|---|---|---|
| `EnableSession` | `true` | Sesión del servidor y cookie de sesión |
| `EnableLocalization` | `true` | Soporte multilingüe (11 idiomas) |
| `EnableAzureAd` | `true` | Autenticación Azure AD / OpenID Connect |
| `EnableAuthorization` | `true` | Políticas de autorización a nivel de ruta |
| `EnableKeyVault` | `false` | Cargar certificado TLS del servidor desde Azure Key Vault |
| `EnableNonceServices` | `false` | Generación de nonce CSP por solicitud |
| `EnableCSP` | `false` | Adjuntar encabezado `Content-Security-Policy` |
| `EnableSecurityHeaders` | `true` | Adjuntar encabezados de seguridad HTTP estándar |
| `EnableBlobStorage` | `false` | Servicio Azure Blob Storage |
| `EnableCosmosDb` | `false` | Servicio Azure Cosmos DB |
| `EnableMtls` | `false` | Requerir certificados TLS de cliente |
| `EnableOcspValidation` | `false` | Verificación de revocación OCSP (stub) |

---

## Requisitos previos

1. **Registro de aplicación Azure AD** – con URI de redirección, secreto de cliente o credencial de certificado.
2. **Azure Key Vault** – con el certificado del servidor PFX como secreto.
3. **Cuenta de Azure Cosmos DB** (opcional).
4. **Cuenta de Azure Blob Storage** (opcional).
5. **.NET 9 SDK / Runtime** – versión 9.0 o posterior.

---

## Instalación – Windows Azure (App Service)

### 1. Crear recursos de Azure

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

### 2. Registrar una aplicación de Azure AD

En el [Portal de Azure](https://portal.azure.com):
1. Navegue a **Microsoft Entra ID → Registros de aplicaciones → Nuevo registro**.
2. Establezca el URI de redirección en `https://<your-app>.azurewebsites.net/signin-oidc`.
3. En **Certificados y secretos**, cree un secreto de cliente y copie el valor.
4. Anote el **ID de inquilino** y el **ID de cliente** desde la hoja de información general.

### 3. Crear Azure Key Vault y cargar el certificado del servidor

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

### 4. Configurar los ajustes de la aplicación

Copie `appsettings.template.json` a `appsettings.json` y rellene los valores de marcador de posición. Los secretos **no deben** almacenarse en el control de código fuente — configúrelos como Configuración de la aplicación de App Service o mediante User Secrets localmente:

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

### 5. Implementar la aplicación

```bash
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
az webapp deployment source config-zip \
  --name MyWebApp26 --resource-group MyResourceGroup --src ../app.zip
```

### 6. Habilitar HTTPS y dominio personalizado (recomendado)

```powershell
# Force HTTPS
az webapp update --name MyWebApp26 --resource-group MyResourceGroup --https-only true

# Bind a custom domain and managed TLS certificate
az webapp config hostname add --webapp-name MyWebApp26 --resource-group MyResourceGroup \
  --hostname www.example.com
az webapp config ssl bind --certificate-thumbprint <THUMBPRINT> \
  --name MyWebApp26 --resource-group MyResourceGroup --ssl-type SNI
```

### 7. Habilitar mTLS en Azure App Service (opcional)

Azure App Service admite certificados de cliente a través del portal:
1. Vaya a **App Service → Configuración de TLS/SSL → Certificados de cliente**.
2. Establezca **Certificados de cliente entrantes** en **Requerir**.

Luego establezca `FeatureFlags__EnableMtls=true` en la Configuración de la aplicación.

---

## Instalación – Servidor OpenBSD con servicios de Azure

> **Importante:** .NET 9 **no** tiene una compilación oficial de Microsoft para OpenBSD. Las instrucciones siguientes utilizan un **contenedor compatible con Linux** (a través de [Podman](https://podman.io/), disponible en el árbol de paquetes de OpenBSD) para ejecutar la aplicación ASP.NET Core 9 en OpenBSD mientras se comunica con los servicios de Azure a través de HTTPS.

### 1. Instalar requisitos previos en OpenBSD

```sh
# As root
pkg_add podman
pkg_add curl git
```

Si Podman ni Docker están disponibles para su versión de OpenBSD, considere ejecutar la aplicación en una **VM de Linux** (p. ej., vmm(4) con un invitado Debian/Ubuntu) y siga la ruta de implementación estándar de Linux desde ese invitado.

### 2. Descargar la imagen de tiempo de ejecución de ASP.NET Core 9

```sh
podman pull mcr.microsoft.com/dotnet/aspnet:9.0
```

### 3. Compilar la aplicación (en una máquina de compilación Linux o Windows)

En una máquina con el SDK de .NET 9 instalado, publique una compilación autocontenida para Linux x64:

```bash
dotnet publish WebAppExperimental26/WebAppExperimental26.csproj \
  -c Release -r linux-x64 --self-contained true -o ./publish
```

Transfiera el directorio `publish/` al host OpenBSD (p. ej., mediante `scp` o un volumen compartido).

### 4. Crear un archivo de configuración

En el host OpenBSD, cree `/etc/webappexp26/appsettings.json` con sus valores de producción (sin secretos en el archivo; use variables de entorno en su lugar):

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

Los secretos se inyectan como variables de entorno en el siguiente paso.

### 5. Ejecutar el contenedor

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

### 6. Configurar el firewall OpenBSD Packet Filter (pf)

Agregue a `/etc/pf.conf` para permitir HTTPS entrante y conexiones salientes a los endpoints de Azure:

```
# Allow inbound HTTPS
pass in on egress proto tcp to port 443

# Allow outbound to Azure AD, Key Vault, Cosmos DB, Blob Storage
pass out on egress proto tcp to port { 443 }
```

Recargar el conjunto de reglas:

```sh
pfctl -f /etc/pf.conf
```

### 7. Configurar DNS y certificados TLS

Asegúrese de que el nombre de host en `AllowedHosts` resuelva a la IP pública del servidor OpenBSD. Azure AD requiere que el URI de redirección (`/signin-oidc`) sea accesible a través de HTTPS, por lo que el certificado del servidor debe ser de confianza. Use un certificado de una CA pública (p. ej., Let's Encrypt mediante `acme-client(1)`) o cargue un certificado firmado por CA en Azure Key Vault y habilite `EnableKeyVault`.

### 8. Conectividad saliente a los servicios de Azure

Los siguientes endpoints de servicio de Azure deben ser accesibles desde el host OpenBSD a través de TCP 443:

| Service | Endpoint |
|---|---|
| Azure AD / Microsoft Identity | `login.microsoftonline.com` |
| Azure Key Vault | `<vault-name>.vault.azure.net` |
| Azure Cosmos DB | `<account>.documents.azure.com` |
| Azure Blob Storage | `<account>.blob.core.windows.net` |

Pruebe la conectividad antes de iniciar el contenedor:

```sh
curl -I https://login.microsoftonline.com
curl -I https://YOUR_KEYVAULT_NAME.vault.azure.net
```

---

## Referencia de configuración

Copie `appsettings.template.json` a `appsettings.json` y reemplace todos los valores `{{PLACEHOLDER}}`.

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

Genere claves de cifrado e IVs usando el script de PowerShell incluido:

```powershell
.\WebAppExperimental26\SupportingScripts\IVandKeySampleGenerator.ps1
```

Almacene todos los secretos en **.NET User Secrets** para el desarrollo local:

```bash
dotnet user-secrets set "AzureAd:ClientSecret" "YOUR_SECRET"
dotnet user-secrets set "AzureKeyVault:KeyVaultSecret" "YOUR_KV_SECRET"
dotnet user-secrets set "NonceEncryption:Key" "YOUR_BASE64_KEY"
dotnet user-secrets set "NonceEncryption:IV" "YOUR_BASE64_IV"
```

---

## Scripts de soporte

El directorio `SupportingScripts/` contiene utilidades de PowerShell:

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

## Notas de seguridad

- **Nunca confirme secretos en el control de código fuente.**
- La implementación de validación OCSP es un **stub** que rechaza todos los certificados. Reemplace `PerformOcspValidationAsync` antes de habilitar `EnableOcspValidation` en producción.
- Los valores nonce **nunca se registran** en los logs.
- El encabezado de respuesta `Server` se enmascara como `webserver`.
