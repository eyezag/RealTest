# RealTest

RealTest is a two-sided marketplace where developers and companies recruit real people to
test websites, mobile apps, and prototypes — and get honest, structured feedback back:
completed tasks, bug reports, and evidence, in exchange for rewards.

This repository contains the **production-ready foundation** for RealTest: a modular-monolith
backend, a Next.js frontend, and the authentication/authorization system both build on. It
does **not** yet include the marketplace features themselves (campaigns, testing assignments,
payments, etc.) — see [Current project scope](#current-project-scope) below.

## Table of contents

- [Architecture overview](#architecture-overview)
- [Technology stack](#technology-stack)
- [Repository structure](#repository-structure)
- [Prerequisites](#prerequisites)
- [Environment configuration](#environment-configuration)
- [Quick start](#quick-start)
- [Database setup & migrations](#database-setup--migrations)
- [Running the backend](#running-the-backend)
- [Running the frontend](#running-the-frontend)
- [Docker Compose](#docker-compose)
- [Running tests](#running-tests)
- [API reference](#api-reference)
- [Troubleshooting](#troubleshooting)
- [Security notes](#security-notes)
- [Current project scope](#current-project-scope)
- [Planned next phase](#planned-next-phase)

## Architecture overview

The backend is a **modular monolith** organized as five .NET projects, with dependencies
pointing strictly inward:

```
RealTest.Api            → RealTest.Application, RealTest.Infrastructure, RealTest.Contracts
RealTest.Infrastructure → RealTest.Application, RealTest.Domain
RealTest.Application    → RealTest.Domain, RealTest.Contracts
RealTest.Domain         → (nothing)
RealTest.Contracts      → (nothing)
```

- **RealTest.Domain** — entities, enums, and domain rules. No framework dependencies beyond
  the lightweight ASP.NET Core Identity abstractions needed for `ApplicationUser`.
- **RealTest.Application** — use cases (`AuthService`), interfaces implemented by
  Infrastructure, FluentValidation validators, custom exceptions, a small `Result<T>` type,
  and authorization requirements. This is where business rules live — controllers stay thin.
- **RealTest.Infrastructure** — EF Core + PostgreSQL, ASP.NET Core Identity configuration,
  JWT/refresh-token generation, repositories, and the email/date-time service implementations.
- **RealTest.Contracts** — public request/response DTOs. Never exposes EF Core entities.
- **RealTest.Api** — controllers, middleware, DI wiring, OpenAPI, health checks, startup.

This is enforced by automated architecture tests (`tests/RealTest.ArchitectureTests`), which
inspect actual assembly references rather than just source-level conventions.

The frontend is a standard Next.js App Router project; see
[`src/frontend`](src/frontend) below for its internal layout.

## Technology stack

**Backend**: .NET 10, ASP.NET Core Web API, C#, Entity Framework Core, PostgreSQL,
ASP.NET Core Identity, JWT access tokens + rotating refresh tokens, FluentValidation, Serilog,
OpenAPI/Swagger, xUnit, Testcontainers.

**Frontend**: Next.js 16 (App Router, Turbopack), React 19, TypeScript (strict mode),
Tailwind CSS v4, React Hook Form, Zod, TanStack Query.

**Infrastructure**: Docker, Docker Compose, PostgreSQL, environment variables, health checks.

## Repository structure

```text
realtest/
  src/
    backend/
      RealTest.sln
      RealTest.Api/              # Controllers, middleware, DI, startup
      RealTest.Application/      # Use cases, validators, interfaces, exceptions
      RealTest.Domain/           # Entities, enums, domain rules
      RealTest.Infrastructure/   # EF Core, Identity, JWT, repositories
      RealTest.Contracts/        # Public request/response DTOs
    frontend/                    # Next.js 16 App Router frontend
  tests/
    RealTest.UnitTests/
    RealTest.IntegrationTests/   # Testcontainers-backed PostgreSQL integration tests
    RealTest.ArchitectureTests/  # Layering rules enforced via assembly-reference checks
  .github/workflows/ci.yml
  docker-compose.yml
  .env.example
  README.md
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20.9+](https://nodejs.org/) (Next.js 16 requirement) and npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL locally,
  and required by the integration tests, which use Testcontainers)
- The EF Core CLI tool: `dotnet tool install --global dotnet-ef`

## Environment configuration

Copy the example environment file and fill in values for local development:

```bash
cp .env.example .env
```

```powershell
Copy-Item .env.example .env
```

Key variables (see [`.env.example`](.env.example) for the full list with descriptions):

| Variable | Purpose |
| --- | --- |
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_PORT` | Docker Compose PostgreSQL container |
| `ConnectionStrings__Postgres` | Backend's database connection string |
| `Jwt__Issuer`, `Jwt__Audience`, `Jwt__SigningKey` | JWT issuance/validation — signing key must be 32+ characters |
| `Jwt__AccessTokenLifetimeMinutes`, `Jwt__RefreshTokenLifetimeDays` | Token lifetimes |
| `Frontend__Url` | Allowed CORS origin for the backend |
| `NEXT_PUBLIC_API_URL` | Backend URL the frontend calls |
| `EMAIL_PROVIDER`, `EMAIL_FROM_ADDRESS`, `EMAIL_API_KEY` | Placeholders — see [Security notes](#security-notes) |

**Required configuration is validated at startup.** If `ConnectionStrings:Postgres` or any
`Jwt:*` value is missing or invalid (e.g. a signing key under 32 characters), the API fails
fast with a clear error instead of starting in a broken state.

The frontend reads its own `.env.local` (Next.js does not read the repo-root `.env`):

```bash
cp src/frontend/.env.local.example src/frontend/.env.local
```

```powershell
Copy-Item src/frontend/.env.local.example src/frontend/.env.local
```

Never commit a real `.env` or `.env.local` — both are already gitignored.

## Quick start

The fastest path to a fully running stack locally:

```bash
cp .env.example .env
docker compose up -d postgres
dotnet tool install --global dotnet-ef   # first time only
cd src/backend
dotnet ef database update --project RealTest.Infrastructure --startup-project RealTest.Api
dotnet run --project RealTest.Api
```

In a second terminal:

```bash
cd src/frontend
cp .env.local.example .env.local
npm install
npm run dev
```

Then open http://localhost:3000. The API listens on http://localhost:5205 by default
(http://localhost:5205/swagger in Development).

## Database setup & migrations

PostgreSQL runs via Docker Compose (see [Docker Compose](#docker-compose)). By default it's
exposed on **port 5436** (not 5432), chosen to avoid clashing with a locally installed
PostgreSQL instance — change `POSTGRES_PORT` in `.env` if you'd prefer a different port, and
update `ConnectionStrings__Postgres` / `appsettings.Development.json` to match.

The API also **applies pending migrations and seeds the fixed Tester/CompanyUser/Administrator
roles automatically at startup** (see `DatabaseInitializer` in `RealTest.Infrastructure`), so
in most cases you won't need to run migrations by hand after the first setup. To manage
migrations explicitly:

```bash
cd src/backend

# Apply all pending migrations
dotnet ef database update --project RealTest.Infrastructure --startup-project RealTest.Api

# Add a new migration after changing entities/configurations
dotnet ef migrations add <MigrationName> --project RealTest.Infrastructure --startup-project RealTest.Api --output-dir Persistence/Migrations

# Remove the most recent (not-yet-applied) migration
dotnet ef migrations remove --project RealTest.Infrastructure --startup-project RealTest.Api
```

```powershell
Set-Location src/backend

dotnet ef database update --project RealTest.Infrastructure --startup-project RealTest.Api

dotnet ef migrations add <MigrationName> --project RealTest.Infrastructure --startup-project RealTest.Api --output-dir Persistence/Migrations
```

## Running the backend

The backend can run natively (fast rebuilds, debugger attach) or in Docker.

**Natively:**

```bash
cd src/backend
dotnet restore RealTest.sln
dotnet build RealTest.sln
dotnet run --project RealTest.Api
```

```powershell
Set-Location src/backend
dotnet restore RealTest.sln
dotnet build RealTest.sln
dotnet run --project RealTest.Api
```

**In Docker** (as part of the optional full Compose stack — see below):

```bash
docker compose --profile full up --build backend
```

## Running the frontend

```bash
cd src/frontend
npm install
npm run dev
```

```powershell
Set-Location src/frontend
npm install
npm run dev
```

Other frontend scripts: `npm run lint`, `npx tsc --noEmit` (type check), `npm run build`,
`npm run start` (serve a production build).

## Docker Compose

By default, `docker compose up` starts **only PostgreSQL** — the backend and frontend are
meant to run natively during day-to-day development for fast rebuilds and debugger support.

```bash
# Start PostgreSQL (persistent volume, health-checked, port 5436 by default)
docker compose up -d postgres

# Stop it
docker compose down

# Stop it AND delete the data volume (fresh database next time)
docker compose down -v
```

An optional **full stack** profile builds and runs the backend and frontend in containers too
(useful for testing the Dockerfiles or a prod-like setup):

```bash
docker compose --profile full up --build
```

This starts PostgreSQL on 5436, the backend on 5205, and the frontend on 3000, wired together
via Compose service names and environment variables.

## Running tests

```bash
cd src/backend  # or run from the repo root and point at each csproj, as below

# Unit tests (validators, domain rules, token hashing — no external dependencies)
dotnet test ../../tests/RealTest.UnitTests/RealTest.UnitTests.csproj

# Architecture tests (layering rules)
dotnet test ../../tests/RealTest.ArchitectureTests/RealTest.ArchitectureTests.csproj

# Integration tests (spins up a real PostgreSQL container via Testcontainers — Docker must be running)
dotnet test ../../tests/RealTest.IntegrationTests/RealTest.IntegrationTests.csproj

# Everything
dotnet test RealTest.sln
```

```powershell
Set-Location src/backend
dotnet test RealTest.sln
```

Frontend checks:

```bash
cd src/frontend
npm run lint
npx tsc --noEmit
npm run build
```

## API reference

With the backend running in Development, open **http://localhost:5205/swagger** for
interactive OpenAPI documentation, including a bearer-token auth button for trying protected
endpoints.

Health checks: `GET /health`, `GET /health/ready` (verifies PostgreSQL connectivity),
`GET /health/live` (process liveness only, no dependency checks).

Authentication endpoints (all under `/api/v1/auth`): `register`, `login`, `refresh`, `logout`,
`me`, `verify-email`, `resend-verification`, `forgot-password`, `reset-password`.

Example protected endpoints proving the `TesterOnly` / `CompanyUserOnly` / `AdministratorOnly`
authorization policies work: `GET /api/v1/samples/tester-only`, `/company-only`, `/admin-only`.
These exist only to demonstrate the policies — see
[Current project scope](#current-project-scope).

## Troubleshooting

**Port 5432/5436 already in use** — another PostgreSQL instance (local install or a different
project's container) is already bound to that port. Change `POSTGRES_PORT` in `.env` and the
matching port in `appsettings.Development.json` / `ConnectionStrings__Postgres`.

**`dotnet ef` fails with "startup project doesn't reference Microsoft.EntityFrameworkCore.Design"**
— that package must be referenced by the *startup* project (`RealTest.Api`), not just
`RealTest.Infrastructure`. It already is in this repo; if you see this error after modifying
the API project, check you haven't removed the reference.

**Integration tests hang or fail immediately** — Testcontainers needs a running Docker daemon.
Start Docker Desktop and re-run. On first run, pulling the `postgres:16-alpine` image can take
a minute.

**Frontend can't reach the API / CORS errors** — confirm `NEXT_PUBLIC_API_URL` in
`src/frontend/.env.local` matches where the API is actually listening, and that
`Frontend__Url` in the backend's config matches where the frontend is actually running (CORS
is origin-specific and credentialed).

**"Connection string 'Postgres' is not configured" on startup** — set
`ConnectionStrings__Postgres` (env var) or `ConnectionStrings:Postgres` (appsettings). The API
deliberately fails fast here rather than starting with a broken database connection.

**Login works but `/auth/me` returns 401 right after** — the access token lives in memory only
(see [Security notes](#security-notes)) and is lost on a full page reload; this is expected.
The frontend automatically retries once via `/auth/refresh` using the refresh-token cookie.

## Security notes

- **Passwords** are hashed and validated by ASP.NET Core Identity (`UserManager`), never
  handled manually. Password policy: 8+ characters, upper/lowercase, digit, special character.
- **Access tokens** are short-lived JWTs (15 minutes by default), signed with a symmetric key
  from configuration (32+ characters required — validated at startup). They are kept **in
  memory only** on the frontend (a module-level variable), never in `localStorage` or
  `sessionStorage`, to reduce exposure to XSS.
- **Refresh tokens** are opaque random values. Only their SHA-256 hash is ever persisted — a
  database leak alone cannot be used to mint new access tokens. They are delivered to the
  browser exclusively via a `Secure` (in non-Development environments), `HttpOnly`,
  `SameSite=Strict` cookie scoped to `/api/v1/auth`, so JavaScript can never read them. They
  are **never** included in a JSON response body.
- **Refresh-token rotation**: every `/auth/refresh` call revokes the presented token and issues
  a new one. **Reuse of an already-rotated token is treated as a compromise signal** — the
  server revokes *every* active refresh token for that user and logs a security warning.
- **Account lockout**: ASP.NET Core Identity locks an account out for 15 minutes after 5 failed
  login attempts.
- **User enumeration protection**: `forgot-password` and `resend-verification` always return
  the same generic success message whether or not the email exists.
- **Error responses** never leak stack traces or internal exception details in production —
  unexpected errors return a generic message plus a trace ID; full details are logged
  server-side only. Validation errors follow a consistent ProblemDetails shape.
- **Administrator accounts** cannot be created through public registration — only `Tester` and
  `CompanyUser` are accepted by `/api/v1/auth/register`, enforced by both client- and
  server-side validation and covered by an integration test.
- **Sensitive data is never logged**: passwords, access tokens, and refresh tokens are excluded
  from all Serilog output; only security-relevant *events* (auth failures, lockouts, refresh
  reuse) are logged, without the credentials/tokens themselves.
- **Email delivery is a logging placeholder** (`LoggingEmailService`) — verification and
  password-reset "emails" are written to the server log instead of actually being sent. Swap
  in a real provider behind the existing `IEmailService` interface when ready; no
  Application-layer changes required.

## Current project scope

This foundation intentionally implements **only** project setup and authentication. The
following are explicitly **out of scope** for this repository and have not been built:
campaign creation, tester profiles, company workspaces, testing assignments, bug reports, file
uploads, financial wallets, payments/payouts, ratings, disputes, and AI feedback summaries.

What *is* included:

- Modular-monolith backend with Domain/Application/Infrastructure/Api/Contracts separation
- Full authentication lifecycle: register, login, JWT + rotating refresh tokens, logout,
  current-user retrieval, email-verification and password-reset foundations, account lockout
- Role-based authorization (`Tester` / `CompanyUser` / `Administrator`) with example protected
  endpoints proving the policies work
- PostgreSQL via EF Core, with migrations and seeded roles
- Global error handling → ProblemDetails, structured Serilog logging, health checks, OpenAPI
- Next.js frontend: landing page, full auth UI, role-based protected dashboard placeholders,
  unauthorized/not-found pages
- Unit, integration (real PostgreSQL via Testcontainers), and architecture tests
- Docker Compose for local PostgreSQL (+ optional full containerized stack), CI workflow

## Planned next phase

The natural next step is the first real marketplace feature slice — most likely **tester
profiles and company workspaces** (the data model everything else hangs off of), followed by
**campaign creation and testing assignments**. Do not start this work automatically; it should
be scoped and confirmed as a separate task.
