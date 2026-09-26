# WhenItFails 1.0 release checklist

Status: **pre-release working checklist** for the first published WhenItFails package.
The current source baseline is maintainer-confirmed **1472/1472 GREEN**.

The historical 0.1.0 package was never published. It exists only as the maintainer's
original local Windows reference artifact and is used strictly for compatibility
comparison.

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

The currently expected count is **1472/1472 GREEN** until a deliberate test addition
changes it.

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
7. performs strict project initialization and verifies all five project catalog files;
8. resolves the canonical `UNKNOWNERROR` descriptor;
9. explicitly resets to bundled defaults and verifies runtime status;
10. writes a SHA-256 report.

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

## 6. 0.1.0 compatibility gate — requires original Windows artifact

This is the only current release-readiness item that cannot be reproduced on the
maintainer's Linux machine until the original 0.1.0 `.nupkg` is available.

Copy or expose the **original** artifact to a directory and run:

```powershell
pwsh ./Toolroom/WhenItFails/PublicApiComparer/Compare-PublicApi.ps1 \
  -Feed "<directory-containing-original-0.1.0-nupkg>" \
  -ReportPath "<report-path>"
```

Also rerun the precompiled-consumer binary smoke modes against that same artifact.

Do not:

- pack current source as version 0.1.0;
- use a rebuilt DLL as the reference package;
- describe 0.1.0 as previously published.

The compatibility gate should require:

- zero unexplained package-only exported types;
- zero unexplained package-only API census entries;
- review of all source-only additions;
- PASS for the targeted precompiled consumer smoke scenarios.

## 7. Public API freeze

After the exact 0.1.0 comparison is refreshed:

- update the exported type/member counts and artifact hashes;
- update the 38-type classification if the current source added or removed exports;
- convert the candidate [1.0 public API policy](../Public-API-1.0-Policy/en.md) into the
  final release policy;
- ensure the nine-method `IErrorCatalogRuntime` core surface remains intentional;
- ensure enum numeric values, nullability, generic constraints and init/set semantics
  have no unexplained changes;
- review every package-only comparer result individually before release.

## 8. Version and package metadata

Only after the compatibility/public-API gate is accepted:

- change `WhenItFails/WhenItFails.csproj` from the development version to the
  intended release version;
- add/update package release notes;
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
