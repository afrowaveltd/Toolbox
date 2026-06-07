using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Services;

/// <summary>
/// Default high-level service for creating runtime error descriptors.
/// </summary>
public sealed class ErrorDescriptorService : IErrorDescriptorService
{
    private readonly IErrorDescriptorResolver _descriptorResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorDescriptorService"/> class.
    /// </summary>
    /// <param name="descriptorResolver">Error descriptor resolver.</param>
    public ErrorDescriptorService(IErrorDescriptorResolver descriptorResolver)
    {
        _descriptorResolver = descriptorResolver
            ?? throw new ArgumentNullException(nameof(descriptorResolver));
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> FromId(
        ErrorCatalogContext? context,
        string errorId)
    {
        return _descriptorResolver.CreateById(
            context,
            errorId);
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> FromName(
        ErrorCatalogContext? context,
        string errorName)
    {
        return _descriptorResolver.CreateByName(
            context,
            errorName);
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> FromCode(
        ErrorCatalogContext? context,
        int code)
    {
        return _descriptorResolver.CreateByCode(
            context,
            code);
    }
}