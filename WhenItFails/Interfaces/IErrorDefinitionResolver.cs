using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Resolves error definitions from a loaded error catalog context.
/// </summary>
public interface IErrorDefinitionResolver
{
    /// <summary>
    /// Finds an error definition by error identifier.
    /// </summary>
    /// <param name="context">Loaded error catalog context.</param>
    /// <param name="errorId">Error identifier.</param>
    /// <returns>Response containing the resolved error definition.</returns>
    Response<ErrorDefinition> FindById(
        ErrorCatalogContext? context,
        string errorId);

    /// <summary>
    /// Finds an error definition by normalized or raw error name.
    /// </summary>
    /// <param name="context">Loaded error catalog context.</param>
    /// <param name="errorName">Error name.</param>
    /// <returns>Response containing the resolved error definition.</returns>
    Response<ErrorDefinition> FindByName(
        ErrorCatalogContext? context,
        string errorName);

    /// <summary>
    /// Finds an error definition by numeric error code.
    /// </summary>
    /// <param name="context">Loaded error catalog context.</param>
    /// <param name="code">Numeric error code.</param>
    /// <returns>Response containing the resolved error definition.</returns>
    Response<ErrorDefinition> FindByCode(
        ErrorCatalogContext? context,
        int code);
}