# MdView 0.1.0-preview.1 — first CLI preview

MdView is an early cross-platform **terminal Markdown reader** built on
.NET 10 and Spectre.Console. This preview focuses on reading real
documents; it is **not** a finished pager or a system-wide installation.

## Supported archives

- Windows x64: mdview-0.1.0-preview.1-win-x64.zip
- Linux x64: mdview-0.1.0-preview.1-linux-x64.tar.gz

The archives contain self-contained, untrimmed .NET 10 applications,
the MIT license and Markdown documentation. No separate .NET runtime
installation is required. Native syntax-highlighting dependencies
may be unpacked on first run. Verify the SHA-256 against the
adjacent .sha256 file.

These builds are **portable archives**, not MSI, setup.exe, DEB,
AppImage, or globally installed shell commands.

## Start on Windows (PowerShell)

Extract the ZIP to a directory you control. In that directory:

~~~powershell
.\mdview.exe --version
.\mdview.exe --help
.\mdview.exe preview .\README.md
.\mdview.exe local .\README.md
.\mdview.exe git afrowaveltd toolbox
~~~

## Start on Linux (Debian or other x64 distribution)

Extract to a writable directory. From the extracted directory:

~~~bash
chmod +x ./mdview
./mdview --version
./mdview --help
./mdview preview ./README.md
./mdview local ./README.md
./mdview git afrowaveltd toolbox
~~~

Use a terminal with interactive ANSI support for pagers and a
TTY over SSH. The no-argument source menu and local pagers
do not work with redirected input. Noninteractive one-shot
output uses explicit local, URL and GitHub commands.

## Scope of this preview

- **No arguments**: Spectre source menu, Markdown file selection,
  HTTPS URL entry and public GitHub repository navigation.
- **local PATH**: render an entire Markdown document with syntax
  highlighting. This is **not bounded RAM**; avoid huge files.
- **source PATH**: a bounded-memory disk-indexed **raw text** pager.
- **preview PATH**: experimental disk-backed responsive Markdown
  pages for a **local** source file, with heading/link-index
  navigation and native table inspection.
- **preview-native PATH**: experimental mixed Markdown and native
  Spectre table paging; optional --max-width N, --center and
  --long-tables.
- **browse PATH**, **browse-url HTTPS_URL**, and
  **browse-git OWNER REPOSITORY**: local/remote link browsing
  with their respective trust and caching limits.
- **git OWNER REPOSITORY** and **url HTTPS_URL**: remote
  Markdown rendering, subject to cache and network limits.

Sources are validated before paging; page indexes and
intermediate journal data are disk-backed and temporary.
The preview never executes fenced code.

## Known limitations

- Native paged Markdown is **opt-in**, not the default CLI mode.
  Full rendering still loads the entire document into memory.
- Page layout can rebuild its disk index after terminal resize;
  some advanced native table behavior remains experimental.
- No private GitHub authentication or unchecked redirects.
- Remote documents use verified caching; browsing is limited
  to supported HTTPS URLs and same-origin navigation.
- The physical slow-HDD journal can show intermittent stalls
  for 1-GiB documents. The default journal append behavior
  was restored after a controlled B–A–B test showed the
  alternative to be 2.64 times slower on that HDD.
- Packaging smoke tests cover --version, --help and a local
  Markdown render on Windows and Linux. A real Windows
  Terminal, Debian and SSH interactive acceptance test
  remains desirable.
- No updater, system installer, auto-registration or NuGet
  package is included in this preview.

## Verification and source

Download only from the Afrowave GitHub repository. Confirm
each SHA-256 checksum before extracting; retain LICENSE.txt.
The source, tests and complete documentation are available at:

https://github.com/afrowaveltd/Toolbox/tree/master/Blackboard/Tools/MdView

This is intentionally a **pre-release**, with the scope
and limitations documented for early users.

## Published prerelease

The first preview is now available at the
[MdView 0.1.0-preview.1 GitHub release](https://github.com/afrowaveltd/Toolbox/releases/tag/mdview-v0.1.0-preview.1).
It was published from commit
`c66859be5dbbfa9d3bf60135ab4a6cc16119cad8`
on 2026-11-22 after Windows x64 and Linux x64
package tests and archive extraction smoke tests.
The immutable release includes two archives and
two matching SHA-256 files. This documentation
on master may receive updates after the release;
the published archives remain tied to the tagged
source commit.
