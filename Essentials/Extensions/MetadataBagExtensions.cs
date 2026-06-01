using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="MetadataBag"/>.
/// </summary>
public static class MetadataBagExtensions
{
   /// <summary>
   /// Sets a metadata value only when the value is not null, empty, or whitespace.
   /// </summary>
   /// <param name="metadata">The metadata bag.</param>
   /// <param name="key">The metadata key.</param>
   /// <param name="value">The metadata value.</param>
   /// <returns>The original metadata bag.</returns>
   public static MetadataBag SetIfNotWhiteSpace(
       this MetadataBag metadata,
       string key,
       string? value)
   {
      ArgumentNullException.ThrowIfNull(metadata);
      ArgumentException.ThrowIfNullOrWhiteSpace(key);

      if(!string.IsNullOrWhiteSpace(value))
      {
         metadata.Set(key, value);
      }

      return metadata;
   }

   /// <summary>
   /// Sets a metadata value only when the value is not null.
   /// </summary>
   /// <param name="metadata">The metadata bag.</param>
   /// <param name="key">The metadata key.</param>
   /// <param name="value">The metadata value.</param>
   /// <returns>The original metadata bag.</returns>
   public static MetadataBag SetIfNotNull(
       this MetadataBag metadata,
       string key,
       object? value)
   {
      ArgumentNullException.ThrowIfNull(metadata);
      ArgumentException.ThrowIfNullOrWhiteSpace(key);

      if(value is not null)
      {
         metadata.Set(key, value.ToString() ?? string.Empty);
      }

      return metadata;
   }

   /// <summary>
   /// Copies all metadata values from another metadata bag.
   /// Existing keys are overwritten.
   /// </summary>
   /// <param name="metadata">The target metadata bag.</param>
   /// <param name="other">The source metadata bag.</param>
   /// <returns>The original target metadata bag.</returns>
   public static MetadataBag MergeFrom(
       this MetadataBag metadata,
       MetadataBag other)
   {
      ArgumentNullException.ThrowIfNull(metadata);
      ArgumentNullException.ThrowIfNull(other);

      foreach(var item in other.Items)
      {
         metadata.Set(item.Key, item.Value);
      }

      return metadata;
   }

   /// <summary>
   /// Creates a shallow copy of the metadata bag.
   /// </summary>
   /// <param name="metadata">The metadata bag to copy.</param>
   /// <returns>A new metadata bag with copied values.</returns>
   public static MetadataBag Copy(this MetadataBag metadata)
   {
      ArgumentNullException.ThrowIfNull(metadata);

      return new MetadataBag(metadata.Items.ToDictionary(
          item => item.Key,
          item => item.Value,
          StringComparer.OrdinalIgnoreCase));
   }
   /// <summary>
   /// Determines whether the metadata bag contains at least one metadata value.
   /// </summary>
   /// <param name="metadata">The metadata bag.</param>
   /// <returns><c>true</c> if the metadata bag contains at least one value; otherwise, <c>false</c>.</returns>
   public static bool HasAnyMetadata(this MetadataBag metadata)
   {
      ArgumentNullException.ThrowIfNull(metadata);

      return !metadata.IsEmpty;
   }

   /// <summary>
   /// Gets a metadata value by key or returns a fallback value when the key does not exist.
   /// </summary>
   /// <param name="metadata">The metadata bag.</param>
   /// <param name="key">The metadata key.</param>
   /// <param name="fallback">The fallback value.</param>
   /// <returns>The metadata value or the fallback value.</returns>
   public static string? GetOrDefault(
      this MetadataBag metadata,
      string key,
      string? fallback = null)
   {
      ArgumentNullException.ThrowIfNull(metadata);
      ArgumentException.ThrowIfNullOrWhiteSpace(key);

      return metadata.TryGet(key, out var metadataValue)
         ? metadataValue
         : fallback;
   }
}