# Localization dictionary regressions

The `DictionaryTextLocalizerTests` class covers dictionaryless key registration and fallback, source/target lookup precedence, placeholder safety, explicit reload, corrupt source fail-closed behavior, target invalidation and parallel source registration.

## Concurrent registration

Three scenarios now protect the file-based store:
1. Twenty-five distinct keys registered in parallel through one localizer.
2. Sixty distinct keys through separate `FileJsonDictionaryStore` instances sharing the same directory.
3. Forty-eight distinct phrases through four independent `DictionaryTextLocalizer` instances sharing that directory.

Every scenario verifies the persisted source dictionary contains all expected keys. Windows CI executes this focused test class in addition to the complete Linux suite. The directory-level OS mutex protects transactions across local participants using the same mutex namespace. Retry protects against transient `IOException`, but permanent I/O errors remain possible.

The regression was reported on a local Windows workstation (24 instead of 25), and the implementation change was verified in [CI #40859339217](https://github.com/afrowaveltd/Toolbox/actions/runs/40859339217).

This is not a test of distributed multi-host shared filesystem coordination; that scenario requires a provider with distributed concurrency semantics.
