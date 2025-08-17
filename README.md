# ShopMVP

E-commerce MVP (electronics) on **ASP.NET Core (.NET 8)**.
The goal is to demonstrate a full, professional workflow: layered architecture, clean code style, diagnostics, and a path to database + integrations.

## Authorship & collaboration
I write **all application code** myself.
I use GPT as a **mentor/product owner** to:
- clarify requirements and architecture trade-offs,
- point me to official docs and learning resources,
- help structure epics/stories/tasks in Notion,
- review plans, PR descriptions, and quality checklists.

GPT does **not** generate any C# code for me; it helps me reason, plan, and improve.

## Stack
- OS: Windows 11
- IDE: Visual Studio 2022 Community / CLI (dotnet)
- .NET 8 LTS, ASP.NET Core
- Swagger (Swashbuckle) - API documentation
- Health Checks - liveness/readiness endpoints
- EditorConfig + dotnet format - consistent style
- (Planned) EF Core + SQL database, external API integration

## Architecture (layered monolith)
- **API (Presentation):** controllers/endpoints, DI composition, middleware (Swagger, health).
- **Application:** use-cases, DTOs, ports (interfaces).
- **Domain:** entities, value objects, invariants (no infra deps).
- **Infrastructure:** EF Core, email/HTTP clients (implement Application ports).

## Quick start
### Visual Studio
1. Open the solution
2. Set startup project: `ShopMVP.Api`.
3. Run (F5). Default environment is **Development**.

### CLI
```bash
dotnet restore
dotnet build -c Debug
dotnet run --project ./src/ShopMVP.Api
```

## API documentation (Swagger)
### Development (default):
- UI: `https://localhost:{port}/swagger`
- JSON: `https://localhost:{port}/swagger/v1/swagger.json`
- Title: ShopMVP API, Version: v1 (descriptions from XML docs of `ShopMVP.Api`).

### Production emulation (Swagger disabled):
PowerShell
```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run --project .\src\ShopMVP.Api --no-launch-profile
```

CMD
```cmd
set ASPNETCORE_ENVIRONMENT=Production && dotnet run --project src\ShopMVP.Api --no-launch-profile
```

Git Bash
```bash
ASPNETCORE_ENVIRONMENT=Production dotnet run --project ./src/ShopMVP.Api --no-launch-profile
```
Expect to see `Hosting environment: Production` in logs and `/swagger` unavailable.


## Health checks

Endpoints:
- Liveness - `GET /health/live` (quick self-check; tag `live`)
- Readiness - `GET /health/ready` (critical dependencies; tag `ready`)

Status codes:
- `Healthy` / `Degraded` → 200 OK
- `Unhealthy` → 503 Service Unavailable

Response shape:
```json
{
  "status": "Healthy",
  "checks": [
    { "name": "self", "status": "Healthy", "duration": "00:00:00.001" }
  ],
  "totalDuration": "00:00:00.001"
}
```


## Error handling (ProblemDetails)

- All errors are returned in RFC 7807 application/problem+json.
- Every response includes a traceId for correlation.
- In Development the detail field contains exception information; in Production internal details are omitted.
- HTTP 400 is produced automatically as ValidationProblemDetails when controllers use [ApiController].
- HTTP 404 and 405 are returned as ProblemDetails via Status Code Pages.
- Error responses are not cached (Cache-Control: no-store, no-cache, max-age=0, must-revalidate, Pragma: no-cache, Expires: 0).


## Code Style
- `.editorconfig` lives at repo root (next to `.sln`).
- Run before committing:
```bash
dotnet format
dotnet build
```
Policy: avoid introducing new warnings (fix or justify in the PR).

## Solution layout
```text
src/
  ShopMVP.Api/            # Web/API (DI, Swagger, Health, endpoints)
  ShopMVP.Application/    # use-cases, ports, DTOs
  ShopMVP.Domain/         # entities, invariants, domain rules
  ShopMVP.Infrastructure/ # EF Core, external services (ports' impl)
tests/
  ShopMVP.Tests.Unit/
  ShopMVP.Tests.Integration/
```

## Development approach
- Branching: 1 user story (US) - 1 feature branch; tasks = separate commits.
- Commits: `US-XX TSK-YY: concise message`.
- PRs: "Squash and merge"; delete the branch after merge.
- Backlog: Notion for epics/stories/tasks; GitHub Milestones reflect epics (e.g., E-01 “Setup & Skeleton” — link in repo milestones).

## Epics & roadmap (MVP)

- [x] US-01: Repo & solution skeleton
- [x] US-02: API baseline — Swagger (Dev/Prod), Health Checks, global error handling (ProblemDetails)
- [ ] US-03: Database + EF Core (migrations, repositories), configuration & secrets
- [ ] US-04: Product catalog (DTOs, paging/sorting), caching
- [ ] US-05: AuthN/AuthZ (customer/admin), roles & policies
- [ ] US-06: External API integration (free/public), resiliency

Note: This is a learning-oriented pet MVP intended to showcase architectural practices (clean layering, testability, diagnostics) and is ready for incremental growth.
