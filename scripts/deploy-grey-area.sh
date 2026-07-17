#!/usr/bin/env bash

set -euo pipefail

source "$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/deploy.local"

readonly SERVER="steve@192.168.0.126"
readonly PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly PUBLISH_DIR="${PROJECT_DIR}/publish"
readonly ARCHIVE="/tmp/serverwatch-agent.tar.gz"
readonly REMOTE_ARCHIVE="/tmp/serverwatch-agent.tar.gz"
readonly REMOTE_APP_DIR="/opt/serverwatch/app"
readonly STATUS_URL="http://192.168.0.126:5189/status"

echo "Publishing ServerWatch Agent..."

rm -rf "${PUBLISH_DIR}"

dotnet publish "${PROJECT_DIR}/ServerWatchAgent.csproj" \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained true \
    --output "${PUBLISH_DIR}"

echo "Creating deployment archive..."

# Production configuration lives on grey-area and must not be overwritten.
tar \
    --create \
    --gzip \
    --file "${ARCHIVE}" \
    --directory "${PUBLISH_DIR}" \
    --exclude="appsettings.json" \
    --exclude="appsettings.Development.json" \
    .

echo "Uploading deployment archive..."

scp "${ARCHIVE}" "${SERVER}:${REMOTE_ARCHIVE}"

echo "Installing deployment and restarting service..."

ssh "${SERVER}" sudo /usr/local/bin/serverwatch-deploy

echo "Checking production endpoint..."

curl \
    --fail \
    --silent \
    --show-error \
    --retry 10 \
    --retry-delay 1 \
    --retry-connrefused \
    --max-time 5 \
    --header "X-ServerWatch-Api-Key: ${SERVERWATCH_RELEASE_API_KEY}" \
    "${STATUS_URL}"

echo
echo "ServerWatch Agent deployment completed successfully."
