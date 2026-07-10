# DayClaim AR Backend

.NET 8 backend for the **AR (Account Receivables)** module of the DayClaim
RCM platform — a microservices-based, event-driven, HIPAA-aligned redesign
of a legacy AR workflow (React SPA + .NET 8 microservices). See
`docs/ARCHITECTURE.md` and `docs/SECURITY.md` for the full design.

## Quick start (Docker Compose)

```bash
cd docker
cp .env.example .env
# edit .env: set real passwords/keys (see comments in the file for how)
docker compose up -d --build
```

- API + Swagger: http://localhost:8080/swagger
- Gateway (Ocelot): http://localhost:8000/gateway/...
- RabbitMQ management UI: http://localhost:15672

In `Development` (the Compose default), the API auto-applies EF Core
migrations and seeds demo data on startup — see
`src/DayClaim.AR.Infrastructure/Persistence/Seed/DevSeeder.cs`. Demo accounts,
all with password `admin`:

| Username | Role |
|---|---|
| `admin` | SuperAdmin |
| `vikram.rao` | Supervisor |
| `priya.s`, `rahul.m` | User |

## Try it

```bash
TOKEN=$(curl -s -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin"}' | jq -r .accessToken)

curl -s http://localhost:8080/api/v1/rule-engine/rules \
  -H "Authorization: Bearer $TOKEN" | jq
```

## Solution layout

```
src/
  DayClaim.AR.Domain          entities, enums, value objects
  DayClaim.AR.Application     CQRS commands/queries, validation, interfaces
  DayClaim.AR.Infrastructure  EF Core/Postgres, JWT, encryption, Redis, RabbitMQ
  DayClaim.AR.Api             ASP.NET Core Web API
  DayClaim.AR.Gateway         Ocelot API Gateway
tests/
  DayClaim.AR.UnitTests
docker/
  Dockerfile.api, Dockerfile.gateway, docker-compose.yml, .env.example
docs/
  ARCHITECTURE.md, SECURITY.md
```

## Local development without Docker

Requires the .NET 8 SDK plus a local Postgres/Redis/RabbitMQ (or point the
connection strings in `appsettings.Development.json` at your own):

```bash
dotnet build DayClaim.AR.sln
dotnet run --project src/DayClaim.AR.Api
```

## Adding a database migration

```bash
dotnet tool restore
dotnet tool run dotnet-ef migrations add <Name> \
  --project src/DayClaim.AR.Infrastructure \
  --startup-project src/DayClaim.AR.Api \
  --output-dir Persistence/Migrations
```
