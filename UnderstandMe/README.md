# Afrowave.Toolbox.UnderstandMe

UnderstandMe is a planned, modular localization, globalization, translation, and translation-workflow toolkit for .NET applications. It belongs to [Afrowave Toolbox](https://github.com/afrowaveltd/Toolbox).

> **Status: design / preparation.** The repository currently contains a .NET 10 class-library skeleton. This README describes the intended system, **not implemented features**.

## Purpose

Provide reusable building blocks for software localization, automatic and human translation, document and dictionary processing, and configurable work orchestration. Consumers can use a small subset or assemble a complete translation service. [Afutrans](https://github.com/afrowaveltd/Afutrans) is a planned full-featured host and editor built from these building blocks; it is not a prerequisite for using UnderstandMe.

## Design principles

- One responsibility per module, explicit interfaces, replaceable implementations and no implementation-level circular dependencies.
- A `QueueMaster` core manages five priority queues, job ownership, durable recovery when enabled, and demand-driven dispatch to translators' small FIFO buffers.
- A `Messenger` distributes events and authorized status information; it does **not** assign work.
- Translation Chains compose configurable translation, splitting, assembly, review, and validation steps.
- Standard .NET localization, optional dictionary-less registration, configurable storage and short-lived RealTime translation are first-class use cases.
- AI, machine translation and human editors are optional workers rather than mandatory dependencies.
- Prefer Microsoft/.NET components when appropriate, including `System.Text.Json`, while keeping external formats and translation providers pluggable.
- Core/library compatibility starts at .NET 10; .NET 11-specific features and API surfaces are design topics, not yet implemented.

## Planned areas

| Area | Responsibility |
| --- | --- |
| QueueMaster | Prioritized work management, persistence abstraction, recovery, buffer dispatch |
| DataAccess | Read/write access to files, databases, APIs and other stores |
| TextSource | Understand input formats and generate translation requests |
| Translation/Chains | Interchangeable workers, prompt composition and processing sequences |
| ResultProcessor | Restore protected content, assemble and route outputs |
| Editor | Compare/edit dictionaries, request suggestions, provide human translation |
| Messenger | Local/remote event delivery and monitoring projection |
| Localization | Standard .NET localization, dictionary-less registration, placeholders and caching |

## Documentation

- [Working requirements and agreed principles](Notes.md)
- [Architecture and module boundaries](Docs/Architecture/en.md)
- [Localization and placeholders](Docs/Localization/en.md)
- [QueueMaster, scheduling and reliability](Docs/QueueMaster-and-Reliability/en.md)
- [Translation Chains and backend workers](Docs/Translation-Chains/en.md)
- [Editor, audit and rollback](Docs/Editor-and-Audit/en.md)
- [Afutrans integration and interfaces](Docs/Afutrans-Integration/en.md)
- [AfroLT future lightweight .NET translation service](Docs/AfroLT-Future-Design/en.md)
- [Hierarchical SystemMonitor live-event routing](Docs/SystemMonitor/en.md)
- [Open design questions](Docs/Open-Questions/en.md)
- [Current implementation status and continuation point](IMPLEMENTATION_STATUS.md)

Only English documentation is maintained manually for now; later translations belong in the same topic directories as additional language-coded Markdown files.

## Current repository facts

`UnderstandMe.csproj` currently targets `net10.0` and declares package references to `Afrowave.Toolbox.Essentials 0.2.0` and `Afrowave.Toolbox.WhenItFails 0.1.0`. These are current project-file declarations, **not** the decided future dependency policy. No working translation engine, QueueMaster or integration tests are claimed here.

The current Afutrans codebase is an experimental application skeleton. Its existing web project and shared core target `net11.0`; future integration work is still to be designed.

**Source of truth:** [Toolbox GitHub repository](https://github.com/afrowaveltd/Toolbox).

