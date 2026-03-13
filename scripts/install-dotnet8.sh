#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
INSTALL_SCRIPT="$REPO_ROOT/dotnet-install.sh"

wget https://dot.net/v1/dotnet-install.sh -O "$INSTALL_SCRIPT"
chmod +x "$INSTALL_SCRIPT"
"$INSTALL_SCRIPT" --channel 8.0

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

dotnet --version
