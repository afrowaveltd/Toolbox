using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;

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

   private readonly Dictionary<string, List<ErrorDefinition>> _errorsByOwner;
   private readonly Dictionary<string, List<ErrorDefinition>> _errorsByCodePrefix;
   private readonly Dictionary<string, List<ErrorDefinition>> _errorsByCodeGroup;
   private readonly Dictionary<string, List<ErrorDefinition>> _errorsByCategory;
   private readonly Dictionary<string, List<ErrorDefinition>> _errorsBySubcategory;
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

      _errorsByOwner = new Dictionary<string, List<ErrorDefinition>>(StringComparer.OrdinalIgnoreCase);
      _errorsByCodePrefix = new Dictionary<string, List<ErrorDefinition>>(StringComparer.OrdinalIgnoreCase);
      _errorsByCodeGroup = new Dictionary<string, List<ErrorDefinition>>(StringComparer.OrdinalIgnoreCase);
      _errorsByCategory = new Dictionary<string, List<ErrorDefinition>>(StringComparer.OrdinalIgnoreCase);
      _errorsBySubcategory = new Dictionary<string, List<ErrorDefinition>>(StringComparer.OrdinalIgnoreCase);
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
      return FindSingle(_errorsById, id);
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
      return FindSingle(_errorsByName, name);
   }

   /// <inheritdoc />
   public IReadOnlyList<ErrorDefinition> FindByOwner(string owner)
   {
      return FindMany(_errorsByOwner, owner);
   }

   /// <inheritdoc />
   public IReadOnlyList<ErrorDefinition> FindByCodePrefix(string codePrefix)
   {
      return FindMany(_errorsByCodePrefix, codePrefix);
   }

   /// <inheritdoc />
   public IReadOnlyList<ErrorDefinition> FindByCodeGroup(string codeGroup)
   {
      return FindMany(_errorsByCodeGroup, codeGroup);
   }

   /// <inheritdoc />
   public IReadOnlyList<ErrorDefinition> FindByCategory(string category)
   {
      return FindMany(_errorsByCategory, category);
   }

   /// <inheritdoc />
   public IReadOnlyList<ErrorDefinition> FindBySubcategory(string subcategory)
   {
      return FindMany(_errorsBySubcategory, subcategory);
   }

   /// <inheritdoc />
   public IReadOnlyList<ErrorDefinition> FindByTag(string tag)
   {
      return FindMany(_errorsByTag, tag);
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

         AddMultiValueIndex(_errorsByOwner, error.Owner, error);
         AddMultiValueIndex(_errorsByCodePrefix, error.CodePrefix, error);
         AddMultiValueIndex(_errorsByCodeGroup, error.CodeGroup, error);

         AddMultiValueIndex(_errorsByCategory, error.PrimaryCategory, error);

         foreach(string category in error.Categories)
         {
            AddMultiValueIndex(_errorsByCategory, category, error);
         }

         foreach(string subcategory in error.Subcategories)
         {
            AddMultiValueIndex(_errorsBySubcategory, subcategory, error);
         }

         foreach(string tag in error.Tags)
         {
            AddMultiValueIndex(_errorsByTag, tag, error);
         }
      }
   }

   private static ErrorDefinition? FindSingle(
       Dictionary<string, ErrorDefinition> index,
       string key)
   {
      if(string.IsNullOrWhiteSpace(key))
      {
         return null;
      }

      string normalizedKey = TextKeyNormalizer.NormalizeKey(key);

      return index.TryGetValue(normalizedKey, out ErrorDefinition? error)
          ? error
          : null;
   }

   private static IReadOnlyList<ErrorDefinition> FindMany(
       Dictionary<string, List<ErrorDefinition>> index,
       string key)
   {
      if(string.IsNullOrWhiteSpace(key))
      {
         return Array.Empty<ErrorDefinition>();
      }

      string normalizedKey = TextKeyNormalizer.NormalizeKey(key);

      return index.TryGetValue(normalizedKey, out List<ErrorDefinition>? errors)
          ? errors
          : Array.Empty<ErrorDefinition>();
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

      string normalizedKey = TextKeyNormalizer.NormalizeKey(key);

      index.TryAdd(normalizedKey, error);
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

      string normalizedKey = TextKeyNormalizer.NormalizeKey(key);

      if(string.IsNullOrWhiteSpace(normalizedKey))
      {
         return;
      }

      if(!index.TryGetValue(normalizedKey, out List<ErrorDefinition>? errors))
      {
         errors = new List<ErrorDefinition>();
         index[normalizedKey] = errors;
      }

      if(!errors.Contains(error))
      {
         errors.Add(error);
      }
   }
}