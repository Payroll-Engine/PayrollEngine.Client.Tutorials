#!/usr/bin/env bash
# Build the extension folder with its manifest: bin/Release/extensions/PayrollConsoleExtension
# Usage: ./Extension.sh [PayrollConsole extensions folder]
# Console command: PAYROLL_CONSOLE, otherwise pecmd (register-pecmd.sh) or PayrollEngine.PayrollConsole

set -euo pipefail

NAME="PayrollConsoleExtension"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TARGET="$SCRIPT_DIR/bin/Release/extensions/$NAME"

CONSOLE="${PAYROLL_CONSOLE:-}"
if [[ -z "$CONSOLE" ]]; then
    if command -v pecmd &>/dev/null; then
        CONSOLE="pecmd"
    elif command -v PayrollEngine.PayrollConsole &>/dev/null; then
        CONSOLE="PayrollEngine.PayrollConsole"
    else
        echo "Error: PayrollConsole not found. Set PAYROLL_CONSOLE to the console executable." >&2
        exit 1
    fi
fi

dotnet build "$SCRIPT_DIR/$NAME.csproj" -c Release -o "$TARGET" --nologo

"$CONSOLE" ExtensionManifest "$TARGET" "Name:$NAME"

if [[ $# -gt 0 ]]; then
    mkdir -p "$1/$NAME"
    cp -R "$TARGET/." "$1/$NAME/"
    "$CONSOLE" Extensions
fi
