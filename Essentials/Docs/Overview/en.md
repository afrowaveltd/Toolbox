# Essentials overview

`Afrowave.Toolbox.Essentials` contains the common low-level contracts used by
the rest of the Toolbox family.

The project currently targets `net10.0` and contains the following major
areas:

- Results: `Result`, `Response` and typed variants.
- Diagnostics: diagnostic information, hints, locations and spans.
- Metadata: `MetadataBag`, factory helpers and JSON conversion.
- Value objects: normalized culture/profile/provider names.
- Guards, enums, interfaces and general extensions.

The package is intentionally dependency-light and should remain suitable as a
foundation for higher-level Toolbox projects such as WhenItFails.

See [Packaging](../Packaging/en.md) for package-specific build rules.
