# Backpack use cases (non-normative)

## 1. Hardware Revival archive

Portable AJIS package for unsupported devices. Candidate domain objects:

`Device`, `HardwareRevision`, `Research`, `Evidence`, `Implementation`, `Artifact`, `BuildRecipe`, `TestReport`, `DistributionPolicy`.

- A binary original driver may be preserved privately; public distribution depends on third-party redistribution rights.
- Each implementation identifies OS, architecture, hardware IDs, device revisions, compatibility and verification evidence.
- RE knowledge is not the same as validated device behavior. Preserve unverified hypotheses distinctly.
- Clients can request only relevant artifacts; contributors can append findings without replacing entire libraries of drivers.
- Compression is for transfer, not required for native AjisFS reading/editing.
- Tooling must never silently execute supplied binaries.

## 2. Portable user profile with picture

One document with a named binary image. Different adapters bind it to a desktop temp file, browser storage, server object storage or **RAM-only** for display. No portability requirement says it must be written to a local directory. `Options` controls allowed locations and actions. The parser itself performs no file writes.

## 3. Agent shared notebook

A `#agent` directive supplies typed metadata/workspaces. Agents keep individual notebooks and a shared workspace in one document. Attach raw bytes, findings, test logs, hypotheses and handoffs. Approval of public project changes is separate from writing a personal note. Cross-model continuity is possible because the state survives the original agent process.

## 4. Database-first-like scaffolding

Inspect an unknown AJIS file, infer a C# type graph, emit C# classes/records plus explicit mapping for unknown fields, references and directives. Inferred optionality or type unions must be indicated as hypotheses rather than guaranteed schema facts.

## 5. FixAjis / normalization

Take imperfect/tolerant input, stage bounded data, separate payload and annotations, reorder pointer declarations before the body, re-emit valid forward-streaming AJIS and validate by strict re-read. Preserve recoverable unknown syntax rather than guessing executable semantics.

**Do not** use these examples as the normative grammar or claim the workflows already exist.
