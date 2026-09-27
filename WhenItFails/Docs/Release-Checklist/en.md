# WhenItFails 1.0 release checklist

Status: **final pre-publication checklist** for the first published WhenItFails package. Runtime/API verification is GREEN; only a clean, immutable final artifact/tag/publication cycle remains.
The current complete-suite baseline is maintainer-confirmed **1475/1475 GREEN**.
The exact exported-type manifest is GREEN for the frozen **126 exported types / 698
API entries** candidate surface. Commit `6afd3f6f` internalized 17 default
orchestration implementations and added the visibility contract; commit `147b9631`
internalized five additional low-level helpers without changing the test count.

The pre-cleanup Linux release-readiness checkpoint is maintainer-confirmed **PASS**.
The exported surface cleanup is complete. The exact stable `1.0.0` candidate
package smoke has been rerun successfully after the frozen 126-type manifest was
accepted. The verified gate covers:

- Essentials tests completed without errors;
- the complete WhenItFails suite is **1475/1475 GREEN**;
- Essentials 0.2.0 and the exact stable WhenItFails 1.0.0 candidate were packed;
- package contents were validated;
- a clean external consumer restored, built and executed from an isolated local
  feed/global-packages directory;
- Markdown-link validation completed without errors.
- the updated external consumer directly verified `UnsupportedSchemaVersion` across all five packaged validators.

The historical local 0.1.0 package was never published and has no external consumers.
It is retained only as optional diagnostic evidence; **it is not a release gate for
the first stable 1.0 package**.

## 1. Source and repository state

Before a release candidate:

- pull the current GitHub `master`;
- require a clean working tree;
- confirm `WhenItFails/IMPLEMENTATION_STATUS.md` reflects the latest verified state;
- review the candidate [1.0 public API policy](../Public-API-1.0-Policy/en.md);
- do not create a replacement 0.1.0 package from current source.

## 2. Library verification

Run the complete library suite:

```bash
dotnet test WhenItFails.Tests/WhenItFails.Tests.csproj -c Release
```

The current expected and confirmed count is **1475/1475 GREEN**.

SDK support-policy messages such as `NETSDK1057` are informational and must not be
recorded as Toolbox compiler warnings.

## 3. Documentation validation

Validate local Markdown links from the repository root:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

When catalog documentation keys changed, also run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
```

Review the root `WhenItFails/README.md` and all changed
`WhenItFails/Docs/<topic>/en.md` files against current runtime behavior.

## 4. Current candidate package smoke

The current package can be tested without the old 0.1.0 artifact.

Run:

```bash
pwsh ./Toolroom/WhenItFails/Release/Test-CandidatePackage.ps1
```

The script:

1. packs the current Essentials package into an isolated local feed and verifies its project README, license, DLL, XML documentation and package icon;
2. packs WhenItFails as temporary `1.0.0-rc.1` by default without modifying the
   project version in Git; the override is scoped to WhenItFails and the
   `ProjectReference` removes parent version properties before building Essentials;
3. verifies required WhenItFails package entries and requires **every** Essentials
   dependency node in the generated nuspec to be exactly `0.2.0`, the minimum
   supported dependency baseline;
4. verifies the packed `README.md` is byte-identical to
   `WhenItFails/README.md`;
5. restores a new external .NET 10 consumer from the isolated feed;
6. validates DI registration with `ValidateOnBuild` and `ValidateScopes`;
7. verifies the packaged assembly is version `1.0.0.0` and exports exactly 126 types;
8. verifies that all five packaged catalog validators reject `schemaVersion = "2.0"`
   with `UnsupportedSchemaVersion` at `schemaVersion`;
9. performs strict project initialization and verifies all five project catalog files;
10. resolves the canonical `UNKNOWNERROR` descriptor;
11. explicitly resets to bundled defaults and verifies runtime status;
12. verifies package ID/version, project URL, repository URL/type/commit metadata,
    release notes and README/license metadata;
13. verifies both Essentials/WhenItFails symbol packages and requires the WhenItFails
    `.snupkg` to contain `lib/net10.0/Afrowave.Toolbox.WhenItFails.pdb`; the smoke
    report records SHA-256 for the `.nupkg`, `.snupkg`, DLL and PDB;
14. writes a SHA-256 report.

A custom candidate version can be supplied without changing source metadata:

```bash
pwsh ./Toolroom/WhenItFails/Release/Test-CandidatePackage.ps1 \
  -PackageVersion 1.0.0-rc.2
```

The smoke uses a workspace-local `NUGET_PACKAGES` directory and disables the
HTTP cache for the external consumer restore. Reusing the same temporary
candidate version must therefore never reuse a previous run from the user's
global NuGet cache. On failure the temporary workspace is retained
automatically so the generated nuspec, local feed, isolated package cache and
external consumer can be inspected without rerunning.

This smoke verifies the **new candidate package only**. It never substitutes for the
old-package compatibility comparison.

## 5. Package README selection

Projects that own a package README add it explicitly in their project file with
a duplicate-safe `None Remove` + `None Include` pack item. Shared packaging
uses the repository-root README only as a fallback when a project has no local
README.

This explicit rule was added after Linux NuGet `NU5039` showed that a
project-local `None Update` in `Directory.Build.props` did not reliably become
a package item.

For WhenItFails the package must contain the exact current
`WhenItFails/README.md`.

The package must also contain at least:

```text
README.md
LICENSE.txt
lib/net10.0/Afrowave.Toolbox.WhenItFails.dll
lib/net10.0/Afrowave.Toolbox.WhenItFails.xml
```

## 6. Historical 0.1.0 diagnostics — optional

The maintainer-held 0.1.0 package may still be used to compare historical API
surfaces or rerun the precompiled-consumer binary smoke modes. Because that package
was never published or used externally, differences from it do not block 1.0.

Do not fabricate a replacement 0.1.0 artifact and do not describe it as a published
release. Historical comparisons remain useful archaeology, not a compatibility
promise.

## 7. Public API freeze

Before the first stable publication:

- verify commit `6afd3f6f` and any later visibility cleanup with the complete suite;
- rerun the source-only comparer and exported-assembly inventory after each deliberate
  visibility batch and record the actual type/member counts;
- internalize any additional default orchestration implementations that are not
  intentional consumer or Toolbox-tooling contracts;
- after the final cleanup batch, record one stable 1.0 source census;
- pin the exact exported-type set in a contract test and require it to stay GREEN;
- review every remaining exported type against the final public API policy;
- ensure the nine-method `IErrorCatalogRuntime` core surface remains intentional;
- ensure enum numeric values, nullability, generic constraints and init/set semantics
  are intentional;
- require the complete test/package/documentation gates to be GREEN.

## 8. Version and package metadata

The public-surface gate is accepted and current source metadata is now:

- package/project version: **1.0.0**;
- assembly version expected from the package: **1.0.0.0**;
- Essentials minimum supported baseline: **0.2.0**;
- release notes: first stable release wording;
- repository URL, license, description and tags remain release inputs.

Candidate versions may still be tested with the script's `-PackageVersion` override.
The final verification must also run once with `-PackageVersion 1.0.0` so the exact
stable package that would be published is inspected.

## 9. Final package verification — accepted

The clean final gate passed on source commit
`6face1742de35fbde665a9075c2e06649c171d7d`.

Verified release record:

- complete WhenItFails suite: **1475/1475 GREEN**;
- Markdown links: **46 files / 431 links / 0 broken**;
- exact stable `1.0.0` package smoke: **PASS**;
- isolated external consumer restore/build/execute: **PASS**;
- assembly version: **1.0.0.0**;
- exported types: **126**;
- package repository commit metadata: `6face1742de35fbde665a9075c2e06649c171d7d`;
- `.nupkg` SHA-256: `FF33822150E18C4A5E3410B74888797354D345BBF9C0664F4B8A6CDB93E974EA`;
- `.snupkg` SHA-256: `4410D1346019258391F83A8AB193EAA09A1759BFDF7DFE7887414266AB90B3C7`;
- packaged DLL SHA-256: `01AEDDB2CB8CD9C1C89CA6D72CE3B6E3665A827495C183B25FDE4CF1353FA4CE`;
- packaged PDB SHA-256: `F79859D6E876F61867E7ED8374B91A67F5A8B2DF0EEC448F6E60A85D7DC5378B`;
- Essentials dependency: `0.2.0` minimum baseline;
- README, license, XML documentation, symbol package and net10.0 PDB: present.

The previously retained DIRTY-worktree candidate is diagnostic only. The artifact
identified above is the immutable first-publication artifact. **Do not rebuild another
package under version `1.0.0` for publication.** Historical 0.1.0 comparer/binary-smoke
results remain optional diagnostic archaeology and are not a first-release gate.

## 10. First publication

WhenItFails has **not previously been published**.

Before the first push to a package feed:

- verify the destination feed/account intentionally;
- inspect the final package from the exact file that will be uploaded;
- keep the final package and hashes as release artifacts;
- tag the corresponding Git commit;
- record publication details in `IMPLEMENTATION_STATUS.md`;
- never rebuild a different package under the same published version.

A successful upload is not itself a release verification. The uploaded package must
still be restorable by package ID/version from a clean external consumer.
