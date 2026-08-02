#!/usr/bin/env bash
set -e

# Fix terminfo bug on Kali Linux
export TERM=xterm
export DOTNET_CLI_COLOR=Never

cd "$(dirname "$0")"
PROJECT_DIR="Restaurante.Api"
OUTPUT_DIR="$PROJECT_DIR/bin/Debug/net8.0"

MODE="${1:-production}"

if [ "$MODE" = "production" ]; then
    echo "==> Backend: PRODUCTION (Supabase PostgreSQL)"
    export ASPNETCORE_ENVIRONMENT=Production
    export ASPNETCORE_URLS=http://localhost:5000
else
    echo "==> Backend: DEVELOPMENT (SQLite local)"
    export ASPNETCORE_ENVIRONMENT=Development
    export ASPNETCORE_URLS=http://localhost:5001
fi

dotnet build -v q
cd "$OUTPUT_DIR"
dotnet exec Restaurante.Api.dll
