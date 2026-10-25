# Descriptor-driven translation provider and dictionary storage editor

**Implemented 2026-10-25, first increment.** The Configurator uses `SettingsDescriptorRegistry.CreateDefault()` from UnderstandMe core. This is not a separate adapter-by-adapter form implementation and does not add a Spectre.Console dependency to core.

## Workflows

While editing a draft, choose **Translation providers** or **Dictionary storages**. The component list supports adding a registration of an available type, modifying one, or returning. Currently registered built-in types are LibreTranslate and FileJson; availability comes from core descriptors.

For a provider, enter a unique registration ID and select a descriptor. The editor initializes its `Options` from the descriptor's typed defaults, allows field-by-field edits based on `SettingsFieldKind`, lets you enable/disable the provider, and lets you remove it after confirmation. For a storage, the editor also exposes source language and dictionaryless `SetValueFromKey`.

`Text`, `HttpEndpoint`, `Integer`, `Boolean`, `Choice` and `SecretReference` fields are interpreted by their descriptors, with their permitted choices and numeric limits enforced by the core validator. Secret references must have the supported `env:NAME` or `secret:NAME` form. They are never displayed in menu values or recycled into plaintext input defaults. The UI does not authenticate any provider or store literal API credentials.

The entire document is held in a `ConfiguratorSession` draft. Its dirty state now compares the complete serialized configuration to the saved baseline rather than checking only two scalar fields; Add/Edit/Delete/Enable/Disable option changes all participate in dirty tracking. Save still uses the existing checked-revision `UnderstandMeSettingsJsonStore` and requires explicit confirmation; invalid configurations cannot be saved, but failed **live connection diagnostics do not contribute** to configuration validation.

## Known limits

Currently bundled core descriptors only, no third-party adapter descriptor registration into the executable, no undo/redo, no granular redacted JSON diff, and no live translation requests. Descriptor labels have manually translated Czech/English UI overrides; descriptor help prose and core validator diagnostic messages remain English. Additional languages and rich provider help/localized validation diagnostics are future work.

See [Live Connection Diagnostics](../Live-Connection-Diagnostics/en.md) for the separate, optional LibreTranslate reachability check.

## CI verification (2026-10-25)

[GitHub Actions #40235904784](https://github.com/afrowaveltd/Toolbox/actions/runs/40235904784): success, 38 Configurator tests, 0 build warnings or errors. Actual terminal/device smoke remains for Debian/Windows/SSH.
