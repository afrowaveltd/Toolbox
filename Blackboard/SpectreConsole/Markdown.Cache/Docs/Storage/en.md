# Validated disk cache: first implementation slice

## Contract

- One logical source key maps to an index file whose name is SHA-256 of the UTF-8 key.
- Content is stored under a SHA-256 digest, independent of source name. A complete stream is spooled before validation, and only then is the index replaced.
- Strict UTF-8 decoding (no byte replacement) and terminal control rejection are required for acceptance. This first slice does not accept UTF-16 or UTF-32.
- A failed or canceled write does not replace an already published valid index. An orphan blob may remain if index publication fails.
- Reads independently verify length, digest and Unicode/control safety. A corrupt entry returns a miss, not unverified document bytes.
- The default per-document safety limit is 256 MiB, configurable through the constructor; it is **not** a viewer truncation mechanism. The feature has no global quota/eviction yet.
- Cache lifetime exceeds process lifetime. Caller manages root path, identity separation, expiration and eventual cleaning.

## Safety and follow-up

Do not cache private/authenticated content until caller authorization partitioning, secure root policy and sensitive-data retention are specified. The current implementation does not prohibit symbolic links/reparse points in the cache root; use an isolated user-owned root, never an adversary-controlled shared directory. Cross-process locking, guaranteed durable fsync, and crash/injection tests around the index replacement are future hardening work. Conditional GET, staleness and source identity normalization belong to the network adapter, not to this low-level store.

The next slice should add isolated fake-clock conditional-refresh metadata, concurrent-process protection and fault injection before connecting a real HTTP client. The existing parser and renderer are unchanged.

[README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
