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

## 2026-11-02 — 1-GiB low-end rotational HDD acceptance target

- Prepared dedicated terminal-only Debian 13 field protocol with
  one virtual CPU, 1 GiB guest RAM and physical rotational-HDD backing.
  Build/publish outside the guest where practical, then measure the
  Release runner under `/usr/bin/time -v` and monitor disk wait,
  swap activity and CPU. Increase only RAM (to 2 GiB) if needed;
  preserve same disk, controller and workloads.
- The virtual stream runner is not an end-to-end disk paging benchmark.
  The VM is planned, **not yet tested**. Reference:
  [low-memory acceptance](../../../Docs/Low-Memory-Linux-Acceptance/en.md).
