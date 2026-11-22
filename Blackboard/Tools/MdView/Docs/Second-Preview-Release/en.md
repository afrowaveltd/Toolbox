# MdView 0.1.0-preview.2 — application icon and desktop menu integration

This preview updates the first CLI prerelease with the official
MdView artwork and packaged desktop integration. Terminal viewing,
Markdown rendering, disk-indexed paging and the proven fast
journal append default remain unchanged.

## What is new

- **Windows x64:** a generated multi-resolution application icon
  is embedded as the standard Win32 application icon of mdview.exe.
  This is visible to Windows Explorer and normal executable shortcuts.
- **Linux x64:** original icon.png is included beside the portable
  executable. An optional user-local menu launcher registers this
  existing unpacked executable with the icon and Terminal=true.
- The PNG in Assets/icon.png remains the **single source** of icon
  design; ICO variants are generated on Windows during builds
  (not edited independently).

Neither platform attempts to draw a PNG into the terminal.
This is an app/executable/desktop-shortcut icon.

## Archives

- mdview-0.1.0-preview.2-win-x64.zip
- mdview-0.1.0-preview.2-linux-x64.tar.gz
- Matching SHA-256 checksum file for each archive.

Both archives contain self-contained .NET 10 binaries (no installed
.NET runtime needed), release README and MIT license. Linux archive
additionally contains install-desktop.sh. These are **portable**
archives, not MSI, DEB or AppImage installers.

## Windows

Extract the Windows ZIP. Start from PowerShell in the extracted
directory:

~~~powershell
.\mdview.exe --version
.\mdview.exe preview .\README.md
.\mdview.exe preview-native .\README.md --center --max-width 100
~~~

Windows Explorer should display the MdView icon on mdview.exe.
An existing Explorer icon cache can delay visual updates.

## Debian / Linux x64

Extract the TAR.GZ to a directory you intend to retain.
From a terminal in the extracted directory:

~~~bash
chmod +x ./mdview ./install-desktop.sh
./mdview --version
./mdview preview ./README.md
./install-desktop.sh
~~~

The last command is **optional**. It creates
\`~/.local/share/applications/afrowave-mdview.desktop\`
(or the corresponding \`XDG_DATA_HOME/applications\` path)
without requiring sudo. The launcher points to the
**absolute path of this extracted executable and icon.png**;
it does not install or copy the actual application.
To uninstall the menu entry, remove that desktop file.
If you move the application directory, rerun the script.

Desktop menus may require logout/login or cache refresh before
a newly registered launcher is visible. A terminal emulator
must be available to handle \`Terminal=true\` launchers.

## Existing scope and limitations

The one-shot \`local PATH\` renderer reads the full Markdown
document into RAM. The bounded-memory, disk-backed local
pagers (\`source\`, \`preview\`, \`preview-native\`) remain
opt-in, and some native table cases are experimental.
Public GitHub navigation and HTTPS browsing have their
documented limitations; private GitHub auth is not supported.
The 1-GiB old-HDD Windows/Debian comparison remains in Docs.

The release CI exercises the full MdView xUnit suite,
published binary version/help/local render on Windows and
Linux, archive extraction, and icon/desktop registration
smoke checks. It cannot guarantee every Linux desktop
environment renders the app icon identically, so a
manual KDE/GNOME launcher check is still recommended.

For the complete project scope, see
https://github.com/afrowaveltd/Toolbox/tree/master/Blackboard/Tools/MdView

This is still an explicitly labeled preview release.

## Published version and verified source

The [public release](https://github.com/afrowaveltd/Toolbox/releases/tag/mdview-v0.1.0-preview.2) was published on
**2026-11-22** from commit
`b9c28a5a22b97cf20d7304817ac6e5a4a875a238`.
GitHub Actions
[run 45064168702](https://github.com/afrowaveltd/Toolbox/actions/runs/45064168702)
passed all **three** jobs (Windows and Linux
portable builds and final prerelease publication).
The release has matching Windows/Linux checksum files.
The original `0.1.0-preview.1` is unaffected.
