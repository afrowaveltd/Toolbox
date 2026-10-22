# FileBrowser terminal encodings: auto detection and legacy code pages

**Status:** design and feasibility analysis (2026-10-22), not implemented.

## Why terminal encoding differs from Unicode glyph support

Three independent concepts must not be conflated:

1. **Byte encoding** for input/output: UTF-8, CP852 (DOS Central European), Windows-1250, ISO-8859-2, etc. This governs conversion between Unicode strings and bytes on the terminal channel.
2. **Renderer capabilities**: ANSI/interactive/Unicode line drawing, colors, emoji assumptions. Spectre.Console's terminal profile helps with presentation but cannot reliably determine a remote font's glyph coverage.
3. **Font/glyph availability and display widths**: a UTF-8 terminal can still lack emoji; an old single-byte console may correctly display Czech characters.

A source filename, its full path and selected location are Unicode/opaque application data. The FileBrowser **must not transcode, transliterate, normalize or otherwise alter a path to make its UI display correctly**. Presentation fallback is distinct from the actual selected path and source-qualified ID.

## Detection and configuration

Default mode: **Auto**. Read actual process/runtime facts without changing them:

- `Console.OutputEncoding` and `Console.InputEncoding` code page and name, when accessible. These are clues to the .NET application's current encoding, not proof of the encoding expected by an SSH client or font.
- Spectre.Console renderer/profile Unicode, ANSI and interactivity capability (use the API of the stable Spectre version ultimately selected).
- Windows console input/output code pages where relevant; avoid assuming that OS version == code page. Windows may have different input and output code pages.
- On Unix/SSH: `LC_ALL`, `LC_CTYPE`, `LANG` and `TERM` are hints. A remote SSH server generally cannot see the actual local client's font or reliably interrogate client encoding. Do not infer CP1250, CP852 or ISO-8859-2 merely because the locale is `cs_CZ`.
- Redirected output, `TERM=dumb` or unknown capabilities should disable decorative icons, Unicode box drawing and rich ANSI, with a readable ASCII/plain-text fallback.

Manual mode: let the **hosting application** select an explicit output encoding by **name or numeric code page** (`utf-8` / `65001`, `cp852` / `852`, `windows-1250` / `1250`, `iso-8859-2` / `28592`), and optionally a separate input encoding. Validate supported encodings, do not accept arbitrary values silently. Add a `Terminal diagnostics` view to show detected/selected input and output encodings, terminal capabilities and effective icon mode, without exposing environment secrets.

The reusable FileBrowser **must not change global `Console.InputEncoding`, `Console.OutputEncoding`, Windows `chcp`, environment variables or terminal state**. Encoding selection is a host/console-output-configuration responsibility: if the application explicitly requests applying it, require a deliberate operation at the host boundary, explain its process-wide effects, and restore previous settings where safe. Not every terminal API or Spectre output path permits a per-widget encoding override. A manual override used solely for **display-capability policy** must not be misrepresented as changing the actual transmitted bytes. Never claim that selecting CP852 changes an SSH client's configuration.

In .NET 10, extra legacy code pages may require `System.Text.Encoding.CodePages` and/or `CodePagesEncodingProvider` registration. Keep this an **optional dependency of explicit legacy-mode support**, not a core requirement for UTF-8/local-only browsing. Registering this provider globally can affect `Encoding.GetEncoding(0)` behavior on Windows; therefore isolate its use and document host impact before implementation. Avoid silent fallback to replacement `?` on an unsupported encoding.

## Display fallback policy

- If the chosen terminal output encoding cannot represent a visual symbol, do **not** emit the symbol as a replacement `?`; use ASCII text labels and ASCII navigation.
- If a filename contains characters not representable by the chosen encoding, display an **escaped ASCII-only representation** of its Unicode code points (e.g. `\u010D` for č, `\u{1F600}` for 😀), with clear differentiation from literal backslash text, but keep the original path and selected item identity unchanged.
- Prefer a selective display escape (only problematic runes, while preserving representable letters) rather than stripping all diacritics. Escape strings for Spectre.Console markup *after* display transformation. Ensure stable selection by entry IDs, not the rendered filename; handle collisions between display labels by extra distinguishing context.
- If detection is uncertain, render **ASCII-only structural UI**; this must not imply that real filenames are ASCII or erase usable Czech characters.
- Preserve UTF-8 as the normal recommended path on contemporary Windows Terminal, SSH and Linux. Old code pages are compatibility options, not default behavior.
- Keep input, output and UI-font behavior independently configurable; the existing **IconMode Auto/On/Off** is a separate policy.

## Suggested user interface

```text
Terminal settings
  Output encoding : Auto (detected UTF-8 / CP65001)
  Input encoding  : Auto (detected UTF-8 / CP65001)
  Icons           : Auto (ASCII labels)
  Unicode borders : Auto (disabled)
  [Test display]  [Help]  [Back]
```

Provide an optional `Test display` command/page: show ASCII, Czech diacritics, box-drawing and emoji plus the encoding detected and selected. This does not prove support automatically — the **user confirms** what renders correctly. An explicit display-only override should remain separate from an explicit host output encoding change.

## Tests

- UTF-8, CP852, CP1250 and ISO-8859-2: round trip where representable, reliable escaped display for unrepresentable characters, no replacement glyphs in structural UI.
- Different input vs output encodings; unknown code page; code pages missing without provider; redirected stdout, `TERM=dumb`, SSH with misleading locale; Windows Terminal vs older console.
- Emoji/font missing despite reported Unicode support; forced icon mode; no colors; Unicode filenames preserved exactly in selection (including combining marks, RTL and emoji), unique entry identities despite identical displayed fallback labels.
- No modification of process-wide console encoding, cwd or terminal capabilities from a library call; explicit host changes are separately tested and restored.
- Ability to cancel configuration/testing and recover from encoding exceptions without saving configuration.

## References

- Microsoft .NET: `Console.OutputEncoding` — https://learn.microsoft.com/dotnet/api/system.console.outputencoding
- Microsoft .NET: `Encoding.RegisterProvider` — https://learn.microsoft.com/dotnet/api/system.text.encoding.registerprovider
- Windows Console Code Pages — https://learn.microsoft.com/windows/console/console-code-pages
- Related design: [Terminal Compatibility](../Terminal-Compatibility/en.md).

## Scope

This is a planned **small capability policy and optional compatibility pathway**, not a promise to support obsolete physical terminals. Do not bundle serial-port drivers, terminal emulators, icon fonts or a conversion engine into the FileBrowser package. Implement the standard UTF-8/local behavior first, and only introduce an opt-in legacy encoding adapter when tests show a concrete need.
