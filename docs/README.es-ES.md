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

## Referencia de configuración

Copie `appsettings.template.json` a `appsettings.json` y reemplace todos los valores `{{PLACEHOLDER}}`. Guarde los secretos en **.NET User Secrets** (local) o en Azure App Settings / Key Vault References (producción), nunca en el código fuente.

---

## Notas de seguridad

- **Nunca confirme secretos en el control de código fuente.**
- La implementación de validación OCSP es un **stub** que rechaza todos los certificados. Reemplace `PerformOcspValidationAsync` antes de habilitar `EnableOcspValidation` en producción.
- Los valores nonce **nunca se registran** en los logs.
- El encabezado de respuesta `Server` se enmascara como `webserver`.
