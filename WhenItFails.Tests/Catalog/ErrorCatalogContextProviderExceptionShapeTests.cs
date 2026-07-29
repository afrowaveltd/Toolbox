using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderExceptionShapeTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPropagateDifferentExceptionTypeUnchanged()
    {
        FormatException expectedException = new("Provider returned malformed catalog data.");

        ErrorCatalogContextProvider provider = new(
            new ThrowingFormatExceptionErrorCatalogProvider(expectedException),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        FormatException actualException = await Assert.ThrowsAsync<FormatException>(
            () => provider.LoadFromJsonsAsync(CreateOptions()));

        Assert.Same(expectedException, actualException);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPropagateCanceledProviderTaskWithOriginalToken()
    {
        using CancellationTokenSource providerCancellation = new();
        providerCancellation.Cancel();

        ErrorCatalogContextProvider provider = new(
            new CanceledErrorCatalogProvider(providerCancellation.Token),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        OperationCanceledException exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadFromJsonsAsync(CreateOptions(), CancellationToken.None));

        Assert.Equal(providerCancellation.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPreserveOuterAndInnerExceptionReferences()
    {
        FormatException innerException = new("Catalog value has an invalid format.");
        ProviderCatalogException expectedException = new(
            "Provider could not load the catalog.",
            innerException);

        ErrorCatalogContextProvider provider = new(
            new ThrowingCustomExceptionErrorCatalogProvider(expectedException),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        ProviderCatalogException actualException = await Assert.ThrowsAsync<ProviderCatalogException>(
            () => provider.LoadFromJsonsAsync(CreateOptions()));

        Assert.Same(expectedException, actualException);
        Assert.Same(innerException, actualException.InnerException);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPreserveExceptionDataEntries()
    {
        ProviderCatalogException expectedException = new(
            "Provider could not load the catalog.",
            new FormatException("Catalog value has an invalid format."));
        expectedException.Data["catalog-path"] = "Jsons/WhenItFails/errors.en.json";
        expectedException.Data["attempt"] = 3;

        ErrorCatalogContextProvider provider = new(
            new ThrowingCustomExceptionErrorCatalogProvider(expectedException),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        ProviderCatalogException actualException = await Assert.ThrowsAsync<ProviderCatalogException>(
            () => provider.LoadFromJsonsAsync(CreateOptions()));

        Assert.Same(expectedException, actualException);
        Assert.Equal("Jsons/WhenItFails/errors.en.json", actualException.Data["catalog-path"]);
        Assert.Equal(3, actualException.Data["attempt"]);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPreserveCustomExceptionProperties()
    {
        const string expectedCatalogPath = "Jsons/WhenItFails/errors.en.json";
        ProviderCatalogException expectedException = new(
            "Provider could not load the catalog.",
            new FormatException("Catalog value has an invalid format."),
            expectedCatalogPath);

        ErrorCatalogContextProvider provider = new(
            new ThrowingCustomExceptionErrorCatalogProvider(expectedException),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        ProviderCatalogException actualException = await Assert.ThrowsAsync<ProviderCatalogException>(
            () => provider.LoadFromJsonsAsync(CreateOptions()));

        Assert.Same(expectedException, actualException);
        Assert.Equal(expectedCatalogPath, actualException.CatalogPath);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldSurfaceNullProviderTaskWithoutCallingLaterProviders()
    {
        ErrorCatalogContextProvider provider = new(
            new NullTaskErrorCatalogProvider(),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        await Assert.ThrowsAsync<NullReferenceException>(
            () => provider.LoadFromJsonsAsync(CreateOptions()));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldSurfaceNullCategoryProviderTaskAfterErrorProviderSucceeds()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new NullTaskCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        await Assert.ThrowsAsync<NullReferenceException>(
            () => provider.LoadFromJsonsAsync(CreateOptions()));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldSurfaceNullCodeGroupProviderTaskAfterEarlierProvidersSucceed()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new SuccessfulCategoryCatalogProvider(),
            new NullTaskCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        await Assert.ThrowsAsync<NullReferenceException>(
            () => provider.LoadFromJsonsAsync(CreateOptions()));
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private sealed class ThrowingFormatExceptionErrorCatalogProvider : IErrorCatalogProvider
    {
        private readonly FormatException _exception;

        public ThrowingFormatExceptionErrorCatalogProvider(FormatException exception)
        {
            _exception = exception;
        }

        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw _exception;
        }
    }

    private sealed class CanceledErrorCatalogProvider : IErrorCatalogProvider
    {
        private readonly CancellationToken _cancellationToken;

        public CanceledErrorCatalogProvider(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromCanceled<Response<ErrorCatalogProviderPayload>>(_cancellationToken);
        }
    }

    private sealed class ThrowingCustomExceptionErrorCatalogProvider : IErrorCatalogProvider
    {
        private readonly ProviderCatalogException _exception;

        public ThrowingCustomExceptionErrorCatalogProvider(ProviderCatalogException exception)
        {
            _exception = exception;
        }

        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw _exception;
        }
    }

    private sealed class NullTaskErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return null!;
        }
    }

    private sealed class SuccessfulErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            ErrorCatalogDocument document = new();

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                new ErrorCatalogProviderPayload
                {
                    Catalog = new ErrorCatalog(document.Errors),
                    Document = document,
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class NullTaskCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return null!;
        }
    }

    private sealed class SuccessfulCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(
                new ErrorCategoryCatalogProviderPayload
                {
                    Document = new ErrorCategoryCatalogDocument(),
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class NullTaskCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return null!;
        }
    }

    private sealed class ProviderCatalogException : Exception
    {
        public ProviderCatalogException(
            string message,
            Exception innerException,
            string? catalogPath = null)
            : base(message, innerException)
        {
            CatalogPath = catalogPath;
        }

        public string? CatalogPath { get; }
    }

    private sealed class UnexpectedCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The category provider must not run after the error provider stops the operation.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The code-group provider must not run after an earlier provider stops the operation.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The owner provider must not run after an earlier provider stops the operation.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The profile provider must not run after an earlier provider stops the operation.");
        }
    }
}
