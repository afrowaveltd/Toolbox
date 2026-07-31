using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderProviderWarningBeforeCrossValidationErrorTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnOnlyCrossValidationError_WhenProviderWarningPrecedesIt()
    {
        const string providerWarningCode = "ErrorCatalogProviderWarning";

        ErrorCatalogContextProvider provider = CreateProvider(
            new WarningErrorCatalogProvider(providerWarningCode));

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            CreateOptions());

        AssertCrossValidationErrorResponse(response);
        Assert.DoesNotContain(
            response.Issues,
            candidate => candidate.Code == providerWarningCode);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnOnlyCrossValidationError_WhenMixedProviderDiagnosticsPrecedeIt()
    {
        const string providerInformationCode = "ErrorCatalogProviderInformation";
        const string providerWarningCode = "ErrorCatalogProviderWarning";

        ErrorCatalogContextProvider provider = CreateProvider(
            new MixedDiagnosticsErrorCatalogProvider(
                providerInformationCode,
                providerWarningCode));

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            CreateOptions());

        AssertCrossValidationErrorResponse(response);
        Assert.DoesNotContain(
            response.Issues,
            candidate => candidate.Code == providerInformationCode);
        Assert.DoesNotContain(
            response.Issues,
            candidate => candidate.Code == providerWarningCode);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldDiscardProviderMetadata_WhenCrossValidationFails()
    {
        const string metadataKey = "provider-source";

        ErrorCatalogContextProvider provider = CreateProvider(
            new MetadataErrorCatalogProvider(metadataKey));

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            CreateOptions());

        AssertCrossValidationErrorResponse(response);
        Assert.Empty(response.Metadata.Items);
        Assert.False(response.Metadata.TryGet(metadataKey, out _));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldDiscardProviderMessage_WhenCrossValidationFails()
    {
        const string providerMessage = "The error catalog provider completed successfully.";

        ErrorCatalogContextProvider provider = CreateProvider(
            new MessageErrorCatalogProvider(providerMessage));

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            CreateOptions());

        AssertCrossValidationErrorResponse(response);
        Assert.NotEqual(providerMessage, response.Message);
    }

    private static ErrorCatalogContextProvider CreateProvider(
        IErrorCatalogProvider errorCatalogProvider)
    {
        return new ErrorCatalogContextProvider(
            errorCatalogProvider,
            new CategoryCatalogProvider(),
            new CodeGroupCatalogProvider(),
            new OwnerCatalogProvider(),
            new EmptyProfileCatalogProvider());
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private static void AssertCrossValidationErrorResponse(
        Response<ErrorCatalogContext> response)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.True(response.HasWarnings);

        IssueInfo issue = Assert.Single(response.Issues);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
        Assert.Equal("UnknownErrorOwner", issue.Code);
        Assert.Equal(
            "Error owner 'MISSING_OWNER' is not defined in the owner catalog.",
            issue.Message);
        Assert.Equal(issue.Message, response.Message);
    }

    private static ErrorCatalogProviderPayload CreatePayload()
    {
        ErrorCatalogDocument document = new()
        {
            Errors =
            [
                new ErrorDefinition
                {
                    Id = "AFW_GEN_0001",
                    Code = 100001,
                    Name = "MISSINGOWNER",
                    Owner = "MISSING_OWNER",
                    CodePrefix = "GEN",
                    CodeGroup = "GENERAL",
                    PrimaryCategory = "GENERAL",
                    Categories = ["GENERAL"],
                    Title = "Missing owner",
                    Message = "Produces a cross-validation error.",
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

    private sealed class WarningErrorCatalogProvider(string warningCode)
        : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IssueInfo warning = IssueInfoFactory.Warning(
                warningCode,
                "The error catalog loaded with a recoverable warning.");

            return Task.FromResult(
                Response<ErrorCatalogProviderPayload>.OkWithWarnings(
                    CreatePayload(),
                    [warning]));
        }
    }

    private sealed class MixedDiagnosticsErrorCatalogProvider(
        string informationCode,
        string warningCode)
        : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Response<ErrorCatalogProviderPayload>.OkWithWarnings(
                    CreatePayload(),
                    [
                        IssueInfoFactory.Information(
                            informationCode,
                            "The error catalog provider reported informational context."),
                        IssueInfoFactory.Warning(
                            warningCode,
                            "The error catalog loaded with a recoverable warning.")
                    ]));
        }
    }

    private sealed class MetadataErrorCatalogProvider(string metadataKey)
        : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Response<ErrorCatalogProviderPayload>
            {
                Status = ResultStatus.Success,
                Data = CreatePayload(),
                Metadata = MetadataBagFactory.From(
                    metadataKey,
                    "error-catalog-provider")
            });
        }
    }

    private sealed class MessageErrorCatalogProvider(string message)
        : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Response<ErrorCatalogProviderPayload>
            {
                Status = ResultStatus.Success,
                Message = message,
                Data = CreatePayload()
            });
        }
    }

    private sealed class CategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(
                new ErrorCategoryCatalogProviderPayload
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
                }));
        }
    }

    private sealed class CodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCodeGroupCatalogProviderPayload>.Ok(
                new ErrorCodeGroupCatalogProviderPayload
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
                }));
        }
    }

    private sealed class OwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorOwnerCatalogProviderPayload>.Ok(
                new ErrorOwnerCatalogProviderPayload
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
                }));
        }
    }

    private sealed class EmptyProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorProfileCatalogProviderPayload>.Ok(
                new ErrorProfileCatalogProviderPayload
                {
                    Document = new ErrorProfileCatalogDocument(),
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }
}
