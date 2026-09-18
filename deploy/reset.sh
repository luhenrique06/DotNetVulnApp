#!/bin/bash
# Recria o container do lab do zero (banco, logs e /tmp limpos),
# arquivando antes as flags que os alunos gravaram em /tmp.
set -euo pipefail

APP_DIR="${APP_DIR:-/opt/vaultbank}"
ARCHIVE_DIR="${ARCHIVE_DIR:-/var/lib/vaultbank-flags}"
COMPOSE="docker compose -f $APP_DIR/deploy/docker-compose.ec2.yml"
STAMP="$(date +%Y%m%d-%H%M%S)"

mkdir -p "$ARCHIVE_DIR/$STAMP"
if docker ps --format '{{.Names}}' | grep -qx vaultbank; then
  docker exec vaultbank sh -c 'ls /tmp/*.txt 2>/dev/null || true' \
    | while read -r f; do
        [ -n "$f" ] && docker cp "vaultbank:$f" "$ARCHIVE_DIR/$STAMP/" || true
      done
  docker cp vaultbank:/app/logs "$ARCHIVE_DIR/$STAMP/logs" 2>/dev/null || true
fi

cd "$APP_DIR"
$COMPOSE down
$COMPOSE up -d --build
echo "Reset concluído. Flags arquivadas em $ARCHIVE_DIR/$STAMP"
