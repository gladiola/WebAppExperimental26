# WebAppExperimental26

Azure AD प्रमाणीकरण, म्यूचुअल TLS (mTLS), Azure Key Vault प्रमाण-पत्र प्रबंधन, Azure Cosmos DB, Azure Blob Storage, और nonce-आधारित Content Security Policy के साथ सुदृढ़ HTTP सुरक्षा परत वाला ASP.NET Core 9 Razor Pages वेब अनुप्रयोग।

---

## विषय-सूची

- [विशेषताएँ](#विशेषताएँ)
- [फ़ीचर फ़्लैग](#फ़ीचर-फ़्लैग)
- [पूर्वापेक्षाएँ](#पूर्वापेक्षाएँ)
- [इंस्टॉलेशन – Windows Azure (App Service)](#इंस्टॉलेशन--windows-azure-app-service)
- [इंस्टॉलेशन – Azure सेवाओं से संचार करने वाला OpenBSD सर्वर](#इंस्टॉलेशन--azure-सेवाओं-से-संचार-करने-वाला-openbsd-सर्वर)
- [कॉन्फ़िगरेशन संदर्भ](#कॉन्फ़िगरेशन-संदर्भ)
- [सहायक स्क्रिप्ट](#सहायक-स्क्रिप्ट)
- [सुरक्षा नोट्स](#सुरक्षा-नोट्स)

---

## विशेषताएँ

### Azure AD प्रमाणीकरण (OpenID Connect)
अनुप्रयोग OpenID Connect प्रोटोकॉल का उपयोग करके **Microsoft Identity Platform** के माध्यम से उपयोगकर्ताओं को प्रमाणित करता है (`Microsoft.Identity.Web` के ज़रिए)। `/Experimental` के अंतर्गत सभी रूट्स के लिए प्रमाणित Azure AD पहचान आवश्यक है। `/Privacy`, `/Error`, और `/About` पृष्ठ सार्वजनिक रूप से सुलभ हैं।

### mTLS क्लाइंट सर्टिफिकेट प्रमाणीकरण
सक्षम होने पर, क्लाइंट को एक वैध X.509 सर्टिफिकेट प्रस्तुत करना होगा। `MtlsSettings` में सेटिंग्स यह नियंत्रित करती हैं कि चेन किए गए, स्व-हस्ताक्षरित या दोनों प्रकार के सर्टिफिकेट, सर्टिफिकेट निरसन जाँच और अनुमत सर्टिफिकेट जारीकर्ता की अनुमति है या नहीं।

### Azure Key Vault एकीकरण
अनुप्रयोग स्टार्टअप पर Azure Key Vault से TLS **सर्वर सर्टिफिकेट** प्राप्त करता है। लोड किया गया `X509Certificate2` सीधे Kestrel के HTTPS डिफ़ॉल्ट में इंजेक्ट किया जाता है, इसलिए डिस्क पर कोई PFX फ़ाइल आवश्यक नहीं है।

### प्रति-अनुरोध Nonce के साथ Content Security Policy
सक्षम होने पर, प्रत्येक HTTP प्रतिक्रिया में एक `Content-Security-Policy` हेडर होता है जिसकी `script-src` निर्देशिका में प्रति-अनुरोध **क्रिप्टोग्राफिक रूप से यादृच्छिक nonce** होती है। CSP इनलाइन स्क्रिप्ट के लिए SHA-256 हैश-आधारित अनुमति सूचियों का भी समर्थन करती है।

### मानक HTTP सुरक्षा हेडर
`UseStandardSecurityHeaders` प्रत्येक प्रतिक्रिया में निम्न जोड़ता है: `X-Frame-Options`, `X-Content-Type-Options`, `Strict-Transport-Security`, `Referrer-Policy`, `Cross-Origin-Opener-Policy`, `Cross-Origin-Resource-Policy`, `Permissions-Policy`, और `Server`, `X-Powered-By`, `X-AspNetMvc-Version` हेडर हटाता है।

### Azure Blob Storage
सक्षम होने पर, `BlobSettingsService` एक कनेक्शन स्ट्रिंग और कॉन्फ़िगर करने योग्य अधिकतम अनुलग्नक संख्या द्वारा समर्थित Scoped सेवा प्रदान करता है।

### Azure Cosmos DB
सक्षम होने पर, अनुप्रयोग `database.ReadAsync()` कॉल करके स्टार्टअप पर Cosmos DB कनेक्शन सत्यापित करता है।

### सुरक्षित सत्र प्रबंधन
सत्र **30 मिनट के निष्क्रियता टाइमआउट** के साथ इन-प्रोसेस वितरित मेमोरी कैश का उपयोग करते हैं। सत्र कुकीज़ `HttpOnly`, `Secure = Always` और `SameSite = Strict` के रूप में कॉन्फ़िगर की जाती हैं।

### स्थानीयकरण
अनुप्रयोग **11 भाषाओं** का समर्थन करता है: en-US, de-DE, es-ES, fr-FR, pt-PT, it-IT, zh-HK, ko-KR, hi-IN, ru-RU और ar-SA। अरबी में स्वचालित RTL लेआउट स्विचिंग शामिल है।

### PII-सुरक्षित लॉगिंग
`LoggingHelper` HMAC-SHA256 का उपयोग करके लॉग आउटपुट में व्यक्तिगत पहचान योग्य जानकारी को हैश करता है। `Logging:PiiHmacKey` के माध्यम से एक स्थिर 32-बाइट कुंजी प्रदान की जा सकती है।

---

## फ़ीचर फ़्लैग

सभी प्रमुख सबसिस्टम `appsettings.json` में बूलियन फ़ीचर फ़्लैग द्वारा नियंत्रित होते हैं।

| फ़्लैग | डिफ़ॉल्ट | विवरण |
|---|---|---|
| `EnableSession` | `true` | सर्वर-साइड सत्र और सत्र कुकी |
| `EnableLocalization` | `true` | बहुभाषी समर्थन (11 भाषाएँ) |
| `EnableAzureAd` | `true` | Azure AD / OpenID Connect प्रमाणीकरण |
| `EnableAuthorization` | `true` | रूट-स्तरीय प्राधिकरण नीतियाँ |
| `EnableKeyVault` | `false` | Azure Key Vault से TLS सर्वर सर्टिफिकेट लोड करें |
| `EnableNonceServices` | `false` | प्रति-अनुरोध CSP nonce जनरेशन |
| `EnableCSP` | `false` | `Content-Security-Policy` हेडर संलग्न करें |
| `EnableSecurityHeaders` | `true` | मानक HTTP सुरक्षा हेडर संलग्न करें |
| `EnableBlobStorage` | `false` | Azure Blob Storage सेवा |
| `EnableCosmosDb` | `false` | Azure Cosmos DB सेवा |
| `EnableMtls` | `false` | क्लाइंट TLS सर्टिफिकेट की आवश्यकता |
| `EnableOcspValidation` | `false` | OCSP सर्टिफिकेट निरसन जाँच (स्टब) |

---

## पूर्वापेक्षाएँ

1. **Azure AD ऐप पंजीकरण** – पुनर्निर्देशन URI, क्लाइंट सीक्रेट या सर्टिफिकेट क्रेडेंशियल के साथ।
2. **Azure Key Vault** – PFX सर्वर सर्टिफिकेट को सीक्रेट के रूप में।
3. **Azure Cosmos DB खाता** (वैकल्पिक)।
4. **Azure Blob Storage खाता** (वैकल्पिक)।
5. **.NET 9 SDK / रनटाइम** – संस्करण 9.0 या उसके बाद का।

---

## कॉन्फ़िगरेशन संदर्भ

`appsettings.template.json` को `appsettings.json` में कॉपी करें और सभी `{{PLACEHOLDER}}` मान बदलें। सीक्रेट को **.NET User Secrets** (स्थानीय) या Azure App Settings / Key Vault References (प्रोडक्शन) में संग्रहीत करें — कभी भी सोर्स कोड में नहीं।

---

## सुरक्षा नोट्स

- **कभी भी सोर्स कंट्रोल में सीक्रेट कमिट न करें।**
- OCSP सत्यापन कार्यान्वयन एक **स्टब** है जो सभी सर्टिफिकेट अस्वीकार करता है। प्रोडक्शन में `EnableOcspValidation` सक्षम करने से पहले `PerformOcspValidationAsync` को बदलें।
- Nonce मान **कभी भी लॉग नहीं किए जाते**।
- `Server` प्रतिक्रिया हेडर को `webserver` पर मास्क किया जाता है।
