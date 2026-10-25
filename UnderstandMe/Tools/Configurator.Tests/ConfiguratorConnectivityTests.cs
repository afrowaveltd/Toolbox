using System.Net;
using System.Text;
using System.Text.Json;
using Afrowave.Toolbox.UnderstandMe.Configurator;
using Afrowave.Toolbox.UnderstandMe.Settings;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Configurator.Tests;

public sealed class ConfiguratorConnectivityTests
{
    [Fact]
    public async Task LibreTranslate_languages_2xx_json_is_a_reachable_endpoint()
    {
        var requests = new List<(HttpMethod Method, Uri? Uri, bool Authorized)>();
        var handler = new FakeHandler(request =>
        {
            requests.Add((request.Method, request.RequestUri, request.Headers.Authorization is not null));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"code\":\"en\"}]", Encoding.UTF8, "application/json")
            };
        });
        using var client = new HttpClient(handler);
        using var probe = new ConfiguratorConnectivityProbe(client);
        var provider = LibreProvider("one", "https://translator.example.test/base", 4);
        provider.Options["ApiKeyReference"] = JsonSerializer.SerializeToElement("env:TOP_SECRET");

        var result = await probe.CheckProviderAsync(provider);

        Assert.Equal(ConnectivityStatus.Responding, result.Status);
        Assert.True(result.IsResponding);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://translator.example.test/base/languages", requests[0].Uri!.AbsoluteUri);
        Assert.False(requests[0].Authorized);
    }

    [Theory]
    [InlineData("{}", true)]
    [InlineData("42", true)]
    [InlineData("[]", true)]
    [InlineData("not-json", false)]
    [InlineData("", false)]
    public async Task Accepts_any_json_model_but_rejects_malformed_bodies(string response, bool valid)
    {
        using var client = new HttpClient(new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(response)
        }));
        using var probe = new ConfiguratorConnectivityProbe(client);

        var result = await probe.CheckProviderAsync(LibreProvider("t"));

        Assert.Equal(valid ? ConnectivityStatus.Responding : ConnectivityStatus.UnexpectedResponse,
            result.Status);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Redirect)]
    public async Task Non_success_http_responses_are_diagnostics_only(HttpStatusCode code)
    {
        using var client = new HttpClient(new FakeHandler(_ => new HttpResponseMessage(code)
        {
            Content = new StringContent("{\"languages\":[]}")
        }));
        using var probe = new ConfiguratorConnectivityProbe(client);
        var result = await probe.CheckProviderAsync(LibreProvider("t"));
        Assert.Equal(ConnectivityStatus.UnexpectedResponse, result.Status);
        Assert.Contains(((int)code).ToString(), result.Detail);
    }

    [Fact]
    public async Task Disabled_unknown_and_bad_endpoints_do_not_send_requests()
    {
        var calls = 0;
        using var client = new HttpClient(new FakeHandler(_ =>
        {
            calls++;
            return new HttpResponseMessage(HttpStatusCode.OK);
        }));
        using var probe = new ConfiguratorConnectivityProbe(client);

        Assert.Equal(ConnectivityStatus.Disabled,
            (await probe.CheckProviderAsync(new TranslationBackendSettings
            { Id = "skip", Type = "LibreTranslate", Enabled = false })).Status);
        Assert.Equal(ConnectivityStatus.Unsupported,
            (await probe.CheckProviderAsync(new TranslationBackendSettings
            { Id = "custom", Type = "Other", Enabled = true })).Status);

        foreach (var endpoint in new[]
        {
            "ftp://invalid.example", "http://user:secret@host.test", "https://host.test?token=secret",
            "not a uri"
        })
        {
            Assert.Equal(ConnectivityStatus.InvalidConfiguration,
                (await probe.CheckProviderAsync(LibreProvider("t", endpoint))).Status);
        }
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Network_errors_are_nonfatal_and_do_not_prevent_saving()
    {
        using var client = new HttpClient(new ThrowingHandler());
        using var probe = new ConfiguratorConnectivityProbe(client);
        using var temp = new TemporaryDirectory();
        var session = ConfiguratorSession.CreateNew(temp.File);
        session.AddComponent(SettingsComponentKind.TranslationBackend, "libre", "LibreTranslate");
        var result = await probe.CheckAsync(session.Draft);
        Assert.Equal(ConnectivityStatus.Unreachable, Assert.Single(result).Status);

        // Connectivity is NOT a validation requirement.
        Assert.Empty(session.Validate());
        session.Save();
        Assert.True(File.Exists(temp.File));
        Assert.False(session.IsDirty);
    }

    [Fact]
    public async Task Explicit_live_check_of_loaded_file_keeps_file_unchanged()
    {
        using var temp = new TemporaryDirectory();
        var session = ConfiguratorSession.CreateNew(temp.File);
        session.AddComponent(SettingsComponentKind.TranslationBackend, "libre", "LibreTranslate");
        session.Save();
        var originalBytes = File.ReadAllBytes(temp.File);
        var calls = 0;
        using var client = new HttpClient(new FakeHandler(_ =>
        {
            calls++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            };
        }));
        using var probe = new ConfiguratorConnectivityProbe(client);
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;

        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter); // Test configuration.
        console.Input.PushKey(ConsoleKey.Enter); // Browse files.
        console.Input.PushKey(ConsoleKey.Enter); // Select JSON.
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter); // Opt-in to live check.
        console.Input.PushKey(ConsoleKey.Enter); // Acknowledge live results.
        console.Input.PushKey(ConsoleKey.Escape); // Exit main menu.

        await new ConfiguratorApp(console, System.IO.Path.GetDirectoryName(temp.File),
            System.IO.Path.Combine(temp.Directory, "ui.json"), probe).RunAsync();
        Assert.Equal(1, calls);
        Assert.Contains("Responding", console.Output);
        Assert.Contains("Diagnostic only:", console.Output);
        Assert.Equal(originalBytes, File.ReadAllBytes(temp.File));
    }

    [Fact]
    public async Task Oversized_response_is_rejected_without_parsing_or_persisting()
    {
        using var client = new HttpClient(new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(new string(' ', 1024 * 1024 + 1))
        }));
        using var probe = new ConfiguratorConnectivityProbe(client);
        var result = await probe.CheckProviderAsync(LibreProvider("one"));
        Assert.Equal(ConnectivityStatus.UnexpectedResponse, result.Status);
        Assert.Contains("too much data", result.Detail);
    }

    private static TranslationBackendSettings LibreProvider(
        string id, string endpoint = "http://127.0.0.1:5000", int timeout = 10) =>
        new()
        {
            Id = id,
            Type = "LibreTranslate",
            Options = new Dictionary<string, JsonElement>
            {
                ["Endpoint"] = JsonSerializer.SerializeToElement(endpoint),
                ["RequestTimeoutSeconds"] = JsonSerializer.SerializeToElement(timeout)
            }
        };

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new HttpRequestException("Sensitive endpoint text must not leak.");
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string Directory { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "um-live-" + Guid.NewGuid().ToString("N"));
        public string File => System.IO.Path.Combine(Directory, "UnderstandMeSettings.json");
        public TemporaryDirectory() => System.IO.Directory.CreateDirectory(Directory);
        public void Dispose() => System.IO.Directory.Delete(Directory, recursive: true);
    }
}
