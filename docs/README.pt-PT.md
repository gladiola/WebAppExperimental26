# WebAppExperimental26

Uma aplicação web ASP.NET Core 9 Razor Pages com autenticação Azure AD, TLS mútuo (mTLS), gestão de certificados via Azure Key Vault, Azure Cosmos DB, Azure Blob Storage e uma camada de segurança HTTP reforçada com política de segurança de conteúdo baseada em nonces.

---

## Índice

- [Funcionalidades](#funcionalidades)
- [Feature Flags](#feature-flags)
- [Pré-requisitos](#pré-requisitos)
- [Instalação – Windows Azure (App Service)](#instalação--windows-azure-app-service)
- [Instalação – Servidor OpenBSD com serviços Azure](#instalação--servidor-openbsd-com-serviços-azure)
- [Referência de configuração](#referência-de-configuração)
- [Scripts de suporte](#scripts-de-suporte)
- [Notas de segurança](#notas-de-segurança)

---

## Funcionalidades

### Autenticação Azure AD (OpenID Connect)
A aplicação autentica utilizadores através da **Plataforma de Identidade da Microsoft** usando o protocolo OpenID Connect (via `Microsoft.Identity.Web`). Todas as rotas em `/Experimental` requerem uma identidade Azure AD autenticada. As páginas `/Privacy`, `/Error` e `/About` são acessíveis publicamente.

### Autenticação mTLS com Certificado de Cliente
Quando ativado, os clientes devem apresentar um certificado X.509 válido. As definições em `MtlsSettings` controlam se são permitidos certificados encadeados, auto-assinados ou ambos, a verificação de revogação de certificados e os emissores de certificados permitidos.

### Integração com Azure Key Vault
A aplicação obtém o **certificado do servidor** TLS a partir do Azure Key Vault no arranque. O `X509Certificate2` carregado é injetado diretamente na configuração HTTPS do Kestrel, não sendo necessário nenhum ficheiro PFX em disco.

### Política de Segurança de Conteúdo com Nonces por Pedido
Quando ativado, cada resposta HTTP contém um cabeçalho `Content-Security-Policy` cuja diretiva `script-src` inclui um **nonce aleatório criptograficamente seguro** por pedido. A CSP também suporta listas de permissões baseadas em hashes SHA-256 para scripts inline.

### Cabeçalhos de Segurança HTTP Padrão
`UseStandardSecurityHeaders` acrescenta a cada resposta: `X-Frame-Options`, `X-Content-Type-Options`, `Strict-Transport-Security`, `Referrer-Policy`, `Cross-Origin-Opener-Policy`, `Cross-Origin-Resource-Policy`, `Permissions-Policy` e remove os cabeçalhos `Server`, `X-Powered-By` e `X-AspNetMvc-Version`.

### Azure Blob Storage
Quando ativado, `BlobSettingsService` fornece um serviço Scoped suportado por uma cadeia de ligação e um número máximo configurável de anexos.

### Azure Cosmos DB
Quando ativado, a aplicação verifica a ligação ao Cosmos DB no arranque através da chamada `database.ReadAsync()`.

### Gestão Segura de Sessões
As sessões utilizam uma cache de memória distribuída em processo com um **tempo limite de inatividade de 30 minutos**. Os cookies de sessão são configurados como `HttpOnly`, `Secure = Always` e `SameSite = Strict`.

### Localização
A aplicação suporta **11 idiomas**: en-US, de-DE, es-ES, fr-FR, pt-PT, it-IT, zh-HK, ko-KR, hi-IN, ru-RU e ar-SA. O árabe inclui comutação automática para layout RTL.

### Registo Seguro para PII
`LoggingHelper` efetua o hash de informações de identificação pessoal na saída de registo usando HMAC-SHA256. Uma chave estável de 32 bytes pode ser fornecida via `Logging:PiiHmacKey`.

---

## Feature Flags

Todos os subsistemas principais são controlados por flags booleanas em `appsettings.json`.

| Flag | Predefinição | Descrição |
|---|---|---|
| `EnableSession` | `true` | Sessão do servidor e cookie de sessão |
| `EnableLocalization` | `true` | Suporte multilingue (11 idiomas) |
| `EnableAzureAd` | `true` | Autenticação Azure AD / OpenID Connect |
| `EnableAuthorization` | `true` | Políticas de autorização ao nível da rota |
| `EnableKeyVault` | `false` | Carregar certificado TLS do servidor a partir do Azure Key Vault |
| `EnableNonceServices` | `false` | Geração de nonce CSP por pedido |
| `EnableCSP` | `false` | Anexar cabeçalho `Content-Security-Policy` |
| `EnableSecurityHeaders` | `true` | Anexar cabeçalhos de segurança HTTP padrão |
| `EnableBlobStorage` | `false` | Serviço Azure Blob Storage |
| `EnableCosmosDb` | `false` | Serviço Azure Cosmos DB |
| `EnableMtls` | `false` | Exigir certificados TLS de cliente |
| `EnableOcspValidation` | `false` | Verificação de revogação OCSP (stub) |

---

## Pré-requisitos

1. **Registo de aplicação Azure AD** – com URI de redirecionamento, segredo de cliente ou credencial de certificado.
2. **Azure Key Vault** – com o certificado PFX do servidor como segredo.
3. **Conta Azure Cosmos DB** (opcional).
4. **Conta Azure Blob Storage** (opcional).
5. **.NET 9 SDK / Runtime** – versão 9.0 ou posterior.

---

## Referência de Configuração

Copie `appsettings.template.json` para `appsettings.json` e substitua todos os valores `{{PLACEHOLDER}}`. Guarde os segredos em **.NET User Secrets** (local) ou em Azure App Settings / Key Vault References (produção) — nunca no código fonte.

---

## Notas de Segurança

- **Nunca confirme segredos no controlo de versões.**
- A implementação de validação OCSP é um **stub** que rejeita todos os certificados. Substitua `PerformOcspValidationAsync` antes de ativar `EnableOcspValidation` em produção.
- Os valores nonce **nunca são registados**.
- O cabeçalho de resposta `Server` é mascarado para `webserver`.
