using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying metadata.
/// </summary>
public static class HasMetadataExtensions
{
   /// <summary>
   /// Determines whether the object has any metadata.
   /// </summary>
   /// <param name="value">The object carrying metadata.</param>
   /// <returns><c>true</c> if the object has metadata; otherwise, <c>false</c>.</returns>
   public static bool HasAnyMetadata(this IHasMetadata value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return !value.Metadata.IsEmpty;
   }

   /// <summary>
   /// Tries to get a metadata value by key.
   /// </summary>
   /// <param name="value">The object carrying metadata.</param>
   /// <param name="key">The metadata key.</param>
   /// <param name="metadataValue">The metadata value, if found.</param>
   /// <returns><c>true</c> if the metadata key exists; otherwise, <c>false</c>.</returns>
   public static bool TryGetMetadata(
       this IHasMetadata value,
       string key,
       out string? metadataValue)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(key);

      return value.Metadata.TryGet(key, out metadataValue);
   }

   /// <summary>
   /// Gets a metadata value by key or returns a fallback value when the key does not exist.
   /// </summary>
   /// <param name="value">The object carrying metadata.</param>
   /// <param name="key">The metadata key.</param>
   /// <param name="fallback">The fallback value.</param>
   /// <returns>The metadata value or the fallback value.</returns>
   public static string? GetMetadataOrDefault(
       this IHasMetadata value,
       string key,
       string? fallback = null)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(key);

      return value.Metadata.TryGet(key, out var metadataValue)
          ? metadataValue
          : fallback;
   }

   /// <summary>
   /// Copies metadata from the object to a new metadata bag.
   /// </summary>
   /// <param name="value">The object carrying metadata.</param>
   /// <returns>A new metadata bag with copied values.</returns>
   public static MetadataBag CopyMetadata(this IHasMetadata value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Metadata.Copy();
   }
}