# Additive API entries on original concrete types

Status: **2026-09-25 observed API census; three new focused tests pending local verification**.

## Confirmed source-only changes

The maintainer supplied the complete excerpt of the API entries added to types that already existed in the requested NuGet 0.1.0 package. The types are exactly `ErrorCatalogContextStore` and `ErrorCatalogRuntime`. Of **219 source-only API census entries**, **14** belong to these two original types (**7 additional implemented-interface entries and 7 added public method entries**). The remaining **205 census entries** are associated with the **38 source-only exported types**; census entries include `kind`, interface declarations and methods/properties and should not be described as 205 new methods.

| Original concrete type | Added interface entries | Added public method entries | Total |
| --- | ---: | ---: | ---: |
| `ErrorCatalogContextStore` | 2 | 2 | 4 |
| `ErrorCatalogRuntime` | 5 | 5 | 10 |
| **Total** | **7** | **7** | **14** |

### Full list of 14 additions

- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogContextStore` — interface: `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogContextPublicationReader`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogContextStore` — interface: `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogContextPublisher`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogContextStore` — method: `Response<ErrorCatalogContextPublication> GetCurrentPublication()`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogContextStore` — method: `ErrorCatalogContextPublication Publish(ErrorCatalogContext context)`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogRuntime` — interface: `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntimeActivationReader`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogRuntime` — interface: `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntimeCombinedObservationReader`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogRuntime` — interface: `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntimeFullObservationReader`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogRuntime` — interface: `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntimePublicationReader`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogRuntime` — interface: `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntimeSupportingObservationReader`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogRuntime` — method: `Response<ErrorCatalogActivationStatusSnapshot> GetCompletedActivation()`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogRuntime` — method: `Response<ErrorCatalogCompletedCombinedSnapshot> GetCompletedCombinedSnapshot()`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogRuntime` — method: `Response<ErrorCatalogCompletedFullSnapshot> GetCompletedFullSnapshot()`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogRuntime` — method: `Response<ErrorCatalogCompletedSupportingCatalogsSnapshot> GetCompletedSupportingCatalogsSnapshot()`
- `Afrowave.Toolbox.WhenItFails.Services.ErrorCatalogRuntime` — method: `Response<ErrorCatalogContextPublication> GetCurrentPublication()`

## Compatibility interpretation

`ErrorCatalogContextStore` still offers its public parameterless constructor and the original `IErrorCatalogContextStore` interface (`IsInitialized`, `Current`, `GetCurrent()`, `Set(context)`). `IErrorCatalogContextPublicationReader` and `IErrorCatalogContextPublisher` are additive optional interfaces; their records retain a **live mutable context**, not a detached application view.

`ErrorCatalogRuntime` retains its six-dependency constructor and the original nine-method `IErrorCatalogRuntime`. Its five added methods expose optional publication and completed-activation observations. Custom runtime implementations are not required to implement these optional reader interfaces.

The comparer found **0 package-only types** and **0 package-only API census entries**, which supports that none of the inspected original public signatures disappeared. It does **not** prove unchanged behavior, complete ABI or compile-time source compatibility, nullability or JSON compatibility, or verified nuget.org provenance of the package restored from configured sources/cache.

## Regression tests and remaining work

`LegacyConcretePublicationExpansionContractTests` adds **three** focused cases covering the original store constructor/interface, its two optional reader/publisher methods, and the runtime's six-dependency constructor and five added methods without modifying `IErrorCatalogRuntime`. These are source-level contracts, not a binary test against an old consumer.

The previous three new `ExportedAssemblyInventoryTests` cases still require explicit test-result confirmation. Last maintainer-confirmed full suite: **1439/1439 GREEN**. The current total with both pending groups is **1445 tests expected** (1439 + 3 inventory + 3 legacy-class contracts), **not yet verified**. The complete current exported assembly inventory Markdown has not been supplied; no 1.0 API freeze is declared.
