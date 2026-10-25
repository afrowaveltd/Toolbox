# Live connectivity diagnostics: LibreTranslate

**2026-10-25 — implemented first opt-in reachability probe.** A provider's settings can be syntactically valid while its network endpoint is offline. We intentionally keep these as **two independent results**.

## Testing behavior

- In the main menu, choose **Test configuration**, select an existing JSON file and inspect the existing offline JSON/schema validation result. The screen then offers **Return to menu** or **Test provider connections (live)**. Live tests run only when explicitly chosen.
- The editor also offers **Test provider connections (live)** against its current **in-memory draft**, including uncommitted provider configuration.
- For every *enabled* LibreTranslate registration, perform one HTTP **GET** to the configured endpoint's `/languages` subpath. Respect optional base paths such as `https://host/translate-api` -> `https://host/translate-api/languages`.
- An HTTP 2xx response with any parseable JSON value counts as **Responding**, without imposing a response DTO/model yet. Non-2xx (including 401, 404 and redirects), malformed or oversized JSON, DNS/connection/timeout failures are shown as a diagnostic failure. Disabled registrations are skipped; other adapters return **Unsupported** rather than attempting speculative HTTP calls.
- The request sends no API key, secret reference or translation text, uses no cookies, does not follow redirects when using the default client, and caps HTTP read size to 1 MiB. A configured request timeout is clamped to 1–30 seconds per provider. External cancellation stops the check.
- The user reads each result until Enter/Esc acknowledgement; the next menu clears prior output.

**A passing probe proves the endpoint answered `GET /languages` with HTTP success and valid JSON.** It does *not* verify translation requests, provider identity, API-key authentication, supported source/target languages, translation accuracy or remote service health beyond that response.

**Failure is always advisory.** It is never inserted into `UnderstandMeSettingsValidator`, never prevents Save, and never modifies the JSON file. An offline schema validation failure still prevents saving invalid syntax/semantics, as before.

## Security and repeatability

The probe is an **explicit network operation to a user-selected endpoint**, including potentially local or private IP addresses. Run only on configurations you trust. It does not store probe results or share them with another service. Diagnostics avoid echoing complete provider URLs, response bodies and exception messages. Endpoint user-info credentials, URI query and fragment are rejected for probes. A later service may add richer, authorized tests with separate permission rules.

Tests inject a fake `HttpMessageHandler` to verify HTTP method/URL, JSON shape tolerance, status failures, no outgoing secret credentials, invalid endpoints, non-blocking failure, oversized response and TUI opt-in flow — all without contacting real servers.

## Planned extensions

Provider-specific authenticated health checks, a real translation smoke request with explicitly chosen test content, remote target discovery, custom user-registered adapter probes, progress reporting for many providers, and richer localized error-code reporting. None is required to save a configuration.

## CI verification (2026-10-25)

[GitHub Actions #40235904784](https://github.com/afrowaveltd/Toolbox/actions/runs/40235904784) completed successfully on the first implementation, with 38 Configurator tests and a clean full solution build. All live network tests used injected fake HTTP handlers; no claims are made for a real remote LibreTranslate installation.
