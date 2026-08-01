# Implementation status

Last updated: 2026-08-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The runtime/public-API audit has also verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `ErrorCatalogContextProviderSuccessfulEnvelopeSuppressionTests`: **2 user-verified green**.
- `ErrorCatalogContextProviderSuccessfulStatusNormalizationTests`: **3 user-verified green**.
- `ErrorCatalogContextProviderEmptyWarningEnvelopeNormalizationTests`: **3 user-verified green**.
- `ErrorCatalogContextProviderNullInnerPayloadTests`: **2 user-verified green**.
- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain is registered with category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first thermal definition is present:

- ID `AFW_THM_0001`;
- code `1000001`;
- name `TEMPERATURELIMITEXCEEDED`;
- parameterized message using `{temperature}`, `{unit}`, and `{limit}`;
- default severity `Warning`;
- categories `THERMAL` and `VALIDATION`;
- documentation key `when-it-fails/errors/thermal/temperature-limit-exceeded`.

The focused catalog test and complete workspace validation are user-verified green.

The authoritative owner catalog uses these non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

No existing error code was renumbered.

The bootstrap synchronization contract first failed on the stale owner boundary, then after introducing resource-backed templates failed because the expected embedded resource was not present in the Linux-built assembly:

```text
Afrowave.Toolbox.WhenItFails.Bootstrap.Templates.errors.en.json
```

Changing path separators and explicit `LogicalName` metadata did not alter this result. `DefaultJsonsTemplateProvider` now includes deterministic diagnostics that list every actual manifest resource name when an expected catalog resource is missing. The next focused run must capture that list before any further build or provider changes are made.

## Focused verification

Rerun the bootstrap synchronization contract:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges
```

Expected current result: the test may remain red, but the exception must now include `Available manifest resources:` followed by the actual assembly resource names or `<none>`. Do not continue without that exact output.

The complete catalog workspace is user-verified green:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Latest user-verified result: **0 errors, 0 warnings, and 0 information issues**.

The focused thermal contract remains available through:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureLimitExceededCatalogTests
```

Latest user-verified result: **1 green test**.

The complete Setter suite remains available through:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Before committing further catalog changes, also run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
git diff --check
```

## Documentation synchronization completed

Maintained English documentation includes:

- `README.md` and `Readme/en.md`;
- `Docs/Overview/en.md`;
- `Docs/Commands/en.md`;
- `Docs/Known Limitations/en.md`;
- `Docs/Roadmap and Future Work/en.md`;
- `Docs/Getting-Started/en.md`;
- `Docs/FAQ/en.md`;
- `Docs/Testing and CI/en.md`;
- `Docs/Reviewing Catalog Changes/en.md`;
- `WhenItFails/Docs/Thermal Errors/en.md`.

Next documentation target: document that bootstrap templates are generated from embedded authoritative catalogs, then synchronize maintained owner-range documentation and reference catalog copies. Keep this file synchronized while the runtime/public-API audit continues.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Any future absurd-temperature wording must never alter the structured contract, severity, metadata, thresholds, or application decision-making.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.
- Prefer one file plus its directly related test, then commit.

## Recommended next step

Pull commit `d97b88896156c65c8ef2b041c201f9ef8d8ef310` and rerun `GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`. Capture the complete `Available manifest resources:` portion of the exception. Use that exact evidence to decide whether the resources are absent or merely named differently.

## Last completed change

The latest slice confirms that cross-platform path syntax alone did not make the catalog resources visible to the assembly. The provider now reports all actual manifest resource names whenever lookup fails, replacing further guesswork with direct assembly evidence.

Commits:

```text
0cfed041c4ca76fa0850b839ac3c740db28048fc
Add thermal bootstrap synchronization contract

74916771c98c3b8281944b6ecf6e43f16997f1ef
Embed authoritative WhenItFails bootstrap catalogs

20eca00c1f02b628a5f678f6a9d1f8791767770d
Synchronize bootstrap templates from embedded catalogs

b1969de2db48e8533e4ae9c6c20337fa026523b9
Fix embedded catalog resource paths

d97b88896156c65c8ef2b041c201f9ef8d8ef310
Improve embedded catalog diagnostics
```
