# NuGet-first publication policy for reusable Toolbox components

**Decision recorded 2026-11-02.** GitHub remains the source of truth for code,
documentation, tests, versions and release evidence. NuGet is the standard
distribution channel for **usable, independently consumable .NET libraries**
and suitable reusable .NET tools.

## Publish early when usable

- Do **not** postpone the first package until a stable `1.0.0` or an entire
  family is finished. Once a component has a coherent supported API, documented
  behavior, passing relevant tests and a verified installable package, publish
  it as a **prerelease** (for example, `0.1.0-preview.1`).
- Publish further preview builds as useful improvements become available.
  Having 5, 30 or more prereleases is acceptable; each distinct version is an
  immutable, traceable snapshot of progress.
- Make prerelease limitations explicit in the package README and change notes.
  Do not advertise planned features as published features.
- Preview is **not** an excuse for unbuildable, undocumented, untested or
  demonstrably broken packages. Preserve a functioning minimum use case.
- Release independent libraries separately, keeping their Package IDs stable.
  NuGet publication is not mandatory for integration tests, one-off examples,
  internal-only projects or unfinished designs; a reusable CLI can be
  packaged as a .NET tool when that is useful.

## Why

Consumers across different repositories should use **PackageReference**,
rather than shared relative paths to source projects. This allows independent
version selection and updates without editing dozens of project paths.
Within the Toolbox monorepo, project references may still be used for
source-level iteration, CI and integrated testing before publication.

NuGet package managers **can offer** updates when newer versions are
available, but installing a new release is generally an explicit dependency
management action (or an intentional automation/policy), not an unconditional
upgrade of every consumer on restore.

## Prerelease versions and update discovery

Use monotonically increasing SemVer-compatible versions such as:

```text
0.1.0-preview.1
0.1.0-preview.2
0.1.0-preview.30
0.1.0-rc.1
0.1.0
```

- Do not reuse a previously pushed **Package ID + version**. Published
  package versions are immutable; correct mistakes with the next version.
- The stable release `0.1.0` ranks above the corresponding
  `0.1.0-rc.1`, which ranks above `0.1.0-preview.30`.
- Consumers who want previews must enable prerelease updates in their NuGet
  client (for example, Visual Studio **Include prerelease**) or explicitly
  reference the prerelease version. Stable-only consumers are not guaranteed
  to see preview updates. CI may automate dependency update proposals, but
  each proposal should build and test before adoption.
- NuGet prohibits a **stable package** from depending on a **prerelease
  package**. Ensure the whole published dependency graph has compatible
  release status.
- Prefer explicit package versions (or centrally managed versions) for
  reproducible consumer builds; updating intentionally is safer than
  silently drifting to an untested preview.

## Minimum release checkpoint

1. Mark the current, *implemented* public API and known preview limitations
   in the component's root `README.md` and `Docs/<Topic>/en.md`.
2. Run the component's tests and applicable solution/OS CI. Check ABI/API and
   package dependencies for any breaking change; prereleases may evolve, but
   changes still require release notes.
3. Run `dotnet pack -c Release <path-to-project.csproj>`; inspect the actual
   `.nupkg` (DLL, targeted TFMs, license, XML docs, README, dependencies,
   intended public metadata).
4. Install/restore the **packed artifact** in an independent small consumer
   project, invoke the supported API and ensure packaging does not depend on
   relative source folders. When a tool is packaged, test its actual entry
   point.
5. Assign a unique version, identify the exact Git commit/tag and record
   results in the owning `IMPLEMENTATION_STATUS.md`. Publish using the
   authorized NuGet credentials; never commit credentials into the repo.
6. Verify that the package version is actually visible/restorable from NuGet,
   then record its URL and status. A green CI run or version in a csproj is
   **not** evidence that publishing succeeded.

## Dependencies and evolution

Prefer stable, narrowly scoped packages over umbrella dependencies. Keep
ExchangeHouse independently usable without a renderer, Blackboard without
UnderstandMe, and optional adapters without forced heavyweight dependencies.
A preview consumer may upgrade at its own cadence. Do not silently break
stable consumers when preview packages change.

## Official references

- [Microsoft: prerelease versions in NuGet](https://learn.microsoft.com/en-us/nuget/create-packages/prerelease-packages)
- [Microsoft: find, install and update packages](https://learn.microsoft.com/en-us/nuget/tools/package-manager-ui)
- [Microsoft: NuGet guidance for .NET library authors](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/nuget)

See [current publication status](../Release-Status/en.md) and
[development verification](../Development-and-Verification/en.md).
