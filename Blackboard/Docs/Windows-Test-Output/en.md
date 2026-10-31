# Windows terminal width and cross-platform test assertions

A local Windows workstation (2026-10-29, .NET 11 RC SDK compiling .NET 10 projects) reported three UI test failures. All three asserted an unbroken path embedded in Spectre.Console.Testing output: FileBrowser's unsuccessful search, Configurator's JSON selection, and Showcase's directory selection.

The default virtual terminal has limited width. Windows temporary paths can be longer than Linux counterparts, and Spectre.Console may wrap these strings across lines. A correct selection does not require its displayed path to remain one unbroken substring.

The three **specific** path-assertion tests now configure a 512-column test console. Normal-width tests continue to exercise ordinary navigation and cancellation. Production rendering and path handling have **not** been changed to force wide terminals.

A new GitHub Actions `windows-ui` job will test FileBrowser, UnderstandMe Configurator and Blackboard Showcase on `windows-latest` using stable .NET 10. This supplements the existing Linux full-solution job. Verify this job before concluding that the Windows issue is fixed.

Unrelated warning cleanup in the same change removes xUnit2031 from inline parser tests and adds XML API comments to Configurator Help. The local SDK preview advisory `NETSDK1057` is not itself a failed test.

### Verified on both platforms — 2026-10-29

- [GitHub Actions #40857564421](https://github.com/afrowaveltd/Toolbox/actions/runs/40857564421) **SUCCESS** for code/test commit `ae52a29345f0fcbef08cbf6cda41686461bc25bd`.
- Ubuntu full solution restore/build/test **PASS**; new Windows runner's **FileBrowser**, **UnderstandMe Configurator**, and **Blackboard Showcase** project test steps all **PASS**.
- The original Windows UI path-wrap failures are resolved in the automated Windows environment. Manual developer-local retest with the latest checkout remains distinct from Windows CI.
