#!/usr/bin/env bash

set -euo pipefail

readonly SERVER="steve@192.168.0.126"
readonly PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly CONFIG_FILE="${PROJECT_DIR}/deploy/appsettings.production.json"
readonly REMOTE_CONFIG="/tmp/serverwatch-appsettings.json"
readonly STATUS_URL="http://192.168.0.126:5189/status"

if [[ ! -f "${CONFIG_FILE}" ]]; then
    echo "Production configuration not found:"
    echo "  ${CONFIG_FILE}"
    exit 1
fi

echo "Uploading production configuration..."

scp "${CONFIG_FILE}" "${SERVER}:${REMOTE_CONFIG}"

echo "Installing production configuration..."

ssh "${SERVER}" \
    "sudo install \
        --owner=serverwatch \
        --group=serverwatch \
        --mode=600 \
        '${REMOTE_CONFIG}' \
        /opt/serverwatch/app/appsettings.json \
     && rm -f '${REMOTE_CONFIG}'"

echo "Restarting qBittorrent..."

ssh "${SERVER}" "docker restart qbittorrent >/dev/null"

echo "Restarting ServerWatch Agent..."

ssh "${SERVER}" "sudo systemctl restart serverwatch-agent.service"

echo "Checking production endpoint..."

curl \
    --fail \
    --silent \
    --show-error \
    --retry 10 \
    --retry-delay 1 \
    --retry-connrefused \
    --max-time 5 \
    "${STATUS_URL}" \
    >/dev/null

echo "Production configuration deployed successfully."
