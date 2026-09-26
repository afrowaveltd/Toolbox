# Implementation status

Last updated: 2026-09-25

- Added PowerShell API-comparison workflow with two independently built temporary net10.0 consumers and shared reflection-inspector source.
- The package consumer requests the exact NuGet `[0.1.0]` version. That package was never published; it is a maintainer-held local Windows artifact. Optional `-Feed` supplies the artifact directory/package source explicitly for reproducible comparison on another machine.
- Writes Markdown comparison containing both loaded DLL paths, SHA-256 hashes, public signature census and published-only/source-only differences. This is not a complete ABI, nullability, JSON or runtime-compatibility checker.
- No production code, public visibility or package version changed.

Verification: maintainer successfully ran both independent .NET 10 consumers and obtained a report on 2026-09-25: **611 source API entries, 611 package-consumer entries, 0 missing and 0 added**. The DLL hashes differ, so byte identity is not established. Package reference was pinned to [0.1.0], but configured sources/cache were used without an explicit feed override; original publishing-feed provenance remains unverified. Last confirmed library suite: **1241/1241 GREEN, zero warnings**.
