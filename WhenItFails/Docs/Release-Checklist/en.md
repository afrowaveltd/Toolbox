# WhenItFails 1.0 release checklist

Status: **pre-release working checklist** for the first published WhenItFails package.
The last confirmed complete-suite baseline is **1474/1474 GREEN** after both pre-1.0
visibility cleanup batches. Commit `b8336c33` adds one exact exported-type manifest
contract for the measured 126-type candidate surface, so the expected next complete
suite is **1475/1475 GREEN**. Commit `6afd3f6f` internalized 17 default
orchestration implementations and added the visibility contract; commit `147b9631`
internalized five additional low-level helpers without changing the test count.

The pre-cleanup Linux release-readiness checkpoint is maintainer-confirmed **PASS**.
Because the exported surface changed in commit `6afd3f6f`, rerun the candidate
package smoke after the new suite is GREEN. The previous PASS covered:

- Essentials tests completed without errors;
- the complete WhenItFails suite is **1473/1473 GREEN**;
- Essentials 0.2.0 and WhenItFails 1.0.0-rc.1 were packed;
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

The current expected count is **1475/1475 GREEN** after the exported-type manifest
contract. The underlying cleanup behavior was already confirmed at 1474/1474.

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
3. verifies required WhenItFails package entries and confirms **every** Essentials
   dependency node in the generated nuspec remains on the 0.2.0 line;
4. verifies the packed `README.md` is byte-identical to
   `WhenItFails/README.md`;
5. restores a new external .NET 10 consumer from the isolated feed;
6. validates DI registration with `ValidateOnBuild` and `ValidateScopes`;
7. verifies that all five packaged catalog validators reject `schemaVersion = "2.0"`
   with `UnsupportedSchemaVersion` at `schemaVersion`;
8. performs strict project initialization and verifies all five project catalog files;
9. resolves the canonical `UNKNOWNERROR` descriptor;
10. explicitly resets to bundled defaults and verifies runtime status;
11. verifies package ID/version, project URL, repository metadata, release notes,
    README/license metadata, and both Essentials/WhenItFails symbol packages;
12. writes a SHA-256 report.

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

Only after the final public-surface cleanup and verification gate is accepted:

- change `WhenItFails/WhenItFails.csproj` from the development version to the
  intended release version;
- replace the current pre-release package notes with the final release notes;
- verify repository URL, license, package description and tags;
- decide the exact compatible Essentials package version;
- rebuild from a clean checkout.

Do not bump the committed release version merely to perform a local package smoke;
use the script's `-PackageVersion` override instead.

## 9. Final package verification

Create the final package into a clean release directory and record:

- `.nupkg` SHA-256;
- `.snupkg` SHA-256 when produced;
- contained DLL SHA-256;
- assembly/package version;
- target framework;
- dependency versions;
- README and license presence;
- complete test result;
- candidate package smoke result;
- compatibility comparer result;
- precompiled consumer smoke results.

Restore and execute at least one completely new external consumer using only the
final local release feed plus public Microsoft dependencies.

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
