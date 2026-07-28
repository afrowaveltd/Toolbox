using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderCallOrderTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldCallProvidersInOrderWithConfiguredPaths()
    {
        List<string> calls = [];

        JsonsOptions options = new()
        {
            RootDirectory = "CustomJsons",
            PackageDirectoryName = "CustomWhenItFails",
            ErrorCatalogFileName = "custom-errors.json",
            CategoryCatalogFileName = "custom-categories.json",
            CodeGroupCatalogFileName = "custom-code-groups.json",
            OwnerCatalogFileName = "custom-owners.json",
            ProfilesFileName = "custom-profiles.json"
        };

        ErrorCatalogContextProvider provider = new(
            new RecordingErrorCatalogProvider(calls),
            new RecordingCategoryCatalogProvider(calls),
            new RecordingCodeGroupCatalogProvider(calls),
            new RecordingOwnerCatalogProvider(calls),
            new RecordingProfileCatalogProvider(calls));

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(options);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(
            [
                $"error:{options.ErrorCatalogFilePath}",
                $"category:{options.CategoryCatalogFilePath}",
                $"code-group:{options.CodeGroupCatalogFilePath}",
                $"owner:{options.OwnerCatalogFilePath}",
                $"profile:{options.ProfilesFilePath}"
            ],
            calls);
    }

    private static ErrorCatalogProviderPayload CreateErrorPayload()
    {
        ErrorCatalogDocument document = new()
        {
            Errors =
            [
                new ErrorDefinition
                {
                    Id = "AFW_GEN_0001",
                    Code = 100001,
                    Name = "UNKNOWNERROR",
                    Owner = "AFW",
                    CodePrefix = "GEN",
                    CodeGroup = "GENERAL",
                    PrimaryCategory = "GENERAL",
                    Categories = ["GENERAL"],
                    Title = "Unknown error",
                    Message = "An unknown error occurred.",
                    DefaultSeverity = "Error"
                }
            ]
        };

        return new ErrorCatalogProviderPayload
        {
            Catalog = new ErrorCatalog(document.Errors),
            Document = document,
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorCategoryCatalogProviderPayload CreateCategoryPayload()
    {
        return new ErrorCategoryCatalogProviderPayload
        {
            Document = new ErrorCategoryCatalogDocument
            {
                Categories =
                [
                    new ErrorCategoryDefinition
                    {
                        Name = "GENERAL",
                        DisplayName = "General"
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorCodeGroupCatalogProviderPayload CreateCodeGroupPayload()
    {
        return new ErrorCodeGroupCatalogProviderPayload
        {
            Document = new ErrorCodeGroupCatalogDocument
            {
                CodeGroups =
                [
                    new ErrorCodeGroupDefinition
                    {
                        Name = "GENERAL",
                        DisplayName = "General",
                        CodePrefix = "GEN",
                        CodeFrom = 100000,
                        CodeTo = 199999
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorOwnerCatalogProviderPayload CreateOwnerPayload()
    {
        return new ErrorOwnerCatalogProviderPayload
        {
            Document = new ErrorOwnerCatalogDocument
            {
                Owners =
                [
                    new ErrorOwnerDefinition
                    {
                        Name = "AFW",
                        DisplayName = "Afrowave",
                        CodeFrom = 0,
                        CodeTo = 999999,
                        IsBuiltIn = true
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorProfileCatalogProviderPayload CreateProfilePayload()
    {
        return new ErrorProfileCatalogProviderPayload
        {
            Document = new ErrorProfileCatalogDocument
            {
                Profiles =
                [
                    new ErrorProfileDefinition
                    {
                        Name = "DEFAULT",
                        DisplayName = "Default"
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private sealed class RecordingErrorCatalogProvider(List<string> calls) : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            calls.Add($"error:{filePath}");
            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(CreateErrorPayload()));
        }
    }

    private sealed class RecordingCategoryCatalogProvider(List<string> calls) : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            calls.Add($"category:{filePath}");
            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(CreateCategoryPayload()));
        }
    }

    private sealed class RecordingCodeGroupCatalogProvider(List<string> calls) : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            calls.Add($"code-group:{filePath}");
            return Task.FromResult(Response<ErrorCodeGroupCatalogProviderPayload>.Ok(CreateCodeGroupPayload()));
        }
    }

    private sealed class RecordingOwnerCatalogProvider(List<string> calls) : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            calls.Add($"owner:{filePath}");
            return Task.FromResult(Response<ErrorOwnerCatalogProviderPayload>.Ok(CreateOwnerPayload()));
        }
    }

    private sealed class RecordingProfileCatalogProvider(List<string> calls) : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            calls.Add($"profile:{filePath}");
            return Task.FromResult(Response<ErrorProfileCatalogProviderPayload>.Ok(CreateProfilePayload()));
        }
    }
}
