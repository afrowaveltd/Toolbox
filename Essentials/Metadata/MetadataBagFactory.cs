namespace Afrowave.Toolbox.Essentials.Metadata;

/// <summary>
/// Provides factory methods for creating <see cref="MetadataBag"/> instances.
/// </summary>
public static class MetadataBagFactory
{
   /// <summary>
   /// Creates an empty metadata bag.
   /// </summary>
   /// <returns>An empty metadata bag.</returns>
   public static MetadataBag Empty()
   {
      return new MetadataBag();
   }

   /// <summary>
   /// Creates a metadata bag with a single metadata value.
   /// </summary>
   /// <param name="key">The metadata key.</param>
   /// <param name="value">The metadata value.</param>
   /// <returns>A metadata bag containing the specified value.</returns>
   public static MetadataBag From(
       string key,
       string value)
   {
      var metadata = new MetadataBag();
      metadata.Set(key, value);

      return metadata;
   }

   /// <summary>
   /// Creates a metadata bag from key-value pairs.
   /// </summary>
   /// <param name="items">The metadata key-value pairs.</param>
   /// <returns>A metadata bag containing the specified values.</returns>
   public static MetadataBag From(
       IEnumerable<KeyValuePair<string, string>> items)
   {
      ArgumentNullException.ThrowIfNull(items);

      var metadata = new MetadataBag();

      foreach(var item in items)
      {
         metadata.Set(item.Key, item.Value);
      }

      return metadata;
   }

   /// <summary>
   /// Creates a metadata bag from key-value tuples.
   /// </summary>
   /// <param name="items">The metadata key-value tuples.</param>
   /// <returns>A metadata bag containing the specified values.</returns>
   public static MetadataBag From(
       params (string Key, string Value)[] items)
   {
      ArgumentNullException.ThrowIfNull(items);

      var metadata = new MetadataBag();

      foreach(var item in items)
      {
         metadata.Set(item.Key, item.Value);
      }

      return metadata;
   }

   /// <summary>
   /// Creates a metadata bag by copying values from another metadata bag.
   /// </summary>
   /// <param name="metadata">The source metadata bag.</param>
   /// <returns>A new metadata bag with copied values.</returns>
   public static MetadataBag CopyFrom(MetadataBag metadata)
   {
      ArgumentNullException.ThrowIfNull(metadata);

      return new MetadataBag(metadata.Items.ToDictionary(
          item => item.Key,
          item => item.Value,
          StringComparer.OrdinalIgnoreCase));
   }
}