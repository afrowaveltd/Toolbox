namespace Afrowave.Toolbox.Essentials.Metadata;

/// <summary>
/// Represents a lightweight collection of metadata values.
/// </summary>
public sealed class MetadataBag
{
   private readonly Dictionary<string, string> _items;

   /// <summary>
   /// Initializes a new empty metadata bag.
   /// </summary>
   public MetadataBag()
   {
      _items = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Initializes a new metadata bag with existing values.
   /// </summary>
   /// <param name="items">Initial metadata values.</param>
   public MetadataBag(IDictionary<string, string> items)
   {
      _items = new Dictionary<string, string>(items, StringComparer.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Gets the number of metadata items.
   /// </summary>
   public int Count => _items.Count;

   /// <summary>
   /// Gets a value indicating whether the metadata bag is empty.
   /// </summary>
   public bool IsEmpty => _items.Count == 0;

   /// <summary>
   /// Gets a read-only view of all metadata values.
   /// </summary>
   public IReadOnlyDictionary<string, string> Items => _items;

   /// <summary>
   /// Gets or sets a metadata value by key.
   /// </summary>
   /// <param name="key">The metadata key.</param>
   /// <returns>The metadata value.</returns>
   public string this[string key]
   {
      get => _items[key];
      set => _items[key] = value;
   }

   /// <summary>
   /// Adds or updates a metadata value.
   /// </summary>
   /// <param name="key">The metadata key.</param>
   /// <param name="value">The metadata value.</param>
   public void Set(string key, string value)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(key);
      _items[key] = value;
   }

   /// <summary>
   /// Tries to get a metadata value.
   /// </summary>
   /// <param name="key">The metadata key.</param>
   /// <param name="value">The metadata value, if found.</param>
   /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
   public bool TryGet(string key, out string? value)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(key);
      return _items.TryGetValue(key, out value);
   }

   /// <summary>
   /// Removes a metadata value.
   /// </summary>
   /// <param name="key">The metadata key.</param>
   /// <returns><c>true</c> if the value was removed; otherwise, <c>false</c>.</returns>
   public bool Remove(string key)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(key);
      return _items.Remove(key);
   }

   /// <summary>
   /// Clears all metadata values.
   /// </summary>
   public void Clear()
   {
      _items.Clear();
   }
}