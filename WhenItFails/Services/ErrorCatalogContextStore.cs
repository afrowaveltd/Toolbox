using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Services;

/// <summary>
/// Thread-safe in-memory store for the current error catalog context.
/// </summary>
public sealed class ErrorCatalogContextStore
    : IErrorCatalogContextStore
{
    private ErrorCatalogContext? _current;

    /// <inheritdoc />
    public bool IsInitialized =>
        Volatile.Read(ref _current) is not null;

    /// <inheritdoc />
    public ErrorCatalogContext? Current =>
        Volatile.Read(ref _current);

    /// <inheritdoc />
    public Response<ErrorCatalogContext> GetCurrent()
    {
        ErrorCatalogContext? current =
            Volatile.Read(ref _current);

        if (current is null)
        {
            return Response<ErrorCatalogContext>.Invalid(
                code: "ErrorCatalogContextNotInitialized",
                message: "Error catalog context has not been initialized.");
        }

        return Response<ErrorCatalogContext>.Ok(current);
    }

    /// <inheritdoc />
    public void Set(ErrorCatalogContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Interlocked.Exchange(
            ref _current,
            context);
    }
}