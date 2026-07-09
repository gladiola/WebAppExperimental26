# Skill 7 — Localization & Documentation

## What This Skill Covers

Making a web application accessible to users in multiple languages and maintaining accurate, up-to-date documentation in all of those languages. This project supports **25 languages** across its UI and its full documentation set. This skill covers ASP.NET Core localization APIs, `.resx` resource files, right-to-left (RTL) layout, BCP-47 culture tags, and a repeatable workflow for detecting and filling documentation gaps across a multi-language folder structure.

---

## Key Concepts

### ASP.NET Core Localization Pipeline
- `AddLocalization` registers `IStringLocalizer<T>` and `IHtmlLocalizer<T>` in the DI container
- `RequestLocalizationOptions`: the three resolution strategies for determining the active culture:
  1. Query string (`?culture=fr-FR&ui-culture=fr-FR`)
  2. Cookie (`Set-Cookie: .AspNetCore.Culture=c%3Dfr-FR%7Cuic%3Dfr-FR`)
  3. `Accept-Language` HTTP header sent by the browser
- `UseRequestLocalization` middleware — must be placed **before** routing in the pipeline so the culture is resolved for all downstream middleware and views
- `SupportedCultures` vs. `SupportedUICultures`: the former controls date/number formatting; the latter controls resource string lookup
- Default culture fallback: if no resource file exists for the requested culture, the framework falls back to the default (typically `en-US`)

### BCP-47 Culture Tags
- Format: `{language}-{region}` — e.g. `en-US`, `fr-FR`, `ar-SA`, `zh-CN`
- Language subtag follows ISO 639-1 (two-letter) or ISO 639-2 (three-letter) codes
- Region subtag follows ISO 3166-1 alpha-2 codes
- Right-to-left scripts: Arabic (`ar-SA`), Hebrew — identified by the Unicode `TextInfo.IsRightToLeft` property
- The 25 cultures in this project: `en-US`, `de-DE`, `es-ES`, `fr-FR`, `pt-PT`, `it-IT`, `zh-HK`, `ko-KR`, `hi-IN`, `ru-RU`, `ar-SA`, `sw-KE`, `ja-JP`, `ht-HT`, `haw-US`, `sm-WS`, `mi-NZ`, `af-ZA`, `nl-NL`, `ha-NG`, `am-ET`, `yo-NG`, `bn-BD`, `zh-CN`, `ga-IE`

### .resx Resource Files
- XML-based key/value files that store localized strings
- Naming convention: `ClassName.resx` (default/invariant), `ClassName.fr-FR.resx` (French), etc.
- The `IStringLocalizer<T>` interface resolves `T` to its matching `.resx` file at runtime
- `@inject IStringLocalizer<MyPage> L` in a Razor view; `@L["Hello"]` renders the localized string
- Satellite assemblies: `.resx` files are compiled into satellite resource assemblies per culture at build time

### Right-to-Left (RTL) Layout
- Arabic and other RTL scripts require the `<html dir="rtl">` attribute
- The `lang` attribute must carry the full BCP-47 tag (e.g. `lang="ar-SA"`) for correct browser text rendering
- CSS logical properties (`margin-inline-start` instead of `margin-left`) allow a single stylesheet to handle both LTR and RTL without duplicating rules
- Detecting RTL programmatically: `CultureInfo.CurrentCulture.TextInfo.IsRightToLeft`

### Language Picker
- A UI control (typically in the navigation bar) that lets users switch language at runtime
- Implementation: a form or link that sets the culture cookie or redirects with `?culture=xx-XX`
- The ASP.NET Core `CookieRequestCultureProvider` persists the selection across requests

### Documentation Parity
- **Documentation parity** means every file in `docs/en-US/` has an accurate translation in every other language folder under `docs/`
- Tracking parity: compare the file inventory of each language folder against `docs/en-US/` to find missing files
- Automation: a Python script can `os.listdir` both directories and print the diff — the `tabulate` and `rich` libraries produce readable console output
- Translation workflow:
  1. Identify missing files with an inventory diff script
  2. Translate the content (human or machine-assisted)
  3. Review translated content for technical accuracy, especially code samples and configuration keys
  4. Open a PR for each language batch; verify the language index in `README.md` is updated

### Documentation File Types in This Project
- `README.md` — project overview, feature list, feature flags, installation steps, configuration reference
- `QUICK_REFERENCE.md` — condensed command and configuration reference for experienced users
- `MTLS_GUIDE.md` — mTLS certificate generation, Kestrel configuration, environment-specific notes
- `AZURE_KEYVAULT_PFX_GUIDE.md` — Azure Key Vault PFX import and retrieval
- `NONCE_OPTIMIZATION_GUIDE.md` — path-filtered nonce generation to reduce Key Vault calls
- `OCSP_GUIDE.md` — OCSP configuration options, caching, and fail-open/fail-closed behaviour
- `SecurityReview-{date}.md` — dated security audit reports listing findings with severity levels
- `SECURITY_FIX_CRITICAL_{n}_*.md` — writeups for each critical security fix

---

## Prerequisites

- Skill 1 (ASP.NET Core & Razor Pages)
- Basic knowledge of XML (for reading `.resx` files)
- Python 3 (for documentation parity scripts)

---

## How It Applies to This Project

| Concept | Location in Codebase |
|---------|----------------------|
| Localization service registration | `Extensions/ServiceCollectionExtensions.cs` → `AddLocalizationConfiguration` |
| Localization middleware | `Extensions/ApplicationBuilderExtensions.cs` → `UseLocalizationConfiguration` |
| Feature flag for localization | `Models/Settings/FeatureFlags.cs` — `EnableLocalization` |
| Resource files | `Resources/` directory (`.resx` files per culture) |
| RTL layout toggle | `Views/Shared/_Layout.cshtml` — `dir` and `lang` attributes derived from `CultureInfo` |
| Language picker UI | `Views/Shared/_Layout.cshtml` — navigation bar dropdown |
| English documentation baseline | `docs/en-US/` (12 markdown files) |
| Translated documentation | `docs/{locale}/` (one folder per language, target: 12 files each) |
| Documentation parity tracking | PR history (PRs #31–48) — each PR fills gaps in one or more language folders |
| Python locale-diff tooling | `pip install tabulate rich` — used in CI setup steps |

---

## Learning Path

1. Add localization to an ASP.NET Core project: call `AddLocalization` and `UseRequestLocalization` with at least two supported cultures.
2. Create a `.resx` file for English and a second `.resx` for French; inject `IStringLocalizer<T>` into a Razor Page and render a localized greeting.
3. Add a language picker that writes the `.AspNetCore.Culture` cookie; verify that switching language persists across page loads.
4. Add an Arabic culture; update the `<html>` tag to conditionally set `dir="rtl"` and `lang="ar-SA"` when Arabic is active.
5. Write a Python script that compares `docs/en-US/` to another language folder and prints the names of missing files.
6. Practice the documentation parity workflow: identify a missing file, write a translated version, and verify the language index links are correct.
7. Review PRs #31–48 in the repository to study the documentation parity pattern at scale.

---

## Suggested Resources

- [ASP.NET Core globalization and localization](https://learn.microsoft.com/aspnet/core/fundamentals/localization)
- [Resource files in .NET](https://learn.microsoft.com/dotnet/core/extensions/resources)
- [BCP-47 language tags — IANA registry](https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry)
- [Unicode CLDR](https://cldr.unicode.org/) — authoritative data on locale-specific formatting
- [CSS logical properties](https://developer.mozilla.org/docs/Web/CSS/CSS_logical_properties_and_values) — for RTL-compatible layouts
- `tabulate` Python library — [https://pypi.org/project/tabulate/](https://pypi.org/project/tabulate/)
- `rich` Python library — [https://github.com/Textualize/rich](https://github.com/Textualize/rich)
- [ISO 639-1 language codes](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes)
- [ISO 3166-1 country codes](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2)
