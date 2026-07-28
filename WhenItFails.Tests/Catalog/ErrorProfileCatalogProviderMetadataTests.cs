using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorProfileCatalogProviderMetadataTests
{
    [Fact]
    public async Task LoadFromFileAsync_ShouldPreserveProfileMetadata()
    {
        MetadataBag metadata = new(
            new Dictionary<string, string>
            {
                ["consumer"] = "web-adapter",
                ["contractVersion"] = "2"
            });

        ErrorProfileCatalogDocument document = new()
        {
            SchemaVersion = "1.0",
            CatalogId = "test.profiles",
            CatalogName = "Test Profiles",
            Language = "en",
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    Name = "WEB_API",
                    DisplayName = "Web API",
                    Metadata = metadata
                }
            ]
        };

        ErrorProfileCatalogProvider provider = new(
            new FixedLoader(document),
            new ErrorProfileCatalogDocumentNormalizer(),
            new ErrorProfileCatalogValidator());

        Response<ErrorProfileCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("profiles.json");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorProfileDefinition profile =
            Assert.Single(response.Data.Document.Profiles);

        Assert.Equal(2, profile.Metadata.Count);
        Assert.True(profile.Metadata.TryGet("CONSUMER", out string? consumer));
        Assert.Equal("web-adapter", consumer);
        Assert.True(profile.Metadata.TryGet("contractversion", out string? version));
        Assert.Equal("2", version);
    }

    private sealed class FixedLoader(ErrorProfileCatalogDocument document)
        : IErrorProfileCatalogLoader
    {
        public Task<Response<ErrorProfileCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Response<ErrorProfileCatalogDocument>.Ok(document));
        }
    }
}
