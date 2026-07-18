using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Resolves workspace catalogs and suggests the first available documentation key.
/// </summary>
internal sealed class WhenItFailsDocumentationKeySuggester
{
    /// <summary>
    /// Resolves workspace options from a project or Jsons/WhenItFails path and suggests a canonical documentation key.
    /// </summary>
    public Task<Response<DocumentationKeySuggestion>> SuggestAsync(
        string inputPath,
        string categoryLookup,
        string title,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);
        return SuggestAsync(options, categoryLookup, title, cancellationToken);
    }

    /// <summary>
    /// Suggests a canonical documentation key from resolved workspace options without modifying the workspace.
    /// </summary>
    public async Task<Response<DocumentationKeySuggestion>> SuggestAsync(
        JsonsOptions options,
        string categoryLookup,
        string title,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryLookup);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Response<ErrorCategoryCatalogDocument> categoryResponse =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                options.CategoryCatalogFilePath,
                cancellationToken);
        if (!categoryResponse.IsSuccess || categoryResponse.Data is null)
        {
            return Response<DocumentationKeySuggestion>.Fail(
                code: categoryResponse.Issues.Count > 0
                    ? categoryResponse.Issues[0].Code
                    : "CategoryCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(categoryResponse.Message)
                    ? "The category catalog could not be loaded."
                    : categoryResponse.Message);
        }

        ErrorCategoryCatalogDocument categories =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(categoryResponse.Data);
        string normalizedLookup = TextKeyNormalizer.NormalizeKey(categoryLookup);
        ErrorCategoryDefinition? category = categories.Categories.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, normalizedLookup, StringComparison.OrdinalIgnoreCase)
            || candidate.Aliases.Any(alias => string.Equals(
                alias,
                normalizedLookup,
                StringComparison.OrdinalIgnoreCase)));
        if (category is null)
        {
            return Response<DocumentationKeySuggestion>.NotFound(
                code: "CategoryNotFound",
                message: $"Category '{normalizedLookup}' was not found.");
        }

        Response<ErrorCatalogDocument> errorResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);
        if (!errorResponse.IsSuccess || errorResponse.Data is null)
        {
            return Response<DocumentationKeySuggestion>.Fail(
                code: errorResponse.Issues.Count > 0
                    ? errorResponse.Issues[0].Code
                    : "ErrorCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(errorResponse.Message)
                    ? "The error catalog could not be loaded."
                    : errorResponse.Message);
        }

        ErrorCatalogDocument errors =
            new ErrorCatalogDocumentNormalizer().Normalize(errorResponse.Data);

        try
        {
            string documentationKey = new WhenItFailsDocumentationKeyGenerator().Generate(
                category.Name,
                title,
                errors.Errors.Select(error => error.DocumentationKey));

            return Response<DocumentationKeySuggestion>.Ok(
                new DocumentationKeySuggestion(
                    category.Name,
                    title.Trim(),
                    documentationKey));
        }
        catch (ArgumentException exception)
        {
            return Response<DocumentationKeySuggestion>.Invalid(
                code: "DocumentationKeyGenerationFailed",
                message: exception.Message);
        }
    }
}

/// <summary>
/// Describes one read-only documentation key suggestion.
/// </summary>
internal sealed record DocumentationKeySuggestion(
    string Category,
    string Title,
    string DocumentationKey);
