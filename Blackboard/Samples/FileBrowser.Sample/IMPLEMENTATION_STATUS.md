# FileBrowser.Sample — Implementation Status

## 2026-10-24 — initial demo

- Added small .NET 10 executable that demonstrates the standalone local FileBrowser picker in an interactive terminal.
- Supports choosing files or folders; icon overrides, initial custom masks such as `*.ajis;*.json`, or regex such as `\.(ajis|json)$`. Interactive filters, tree navigation, recursive searching and multi-match selection are implemented in the consumed library.
- No production/domain settings logic, translator dependencies, FTP or GUI libraries included.
- CI verification and manual terminal smoke tests are still required; no claim of runtime UI testing is made from a compilation alone.

## 2026-10-24 — compiled demo in Toolbox solution

- Registered `FileBrowser.Sample.csproj` in `Toolbox.sln`, with only a project reference to independent FileBrowser; no translation/UnderstandMe/FTP dependency.
- Offers `--directory`, `--icons-on`, `--icons-off`, `--filter "*.ajis;*.json"`, `--regex "\.(ajis|json)$"` and an optional start folder. Prompts delegate to the shared SpectreFileBrowser.
- GitHub Actions [40099094982](https://github.com/afrowaveltd/Toolbox/actions/runs/40099094982) succeeded with demo included in the solution build. The sample has **not** been manually run in a real terminal in this session.

### Library/UI integration CI

- Latest relevant [GitHub Actions run 40099273392](https://github.com/afrowaveltd/Toolbox/actions/runs/40099273392): build green with sample in solution; FileBrowser library had 147 passing tests including virtual keyboard navigation. Manual demo smoke in a real terminal is still not verified.
