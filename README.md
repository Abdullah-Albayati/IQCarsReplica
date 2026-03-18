# BaseStructure

`BaseStructure` is a reusable ASP.NET Core Web API template based on the `Template` source project.

It includes:

- ASP.NET Core 9 Web API
- Entity Framework Core + PostgreSQL
- AutoMapper
- FluentValidation
- Swagger/OpenAPI
- JWT authentication (access + refresh token flow)
- Docker + docker-compose
- Scaffolding helper scripts

## Template metadata

- Template name: `BaseStructure API Template`
- Template short name: `basestructure`
- Source token: `Template`

## Install from GitHub

```powershell
dotnet new uninstall BaseStructure.Template
dotnet new install https://github.com/Abdullah-Albayati/BaseStructure.git
dotnet new list basestructure
```

## Create a new API project

```powershell
dotnet new basestructure -n MyApi
Set-Location .\MyApi
dotnet restore
dotnet build
```

## Local development of the template

From the repository root:

```powershell
dotnet new uninstall BaseStructure.Template
dotnet new install .
dotnet new basestructure -n TemplateSmokeTest -o .\_template-smoke-test
```

## Run the generated API

```powershell
Set-Location .\MyApi\MyApi
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Swagger UI is available at the app root.

## Auth configuration

Set these keys in the generated project's `appsettings*.json` or environment variables:

- `AppSettings:Token`
- `AppSettings:Issuer`
- `AppSettings:Audience`

Default auth endpoints:

- `POST /api/auth/register`
- `POST /api/auth/login?username={username}&password={password}`
- `POST /api/auth/refresh-token`
- `GET /api/auth/me` (requires Bearer token)

## Helper script (Windows)

Use `new-api.ps1` to install the template (local path by default), generate a project, and optionally build it:

```powershell
.\new-api.ps1 -ProjectName MyApi
```

Install directly from GitHub in the script:

```powershell
.\new-api.ps1 -ProjectName MyApi -TemplateSource "https://github.com/Abdullah-Albayati/BaseStructure.git"
```

## Notes

- The source project is intentionally named `Template` so `dotnet new` renaming works cleanly.
- Template output excludes `bin`, `obj`, `.git`, `.github`, IDE folders, and `.template.config`.
