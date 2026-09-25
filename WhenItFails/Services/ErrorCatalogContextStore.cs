using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Services;

/// <summary>
/// Thread-safe in-memory store for the current error catalog context.
/// </summary>
/// <remarks>
/// The context reference and its store-scoped generation are published
/// together as one immutable record. Nested context objects are still mutable.
/// </remarks>
public sealed class ErrorCatalogContextStore
    : IErrorCatalogContextStore, IErrorCatalogContextPublicationReader,
      IErrorCatalogContextPublisher
{
    private readonly Guid _storeId = Guid.NewGuid();
    private ErrorCatalogContextPublication? _current;

    /// <inheritdoc />
    public bool IsInitialized =>
        Volatile.Read(ref _current) is not null;

    /// <inheritdoc />
    public ErrorCatalogContext? Current =>
        Volatile.Read(ref _current)?.Context;

    /// <inheritdoc />
    public Response<ErrorCatalogContext> GetCurrent()
    {
        ErrorCatalogContextPublication? current =
            Volatile.Read(ref _current);

        if (current is null)
        {
            return Response<ErrorCatalogContext>.Invalid(
                code: "ErrorCatalogContextNotInitialized",
                message: "Error catalog context has not been initialized.");
        }

        return Response<ErrorCatalogContext>.Ok(current.Context);
    }

    /// <inheritdoc />
    public Response<ErrorCatalogContextPublication> GetCurrentPublication()
    {
        ErrorCatalogContextPublication? current =
            Volatile.Read(ref _current);

        if (current is null)
        {
            return Response<ErrorCatalogContextPublication>.Invalid(
                code: "ErrorCatalogContextNotInitialized",
                message: "Error catalog context has not been initialized.");
        }

        return Response<ErrorCatalogContextPublication>.Ok(current);
    }

    /// <inheritdoc />
    public void Set(ErrorCatalogContext context)
    {
        _ = Publish(context);
    }

    /// <inheritdoc />
    public ErrorCatalogContextPublication Publish(ErrorCatalogContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        while (true)
        {
            ErrorCatalogContextPublication? previous =
                Volatile.Read(ref _current);

            long nextGeneration =
                checked((previous?.Generation ?? 0L) + 1L);

            ErrorCatalogContextPublication next = new(
                _storeId,
                nextGeneration,
                context);

            if (ReferenceEquals(
                Interlocked.CompareExchange(ref _current, next, previous),
                previous))
            {
                // Return this call's successful CAS record, not a later
                // GetCurrentPublication read that another writer could replace.
                return next;
            }
        }
    }
}
