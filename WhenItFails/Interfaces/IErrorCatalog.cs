using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides indexed access to error definitions loaded from an error catalog.
/// </summary>
/// <remarks>
/// An error catalog is a runtime lookup structure built from one or more
/// error catalog documents. It should allow fast searching by the most common
/// error identity fields.
/// </remarks>
public interface IErrorCatalog
{
   /// <summary>
   /// Gets all error definitions contained in this catalog.
   /// </summary>
   /// <returns>All error definitions.</returns>
   IReadOnlyList<ErrorDefinition> GetAll();

   /// <summary>
   /// Finds an error definition by its stable human-readable identifier.
   /// </summary>
   /// <param name="id">Error identifier, for example <c>CFG-0001</c>.</param>
   /// <returns>The matching error definition, or <c>null</c> when not found.</returns>
   ErrorDefinition? FindById(string id);

   /// <summary>
   /// Finds an error definition by its numeric code.
   /// </summary>
   /// <param name="code">Stable numeric error code.</param>
   /// <returns>The matching error definition, or <c>null</c> when not found.</returns>
   ErrorDefinition? FindByCode(int code);

   /// <summary>
   /// Finds an error definition by its machine-friendly name.
   /// </summary>
   /// <param name="name">Error name, for example <c>MissingConfigurationValue</c>.</param>
   /// <returns>The matching error definition, or <c>null</c> when not found.</returns>
   ErrorDefinition? FindByName(string name);

   /// <summary>
   /// Finds all error definitions in the specified category.
   /// </summary>
   /// <param name="category">Category name, for example <c>Configuration</c>.</param>
   /// <returns>Matching error definitions. Empty list when none are found.</returns>
   IReadOnlyList<ErrorDefinition> FindByCategory(string category);

   /// <summary>
   /// Finds all error definitions with the specified category prefix.
   /// </summary>
   /// <param name="categoryPrefix">Category prefix, for example <c>CFG</c>.</param>
   /// <returns>Matching error definitions. Empty list when none are found.</returns>
   IReadOnlyList<ErrorDefinition> FindByCategoryPrefix(string categoryPrefix);

   /// <summary>
   /// Finds all error definitions containing the specified tag.
   /// </summary>
   /// <param name="tag">Tag value.</param>
   /// <returns>Matching error definitions. Empty list when none are found.</returns>
   IReadOnlyList<ErrorDefinition> FindByTag(string tag);
}