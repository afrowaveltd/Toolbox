using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogRuntimeTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenContextStoreIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogRuntime(
                null!,
                new FakeDescriptorService(),
                new FakeProfileSelectionService()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDescriptorServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogRuntime(
                new FakeContextStore(),
                null!,
                new FakeProfileSelectionService()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenProfileSelectionServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogRuntime(
                new FakeContextStore(),
                new FakeDescriptorService(),
                null!));
    }

    [Fact]
    public void FromId_ShouldReturnContextFailure_WhenStoreIsNotInitialized()
    {
        ErrorCatalogRuntime runtime = new(
            new FakeContextStore(),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorDescriptor> response =
            runtime.FromId("AFW-GEN-0001");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(
            "ErrorCatalogContextNotInitialized",
            response.Issues[0].Code);
    }

    [Fact]
    public void FromId_ShouldDelegateToDescriptorService()
    {
        ErrorCatalogContext context = new();
        ErrorDescriptor descriptor = CreateDescriptor();

        FakeDescriptorService descriptorService = new(
            Response<ErrorDescriptor>.Ok(descriptor));

        ErrorCatalogRuntime runtime = new(
            new FakeContextStore(context),
            descriptorService,
            new FakeProfileSelectionService());

        Response<ErrorDescriptor> response =
            runtime.FromId("AFW-GEN-0001");

        Assert.True(response.IsSuccess);
        Assert.Same(descriptor, response.Data);

        Assert.Equal("FromId", descriptorService.LastCalledMethod);
        Assert.Same(context, descriptorService.LastContext);
        Assert.Equal(
            "AFW-GEN-0001",
            descriptorService.LastTextValue);
    }

    [Fact]
    public void FromName_ShouldDelegateToDescriptorService()
    {
        ErrorCatalogContext context = new();

        FakeDescriptorService descriptorService = new();

        ErrorCatalogRuntime runtime = new(
            new FakeContextStore(context),
            descriptorService,
            new FakeProfileSelectionService());

        runtime.FromName("UnknownError");

        Assert.Equal("FromName", descriptorService.LastCalledMethod);
        Assert.Same(context, descriptorService.LastContext);
        Assert.Equal(
            "UnknownError",
            descriptorService.LastTextValue);
    }

    [Fact]
    public void FromCode_ShouldDelegateToDescriptorService()
    {
        ErrorCatalogContext context = new();

        FakeDescriptorService descriptorService = new();

        ErrorCatalogRuntime runtime = new(
            new FakeContextStore(context),
            descriptorService,
            new FakeProfileSelectionService());

        runtime.FromCode(100001);

        Assert.Equal("FromCode", descriptorService.LastCalledMethod);
        Assert.Same(context, descriptorService.LastContext);
        Assert.Equal(100001, descriptorService.LastCode);
    }

    [Fact]
    public void ResolveProfile_ShouldDelegateToProfileSelectionService()
    {
        ErrorCatalogContext context = new();

        IReadOnlyList<ErrorDefinition> errors =
        [
            new ErrorDefinition
            {
                Id = "AFW_GEN_0001",
                Code = 100001,
                Name = "UNKNOWN_ERROR"
            }
        ];

        FakeProfileSelectionService profileService = new(
            Response<IReadOnlyList<ErrorDefinition>>.Ok(errors));

        ErrorCatalogRuntime runtime = new(
            new FakeContextStore(context),
            new FakeDescriptorService(),
            profileService);

        Response<IReadOnlyList<ErrorDefinition>> response =
            runtime.ResolveProfile("WEB");

        Assert.True(response.IsSuccess);
        Assert.Same(errors, response.Data);

        Assert.Same(context, profileService.LastContext);
        Assert.Equal("WEB", profileService.LastProfileName);
    }

    private static ErrorDescriptor CreateDescriptor()
    {
        return new ErrorDescriptor
        {
            Id = "AFW_GEN_0001",
            Code = 100001,
            Name = "UNKNOWN_ERROR"
        };
    }

    private sealed class FakeContextStore
        : IErrorCatalogContextStore
    {
        public FakeContextStore(
            ErrorCatalogContext? context = null)
        {
            Current = context;
        }

        public bool IsInitialized =>
            Current is not null;

        public ErrorCatalogContext? Current { get; private set; }

        public Response<ErrorCatalogContext> GetCurrent()
        {
            return Current is null
                ? Response<ErrorCatalogContext>.Invalid(
                    code: "ErrorCatalogContextNotInitialized",
                    message:
                        "Error catalog context has not been initialized.")
                : Response<ErrorCatalogContext>.Ok(Current);
        }

        public void Set(ErrorCatalogContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            Current = context;
        }
    }

    private sealed class FakeDescriptorService
        : IErrorDescriptorService
    {
        private readonly Response<ErrorDescriptor> _response;

        public FakeDescriptorService(
            Response<ErrorDescriptor>? response = null)
        {
            _response = response
                ?? Response<ErrorDescriptor>.Ok(
                    CreateDescriptor());
        }

        public string? LastCalledMethod { get; private set; }

        public ErrorCatalogContext? LastContext { get; private set; }

        public string? LastTextValue { get; private set; }

        public int? LastCode { get; private set; }

        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context,
            string errorId)
        {
            LastCalledMethod = "FromId";
            LastContext = context;
            LastTextValue = errorId;
            LastCode = null;

            return _response;
        }

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext? context,
            string errorName)
        {
            LastCalledMethod = "FromName";
            LastContext = context;
            LastTextValue = errorName;
            LastCode = null;

            return _response;
        }

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext? context,
            int code)
        {
            LastCalledMethod = "FromCode";
            LastContext = context;
            LastTextValue = null;
            LastCode = code;

            return _response;
        }
    }

    private sealed class FakeProfileSelectionService
        : IErrorProfileSelectionService
    {
        private readonly Response<IReadOnlyList<ErrorDefinition>>
            _response;

        public FakeProfileSelectionService(
            Response<IReadOnlyList<ErrorDefinition>>? response = null)
        {
            _response = response
                ?? Response<IReadOnlyList<ErrorDefinition>>.Ok([]);
        }

        public ErrorCatalogContext? LastContext { get; private set; }

        public string? LastProfileName { get; private set; }

        public Response<IReadOnlyList<ErrorDefinition>>
            ResolveByProfileName(
                ErrorCatalogContext? context,
                string profileName)
        {
            LastContext = context;
            LastProfileName = profileName;

            return _response;
        }
    }
}