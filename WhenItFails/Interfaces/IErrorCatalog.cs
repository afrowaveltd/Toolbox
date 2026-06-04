using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides indexed access to error definitions loaded from an error catalog.
/// </summary>
/// <remarks>
/// An error catalog is a runtime lookup structure built from one or more
/// error catalog documents. It allows fast searching by the most common
/// error identity and classification fields.
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
   /// <param name="id">Error identifier, for example <c>AFW-CFG-0001</c>.</param>
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
   /// Finds all error definitions owned by the specified owner.
   /// </summary>
   /// <param name="owner">Owner name, for example <c>Afrowave</c> or <c>Application</c>.</param>
   /// <returns>Matching error definitions. Empty list when none are found.</returns>
   IReadOnlyList<ErrorDefinition> FindByOwner(string owner);

   /// <summary>
   /// Finds all error definitions with the specified code prefix.
   /// </summary>
   /// <param name="codePrefix">Code prefix, for example <c>CFG</c>.</param>
   /// <returns>Matching error definitions. Empty list when none are found.</returns>
   IReadOnlyList<ErrorDefinition> FindByCodePrefix(string codePrefix);

   /// <summary>
   /// Finds all error definitions in the specified code group.
   /// </summary>
   /// <param name="codeGroup">Code group, for example <c>Configuration</c>.</param>
   /// <returns>Matching error definitions. Empty list when none are found.</returns>
   IReadOnlyList<ErrorDefinition> FindByCodeGroup(string codeGroup);

   /// <summary>
   /// Finds all error definitions in the specified logical category.
   /// </summary>
   /// <remarks>
   /// This searches both <c>PrimaryCategory</c> and additional <c>Categories</c>.
   /// </remarks>
   /// <param name="category">Category name, for example <c>Configuration</c>.</param>
   /// <returns>Matching error definitions. Empty list when none are found.</returns>
   IReadOnlyList<ErrorDefinition> FindByCategory(string category);

   /// <summary>
   /// Finds all error definitions in the specified logical subcategory.
   /// </summary>
   /// <param name="subcategory">Subcategory name, for example <c>RequiredValue</c>.</param>
   /// <returns>Matching error definitions. Empty list when none are found.</returns>
   IReadOnlyList<ErrorDefinition> FindBySubcategory(string subcategory);

   /// <summary>
   /// Finds all error definitions containing the specified tag.
   /// </summary>
   /// <param name="tag">Tag value.</param>
   /// <returns>Matching error definitions. Empty list when none are found.</returns>
   IReadOnlyList<ErrorDefinition> FindByTag(string tag);
}