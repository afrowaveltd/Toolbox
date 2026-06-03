using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default in-memory implementation of an indexed error catalog.
/// </summary>
/// <remarks>
/// This catalog is built from already loaded and preferably validated error definitions.
/// It stores all definitions in memory and creates lookup indexes for fast searching.
/// </remarks>
public sealed class ErrorCatalog : IErrorCatalog
{
   private readonly IReadOnlyList<ErrorDefinition> _errors;

   private readonly Dictionary<string, ErrorDefinition> _errorsById;
   private readonly Dictionary<int, ErrorDefinition> _errorsByCode;
   private readonly Dictionary<string, ErrorDefinition> _errorsByName;

   private readonly Dictionary<string, List<ErrorDefinition>> _errorsByCategory;
   private readonly Dictionary<string, List<ErrorDefinition>> _errorsByCategoryPrefix;
   private readonly Dictionary<string, List<ErrorDefinition>> _errorsByTag;

   /// <summary>
   /// Initializes a new instance of the <see cref="ErrorCatalog"/> class.
   /// </summary>
   /// <param name="errors">Error definitions to store in the catalog.</param>
   /// <exception cref="ArgumentNullException">
   /// Thrown when <paramref name="errors"/> is <c>null</c>.
   /// </exception>
   public ErrorCatalog(IEnumerable<ErrorDefinition> errors)
   {
      ArgumentNullException.ThrowIfNull(errors);

      _errors = errors.ToArray();

      _errorsById = new Dictionary<string, ErrorDefinition>(StringComparer.OrdinalIgnoreCase);
      _errorsByCode = new Dictionary<int, ErrorDefinition>();
      _errorsByName = new Dictionary<string, ErrorDefinition>(StringComparer.OrdinalIgnoreCase);

      _errorsByCategory = new Dictionary<string, List<ErrorDefinition>>(StringComparer.OrdinalIgnoreCase);
      _errorsByCategoryPrefix = new Dictionary<string, List<ErrorDefinition>>(StringComparer.OrdinalIgnoreCase);
      _errorsByTag = new Dictionary<string, List<ErrorDefinition>>(StringComparer.OrdinalIgnoreCase);

      BuildIndexes();
   }

   /// <inheritdoc />
   public IReadOnlyList<ErrorDefinition> GetAll()
   {
      return _errors;
   }

   /// <inheritdoc />
   public ErrorDefinition? FindById(string id)
   {
      if(string.IsNullOrWhiteSpace(id))
      {
         return null;
      }

      return _errorsById.TryGetValue(id.Trim(), out ErrorDefinition? error)
          ? error
          : null;
   }

   /// <inheritdoc />
   public ErrorDefinition? FindByCode(int code)
   {
      return _errorsByCode.TryGetValue(code, out ErrorDefinition? error)
          ? error
          : null;
   }

   /// <inheritdoc />
   public ErrorDefinition? FindByName(string name)
   {
      if(string.IsNullOrWhiteSpace(name))
      {
         return null;
      }

      return _errorsByName.TryGetValue(name.Trim(), out ErrorDefinition? error)
          ? error
          : null;
   }

   /// <inheritdoc />
   public IReadOnlyList<ErrorDefinition> FindByCategory(string category)
   {
      if(string.IsNullOrWhiteSpace(category))
      {
         return Array.Empty<ErrorDefinition>();
      }

      return _errorsByCategory.TryGetValue(category.Trim(), out List<ErrorDefinition>? errors)
          ? errors
          : Array.Empty<ErrorDefinition>();
   }

   /// <inheritdoc />
   public IReadOnlyList<ErrorDefinition> FindByCategoryPrefix(string categoryPrefix)
   {
      if(string.IsNullOrWhiteSpace(categoryPrefix))
      {
         return Array.Empty<ErrorDefinition>();
      }

      return _errorsByCategoryPrefix.TryGetValue(categoryPrefix.Trim(), out List<ErrorDefinition>? errors)
          ? errors
          : Array.Empty<ErrorDefinition>();
   }

   /// <inheritdoc />
   public IReadOnlyList<ErrorDefinition> FindByTag(string tag)
   {
      if(string.IsNullOrWhiteSpace(tag))
      {
         return Array.Empty<ErrorDefinition>();
      }

      return _errorsByTag.TryGetValue(tag.Trim(), out List<ErrorDefinition>? errors)
          ? errors
          : Array.Empty<ErrorDefinition>();
   }

   private void BuildIndexes()
   {
      foreach(ErrorDefinition error in _errors)
      {
         AddSingleValueIndex(_errorsById, error.Id, error);
         AddSingleValueIndex(_errorsByName, error.Name, error);

         if(error.Code > 0)
         {
            _errorsByCode.TryAdd(error.Code, error);
         }

         AddMultiValueIndex(_errorsByCategory, error.Category, error);
         AddMultiValueIndex(_errorsByCategoryPrefix, error.CategoryPrefix, error);

         foreach(string tag in error.Tags)
         {
            AddMultiValueIndex(_errorsByTag, tag, error);
         }
      }
   }

   private static void AddSingleValueIndex(
       Dictionary<string, ErrorDefinition> index,
       string key,
       ErrorDefinition error)
   {
      if(string.IsNullOrWhiteSpace(key))
      {
         return;
      }

      index.TryAdd(key.Trim(), error);
   }

   private static void AddMultiValueIndex(
       Dictionary<string, List<ErrorDefinition>> index,
       string key,
       ErrorDefinition error)
   {
      if(string.IsNullOrWhiteSpace(key))
      {
         return;
      }

      string normalizedKey = key.Trim();

      if(!index.TryGetValue(normalizedKey, out List<ErrorDefinition>? errors))
      {
         errors = new List<ErrorDefinition>();
         index[normalizedKey] = errors;
      }

      errors.Add(error);
   }
}