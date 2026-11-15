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
# Fail closed if an existing directory is a symlink, or hides a
# nested mount on another volume. All data, logs and temporary
# indexes must remain on the designated rotational disk.
for FOLDER in "$ROOT/tmp" "$ROOT/data" "$ROOT/logs"; do
    ACTUAL="$(realpath -e -- "$FOLDER")"
    [[ "$ACTUAL" == "$ROOT/"* ]] || {
        echo "Refusing directory outside test disk: $FOLDER -> $ACTUAL" >&2
        exit 2
    }
    FOLDER_MOUNT="$(findmnt -n -T "$ACTUAL" -o TARGET)"
    [[ "$FOLDER_MOUNT" == "$MOUNT_TARGET" ]] || {
        echo "Refusing directory on a different filesystem: $ACTUAL" >&2
        exit 2
    }
done

SIZE_MIB=2048
ARGUMENTS=( "$@" )
for (( I=0; I<${#ARGUMENTS[@]}; I++ )); do
    if [[ "${ARGUMENTS[I]}" == "--size-mib" ]]; then
        [[ $((I+1)) -lt ${#ARGUMENTS[@]} ]] || {
            echo "Expected a value after --size-mib" >&2; exit 2;
        }
        SIZE_MIB="${ARGUMENTS[I+1]}"
    fi
done
[[ "$SIZE_MIB" =~ ^[0-9]+$ ]] || {
    echo "Size must be a positive integer of MiB" >&2; exit 2;
}
(( SIZE_MIB >= 8 && SIZE_MIB <= 8192 )) || {
    echo "Size must be 8–8192 MiB" >&2; exit 2;
}
# Conservative reserve: source, validated snapshot, journal,
# assorted indexes and room for the operating system/filesystem.
AVAILABLE_KIB="$(df -Pk -- "$ROOT" | awk 'NR==2 {print $4}')"
REQUIRED_KIB=$(( (SIZE_MIB * 6 + 1024) * 1024 ))
if (( AVAILABLE_KIB < REQUIRED_KIB )); then
    echo "Insufficient disk free space: ${AVAILABLE_KIB} KiB available;" >&2
    echo "need ${REQUIRED_KIB} KiB for source, snapshot, journal and safety reserve." >&2
    exit 2
fi

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
