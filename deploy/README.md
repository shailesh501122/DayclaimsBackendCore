# Self-hosted deployment (Oracle Cloud / any Ubuntu box)

Runs the full stack — Postgres, Redis, RabbitMQ, the AR API, the Ocelot
gateway, and an nginx reverse proxy that also serves the frontend's static
build — as one Docker Compose stack on a single server, with GitHub Actions
redeploying automatically on every push to `main`.

The frontend is served **same-origin** with the API (via the nginx reverse
proxy below), which is why this setup doesn't need any of the CORS
configuration the split-origin Render deployment required.

## One-time server setup

SSH into the box and run:

```bash
curl -fsSL https://raw.githubusercontent.com/shailesh501122/DayclaimsBackendCore/main/deploy/server-setup.sh | bash
```

This installs Docker, opens the firewall (ufw) for 22/80/443, clones this
repo to `/opt/dayclaim/backend`, generates `/opt/dayclaim/backend/deploy/.env`
with fresh random secrets (you'll be prompted for the server's public
IP/domain), and brings the stack up for the first time.

**Also required, separately**: Oracle Cloud enforces its own Security
List / Network Security Group at the cloud-network layer, on top of this
box's own firewall. Open TCP 80 and 443 (and 22, if not already) there too
in the OCI console — otherwise nothing above is reachable from the
internet no matter how the server itself is configured.

## GitHub Actions secrets (both repos)

Add these under **Settings → Secrets and variables → Actions** in **both**
`DayclaimsBackendCore` and the frontend (`dayclaim`) repo:

| Secret | Value |
|---|---|
| `SSH_HOST` | the server's public IP, e.g. `137.23.41.70` |
| `SSH_USER` | `ubuntu` (default for Oracle's Canonical Ubuntu images — confirm yours) |
| `SSH_PRIVATE_KEY` | the *private* key matching the public key you added to the server — paste its full contents (`-----BEGIN OPENSSH PRIVATE KEY-----...`). Never commit this anywhere. |

Never paste the private key into a chat, issue, or commit — only into
GitHub's encrypted secrets field.

## What happens on every push

- **Backend** (`DayclaimsBackendCore`, this repo): `.github/workflows/deploy.yml`
  SSHes in, hard-resets `/opt/dayclaim/backend` to the latest `main`, and runs
  `docker compose up -d --build` — Postgres/Redis/RabbitMQ data volumes are
  untouched; only the app containers rebuild.
- **Frontend** (`dayclaim` repo): its own workflow builds the Vite app with
  `VITE_API_BASE_URL` set to the server's own origin (same-origin as the
  page, so no CORS) and rsyncs `dist/` into `/opt/dayclaim/frontend/dist`,
  which nginx serves directly — no container rebuild needed for a frontend
  change.

## Operating it

```bash
ssh ubuntu@<server-ip>
cd /opt/dayclaim/backend/deploy
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs -f ar-api
```

Rotate a secret: edit `/opt/dayclaim/backend/deploy/.env` on the server,
then `docker compose -f docker-compose.prod.yml up -d` to apply it (`.env`
is gitignored and never touched by deploys).

## Adding a real domain + HTTPS later

Once you point a domain's A record at the server, swap `deploy/nginx/nginx.conf`
for a certbot-fronted config (or run `certbot --nginx`) and update
`PUBLIC_ORIGIN` in `.env` and `VITE_API_BASE_URL` in the frontend's deploy
workflow to `https://your-domain`.
