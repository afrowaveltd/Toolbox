#!/usr/bin/env bash
# Register the unpacked *portable* MdView in the current user's app menu.
# No root access, no system-wide installation and no executable relocation.
set -Eeuo pipefail

APP_DIR="$(cd -- "$(dirname -- "$0")" && pwd -P)"
APP="$APP_DIR/mdview"
ICON="$APP_DIR/icon.png"

if [[ ! -f "$APP" || ! -x "$APP" ]]; then
    printf 'Missing executable %s (or not executable). Run chmod +x ./mdview.\n' "$APP" >&2
    exit 2
fi
if [[ ! -f "$ICON" ]]; then
    printf 'Missing PNG icon: %s\n' "$ICON" >&2
    exit 2
fi
# Spaces work in a quoted Exec path. Reject characters that need special
# Desktop Entry escaping or would introduce unexpected field codes.
case "$APP_DIR" in
    *$'\n'*|*$'\r'*|*'"'*|*'\'*|*'$'*|*$'\x60'*|*'%'*)
        printf 'Unsupported special character in application path: %s\n' "$APP_DIR" >&2
        exit 2
        ;;
esac

DATA_HOME="$HOME/.local/share"
if [[ -v XDG_DATA_HOME && -n "$XDG_DATA_HOME" ]]; then
    DATA_HOME="$XDG_DATA_HOME"
fi
case "$DATA_HOME" in
    /*) ;;
    *) printf 'XDG_DATA_HOME must be an absolute path: %s\n' "$DATA_HOME" >&2; exit 2 ;;
esac
DESKTOP_DIR="$DATA_HOME/applications"
DESKTOP_FILE="$DESKTOP_DIR/afrowave-mdview.desktop"
mkdir -p -- "$DESKTOP_DIR"
if [[ -L "$DESKTOP_FILE" ]]; then
    printf 'Refusing symlink destination: %s\n' "$DESKTOP_FILE" >&2
    exit 2
fi
cat > "$DESKTOP_FILE" <<EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=MdView
GenericName=Markdown Reader
Comment=Terminal Markdown reader by Afrowave
Exec="$APP"
Icon=$ICON
Terminal=true
Categories=Utility;Viewer;
StartupNotify=false
EOF
chmod 644 -- "$DESKTOP_FILE"
printf 'Installed user desktop launcher: %s\n' "$DESKTOP_FILE"
printf 'Keep the extracted MdView folder at this path; re-run after moving it.\n'
printf 'To remove the launcher: rm -- "%s"\n' "$DESKTOP_FILE"
