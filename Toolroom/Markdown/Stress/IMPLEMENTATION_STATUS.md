# Markdown.Stress — Implementation status

## 2026-11-02 — first diskless streaming stress harness

- Added a standalone optional .NET 10 console app with a deterministic
  virtual TextReader. No huge file or complete source string is created.
- Sizes include 10M, 100M, 1G, 10G; caller can select seed, sampled heap
  growth threshold and opt-out rendering.
- Uses ExchangeHouse.Markdown.MarkdownStreamingReader.ScanAsync, with each
  event optionally rendered by Blackboard Spectre Markdown.
- Verifies contiguous 64-bit offsets, UTF-8 byte total, code-fence balance
  and sampled managed heap; reports time and process RSS.
- Added a dedicated pull-request smoke and manual GitHub Actions workflow.
- **Verification pending:** no large run is claimed complete yet. The
  authoring environment has no dotnet SDK; wait for CI evidence.
- The streaming scanner does not yet replace the original full-document
  parser/Showcase UI or guarantee full multiline CommonMark behavior.

### Next

1. Check CI build/tests and smoke benchmark results.
2. Run 100M and 1G, then 10G under an OS memory constraint.
3. Evolve streaming AST ownership, indexes and terminal paging.
4. Validate Windows/Debian interactive formatting separately.
