# Terminal capabilities and icon fallback

**Status: design direction, 2026-10-22. Not yet implemented.**

## Background

The user's original `Lutonet/SpectreConsoleFileBrowser` source includes `public bool DisplayIcons { get; set; } = true;` and uses Spectre.Console emoji markup such as `:file_folder:` and `:abacus:` for directory/file entries. This works on supported terminals but older Windows console hosts and remote terminals may not render these glyphs.

Current Spectre.Console exposes detected terminal capabilities through `AnsiConsole.Profile.Capabilities`, including `Unicode`, `Legacy`, `Interactive`, `Ansi` and color capabilities. Spectre also provides Unicode/ASCII layout fallbacks. Documentation: https://spectreconsole.net/console/reference/capabilities-reference/ and https://spectreconsole.net/console/explanation/understanding-rendering-model/.

**Important:** `Unicode == true` or `Console.OutputEncoding.CodePage == 65001` does **not** prove emoji glyph coverage, font fallback or correct display-cell widths. Fonts and terminal hosts decide whether emoji render; blindly writing them can produce tofu glyphs or alignment errors. See https://learn.microsoft.com/dotnet/api/system.console.outputencoding.

## User-facing setting

Replace the old boolean's behavioral default with a tri-state policy (exact API name TBD):

- **Auto (default):** inspect the renderer's detected capabilities. If Unicode is unavailable, output is redirected/noninteractive, or capability detection is uncertain, render **no icon glyphs** and use clear textual directory/file labels. On a known Unicode-capable terminal, prefer a conservative, narrow Unicode symbol set (not wide emoji by default); when uncertain, choose text.
- **On:** user explicitly forces the richer icon set, including emoji where configured, accepting that the host terminal's font may still not support it.
- **Off:** no icon glyphs regardless of capabilities.

Retain a backwards-compatible simple `DisplayIcons` toggle only if an actual compatibility contract is needed; the old package remains unchanged on NuGet. In the new package an explicit `IconMode` is preferable. Keep icon mode independent from color/ANSI settings and from the application's localized UI text. Optional application CLI/config overrides may map `--icons auto|on|off` to this policy.

Display without icons must remain clear and accessible: directory names can have trailing separators or a translatable `Directory` label, with distinct parent/navigation actions. **No essential navigation meaning may be conveyed by an icon alone.** Support narrow terminals and keyboard-only browsing.

## Implementation approach

- Determine capabilities through an injected terminal/profile abstraction; in the Spectre.Console adapter use the current `IAnsiConsole` profile instead of detecting OS version alone. Do not hardcode `Windows == no Unicode`: modern Windows Terminal is capable, and Linux/SSH clients can also be misconfigured.
- Treat `Console.OutputEncoding`, `TERM` and interactive state as hints only. Do **not** silently alter system code pages (`chcp`), `Console.OutputEncoding` or process-wide Spectre profile just to enable icons. No intrusive runtime glyph probe or terminal escape query should block startup over SSH.
- Resolve the icon policy once per display session and let a user action override it without restarting. The caller can explicitly disable icons regardless of detected capabilities.
- Separate semantic entry type (`File`, `Directory`, `Parent`, `Root`, `Error`) from optional decorative glyph rendering. Use safe Spectre text/escaped markup for filenames and localization values.
- If implementing an optional **Test Display** page, make it an explicit user action, with samples of ASCII, box-drawing Unicode and emoji; users decide which renders properly. This can be more trustworthy than guessing glyph support.
- Source paths, selected file IDs and JSON configuration values remain Unicode data even when **decorative icons** are disabled. Do not transliterate, drop or corrupt real Unicode filenames. If the console truly cannot render a filename, keep exact internal path and offer a safe escaped display fallback where needed.

## Test matrix

- Legacy Windows terminal and modern Windows Terminal (capabilities vary); PowerShell, cmd and ConPTY.
- Debian/Linux console, SSH from different clients, `tmux`/`screen`, `TERM=dumb`, ANSI disabled, redirected stdout and CI.
- Stubbed profile: Unicode true/false/unknown, interactive true/false, forced On/Off, font lacking emoji, no-color with Unicode enabled.
- Verify no emoji escape sequences in ASCII/Auto fallback, correct names and selectable identity, unchanged process-wide output encoding, Unicode filenames not mutated, missing glyph fallback remains usable and no silent override of a user-selected policy.
- Ensure the future Blackboard sample app demonstrates each mode and clearly labels current effective mode.

## Delivery order

Implement and test the small capability-policy decision independently of file enumeration and FTP adapters; then wire it to the FileBrowser renderer. The base package must remain lightweight and must not depend on external Unicode/emoji probing packages. All public APIs receive English XML comments and tricky fallbacks get focused C# comments.
