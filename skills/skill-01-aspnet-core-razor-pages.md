# Skill 1 — ASP.NET Core & Razor Pages

## What This Skill Covers

The structural foundation of the application: creating an ASP.NET Core 9 project, wiring up the middleware pipeline, organising services with dependency injection, and building pages with the Razor Pages model. Every other skill depends on this one.

---

## Key Concepts

### Project Structure
- Solution file (`.sln`) and project file (`.csproj`)
- `Program.cs` as the single entry point (minimal hosting model)
- Separation of concerns via `Controllers/`, `Services/`, `Models/`, `Helpers/`, `Interfaces/`, `Extensions/`
- `appsettings.json` / `appsettings.Development.json` for environment-specific configuration
- User Secrets (`dotnet user-secrets`) for local development secrets

### Middleware Pipeline
- The `WebApplication.CreateBuilder` / `builder.Build()` / `app.Run()` lifecycle
- Middleware ordering — why sequence matters (routing, auth, security headers, session)
- Built-in middleware: `UseHttpsRedirection`, `UseStaticFiles`, `UseRouting`, `UseAuthentication`, `UseAuthorization`, `UseSession`
- Custom middleware classes implementing `IMiddleware` or the convention-based `Invoke(HttpContext)` pattern

### Dependency Injection
- `IServiceCollection` extension methods for grouping related registrations
- Service lifetimes: `AddSingleton`, `AddScoped`, `AddTransient`
- `IOptions<T>` / `IOptionsSnapshot<T>` for strongly-typed configuration sections
- `IHostedService` / `BackgroundService` for background tasks

### Razor Pages
- Page model classes (`PageModel`) and their `OnGet` / `OnPost` handlers
- `_Layout.cshtml` shared layout and `_ViewImports.cshtml`
- Tag helpers (`asp-for`, `asp-page`, `asp-action`)
- Partial views and view components

### Feature Flags
- Boolean flags in `appsettings.json` under a `FeatureFlags` section
- Binding flags to a strongly-typed `FeatureFlags` model with `GetSection().Get<T>()`
- Using flags at startup to conditionally register services and at runtime to conditionally activate middleware

### MVC Controllers
- `Controller` base class and `IActionResult` return types
- Route templates (`[Route]`, `[HttpGet]`, convention-based `{controller}/{action}/{id?}`)
- `[Authorize]` attribute to require authentication on a controller or action

---

## Prerequisites

- C# fundamentals (classes, interfaces, generics, async/await, LINQ)
- Basic understanding of HTTP (request/response, headers, status codes)
- Familiarity with JSON configuration files

---

## How It Applies to This Project

| Concept | Location in Codebase |
|---------|----------------------|
| Middleware pipeline wiring | `Program.cs` |
| Service registration extension methods | `Extensions/ServiceCollectionExtensions.cs` |
| Middleware pipeline extension methods | `Extensions/ApplicationBuilderExtensions.cs` |
| Feature flags model | `Models/Settings/FeatureFlags.cs` |
| Strongly-typed settings models | `Models/Settings/*.cs` |
| MVC controller with `[Authorize]` | `Controllers/HomeController.cs` |
| Background nonce refresh service | `Services/NonceRefresherService.cs` |
| Custom logging middleware | `Services/LoggingMiddleware.cs` |
| Template for new projects | `appsettings.template.json`, `SupportingScripts/SetupFromTemplate.ps1` |

---

## Learning Path

1. Create a new Razor Pages project with `dotnet new webapp`.
2. Understand `Program.cs`: builder phase vs. app phase.
3. Add a custom middleware class; observe its position in the pipeline.
4. Create a strongly-typed settings class and bind it with `IOptions<T>`.
5. Add a feature-flag section to `appsettings.json`; conditionally register a service based on it.
6. Create an `IServiceCollection` extension method that encapsulates a group of related service registrations.
7. Add a background service using `IHostedService`.
8. Add an MVC controller with a `[Route]` template and an `[Authorize]`-protected action.

---

## Suggested Resources

- [ASP.NET Core documentation](https://learn.microsoft.com/aspnet/core/) — official Microsoft docs
- [Razor Pages introduction](https://learn.microsoft.com/aspnet/core/razor-pages/)
- [Dependency injection in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [Options pattern](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options)
- [ASP.NET Core middleware](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/)
- `dotnet new` scaffolding: `dotnet new webapp`, `dotnet new mvc`, `dotnet new classlib`
