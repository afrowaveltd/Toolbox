using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Stores the current initialized error catalog context.
/// </summary>
/// <remarks>
/// The complete context is replaced as one value, allowing catalog reloads
/// without exposing a partially updated state.
/// </remarks>
public interface IErrorCatalogContextStore
{
    /// <summary>
    /// Gets a value indicating whether a context has been stored.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets the current context, or <see langword="null"/>
    /// when the store has not been initialized.
    /// </summary>
    ErrorCatalogContext? Current { get; }

    /// <summary>
    /// Gets the current context as a response.
    /// </summary>
    /// <returns>
    /// A successful response containing the current context,
    /// or an invalid response when no context has been stored.
    /// </returns>
    Response<ErrorCatalogContext> GetCurrent();

    /// <summary>
    /// Stores or atomically replaces the current context.
    /// </summary>
    /// <param name="context">Context to store.</param>
    void Set(ErrorCatalogContext context);
}