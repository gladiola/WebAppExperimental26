# WebAppExperimental26

تطبيق ويب ASP.NET Core 9 Razor Pages يتضمن مصادقة Azure AD، وبروتوكول TLS المتبادل (mTLS)، وإدارة الشهادات عبر Azure Key Vault، وAzure Cosmos DB، وAzure Blob Storage، وطبقة أمان HTTP مُعززة مع سياسة أمان المحتوى المستندة إلى nonce.

---

## جدول المحتويات

- [الميزات](#الميزات)
- [أعلام الميزات](#أعلام-الميزات)
- [المتطلبات الأساسية](#المتطلبات-الأساسية)
- [التثبيت – Windows Azure (App Service)](#التثبيت--windows-azure-app-service)
- [التثبيت – خادم OpenBSD مع خدمات Azure](#التثبيت--خادم-openbsd-مع-خدمات-azure)
- [مرجع التكوين](#مرجع-التكوين)
- [النصوص البرمجية المساعدة](#النصوص-البرمجية-المساعدة)
- [ملاحظات الأمان](#ملاحظات-الأمان)

---

## الميزات

### مصادقة Azure AD (OpenID Connect)
يُصادق التطبيق المستخدمين عبر **منصة هوية Microsoft** باستخدام بروتوكول OpenID Connect (عبر `Microsoft.Identity.Web`). تتطلب جميع المسارات ضمن `/Experimental` هوية Azure AD مُصادقاً عليها. صفحات `/Privacy` و`/Error` و`/About` متاحة للعموم.

### مصادقة mTLS بشهادة العميل
عند التفعيل، يجب على العملاء تقديم شهادة X.509 صالحة. تتحكم الإعدادات في `MtlsSettings` في أنواع الشهادات المسموح بها (المتسلسلة أو الموقّعة ذاتياً أو كلتيهما)، والتحقق من إلغاء الشهادات، والجهات المُصدِرة المسموح بها.

### تكامل Azure Key Vault
يستعيد التطبيق **شهادة الخادم** TLS من Azure Key Vault عند بدء التشغيل. يُحقن `X509Certificate2` المُحمَّل مباشرةً في إعدادات HTTPS الافتراضية لـ Kestrel، دون الحاجة إلى ملف PFX على القرص.

### سياسة أمان المحتوى مع nonce لكل طلب
عند التفعيل، تحمل كل استجابة HTTP رأساً `Content-Security-Policy` يتضمن توجيه `script-src` قيمة **nonce عشوائية مشفرة** لكل طلب. كما تدعم سياسة أمان المحتوى قوائم السماح المستندة إلى تجزئة SHA-256 للنصوص البرمجية المضمّنة.

### رؤوس أمان HTTP القياسية
يُضيف `UseStandardSecurityHeaders` إلى كل استجابة: `X-Frame-Options`، و`X-Content-Type-Options`، و`Strict-Transport-Security`، و`Referrer-Policy`، و`Cross-Origin-Opener-Policy`، و`Cross-Origin-Resource-Policy`، و`Permissions-Policy`، ويزيل رؤوس `Server` و`X-Powered-By` و`X-AspNetMvc-Version`.

### Azure Blob Storage
عند التفعيل، يوفر `BlobSettingsService` خدمة Scoped مدعومة بسلسلة اتصال وحد أقصى قابل للتهيئة لعدد المرفقات.

### Azure Cosmos DB
عند التفعيل، يتحقق التطبيق من اتصال Cosmos DB عند بدء التشغيل باستدعاء `database.ReadAsync()`.

### إدارة الجلسات الآمنة
تستخدم الجلسات ذاكرة تخزين مؤقت موزعة داخل العملية مع **مهلة خمول مدتها 30 دقيقة**. تُضبط ملفات تعريف ارتباط الجلسة على `HttpOnly` و`Secure = Always` و`SameSite = Strict`.

### التوطين
يدعم التطبيق **11 لغة**: en-US، وde-DE، وes-ES، وfr-FR، وpt-PT، وit-IT، وzh-HK، وko-KR، وhi-IN، وru-RU، وar-SA. يتضمن العربي تبديلاً تلقائياً لتخطيط RTL.

### تسجيل آمن لمعلومات PII
يُجزّئ `LoggingHelper` معلومات التعريف الشخصية في مخرجات التسجيل باستخدام HMAC-SHA256. يمكن توفير مفتاح ثابت من 32 بايت عبر `Logging:PiiHmacKey`.

---

## أعلام الميزات

تتحكم أعلام ميزات منطقية في `appsettings.json` في جميع الأنظمة الفرعية الرئيسية.

| العلم | القيمة الافتراضية | الوصف |
|---|---|---|
| `EnableSession` | `true` | جلسة جانب الخادم وملف تعريف ارتباط الجلسة |
| `EnableLocalization` | `true` | دعم متعدد اللغات (11 لغة) |
| `EnableAzureAd` | `true` | مصادقة Azure AD / OpenID Connect |
| `EnableAuthorization` | `true` | سياسات التخويل على مستوى المسار |
| `EnableKeyVault` | `false` | تحميل شهادة خادم TLS من Azure Key Vault |
| `EnableNonceServices` | `false` | توليد nonce لـ CSP لكل طلب |
| `EnableCSP` | `false` | إرفاق رأس `Content-Security-Policy` |
| `EnableSecurityHeaders` | `true` | إرفاق رؤوس أمان HTTP القياسية |
| `EnableBlobStorage` | `false` | خدمة Azure Blob Storage |
| `EnableCosmosDb` | `false` | خدمة Azure Cosmos DB |
| `EnableMtls` | `false` | اشتراط شهادات TLS للعميل |
| `EnableOcspValidation` | `false` | فحص إلغاء شهادة OCSP (نموذج أولي) |

---

## المتطلبات الأساسية

1. **تسجيل تطبيق Azure AD** – بعنوان URI لإعادة التوجيه وسر العميل أو بيانات اعتماد الشهادة.
2. **Azure Key Vault** – يحتوي على شهادة PFX للخادم كسر.
3. **حساب Azure Cosmos DB** (اختياري).
4. **حساب Azure Blob Storage** (اختياري).
5. **.NET 9 SDK / Runtime** – الإصدار 9.0 أو أحدث.

---

## مرجع التكوين

انسخ `appsettings.template.json` إلى `appsettings.json` واستبدل جميع قيم `{{PLACEHOLDER}}`. احتفظ بالأسرار في **.NET User Secrets** (محلياً) أو Azure App Settings / Key Vault References (الإنتاج) — ولا تضعها أبداً في الكود المصدري.

---

## ملاحظات الأمان

- **لا تلتزم أبداً بالأسرار في نظام التحكم في الإصدار.**
- تُعدّ تنفيذ التحقق من OCSP **نموذجاً أولياً** يرفض جميع الشهادات. استبدل `PerformOcspValidationAsync` قبل تفعيل `EnableOcspValidation` في الإنتاج.
- قيم nonce **لا تُسجَّل أبداً**.
- رأس الاستجابة `Server` مُقنَّع بالقيمة `webserver`.
