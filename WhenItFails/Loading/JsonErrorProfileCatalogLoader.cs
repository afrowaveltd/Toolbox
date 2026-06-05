using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Loading;

/// <summary>
/// Loads error profile catalog documents from JSON files.
/// </summary>
public sealed class JsonErrorProfileCatalogLoader : IErrorProfileCatalogLoader
{
    private readonly JsonCatalogDocumentLoader _documentLoader;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonErrorProfileCatalogLoader"/> class.
    /// </summary>
    public JsonErrorProfileCatalogLoader()
        : this(new JsonCatalogDocumentLoader())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonErrorProfileCatalogLoader"/> class.
    /// </summary>
    /// <param name="documentLoader">Shared JSON document loader.</param>
    public JsonErrorProfileCatalogLoader(JsonCatalogDocumentLoader documentLoader)
    {
        _documentLoader = documentLoader
            ?? throw new ArgumentNullException(nameof(documentLoader));
    }

    /// <inheritdoc />
    public Task<Response<ErrorProfileCatalogDocument>> LoadFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        return _documentLoader.LoadFromFileAsync<ErrorProfileCatalogDocument>(
            filePath,
            cancellationToken);
    }
}