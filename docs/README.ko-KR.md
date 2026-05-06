# WebAppExperimental26

Azure AD 인증, mTLS(상호 TLS), Azure Key Vault 인증서 관리, Azure Cosmos DB, Azure Blob Storage, nonce 기반 Content Security Policy를 갖춘 강화된 HTTP 보안 레이어를 포함하는 ASP.NET Core 9 Razor Pages 웹 애플리케이션입니다.

---

## 목차

- [기능](#기능)
- [기능 플래그](#기능-플래그)
- [사전 요구 사항](#사전-요구-사항)
- [설치 – Windows Azure(App Service)](#설치--windows-azure-app-service)
- [설치 – Azure 서비스와 통신하는 OpenBSD 서버](#설치--azure-서비스와-통신하는-openbsd-서버)
- [구성 참조](#구성-참조)
- [지원 스크립트](#지원-스크립트)
- [보안 참고 사항](#보안-참고-사항)

---

## 기능

### Azure AD 인증(OpenID Connect)
애플리케이션은 OpenID Connect 프로토콜을 사용하여 **Microsoft ID 플랫폼**을 통해 사용자를 인증합니다(`Microsoft.Identity.Web` 사용). `/Experimental` 하위의 모든 경로는 인증된 Azure AD ID가 필요합니다. `/Privacy`, `/Error`, `/About` 페이지는 공개적으로 접근 가능합니다.

### mTLS 클라이언트 인증서 인증
활성화되면 클라이언트는 유효한 X.509 인증서를 제공해야 합니다. `MtlsSettings`의 설정은 체인 인증서, 자체 서명 인증서 또는 둘 다의 허용 여부, 인증서 해지 확인 및 허용된 인증서 발급자를 제어합니다.

### Azure Key Vault 통합
애플리케이션은 시작 시 Azure Key Vault에서 TLS **서버 인증서**를 가져옵니다. 로드된 `X509Certificate2`는 Kestrel의 HTTPS 기본값에 직접 주입되므로 디스크에 PFX 파일이 필요 없습니다.

### 요청별 Nonce를 사용한 Content Security Policy
활성화되면 모든 HTTP 응답에는 `script-src` 지시문에 요청별 **암호학적으로 안전한 무작위 nonce**가 포함된 `Content-Security-Policy` 헤더가 포함됩니다. CSP는 인라인 스크립트에 대한 SHA-256 해시 기반 허용 목록도 지원합니다.

### 표준 HTTP 보안 헤더
`UseStandardSecurityHeaders`는 모든 응답에 다음을 추가합니다: `X-Frame-Options`, `X-Content-Type-Options`, `Strict-Transport-Security`, `Referrer-Policy`, `Cross-Origin-Opener-Policy`, `Cross-Origin-Resource-Policy`, `Permissions-Policy` 및 `Server`, `X-Powered-By`, `X-AspNetMvc-Version` 헤더 제거.

### Azure Blob Storage
활성화되면 `BlobSettingsService`는 연결 문자열과 구성 가능한 최대 첨부 파일 수로 지원되는 Scoped 서비스를 제공합니다.

### Azure Cosmos DB
활성화되면 애플리케이션은 시작 시 `database.ReadAsync()`를 호출하여 Cosmos DB 연결을 확인합니다.

### 보안 세션 관리
세션은 **30분 유휴 시간 초과**를 사용하는 프로세스 내 분산 메모리 캐시를 사용합니다. 세션 쿠키는 `HttpOnly`, `Secure = Always`, `SameSite = Strict`로 구성됩니다.

### 현지화
애플리케이션은 **11개 언어**를 지원합니다: en-US, de-DE, es-ES, fr-FR, pt-PT, it-IT, zh-HK, ko-KR, hi-IN, ru-RU, ar-SA. 아랍어에는 자동 RTL 레이아웃 전환이 포함됩니다.

### PII 안전 로깅
`LoggingHelper`는 HMAC-SHA256을 사용하여 로그 출력의 개인 식별 정보를 해시합니다. `Logging:PiiHmacKey`를 통해 안정적인 32바이트 키를 제공할 수 있습니다.

---

## 기능 플래그

모든 주요 하위 시스템은 `appsettings.json`의 boolean 기능 플래그로 제어됩니다.

| 플래그 | 기본값 | 설명 |
|---|---|---|
| `EnableSession` | `true` | 서버 측 세션 및 세션 쿠키 |
| `EnableLocalization` | `true` | 다국어 지원(11개 언어) |
| `EnableAzureAd` | `true` | Azure AD / OpenID Connect 인증 |
| `EnableAuthorization` | `true` | 경로 수준 권한 부여 정책 |
| `EnableKeyVault` | `false` | Azure Key Vault에서 TLS 서버 인증서 로드 |
| `EnableNonceServices` | `false` | 요청별 CSP nonce 생성 |
| `EnableCSP` | `false` | `Content-Security-Policy` 헤더 추가 |
| `EnableSecurityHeaders` | `true` | 표준 HTTP 보안 헤더 추가 |
| `EnableBlobStorage` | `false` | Azure Blob Storage 서비스 |
| `EnableCosmosDb` | `false` | Azure Cosmos DB 서비스 |
| `EnableMtls` | `false` | 클라이언트 TLS 인증서 요구 |
| `EnableOcspValidation` | `false` | OCSP 인증서 해지 확인(스텁) |

---

## 사전 요구 사항

1. **Azure AD 앱 등록** – 리디렉션 URI, 클라이언트 암호 또는 인증서 자격 증명 포함.
2. **Azure Key Vault** – PFX 서버 인증서를 비밀로 포함.
3. **Azure Cosmos DB 계정**(선택 사항).
4. **Azure Blob Storage 계정**(선택 사항).
5. **.NET 9 SDK / 런타임** – 버전 9.0 이상.

---

## 구성 참조

`appsettings.template.json`을 `appsettings.json`으로 복사하고 모든 `{{PLACEHOLDER}}` 값을 교체합니다. 비밀은 **.NET User Secrets**(로컬) 또는 Azure App Settings / Key Vault References(프로덕션)에 저장하십시오 — 소스 코드에 절대 저장하지 마십시오.

---

## 보안 참고 사항

- **비밀을 소스 제어에 커밋하지 마십시오.**
- OCSP 유효성 검사 구현은 모든 인증서를 거부하는 **스텁**입니다. 프로덕션에서 `EnableOcspValidation`을 활성화하기 전에 `PerformOcspValidationAsync`를 교체하십시오.
- Nonce 값은 **절대 로깅되지 않습니다**.
- `Server` 응답 헤더는 `webserver`로 마스킹됩니다.
