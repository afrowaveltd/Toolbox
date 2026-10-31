# Test provider connections

The Configurator provides two distinct checks:

- **Offline validation** checks JSON syntax, registered adapter options and settings constraints. It makes no network request.
- **Test provider connections (live)** is optional and checks enabled LibreTranslate endpoints by sending GET `/languages`. An HTTP 2xx response containing valid JSON counts as **responding**, even if actual translation or authentication would fail.

Disabled providers are skipped. Connection errors are reported as diagnostics and **never prevent saving**. No actual text is translated, and no API keys are sent by this diagnostic.

The response time of `/languages` is not a measurement of translation speed. Future Afutrans routing will need measurements from real requests in the running deployment.
