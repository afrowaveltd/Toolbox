# Implementation status

Last updated: 2026-09-03

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit protects bootstrap DTOs, validation contracts, stable enum values, descriptor and definition models, provider payloads, catalog context and initialization payloads, `CatalogProviderPipeline`, `ErrorCatalog`, factories, resolvers, runtime services, provider composition, bootstrap initialization, read-only profile-resolution results, malformed dependency-result boundaries, public JSON workspace path configuration, malformed document file paths, and malformed dependency diagnostics.

Recent focused hardening includes null/whitespace `JsonsOptions` workspace values, malformed path handling in both `JsonCatalogDocumentWriter` and `JsonCatalogDocumentLoader`, and null diagnostics handling in provider composition. `CatalogProviderPipeline` and `ErrorCatalogProvider` now tolerate failed dependency responses whose mutable `Issues` collection is null or contains runtime-null issue entries, using stable caller-supplied fallback codes/messages instead of dereferencing malformed diagnostics. Earlier runtime/public-API protections for `ErrorCatalogCrossValidator`, `ErrorProfileSelectionService`, `ErrorCatalogRuntime`, `ErrorDescriptorResolver`, `ErrorCatalogInitializer`, `BuiltInErrorCatalogContextProvider`, `JsonsBootstrapper`, catalog context composition, descriptors, definitions, normalization, validation, factories, resolvers, runtime services, and profile selection remain in place.

The current user-verified complete regression baselines are fully green:

- complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**;
- complete `WhenItFails.Tests`: **872 passed, 0 failed, 0 skipped**.

## Verification status

The latest user-verified Setter test run:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Result: **1,241 passed, 0 failed, 0 skipped**.

The latest complete core test run:

```powershell
dotnet test WhenItFails.Tests
```

Result: **872 passed, 0 failed, 0 skipped**.

Focused runtime/public-API checkpoints added after that complete core baseline include:

- all five cross-validator null-definition contracts: **5 passed**, warning-free;
- document-level and nested mutable-collection cross-validator contracts: user-verified green;
- `ErrorProfileSelectionService` null profile collection/entry contracts: user-verified green;
- `ErrorCatalogRuntime` null `Issues` contracts across reset, fallback, forwarding, recovery and initialization metadata: user-verified green;
- `ErrorDescriptorResolverNullIssuesCollectionContractTests`: **1 passed**;
- `CatalogProviderPipelineNullIssuesCollectionContractTests`: **1 passed**;
- `CatalogProviderPipelineNullFirstIssueContractTests`: **1 passed**;
- `ErrorCatalogProviderNullIssuesCollectionContractTests`: **1 passed**;
- `ErrorCatalogProviderNullFirstIssueContractTests`: **1 passed**;
- `ErrorCatalogInitializerNullIssuesCollectionContractTests`: **1 passed**;
- `BuiltInErrorCatalogContextProviderNullTemplatesCollectionContractTests`: **1 passed**;
- `JsonsBootstrapperNullTemplateContentContractTests`: **1 passed**;
- `JsonsBootstrapperNullTemplateTargetFileNameContractTests`: **1 passed**;
- `JsonsBootstrapperWhitespaceTemplateTargetFileNameContractTests`: **1 passed**;
- `JsonsBootstrapperEscapingTemplateTargetFileNameContractTests`: **1 passed**;
- `ErrorCatalogContextProviderNullErrorCatalogFileNameContractTests`: **1 passed**;
- `ErrorCatalogContextProviderNullSupportingCatalogFileNameContractTests`: **4 passed**;
- `ErrorCatalogContextProviderWhitespaceCatalogFileNameContractTests`: **5 passed**;
- `ErrorCatalogContextProviderNullWorkspacePathOptionsContractTests`: **2 passed**;
- `ErrorCatalogContextProviderWhitespaceWorkspacePathOptionsContractTests`: **2 passed**;
- `JsonCatalogDocumentWriterInvalidFilePathContractTests`: **1 passed**;
- `JsonCatalogDocumentLoaderInvalidFilePathContractTests`: **1 passed**.

`CatalogProviderPipeline.GetFirstIssueCode` and `ErrorCatalogProvider.GetFirstIssueCode` now skip runtime-null issue entries and use their stable fallback code when diagnostics are null, empty, or contain no usable issue. `JsonCatalogDocumentWriter.SaveToFileAsync` and `JsonCatalogDocumentLoader.LoadFromFileAsync` both map syntactically malformed paths to stable `FilePathIsInvalid` / `JSON catalog file path is invalid.` behavior; valid missing source paths remain `NotFound` / `FileNotFound`. `ErrorCatalogContextProvider` rejects null and empty/whitespace values for `RootDirectory`, `PackageDirectoryName`, and all five catalog filename options before evaluating derived paths or calling providers.

Numeric compatibility is directly protected for validation severity, catalog context source, and runtime state. `ErrorCatalogInitializationMode` remains protected by the existing numeric-value theory in `WhenItFailsOptionsTests`.

Other verified gates for the completed thermal slice:

- focused `ThermalFallbackProtectionActionUnverifiedCatalogTests`: **1 passed**;
- catalog validation after `AFW_THM_0012`: user-verified green;
- documentation-key validation: user-verified green;
- Markdown-link validation: user-verified green;
- `git diff --check`: user-verified clean.

The bundled reference catalog contains **17 categories**, **10 code groups**, **5 profiles**, and **37 bundled reference errors**. The project-local authoritative catalog additionally contains twelve thermal definitions through `AFW_THM_0012`.

## Thermal catalog state

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The current thermal contracts run from `AFW_THM_0001` through `AFW_THM_0012`. `AFW_THM_0011` represents confirmed failure of a distinct approved fallback. `AFW_THM_0012` represents a distinct approved fallback that was initiated but whose required result remains unverified.

## Documentation synchronization completed

The English documentation remains synchronized in the project root and topic-based `Docs` folders. Only `en.md` localized documents are maintained manually at this stage.

Current synchronized documentation includes:

- `README.md` and `Readme/en.md`;
- `Docs/Overview/en.md`;
- `Docs/Commands/en.md`;
- `Docs/Known Limitations/en.md`;
- `Docs/Roadmap and Future Work/en.md`;
- `Docs/Getting-Started/en.md`;
- `Docs/FAQ/en.md`;
- `Docs/Testing and CI/en.md`;
- `Docs/Reviewing Catalog Changes/en.md`;
- `Docs/Catalog Author Checklist/en.md`;
- `WhenItFails/Docs/Bootstrap/en.md`;
- `WhenItFails/Docs/Thermal Errors/en.md`.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every user-verified change.
- Prefer one file plus its directly related test, then commit.
- Match command examples to the user's current PowerShell environment.
- Use the user's `to-clipboard` helper whenever a long local output or file content is needed.

## Recommended next step

Continue the runtime/public-API audit from the **872-test** green baseline by completing the malformed-diagnostics pass. Inspect remaining core helpers that extract a dependency issue (`ErrorDescriptorResolver`, `ErrorCatalogInitializer`, runtime forwarding and adjacent composition helpers) for direct first-element dereferences or assumptions about runtime-null issue entries. Add a red-first contract only where a concrete exception or unstable public response is proven; do not mechanically sweep Setter command output helpers.

Prefer one narrow contract with a clear public response shape.

## Last completed change

`ErrorCatalogProviderNullFirstIssueContractTests` is user-verified green: **1 passed**. A failed `IErrorCatalogLoader` response whose mutable `Issues` collection contains a runtime-null first element no longer throws in `ErrorCatalogProvider.GetFirstIssueCode`; the provider now skips null issue entries and falls back to `CatalogLoadFailed` / `Error catalog loading failed.` when no usable diagnostic exists. Production commit: `15d6fee902324b8e8c73504568db03d49bd7cf52`. Red-first test commit: `3f2c5aa53eabf02935c340a2c4d50cb45935a687`.
