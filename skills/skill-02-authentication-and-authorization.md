# Skill 2 — Authentication & Authorization

## What This Skill Covers

Proving who a user is (authentication) and deciding what they are allowed to do (authorization). This project supports three identity providers — Microsoft Entra ID (Azure AD), AWS Cognito, and GCP Identity Platform — plus mutual TLS (mTLS) client-certificate authentication. Understanding this skill requires knowing OpenID Connect, JWT validation, cookie authentication, and ASP.NET Core's authorization middleware.

---

## Key Concepts

### OpenID Connect (OIDC) and OAuth 2.0
- The difference between authentication (OIDC) and authorization (OAuth 2.0 scopes)
- The authorization code flow with PKCE: redirect to identity provider → receive authorization code → exchange for tokens
- ID token (who the user is) vs. access token (what the user can call)
- Discovery documents (`.well-known/openid-configuration`) and how middleware consumes them
- Token validation: signature, issuer, audience, expiry

### Microsoft Entra ID (Azure AD) Authentication
- Registering an application in the Azure portal (App registration)
- `ClientId`, `TenantId`, `ClientSecret` — what each value means and where it comes from
- `Microsoft.Identity.Web` NuGet package and `AddMicrosoftIdentityWebApp` extension
- Configuring `OpenIdConnectOptions`: `SignedOutCallbackPath`, `CallbackPath`, required scopes
- `Microsoft.Identity.Web.UI` for the built-in sign-in/sign-out UI

### AWS Cognito Authentication
- Cognito User Pool concepts: User Pool ID, App Client, Hosted UI, domain
- Cognito's OIDC discovery endpoint: `https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}/.well-known/openid-configuration`
- Configuring ASP.NET Core OIDC middleware to point at a Cognito endpoint
- `AppClientId`, `AppClientSecret`, callback path (`/signin-aws-cognito`)

### GCP Identity Platform Authentication
- Google OAuth 2.0 / OIDC: creating credentials in Google Cloud Console → APIs & Services → Credentials
- Google's discovery endpoint: `https://accounts.google.com/.well-known/openid-configuration`
- `ClientId`, `ClientSecret`, callback path (`/signin-gcp`)
- Difference between Google Identity and a GCP Service Account

### JWT ******
- Structure of a JWT: header, payload (claims), signature
- Validating tokens with `AddJwtBearer`: `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `IssuerSigningKey`
- `JwtSecurityTokenHandler` and symmetric vs. asymmetric signing keys

### Cookie Authentication
- `AddCookie` and `CookieAuthenticationOptions`
- `HttpOnly`, `Secure`, `SameSite` cookie flags and why each matters for security
- Session timeout vs. cookie expiry

### ASP.NET Core Authorization
- `[Authorize]` attribute on controllers, actions, and Razor Pages
- Authorization policies: `AddPolicy`, `RequireAuthenticatedUser`, `RequireClaim`, `RequireRole`
- `AddRazorPagesConfiguration` with a global `AuthorizeFilter`
- Allowing anonymous access with `[AllowAnonymous]` on specific pages

### Mutual TLS (mTLS) Client Certificate Authentication
- What mTLS is: both sides of the TLS handshake present X.509 certificates
- Kestrel's `ClientCertificateMode`: `RequireCertificate` vs. `AllowCertificate`
- Reading and validating the client certificate in middleware: chain validation, self-signed allowance, issuer DN matching
- `MtlsSettings`: `AllowChainedCertificates`, `AllowSelfSigned`, `CheckRevocation`, `AllowedIssuers`

---

## Prerequisites

- Skill 1 (ASP.NET Core & Razor Pages)
- Understanding of TLS/HTTPS at a conceptual level
- Familiarity with JSON Web Tokens at a conceptual level

---

## How It Applies to This Project

| Concept | Location in Codebase |
|---------|----------------------|
| Azure AD OIDC setup | `Extensions/ServiceCollectionExtensions.cs` → `AddAzureAdAuthentication` |
| Cognito OIDC setup | `Extensions/ServiceCollectionExtensions.cs` → `AddAwsCognitoAuthentication` |
| GCP OIDC setup | `Extensions/ServiceCollectionExtensions.cs` → `AddGcpIdentityAuthentication` |
| mTLS Kestrel configuration | `Extensions/ServiceCollectionExtensions.cs` → `AddMtlsAuthentication` |
| mTLS settings model | `Models/Settings/MtlsSettings.cs` |
| Azure AD settings model | `Models/Settings/AzureADSettings.cs` |
| AWS Cognito settings model | `Models/Settings/AwsCognitoSettings.cs` |
| GCP Identity settings model | `Models/Settings/GcpIdentitySettings.cs` |
| Authorization on controller | `Controllers/HomeController.cs` — `[Authorize]` |
| Session cookie hardening | `Extensions/ServiceCollectionExtensions.cs` → `AddSessionConfiguration` |
| Claims loading | `Services/UserClaimsLoader.cs` |

---

## Learning Path

1. Register a test application in the Azure portal and note its `ClientId` and `TenantId`.
2. Add `Microsoft.Identity.Web` to an ASP.NET Core project and configure OIDC sign-in.
3. Protect a Razor Page with `[Authorize]`; verify a redirect to the identity provider occurs.
4. Decode a received ID token using [jwt.io](https://jwt.io) and identify the standard claims.
5. Add a custom authorization policy that requires a specific claim value.
6. Configure cookie options (`HttpOnly`, `Secure`, `SameSite`) and observe the `Set-Cookie` header.
7. Repeat steps 1–3 for AWS Cognito using a free-tier User Pool.
8. Repeat steps 1–3 for GCP Identity Platform using a Google Cloud project.
9. Generate a self-signed client certificate with `mkcert` or `openssl` and configure Kestrel to require it.
10. Write middleware that reads and validates the `HttpContext.Connection.ClientCertificate`.

---

## Suggested Resources

- [Microsoft Identity Web documentation](https://learn.microsoft.com/azure/active-directory/develop/microsoft-identity-web)
- [OpenID Connect specification](https://openid.net/specs/openid-connect-core-1_0.html)
- [JWT introduction](https://jwt.io/introduction)
- [ASP.NET Core authentication overview](https://learn.microsoft.com/aspnet/core/security/authentication/)
- [ASP.NET Core authorization](https://learn.microsoft.com/aspnet/core/security/authorization/introduction)
- [Amazon Cognito OIDC documentation](https://docs.aws.amazon.com/cognito/latest/developerguide/cognito-userpools-server-contract-reference.html)
- [Google Identity — OAuth 2.0](https://developers.google.com/identity/protocols/oauth2)
- [Kestrel HTTPS and client certificates](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/endpoints)
- `mkcert` — local CA tool: [https://github.com/FiloSottile/mkcert](https://github.com/FiloSottile/mkcert)
