# FileBrowser target framework compatibility

**Decision under evaluation (2026-10-24):** prefer the lowest practical target framework for the reusable FileBrowser **library**, independent from the .NET 10 Showcase and tests. The target is **netstandard2.0**, provided an actual compile, NuGet pack and consumer tests pass. **Not yet implemented.**

## Why netstandard2.0

Spectre.Console 0.57.2 already publishes a netstandard2.0 asset. This potentially permits consumption from .NET Framework 4.6.1+ (prefer >=4.7.2/4.8 for real-world compatibility), .NET Core and newer .NET. Host OS and Spectre terminal input compatibility still need testing. A self-contained .NET 10 publish cannot make .NET 10 run on unsupported Windows versions.

## Confirmed API migration items

- Replace `OperatingSystem.IsWindows/IsLinux/IsMacOS` with compatible runtime detection (e.g. `RuntimeInformation.IsOSPlatform`).
- Replace `Path.IsPathFullyQualified` with an equivalent carefully tested local-path validation for Windows UNC, drive-relative paths and POSIX.
- Replace `ArgumentNullException.ThrowIfNull` with conventional null checks and preserve parameter names.
- Replace newer generic `Enum.IsDefined(value)` overloads with `Enum.IsDefined(typeof(TEnum), value)` or an equivalent, without changing validation.
- Review other newer BCL overloads (`string.Contains(char/string, StringComparison)`, `StringSplitOptions`, span comparers, async console APIs) and modern language constructs.
- Modern C# records/init members and collections expressions may require compatibility attributes/compiler configuration on netstandard2.0. Preserve the same public API and semantics.
- Review `Spectre.Console`'s netstandard2.0-specific dependency graph; test package resolution on .NET Framework 4.8.
- Verify Windows 7 SP1 only using an isolated VM with .NET Framework 4.8; neither netstandard2.0 nor NuGet compatibility tables guarantee console behavior on that OS.

## Release gates

1. Create an isolated compatibility branch or atomic migration and compile `netstandard2.0`; keep existing consumer apps targeting `net10.0`.
2. Run all existing FileBrowser tests, the Spectre.Console virtual-terminal interaction tests, the Showcase tests and solution build.
3. Pack the NuGet, inspect its target framework assets, dependencies, license and README.
4. Build and run a **separate net48 consumer**, covering picker navigation, cancel, Unicode fallback and path validation; ensure it requires no .NET 10 runtime.
5. Manually test a matrix of Windows 10/11, Debian and optional isolated Windows 7 + .NET Framework 4.8. Do not promise unsupported runtime/OS combinations.
6. Update the README and release metadata **only after** successful verification; then publish a preview.

## Non-goals

No legacy-specific fork, no dependency on UnderstandMe/Essentials/WhenItFails, no changes to test app target framework, no loss of safety limits or source-qualified paths. Prefer a single multi-consumer package rather than a separate legacy product.

## Conditional compilation and multi-targeting policy (2026-10-24)

Use the oldest compatible and maintainable API in shared source whenever it has equivalent behavior on modern runtimes. Do **not** add preprocessor branches merely because a newer API exists. For genuine platform-specific behavior, use multi-target builds from the same sources with framework constants (`NET8_0_OR_GREATER`, `NETSTANDARD2_0`, `NETFRAMEWORK`) and a small compatibility facade, rather than duplicating the entire browser.

Possible project configuration **after the netstandard2.0 build is verified**:

```xml
<TargetFrameworks>netstandard2.0;net10.0</TargetFrameworks>
```

Multi-target only when the net10.0 asset produces measurable benefit; otherwise use netstandard2.0 alone. Conditional compilation must preserve the same public contracts and cancellation/error behavior. Distinct frameworks need individual build/test/pack validation and a separate net48 consumer smoke test.

Example principle: `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)` can be shared across both targets; no `#if` required. Windows 7 and IE11 are not implied to be supported merely because a compatible binary can be produced. Legacy environments must be tested, and security-support status documented.
