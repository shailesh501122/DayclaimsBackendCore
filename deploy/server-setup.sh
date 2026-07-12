#!/usr/bin/env bash
# One-time bootstrap for a fresh Ubuntu box (Oracle Cloud or otherwise).
# Run this ONCE, over SSH, logged in as a sudo-capable user (e.g. ubuntu):
#
#   ssh ubuntu@<server-ip>
#   curl -fsSL https://raw.githubusercontent.com/shailesh501122/DayclaimsBackendCore/main/deploy/server-setup.sh | bash
#
# After this finishes, GitHub Actions handles every subsequent deploy — you
# should not need to SSH in again except to inspect logs or rotate secrets.
set -euo pipefail

BACKEND_REPO="https://github.com/shailesh501122/DayclaimsBackendCore.git"
APP_DIR="/opt/dayclaim"
BACKEND_DIR="$APP_DIR/backend"

echo "==> Installing Docker Engine + Compose plugin"
if ! command -v docker >/dev/null 2>&1; then
  curl -fsSL https://get.docker.com | sudo sh
fi
sudo usermod -aG docker "$USER"

echo "==> Opening firewall (ufw) for SSH/HTTP/HTTPS"
sudo apt-get update -y
sudo apt-get install -y ufw
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw --force enable

echo "==> NOTE: Oracle Cloud also enforces its own Security List / Network"
echo "    Security Group at the cloud-network layer, separate from this"
echo "    box's ufw rules. Open ports 80 and 443 (and 22 if not already)"
echo "    there too, in the OCI console, or none of this will be reachable"
echo "    from the internet."

echo "==> Creating app directories"
sudo mkdir -p "$APP_DIR" "$APP_DIR/frontend/dist"
sudo chown -R "$USER":"$USER" "$APP_DIR"

echo "==> Cloning backend repo"
if [ -d "$BACKEND_DIR/.git" ]; then
  git -C "$BACKEND_DIR" pull
else
  git clone "$BACKEND_REPO" "$BACKEND_DIR"
fi

ENV_FILE="$BACKEND_DIR/deploy/.env"
if [ ! -f "$ENV_FILE" ]; then
  echo "==> Generating $ENV_FILE with fresh random secrets"
  # Piping this script through `curl | bash` consumes stdin with the script
  # itself, so a plain `read` here silently gets nothing — read from the
  # controlling terminal explicitly instead.
  read -rp "Public IP or domain the site will be served from (e.g. http://137.23.41.70): " PUBLIC_ORIGIN < /dev/tty
  if [ -z "$PUBLIC_ORIGIN" ]; then
    echo "==> No value entered — refusing to generate a .env with an empty PUBLIC_ORIGIN." >&2
    exit 1
  fi
  cat > "$ENV_FILE" <<EOF
POSTGRES_USER=dayclaim_ar
POSTGRES_PASSWORD=$(openssl rand -base64 24)
REDIS_PASSWORD=$(openssl rand -base64 24)
RABBITMQ_USER=dayclaim_ar
RABBITMQ_PASSWORD=$(openssl rand -base64 24)
JWT_SIGNING_KEY=$(openssl rand -base64 48)
ENCRYPTION_DATA_KEY=$(openssl rand -base64 32)
ENCRYPTION_BLIND_INDEX_KEY=$(openssl rand -base64 32)
PUBLIC_ORIGIN=${PUBLIC_ORIGIN}
SEED_DEMO_DATA=true
EOF
  chmod 600 "$ENV_FILE"
else
  echo "==> $ENV_FILE already exists, leaving it as-is"
fi

echo "==> Building and starting the stack (this can take a few minutes on first run)"
cd "$BACKEND_DIR/deploy"
sudo docker compose -f docker-compose.prod.yml --env-file .env up -d --build

echo "==> Done. Check status with:"
echo "    cd $BACKEND_DIR/deploy && sudo docker compose -f docker-compose.prod.yml ps"
echo "==> Note: your shell session needs to log out/in once for the docker"
echo "    group membership to take effect without sudo."
