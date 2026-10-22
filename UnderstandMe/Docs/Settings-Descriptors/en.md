# Generic component settings descriptors

Status: initial API implemented on 2026-10-22. TUI renderer, provider execution adapters and schema migration remain future work.

## Purpose

A translation provider or storage adapter describes its editable options once. The same typed metadata can drive an SSH/TUI configurator, JSON settings validation and future GUI. Neither the core library nor this descriptor API depends on Spectre.Console.

Implemented in `UnderstandMe/Settings/SettingsDescriptorRegistry.cs`:

- `SettingsComponentKind` distinguishes translation backends and localization storages.
- `SettingsFieldKind` identifies text, HTTP(S) endpoint, integer, boolean, choice and secure secret-reference inputs.
- `SettingsFieldDescriptor` includes stable JSON key, display label, contextual help, optional typed JSON default, required flag, numeric bounds, choices and a simple choice-based conditional requirement.
- `SettingsComponentDescriptor` groups field descriptors for an adapter type and display name.
- `SettingsDescriptorRegistry` supports registration, listing, lookup and pure options validation (per-field JSON paths), plus `CreateDefault()` for initial LibreTranslate and FileJson schemas.

`TranslationBackendSettings` and `LocalizationStorageSettings` carry `Options: Dictionary<string, JsonElement>`; storage also has a `Type` (defaults to `FileJson`). The runtime-independent validator checks component options through the catalog. Multiple registrations may share one adapter type while retaining distinct IDs/options. Unknown adapter types and unknown or incorrectly cased options fail closed unless the descriptor is registered; a plugin host must supply its expanded registry to `UnderstandMeSettingsValidator.Validate` and `UnderstandMeSettingsJsonStore.Load/Save`.

## Illustrative configured JSON

```json
{
  "SchemaVersion": 1,
  "Localization": {
    "DefaultSourceLanguage": "en",
    "Storages": [
      {
        "Id": "main",
        "Type": "FileJson",
        "SourceLanguage": "en",
        "SetValueFromKey": true,
        "Options": { "Directory": "Locales", "ReadOnly": false }
      }
    ]
  },
  "Translation": {
    "Providers": [
      {
        "Id": "libre-local",
        "Type": "LibreTranslate",
        "Enabled": true,
        "Options": {
          "Endpoint": "http://localhost:5000",
          "AuthenticationMode": "None",
          "RequestTimeoutSeconds": 10,
          "RequestThrottleMs": 80,
          "MaxRetryAttempts": 3
        }
      }
    ],
    "Queues": { "DefaultWorkerBufferCapacity": 5 }
  }
}
```

LibreTranslate metadata also describes per-instance `RetryBackoffMs` (initial delay), `OnFailure` (`Fail` or `Fallback`) and `MaxConcurrentRequests`. These are **configuration descriptions, not running retry/fallback logic yet**. Backoff is only for errors classified as retryable; the future orchestrator enforces global resource limits and allowed provider fallbacks.

Fields with descriptor defaults are optional, so a missing `Endpoint` currently means the **metadata-defined default** (for displaying choices); no running provider consumes that default yet. A future adapter activation layer must resolve defaults and validate adapter-specific semantics. Do not interpret this as an already-working LibreTranslate connector.

## Security and extension rules

- Use `ApiKeyReference: "env:LIBRE_KEY"` or `"secret:entry-name"`, never literal access tokens. `AuthenticationMode: "ApiKey"` makes that field mandatory.
- HTTP endpoint fields reject URLs with embedded user/password information and non-HTTP schemes. Unrecognized options are rejected; validators do not include secret values in messages.
- Each future adapter supplies a descriptor and should own its strongly typed internal options mapping and semantic validation. Dynamic manifests are not executable code and cannot be accepted from untrusted remote servers without a trust model.
- Descriptor descriptions remain English first and may later be localized by the same offline fallback mechanism as the Configurator.
- The initial registry is intended to be populated at startup, not concurrently modified while requests validate. Runtime adapters must still reject unregistered or disallowed providers: a descriptor alone grants no access or execution permission.
- Host JSON settings serialization is `System.Text.Json` and uses the same strict field-path validation when provided with the same registry. The `SchemaVersion` remains 1 because the new fields are optional and legacy settings documents remain valid.

## Next work

Build a generic RAM-draft form renderer, test keyboard navigation and `Ctrl+V`/ `Ctrl+S` workflows; integrate component-specific strongly typed options adapters and safe defaults resolution. Add per-provider retry policies, authenticated connectivity diagnostics and type-specific cross-field validators. Implement and test a safe extension discovery/trust policy, avoiding leaking secret references or raw credentials in logs.

See [Central Configuration](../Central-Configuration/en.md) and [Configurator TUI](../Configurator-TUI/en.md).
