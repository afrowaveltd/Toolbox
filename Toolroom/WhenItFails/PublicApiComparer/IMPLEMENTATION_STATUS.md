# Implementation status

Last updated: 2026-09-25

- Added PowerShell API-comparison workflow with two independently built temporary net10.0 consumers and shared reflection-inspector source.
- The package consumer requests the exact NuGet `[0.1.0]` version; optional `-Feed` specifies a published feed. A cached restore without feed provenance cannot establish official NuGet.org publication.
- Writes Markdown comparison containing both loaded DLL paths, SHA-256 hashes, public signature census and published-only/source-only differences. This is not a complete ABI, nullability, JSON or runtime-compatibility checker.
- No production code, public visibility or package version changed.

Verification pending: run script on a .NET 10 machine with access to the actual published 0.1.0 feed, review report and update this status together with WhenItFails/IMPLEMENTATION_STATUS.md. Last confirmed library suite: **1241/1241 GREEN, zero warnings**.
