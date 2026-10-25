using System.Net;
using System.Text.Json;
using Afrowave.Toolbox.UnderstandMe.Settings;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>Possible outcomes of a deliberate, read-only provider reachability check.</summary>
public enum ConnectivityStatus
{
    /// <summary>The provider answered GET /languages with a 2xx status and parseable JSON.</summary>
    Responding,
    /// <summary>No probe exists yet for this registered adapter.</summary>
    Unsupported,
    /// <summary>The configured provider was disabled and was not contacted.</summary>
    Disabled,
    /// <summary>Endpoint options were missing or malformed.</summary>
    InvalidConfiguration,
    /// <summary>An HTTP response arrived, but not with the expected status or JSON payload.</summary>
    UnexpectedResponse,
    /// <summary>The network call failed or timed out.</summary>
    Unreachable,
    /// <summary>The caller stopped the check.</summary>
    Cancelled
}

/// <summary>One bounded provider diagnostic, deliberately separate from schema validation.</summary>
public sealed record ProviderConnectivityResult(string Id, string Type, ConnectivityStatus Status, string Detail)
{
    /// <summary>Diagnostic success only; it does not verify translation operations or credentials.</summary>
    public bool IsResponding => Status == ConnectivityStatus.Responding;
}

/// <summary>
/// An explicit opt-in, nonblocking HTTP reachability check for built-in LibreTranslate.
/// Never sends a translation request, provider API keys, or user data.
/// </summary>
public sealed class ConfiguratorConnectivityProbe : IDisposable
{
    private const int MaxJsonBytes = 1024 * 1024;
    private readonly HttpClient _client;
    private readonly bool _ownsClient;

    /// <summary>Optionally inject an HTTP client for deterministic offline tests.</summary>
    public ConfiguratorConnectivityProbe(HttpClient? client = null)
    {
        _ownsClient = client is null;
        _client = client ?? new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false
        });
    }

    /// <summary>
    /// Check every registered provider in turn; invalid and unsupported registrations
    /// produce diagnostic results, never a fatal save/validation error.
    /// </summary>
    public async Task<IReadOnlyList<ProviderConnectivityResult>> CheckAsync(
        UnderstandMeSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var results = new List<ProviderConnectivityResult>();
        foreach (var provider in settings.Translation?.Providers ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(await CheckProviderAsync(provider, cancellationToken));
        }
        return results;
    }

    /// <summary>Probe a provider once, using an HTTP GET with a bounded timeout and JSON response.</summary>
    public async Task<ProviderConnectivityResult> CheckProviderAsync(
        TranslationBackendSettings provider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (!provider.Enabled)
            return new(provider.Id, provider.Type, ConnectivityStatus.Disabled, "Provider is disabled.");
        if (!string.Equals(provider.Type, "LibreTranslate", StringComparison.OrdinalIgnoreCase))
            return new(provider.Id, provider.Type, ConnectivityStatus.Unsupported, "No live check exists for this adapter.");

        var options = provider.Options;
        if (options is null || !options.TryGetValue("Endpoint", out var endpointOption) ||
            endpointOption.ValueKind != JsonValueKind.String ||
            !Uri.TryCreate(endpointOption.GetString(), UriKind.Absolute, out var baseUri) ||
            baseUri.Scheme is not ("http" or "https") || !string.IsNullOrEmpty(baseUri.UserInfo) ||
            !string.IsNullOrEmpty(baseUri.Query) || !string.IsNullOrEmpty(baseUri.Fragment))
            return new(provider.Id, provider.Type, ConnectivityStatus.InvalidConfiguration,
                "A valid HTTP(S) endpoint without embedded credentials, query or fragment is required.");

        // Preserve an optional installation base path, e.g. /translate-api -> /translate-api/languages.
        var uri = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/languages", UriKind.Absolute);
        var configuredTimeout = 10;
        if (options.TryGetValue("RequestTimeoutSeconds", out var timeoutValue) &&
            timeoutValue.ValueKind == JsonValueKind.Number && timeoutValue.TryGetInt32(out var parsed))
            configuredTimeout = parsed;
        var seconds = Math.Clamp(configuredTimeout, 1, 30);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(seconds));

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await _client.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (response.StatusCode is < HttpStatusCode.OK or >= HttpStatusCode.MultipleChoices)
                return new(provider.Id, provider.Type, ConnectivityStatus.UnexpectedResponse,
                    "GET /languages returned HTTP " + (int)response.StatusCode + ".");
            if (response.Content.Headers.ContentLength > MaxJsonBytes)
                return new(provider.Id, provider.Type, ConnectivityStatus.UnexpectedResponse,
                    "GET /languages returned too much data.");

            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var buffer = new MemoryStream();
            var block = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(block, timeout.Token)) != 0)
            {
                if (buffer.Length + count > MaxJsonBytes)
                    return new(provider.Id, provider.Type, ConnectivityStatus.UnexpectedResponse,
                        "GET /languages returned too much data.");
                buffer.Write(block, 0, count);
            }

            try
            {
                using var _ = JsonDocument.Parse(buffer.ToArray(), new JsonDocumentOptions { MaxDepth = 32 });
                return new(provider.Id, provider.Type, ConnectivityStatus.Responding,
                    "GET /languages returned HTTP " + (int)response.StatusCode + " and valid JSON.");
            }
            catch (JsonException)
            {
                return new(provider.Id, provider.Type, ConnectivityStatus.UnexpectedResponse,
                    "GET /languages responded, but the body is not valid JSON.");
            }
        }
        catch (OperationCanceledException)
        {
            return new(provider.Id, provider.Type,
                cancellationToken.IsCancellationRequested ? ConnectivityStatus.Cancelled : ConnectivityStatus.Unreachable,
                cancellationToken.IsCancellationRequested ? "Check cancelled." : "Request timed out.");
        }
        catch (HttpRequestException)
        {
            return new(provider.Id, provider.Type, ConnectivityStatus.Unreachable, "HTTP connection failed.");
        }
        catch (IOException)
        {
            return new(provider.Id, provider.Type, ConnectivityStatus.Unreachable, "HTTP response could not be read.");
        }
    }

    /// <summary>Dispose an internally created HTTP client, without disposing injected clients.</summary>
    public void Dispose()
    {
        if (_ownsClient)
            _client.Dispose();
    }
}
