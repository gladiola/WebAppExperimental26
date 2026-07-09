# Skill 3 — HTTP Security Hardening

## What This Skill Covers

Applying defence-in-depth at the HTTP layer: generating per-request cryptographic nonces for Content Security Policy, attaching standard security response headers, enforcing HTTPS, and writing PII-safe log output. The security reviews documented in this project (May 2026) provide a real-world case study of the bugs that arise when these techniques are implemented incorrectly.

---

## Key Concepts

### Content Security Policy (CSP)
- What CSP is: an HTTP response header that tells the browser which sources of content are allowed
- Key directives: `script-src`, `style-src`, `img-src`, `default-src`, `frame-ancestors`
- The `nonce` attribute: a random token embedded in both the CSP header and each `<script>` or `<style>` tag; the browser executes only elements whose nonce matches
- SHA-256 hash allow-listing: alternative to nonces for static inline scripts
- Report-only mode (`Content-Security-Policy-Report-Only`) for testing without breaking pages

### Per-Request Nonce Generation (Correctly)
- A CSP nonce must be **cryptographically random** and **unique per response**
- Correct approach: `RandomNumberGenerator.GetBytes(16)` → Base64-encode → embed in CSP header and in `HttpContext.Items`
- Common mistakes (all found and fixed in this project):
  - Reusing a fixed IV with AES-GCM — breaks the cipher's authentication guarantee and allows XOR attacks
  - Logging the nonce value — gives log readers a valid nonce to bypass CSP
  - Using a hardcoded fallback nonce — provides a predictable, reusable value
- Thread-safe nonce catalogue: using `ConcurrentDictionary` to track valid nonces per request without races

### Standard Security Response Headers
| Header | Purpose |
|--------|---------|
| `Strict-Transport-Security` | Forces HTTPS for a configurable `max-age`; `includeSubDomains` extends to all subdomains |
| `X-Frame-Options: DENY` | Prevents the page from being embedded in an `<iframe>` (clickjacking defence) |
| `X-Content-Type-Options: nosniff` | Stops browsers from MIME-sniffing responses away from the declared `Content-Type` |
| `Referrer-Policy: strict-origin-when-cross-origin` | Limits what the browser sends in the `Referer` header for cross-origin requests |
| `Cross-Origin-Opener-Policy: same-origin` | Isolates the browsing context from cross-origin windows |
| `Cross-Origin-Resource-Policy: same-site` | Blocks cross-origin no-cors fetches of the resource |
| `Permissions-Policy` | Disables browser features (geolocation, camera, microphone, FLoC) the app does not use |
- Removing fingerprinting headers: `Server`, `X-Powered-By`, `X-AspNetMvc-Version`
- `Cache-Control: no-cache, no-store, must-revalidate` for authenticated endpoints

### HTTPS Enforcement
- `UseHttpsRedirection` — redirects HTTP requests to HTTPS
- `UseHsts` — sends `Strict-Transport-Security` in production
- Kestrel TLS configuration: loading a certificate from a file, from User Secrets, or from Azure Key Vault

### Middleware Ordering for Security
- Security headers middleware must run **before** routing and authentication so that even 401/403 short-circuit responses carry the headers
- The consequences of placing `UseAuthentication` or `UseAuthorization` before security headers middleware

### PII-Safe Logging
- Personally Identifiable Information (PII) in logs is a compliance risk
- HMAC-SHA256 hashing: hash PII with a stable secret key before writing to logs; the hash is consistent enough for correlation but cannot be reversed
- Providing a stable key via configuration (`Logging:PiiHmacKey`); falling back to a per-process random key if no key is configured
- Logging helper patterns: a central `LoggingHelper` class that applies hashing before any write

### AES Encryption Fundamentals (for the nonce history)
- AES-GCM vs. AES-CBC: what each mode provides and when IV reuse is catastrophic
- Why CSP nonces should not use encryption at all — `RandomNumberGenerator` is simpler and correct

---

## Prerequisites

- Skill 1 (ASP.NET Core & Razor Pages)
- Basic understanding of how browsers enforce same-origin policy
- Awareness of OWASP Top 10 attack categories (XSS, clickjacking, CSRF)

---

## How It Applies to This Project

| Concept | Location in Codebase |
|---------|----------------------|
| Nonce generation (correct) | `Services/NonceRefresherService.cs` |
| Nonce middleware | `Services/NonceMiddleware.cs`, `Services/OptimizedNonceMiddleware.cs` |
| Nonce catalogue | `Services/NonceCatalogService.cs` |
| CSP builder | `Services/ContentSecurityPolicyBuilder.cs` |
| SHA-256 hash settings | `Models/Settings/CSPScriptHashSettings.cs` |
| Standard security headers | `Extensions/ApplicationBuilderExtensions.cs` → `UseStandardSecurityHeaders` |
| CSP + nonce middleware attachment | `Extensions/ApplicationBuilderExtensions.cs` → `UseNonceAndSecurityHeadersAsync` |
| PII logging helper | `Services/LoggingHelper.cs`, `Helpers/LoggingHelper.cs` |
| Nonce encryption settings (legacy) | `Models/Settings/NonceEncryptionSettings.cs` |
| Security reviews (bug case studies) | `docs/en-US/SecurityReview-2026-05-05.md`, `SecurityReview-2026-05-06.md`, `SecurityReview-2026-05-07.md` |
| Critical fix writeups | `docs/en-US/SECURITY_FIX_CRITICAL_1_AES_GCM_IV_REUSE.md` and siblings |
| Nonce optimization guide | `docs/en-US/NONCE_OPTIMIZATION_GUIDE.md` |

---

## Learning Path

1. Read [MDN Content Security Policy](https://developer.mozilla.org/docs/Web/HTTP/CSP) and understand the nonce flow.
2. Write an ASP.NET Core middleware that adds `X-Frame-Options`, `X-Content-Type-Options`, and `Strict-Transport-Security` to every response.
3. Read the three critical security fix documents in `docs/en-US/` to understand what goes wrong with naive nonce implementations.
4. Implement a nonce middleware that generates `RandomNumberGenerator.GetBytes(16)` per request, stores it in `HttpContext.Items`, and injects it into the CSP header.
5. Update a Razor layout (`_Layout.cshtml`) to read the nonce from `HttpContext.Items` and add it to each `<script>` tag.
6. Implement `LoggingHelper` with HMAC-SHA256: accept a string, hash it with a configurable key, and return the hex digest.
7. Test CSP enforcement: try adding an inline script without a nonce and confirm the browser blocks it.
8. Run [securityheaders.com](https://securityheaders.com) against a local tunnel to audit your header output.

---

## Suggested Resources

- [MDN CSP documentation](https://developer.mozilla.org/docs/Web/HTTP/CSP)
- [OWASP Secure Headers Project](https://owasp.org/www-project-secure-headers/)
- [OWASP XSS Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html)
- `docs/en-US/SecurityReview-2026-05-05.md` — 19-finding security audit of this codebase
- `docs/en-US/SECURITY_FIX_CRITICAL_1_AES_GCM_IV_REUSE.md` — why AES-GCM IV reuse is catastrophic
- `docs/en-US/NONCE_OPTIMIZATION_GUIDE.md` — path-filtered nonce generation to reduce Key Vault calls
- [RFC 6797 — HTTP Strict Transport Security](https://datatracker.ietf.org/doc/html/rfc6797)
- [NIST AES-GCM guidance (SP 800-38D)](https://csrc.nist.gov/publications/detail/sp/800-38d/final)
