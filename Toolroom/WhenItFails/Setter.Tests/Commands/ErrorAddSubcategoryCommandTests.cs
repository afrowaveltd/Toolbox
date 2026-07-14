using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ErrorAddSubcategoryCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_AddsCanonicalSubcategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (ErrorDefinition error, string subcategory) =
            await LoadErrorAndUnusedSubcategoryAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await ErrorAddSubcategoryCommand.ExecuteAsync(
        [
            "error-add-subcategory",
            workspace.ProjectRootPath,
            error.Name,
            subcategory.ToLowerInvariant().Replace('_', '-')
        ]);

        Assert.Equal(0, exitCode);

        ErrorDefinition savedError = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            error.Id);
        Assert.Contains(subcategory, savedError.Subcategories, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownSubcategory_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await ErrorAddSubcategoryCommand.ExecuteAsync(
        [
            "error-add-subcategory",
            workspace.ProjectRootPath,
            error.Id,
            "DOES_NOT_EXIST"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "error-add-subcategory" },
            new[] { "error-add-subcategory", "." },
            new[] { "error-add-subcategory", ".", "AFW_NET_0001" },
            new[] { "error-add-subcategory", ".", "AFW_NET_0001", "TIMEOUT", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ErrorAddSubcategoryCommand.ExecuteAsync(args));
    }

    private static async Task<(ErrorDefinition Error, string Subcategory)>
        LoadErrorAndUnusedSubcategoryAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        string[] subcategories = catalog.Errors
            .SelectMany(error => error.Subcategories)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (ErrorDefinition error in catalog.Errors)
        {
            string? subcategory = subcategories.FirstOrDefault(candidate =>
                !error.Subcategories.Contains(candidate, StringComparer.OrdinalIgnoreCase));
            if (subcategory is not null)
            {
                return (error, subcategory);
            }
        }

        throw new InvalidOperationException("The test workspace contains no unused subcategory for any error.");
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault();
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(string jsonsPath, string errorId)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, errorId, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorCatalogDocument> LoadCatalogAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }
}
