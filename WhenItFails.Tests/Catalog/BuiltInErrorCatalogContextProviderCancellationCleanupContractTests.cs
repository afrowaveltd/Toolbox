using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class BuiltInErrorCatalogContextProviderCancellationCleanupContractTests
{
    [Fact]
    public async Task LoadAsync_WhenDelegatedCatalogLoadIsCancelled_RemovesTemporaryWorkspaceAndRethrowsSameToken()
    {
        BlockingContextProvider contextProvider = new();

        BuiltInErrorCatalogContextProvider provider = new(
            new SingleTemplateProvider(),
            contextProvider);

        using CancellationTokenSource cancellationSource = new();

        Task<Response<ErrorCatalogContext>> loadTask =
            provider.LoadAsync(
                cancellationSource.Token);

        try
        {
            await contextProvider.Entered.WaitAsync(
                TimeSpan.FromSeconds(10));

            JsonsOptions options =
                Assert.IsType<JsonsOptions>(
                    contextProvider.LastOptions);

            string rootDirectory =
                options.RootDirectory;

            Assert.True(
                Directory.Exists(
                    rootDirectory));

            Assert.True(
                File.Exists(
                    Path.Combine(
                        options.PackageDirectoryPath,
                        "errors.json")));

            await cancellationSource.CancelAsync();

            OperationCanceledException exception =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    () => loadTask);

            Assert.Equal(
                cancellationSource.Token,
                exception.CancellationToken);

            Assert.False(
                Directory.Exists(
                    rootDirectory));
        }
        finally
        {
            if (!cancellationSource.IsCancellationRequested)
            {
                await cancellationSource.CancelAsync();
            }

            try
            {
                await loadTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when the test reaches or leaves the blocked load.
            }

            JsonsOptions? options =
                contextProvider.LastOptions;

            if (options is not null
                && Directory.Exists(
                    options.RootDirectory))
            {
                Directory.Delete(
                    options.RootDirectory,
                    recursive: true);
            }
        }
    }

    private sealed class SingleTemplateProvider
        : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    TargetFileName = "errors.json",
                    Content = "{ \"errors\": [] }"
                }
            ];
        }
    }

    private sealed class BlockingContextProvider
        : IErrorCatalogContextProvider
    {
        private readonly TaskCompletionSource<bool> _entered =
            new(
                TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Entered =>
            _entered.Task;

        public JsonsOptions? LastOptions { get; private set; }

        public async Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LastOptions = options;

            _entered.TrySetResult(true);

            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                cancellationToken);

            throw new InvalidOperationException(
                "The blocking context provider should only complete by cancellation.");
        }
    }
}
