# DayClaim AR Backend — High-Level Architecture

Scope: this covers the **AR (Account Receivables)** module only, modernized
from a legacy per-client .NET MVC monolith into a microservices-based,
HIPAA-aligned platform, following the AR workflow diagram below. EVBV/Cost
Estimator and Coding are out of scope here; the design leaves room for them
(see "Centralized vs per-client" below) without requiring rework.

## 1. Source workflow this implements

```
Setup            Data Processing                          Dashboard / Report
─────            ───────────────                           ──────────────────
PMS Reports  ──▶  Data Download (BOT/ETL/MIS)
Importer Setup ─▶ Data Upload (batch + validation) ──▶ approve? ──No──▶ Roll back / Stop
                                                        │Yes
                                                        ▼
                                                  Data Warehouse ────────▶ Business/KPI/Project dashboards
Rule Engine Setup ▶ Data Processing (rule exec,                          Rule Engine summary/report/
                    best-case, analytics) ──▶ approve? ──No──▶ Roll back  dashboard, Inventory summary/
                                              │Yes                        report/dashboard
                                              ▼
WFM Setup ──────▶ Data Distribution (auto/manual/          ──────────────▶ Assignment summary/report/
                  special-project allocation, rollback)                   dashboard
                                              │
User/Supervisor Screen ▶ User's Input (analytics, notes,   ──────────────▶ Production summary/report,
                          status, action) ▶ Production DB                 Performance dashboard, Project
                                                                           overview
```

This backend's modules map 1:1 onto that diagram:

| Diagram stage | Backend module | Key entities |
|---|---|---|
| Importer Setup | `Application.Features.ImporterConfig` | `ImporterConfig`, `ImporterFieldMapping` |
| Data Upload / Download | `Application.Features.ImporterStorage` | `DataImportSourceReference`, `PatientMaster`, `EnterprisePatientIndex`, `RcmReportEntry` |
| Rule Engine Setup/Processing | `Application.Features.RuleEngine` | `Rule`, `RuleExecutionRun`, `RuleExecutionResult` |
| WFM Setup / Data Distribution | `Application.Features.Wfm` | `Team`, `WfmAllocationRule`, `Allocation` |
| User's Input (notes/status/action) | `Application.Features.Notes` | `ScenarioMaster`, `ClaimNote` |
| Dashboard / Report | `Application.Features.Dashboard` | read-only queries over the above |
| Centralized Users + RBAC | `Application.Features.Auth`, `Users` | `User`, `Role`, `Permission`, `RefreshToken` |

## 2. Layers (Clean Architecture)

```
DayClaim.AR.Domain           entities, enums, value objects — no framework dependencies
   ▲
DayClaim.AR.Application       CQRS (MediatR) commands/queries, validators (FluentValidation),
   ▲                        interfaces the Infrastructure layer implements
DayClaim.AR.Infrastructure    EF Core + Npgsql, JWT, field encryption, Redis, MassTransit/RabbitMQ
   ▲
DayClaim.AR.Api               ASP.NET Core Web API — controllers, authZ policies, middleware
DayClaim.AR.Gateway           Ocelot API Gateway — JWT validation + rate limiting + routing
```

Dependencies point inward only: Domain has no references; Application
depends on Domain; Infrastructure depends on Application+Domain; Api and
Gateway depend on the layers below them. This is what makes "swap Postgres
for something else" or "swap RabbitMQ for Kafka" a change confined to
Infrastructure.

## 3. Request flow (local Docker Compose)

```
Client ──▶ Gateway (Ocelot, :8000)  ── JWT validate + rate limit ──▶ Api (:8080)
                                                                        │
                                                          ┌─────────────┼──────────────┐
                                                          ▼             ▼              ▼
                                                     PostgreSQL       Redis         RabbitMQ
                                                     (EF Core)      (cache)      (MassTransit)
```

In the target AWS design (deck slides 13-18) this becomes:
`CloudFront + WAF → ALB → Ocelot on EKS → AR microservices on EKS`, with
Aurora PostgreSQL (OLTP), Redshift (OLAP via CDC+Glue), OpenSearch (full-text
search), ElastiCache (Redis), AmazonMQ (RabbitMQ-compatible), and Valkey/MSK
streams per RCM report. The local Compose stack is a topology-preserving
substitute: same protocols (Postgres wire protocol, Redis protocol, AMQP),
same layering, just single-node and un-managed.

## 4. Module registry pattern (mirrors the frontend)

Like the DayClaim.ai frontend's generated-module registry, every AR
sub-domain here is a self-contained feature folder under
`Application/Features/<Module>/` with its own commands, queries, and DTOs.
Adding a new AR capability means adding a new feature folder plus (if it
touches persistence) an EF Core configuration and a controller — no other
file needs to change.

## 5. Data model notes

- **Event-sourced write model**: `RcmReportEntry` is insert-and-select only
  (deck slide 26). A `RcmReportDataEntrySyncRequest` row is queued whenever
  an entry becomes `AutoApproved`/`ReOpened`; a background sync job
  (not yet built — see "Not yet built" below) is what would project these
  into report-type-specific read-model tables in the full design.
- **Four-way field segregation**: `RcmReportEntry` stores
  `StandardFieldsJson` / `CustomerSpecificFieldsJson` / `CustomDefinedFieldsJson`
  / `StatusStoringFieldsJson` / `UnmappedFieldsJson` as Postgres `jsonb`,
  driven by each client's `ImporterFieldMapping` rows — this is what lets one
  table serve every client's customized report layout without a schema
  migration per client (deck slide 24).
- **UUIDv7 primary keys**, generated in the application (`IdGenerator`), not
  the database — sorts roughly by creation time for index locality, doesn't
  leak a sequential count (deck slide 4/20).
- **Centralized vs per-client** (deck slides 12, 16, 21): this build keeps
  everything in one database for local simplicity, but table/module
  boundaries already match the target split — Users/RBAC and
  Importer/Rule/WFM *configuration* are the "centralized" tables; Importer
  *storage*, Rule *execution results*, and *allocations* are the "per-client"
  tables. Splitting to database-per-service later is a deployment change
  (separate connection strings per schema), not a data-model rewrite.

## 6. Not yet built (explicitly out of scope for this pass)

- The async sync-job that drains `RcmReportDataEntrySyncRequest` into
  read-model tables (currently: dashboard queries read the write-model
  directly, which is fine at this scale but not the target design).
- OpenSearch-backed full-text search (Rule Engine/Knowledgebase — deck slide 32).
- Redshift/OLAP sync via CDC+Glue (deck slide 31).
- EVBV, Coding, and Knowledgebase modules (different modules in the same
  suite; AR only per this task's scope).
- A migration bundle / job for applying EF Core migrations outside of
  `Development` — `Program.cs` only auto-migrates in Development for Compose
  convenience; a real environment should run migrations as an explicit
  release step, not on every container start.

## 7. Local development

```bash
cd docker
cp .env.example .env    # fill in real values — see comments in the file
docker compose up -d --build
# API:      http://localhost:8080/swagger
# Gateway:  http://localhost:8000/gateway/...
```

Seeded demo accounts (opt-in via `SeedDemoData=true` — see
`Persistence/Seed/DevSeeder.cs`): `Admin` / `Admin@123` (Admin), plus
`sanjay.k` (Manager), `vikram.rao` (Team Leader), `priya.s` / `rahul.m` (User) —
password `admin` for those four.
