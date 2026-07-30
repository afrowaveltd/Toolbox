# Implementation status

Last updated: 2026-07-30

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the WhenItFails JSON catalog workspace under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **5,029 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

## Runtime/public-API verification

The latest user-verified Setter test run remains the source anchor for the complete Setter suite.

Recently verified focused contracts:

- `ErrorCatalogContextProviderNullResponseTaskResultTests`: **5 green**.
- `ErrorCatalogContextProviderFailureFallbackTests`: **7 green**.
- `ErrorCatalogContextProviderCategorySourceMessageFallbackTests`: **2 green**.
- `ErrorCatalogContextProviderCodeGroupSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderOwnerSourceEnvelopeTests`: **1 user-verified green**.
- Current `ErrorCatalogContextProviderProfileSourceEnvelopeTests`: **1 test pending verification**.

The audit currently protects provider ordering and paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallback details, source-message preservation, source-code preservation, first-issue selection, and later-issue suppression.

The new profile source-envelope contract requires:

- successful error, category, code-group, and owner providers;
- a profile failure with `ResultStatus.Invalid`;
- preservation of the source response message;
- preservation of the first source issue code;
- exactly one output issue;
- suppression of later source issues;
- no context construction.

Do not invent automatic `DefaultMappings` consumption.

## Focused verification

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderProfileSourceEnvelopeTests
```

Expected result: **1 green test**.

Primary Setter verification command:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Before committing catalog changes, also run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
git diff --check
```

## Documentation status

High-value Setter documentation synchronization is complete. Maintain `README.md`, `Readme/en.md`, and the existing English documents under `Docs/<topic>/en.md` in line with actual behavior.

Next documentation target: keep this file synchronized while the runtime/public-API audit continues; no separate documentation expansion is currently required.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, backup-retention cleanup, complete localization lifecycle management, remote catalog synchronization, package publishing, a GUI or interactive TUI, complete source-code dependency discovery, or a full security audit and semantic duplicate detector.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.

## Recommended next step

First verify `ErrorCatalogContextProviderProfileSourceEnvelopeTests`; the expected count is **1 green test**.

If green, the full-source failure-envelope sequence is complete across error, category, code-group, owner, and profile boundaries. Inspect exactly one adjacent contract outside this sequence.

## Last completed change

The eighty-second runtime/public-API audit slice adds `ErrorCatalogContextProviderProfileSourceEnvelopeTests`. After valid error, category, code-group, and owner responses, the profile provider returns `ResultStatus.Invalid`, a non-empty response message, and two source issues. The context provider must preserve the status, response message, and first issue code; emit one issue; discard the later issue; and return no context.

Commit:

```text
072892ad08f99b070c10c1609b7dc15328437e4b
Protect full profile failure source envelope
```
