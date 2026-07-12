# DayClaim AR Backend — Security

This is a US-client, healthcare-adjacent (HIPAA-relevant) RCM system. This
document maps what's implemented to the risks called out in the platform's
modernization design, and is explicit about what still needs work before a
real production go-live.

## 1. Authentication & session

- **JWT access token (HMAC-SHA256, 15 min default) + rotating opaque refresh
  token (7 days default)**, replacing the legacy per-app session/cookie
  scheme and its MD5+3DES password hashing (deck slide 10 — 3DES has been
  officially disallowed for new applications since 2023).
- Passwords are hashed with **bcrypt** (work factor 12) — `PasswordHasher`.
  Minimum password length is enforced at 12 characters (`CreateUserCommandValidator`).
  Legacy MD5+3DES hashes, if migrated as-is, simply fail `Verify()` rather
  than being silently accepted — a migration path must re-hash them, not
  carry them forward.
- **Account lockout** after 5 failed logins (`LoginCommandHandler`), closing
  an unauthenticated brute-force gap the legacy system didn't have.
- Refresh tokens are stored **hashed** (SHA-256) in `RefreshTokens`, rotate
  on every use, and the prior token is revoked — a stolen refresh token
  can be used at most once, and rotation makes reuse detectable if you add
  reuse-detection alerting later.
- Login returns the **same generic error** for "user doesn't exist" and
  "wrong password" (username enumeration defense).

## 2. Authorization (closing the deck's flagged gap)

Deck slide 10 flags: *"Hidden feature URLs can still be reached if a user
manually copies the URL → BROKEN ACCESS CONTROL (OWASP A01:2021)"* — because
the legacy app only hid menu items client-side.

This build enforces authorization **server-side, per endpoint**, via ASP.NET
Core `[Authorize(Policy = ...)]` — there is no client-visibility-only gate.
Policies (`Application.Common.Authorization.PolicyNames`):

| Policy | Roles | Used by |
|---|---|---|
| `InternalStaff` | Admin, Manager, Supervisor, User | (reserved for future internal-only reads) |
| `AdminOnly` | Admin | System-wide config, Rule CRUD |
| `UserManagement` | Admin, Manager | User CRUD, role assignment (handlers additionally forbid a Manager granting Admin/Manager or editing an Admin account) |
| `SupervisorOrAbove` | Admin, Manager, Supervisor | Importer config write, ingestion, rule execution, WFM |
| `AnyAuthenticatedUser` | any valid token | Dashboard reads, notes |

Role hierarchy, narrowest responsibility first: **User** (works assigned
claims) → **Supervisor** (manages a team, approvals) → **Manager** (oversees
Supervisors/teams org-wide) → **Admin** (full control, incl. user/role
management).

Role and client-org membership are carried as **JWT claims** (`role`, `org`),
validated on every request by the JWT bearer middleware — not looked up
per-request from a mutable session store, and not something the client can
influence after login.

**Known gap**: controllers currently authorize by *role*, not yet by
*client-org membership* — e.g. a Supervisor for Client A can currently call
an endpoint passing Client B's `clientOrganizationId`. Closing this means
adding an authorization filter that checks `ICurrentUserService.ClientOrganizationIds`
contains the requested `clientOrganizationId` (or the caller is internal staff
internal) — flagged here rather than silently shipped as complete.

## 3. Data protection (HIPAA technical controls)

Deck slide 30's design, implemented in `FieldEncryptionService`:

- **Column-level authenticated encryption (AES-256-GCM)** for PHI fields:
  patient name, DOB, account number, claim balance. Never whole-JSON
  encryption — each field is its own ciphertext, so a partial-record read
  doesn't require decrypting everything.
- **Blind index** (HMAC-SHA256 under a *separate* key from the data key) per
  encrypted field, enabling equality lookups (dedup, "does this patient
  already exist") **without decrypting** and without weakening the cipher —
  the blind index only supports exact-match, never partial/range queries, so
  it doesn't leak ordering or prefix information the way format-preserving
  or deterministic-order-preserving encryption would.
- **Key separation**: data-encryption key and blind-index key are distinct;
  compromising one doesn't help decrypt the other's outputs.
- **Transport**: TLS terminates at the edge (ALB/CloudFront in the target
  design); `RequireHttpsMetadata` is enforced outside Development in this API.
- **At rest**: Postgres/EF Core don't add disk-level encryption themselves —
  that's Aurora's TDE + KMS-managed CMKs in the target AWS design. Locally,
  rely on the host's disk encryption; this app never assumes the disk layer
  is untrusted on its own.

**Key management**: `Encryption:DataKey` / `Encryption:BlindIndexKey` and
`Jwt:SigningKey` are configuration values with **no default** — the app
throws at startup if they're missing or too short, rather than silently
running with a weak/absent key. In Development they come from
`appsettings.Development.json` (clearly-labeled dev-only values, fine to
commit since they're not real secrets and only work against a local
Postgres nobody else can reach). In any other environment they must come
from **AWS Secrets Manager / KMS** (or equivalent), never from a committed
file — `docker-compose.yml` requires them via `${VAR:?...}` so Compose
refuses to start without real values in `.env` (which is gitignored).

## 4. Input validation & injection

- **FluentValidation** on every command/query (`ValidationBehavior`) —
  centralized, can't be bypassed by a controller that forgets to check
  `ModelState`.
- **EF Core parameterized queries** throughout — no raw SQL string
  concatenation anywhere in the codebase. The Rule Engine's condition
  expressions are evaluated by a **hand-rolled, intentionally tiny**
  evaluator (`RuleConditionEvaluator`) — field/operator/value only, no
  `eval`, no reflection, no way for a rule author to run arbitrary code.
- **RBAC-gated writes**: only Supervisor+ can create importer configs,
  ingest files, execute rules, or run allocations — a compromised low-
  privilege account can't rewrite claim classification rules.

## 5. Availability / abuse controls

- **Rate limiting** at two layers: the Ocelot gateway (per-route limits in
  `ocelot.json`) and a per-IP fixed-window limiter in the API itself
  (`Program.cs`) — defense in depth if the gateway is ever bypassed.
- **Non-root containers**: both `Dockerfile.api` and `Dockerfile.gateway`
  run as an unprivileged `dayclaim` user, not root.
- **Health checks** on every service in `docker-compose.yml`, so a
  dependency that isn't actually ready (Postgres still initializing, Redis
  not accepting auth yet) blocks dependent containers from starting instead
  of crash-looping against a half-up dependency.

## 6. Logging & auditability

- Every command/query passes through `LoggingBehavior`, which logs the
  request type, the acting user ID, elapsed time, and outcome — but
  deliberately **never logs the request payload**, since payloads can carry
  PHI. Unhandled exceptions are logged server-side with full detail;
  clients only ever receive a generic "unexpected error" message
  (`ExceptionHandlingMiddleware`) — no stack traces or internal exception
  text cross the trust boundary.
- Structured console logging (Serilog) is the container log driver's
  source; in the target AWS design this ships to CloudWatch (deck slide 18)
  with CloudTrail separately covering AWS-account-level audit.

## 7. Security headers & transport

`SecurityHeadersMiddleware` sets `X-Content-Type-Options: nosniff`,
`X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`,
a restrictive `Permissions-Policy`, and `Cache-Control: no-store` on every
response — defense in depth for when the API is reachable directly (dev/test)
without WAF/ALB/CloudFront in front of it.

## 8. OWASP Top 10 (2021) — where each is addressed

| Risk | Mitigation |
|---|---|
| A01 Broken Access Control | Server-side `[Authorize(Policy=...)]` per endpoint (see §2); known client-org gap flagged above |
| A02 Cryptographic Failures | AES-256-GCM field encryption, bcrypt password hashing, TLS enforced outside dev (see §3) |
| A03 Injection | EF Core parameterization, no raw SQL, constrained rule-expression evaluator (see §4) |
| A04 Insecure Design | RBAC by policy, event-sourced write model, immutable audit trail (soft-delete only) |
| A05 Security Misconfiguration | Secrets fail-fast if missing/weak, non-root containers, restrictive CORS |
| A07 Auth Failures | bcrypt, lockout after 5 attempts, rotating refresh tokens, generic login errors |
| A08 Data Integrity Failures | JWT signature validation, refresh-token hash comparison, blind-index dedup |
| A09 Logging Failures | Structured logging on every request, PHI never logged, generic client-facing errors |

## 9. Explicitly not done yet (don't assume otherwise)

- Client-org-scoped authorization (see §2's known gap).
- OpenSearch/Redshift pieces are unimplemented (see ARCHITECTURE.md §6),
  so there's nothing to secure there yet.
- No WAF/CloudFront in local Compose — `SecurityHeadersMiddleware` and the
  gateway's rate limiter are the local stand-ins; a real deployment still
  needs the AWS WAF layer from the target design.
- No automated dependency/vulnerability scanning (SAST/DAST) wired into a
  CI/CD pipeline yet — the deck calls for this in CI/CD (slide 15); this
  repo doesn't have a pipeline defined at all yet.
- No secrets-manager integration — this build reads secrets from
  environment variables (12-factor style), which is compatible with
  injecting them from Secrets Manager/KMS at deploy time, but doesn't call
  those APIs directly.
