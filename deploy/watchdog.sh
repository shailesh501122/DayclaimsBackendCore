#!/usr/bin/env bash
# Self-heal watchdog: checks the stack's own health endpoint and, if it's
# not answering, restarts the compose stack. Handles a crashed/OOM-killed
# container or a hung ar-api process; it can NOT bring the box back if the
# whole VM itself is stopped/unreachable (that's a hosting-layer problem —
# see deploy/README.md's "keep-alive" section for that case).
set -uo pipefail

APP_DIR="/opt/dayclaim/backend/deploy"
COMPOSE_FILE="docker-compose.prod.yml"
LOG_FILE="/var/log/dayclaim-watchdog.log"
HEALTH_URL="http://127.0.0.1/health/live"

log() {
  echo "$(date -u '+%Y-%m-%dT%H:%M:%SZ') $*" >> "$LOG_FILE"
}

if curl -fsS --max-time 10 "$HEALTH_URL" >/dev/null 2>&1; then
  exit 0
fi

log "Health check failed at $HEALTH_URL — restarting stack"
cd "$APP_DIR" || { log "ERROR: $APP_DIR not found"; exit 1; }

if sudo docker compose -f "$COMPOSE_FILE" up -d >> "$LOG_FILE" 2>&1; then
  log "docker compose up -d completed"
else
  log "ERROR: docker compose up -d failed, see log above"
fi

sleep 15

if curl -fsS --max-time 10 "$HEALTH_URL" >/dev/null 2>&1; then
  log "Recovered — health check now passing"
else
  log "WARNING: still failing after restart attempt"
fi
