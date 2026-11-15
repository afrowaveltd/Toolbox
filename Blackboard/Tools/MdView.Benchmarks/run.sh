#!/usr/bin/env bash
# MdView HDD benchmark launcher. Never deletes the dataset or changes OS cache.
set -Eeuo pipefail
ROOT_INPUT="${1:-/mnt/TEST}"
MODE="${2:-all}"
if (( $# >= 2 )); then shift 2; else shift "$#"; fi

command -v realpath >/dev/null || { echo "Missing realpath" >&2; exit 2; }
command -v findmnt >/dev/null || { echo "Missing findmnt (util-linux)" >&2; exit 2; }
command -v dotnet >/dev/null || { echo "Missing .NET 10 SDK" >&2; exit 2; }
[[ -d "$ROOT_INPUT" ]] || { echo "Mount directory not found: $ROOT_INPUT" >&2; exit 2; }

ROOT="$(realpath -e -- "$ROOT_INPUT")"
[[ "$ROOT" != "/" ]] || { echo "Refusing the filesystem root" >&2; exit 2; }
MOUNT_TARGET="$(findmnt -n -T "$ROOT" -o TARGET)"
[[ -n "$MOUNT_TARGET" ]] || { echo "Could not identify the target filesystem" >&2; exit 2; }
if [[ "$MOUNT_TARGET" == "/" ]]; then
    echo "Refusing to run on the system filesystem (mount /)." >&2
    echo "Mount your HGST at /mnt/TEST or supply another separate mount." >&2
    exit 2
fi

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/MdView.Benchmarks.csproj"
mkdir -p -- "$ROOT/tmp" "$ROOT/data" "$ROOT/logs"
export TMPDIR="$ROOT/tmp"
export TMP="$TMPDIR"
export TEMP="$TMPDIR"
export DOTNET_CLI_TELEMETRY_OPTOUT=1

STAMP="$(date -u +%Y%m%dT%H%M%SZ)"
SYSTEM_LOG="$ROOT/logs/system-$STAMP-$$.txt"
{
    printf 'Timestamp (UTC): %s\n' "$(date -u --iso-8601=seconds)"
    printf 'Benchmark root: %s\n' "$ROOT"
    printf 'Temporary file root: %s\n' "$TMPDIR"
    printf 'Mount target: %s\n' "$MOUNT_TARGET"
    printf '\n-- uname --\n'
    uname -a
    printf '\n-- findmnt --\n'
    findmnt -T "$ROOT" -o SOURCE,TARGET,FSTYPE,OPTIONS
    printf '\n-- lsblk (rotational flag ROTA) --\n'
    lsblk -o NAME,SIZE,ROTA,TYPE,FSTYPE,MOUNTPOINTS
    printf '\n-- df --\n'
    df -hT -- "$ROOT"
    printf '\n-- available memory --\n'
    free -h
    printf '\n-- dotnet --\n'
    dotnet --version
} > "$SYSTEM_LOG" 2>&1
printf 'System information: %s\n' "$SYSTEM_LOG"
printf 'Building separate benchmark utility (not the Toolbox test suite)...\n'
dotnet build "$PROJECT" -c Release --nologo
printf 'Benchmarking on mounted filesystem %s\n' "$MOUNT_TARGET"
printf 'All temporary files redirected to: %s\n' "$TMPDIR"
exec dotnet "$SCRIPT_DIR/bin/Release/net10.0/MdView.Benchmarks.dll" "$MODE" "$ROOT" "$@"
