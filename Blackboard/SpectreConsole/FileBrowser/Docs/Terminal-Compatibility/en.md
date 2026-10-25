# Terminal capabilities and icon fallback

**Status:** Original design notes from 2026-10-22. Capability-aware icon fallback is now implemented. Manual smoke tests reported on 2026-10-24/25 confirmed Debian and Windows terminal behavior, including the legacy Windows Console Host (`conhost.exe`). The remaining matrix entries below are not automatically implied to pass.

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

## Manual terminal smoke verification (2026-10-25)

These observations were reported by the maintainer after updating and running the current FileBrowser on real machines; they are **manual observations**, not new automated xUnit or CI tests.

| Host / profile | Reported outcome | Notes |
| --- | --- | --- |
| Debian interactive terminal | PASS (2026-10-24) | Navigation and file preview worked; a subsequent check displayed the conservative Unicode directory symbol in Auto mode. |
| Windows Terminal profiles | PASS (2026-10-25) | Available shell profiles (including Command Prompt, PowerShell, Developer Command Prompt and Git shell) were exercised successfully after the update. |
| Legacy Windows Console Host | PASS (2026-10-25) | The maintainer launched `conhost.exe` directly; FileBrowser functionality and colors were reported working. |
| SSH / tmux / screen, narrow consoles, specific legacy code pages | NOT VERIFIED by this report | Retain separate compatibility coverage for these cases. |

**Shell vs host:** Opening `cmd.exe` in Windows Terminal does not test the old console renderer. The direct `conhost.exe` check above covers that distinct Windows host. No extra implementation change or forced icon/encoding setting was needed based on the reported result. This does not prove every font, Unicode glyph, encoding or remote terminal configuration is supported.

Latest verified GitHub Actions workflow before this documentation update: [40121980599](https://github.com/afrowaveltd/Toolbox/actions/runs/40121980599) (SUCCESS on commit `85ac731e`). No new test execution is claimed from documentation updates.

## Delivery order

Implement and test the small capability-policy decision independently of file enumeration and FTP adapters; then wire it to the FileBrowser renderer. The base package must remain lightweight and must not depend on external Unicode/emoji probing packages. All public APIs receive English XML comments and tricky fallbacks get focused C# comments.

## Output character encoding (separate compatibility concern)

**Do not equate Unicode rendering capability with the input/output character encoding** used by the host process and terminal channel. The file picker should read current input/output encoding hints and expose optional host-driven code-page selection, supporting UTF-8 and opt-in legacy CP852/Windows-1250/ISO-8859-2, without globally mutating console settings. If a filename cannot be rendered faithfully in the selected output encoding, use an unambiguous ASCII escape for display while preserving the original Unicode path internally. See [Encoding Support](../Encoding-Support/en.md).
