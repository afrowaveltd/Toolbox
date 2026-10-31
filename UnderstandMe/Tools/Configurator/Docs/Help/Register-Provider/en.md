# Register a translation provider

1. Open or create a configuration and choose **Translation providers**.
2. Choose **Add registration**, select an adapter type (currently LibreTranslate) and enter a unique ID.
3. Edit the server URL and other descriptor-driven options. Keep **Provider enabled** on when you want it to receive translation work.
4. Use **Test provider connections (live)** for an optional connectivity check. A successful response does not prove that real translation will succeed.
5. Review, validate and save the configuration explicitly.

For local LibreTranslate installations, the usual initial endpoint is `http://localhost:5000`. Change it to your actual endpoint. For API keys, enter references such as `env:LIBRE_KEY` or `secret:LIBRE_KEY`; never paste the secret into the configuration.

The connection test currently sends only GET `/languages` and does not send translation text or credentials. Failed checks never block saving. A new registration remains in the in-memory draft until Save.
