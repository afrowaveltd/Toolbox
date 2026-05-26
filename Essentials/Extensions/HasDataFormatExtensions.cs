using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying a data format.
/// </summary>
public static class HasDataFormatExtensions
{
   /// <summary>
   /// Determines whether the object's data format matches the specified format.
   /// </summary>
   /// <param name="value">The object carrying a data format.</param>
   /// <param name="dataFormat">The data format to compare with.</param>
   /// <returns><c>true</c> if the object has the specified data format; otherwise, <c>false</c>.</returns>
   public static bool HasDataFormat(
       this IHasDataFormat value,
       DataFormat dataFormat)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.DataFormat == dataFormat;
   }

   /// <summary>
   /// Determines whether the object's data format is text-based.
   /// </summary>
   /// <param name="value">The object carrying a data format.</param>
   /// <returns><c>true</c> if the data format is text-based; otherwise, <c>false</c>.</returns>
   public static bool HasTextBasedDataFormat(this IHasDataFormat value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.DataFormat.IsTextBased();
   }

   /// <summary>
   /// Determines whether the object's data format is binary.
   /// </summary>
   /// <param name="value">The object carrying a data format.</param>
   /// <returns><c>true</c> if the data format is binary; otherwise, <c>false</c>.</returns>
   public static bool HasBinaryDataFormat(this IHasDataFormat value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.DataFormat.IsBinary();
   }

   /// <summary>
   /// Determines whether the object's data format is structured text.
   /// </summary>
   /// <param name="value">The object carrying a data format.</param>
   /// <returns><c>true</c> if the data format is structured text; otherwise, <c>false</c>.</returns>
   public static bool HasStructuredTextDataFormat(this IHasDataFormat value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.DataFormat.IsStructuredText();
   }

   /// <summary>
   /// Determines whether the object's data format is unknown.
   /// </summary>
   /// <param name="value">The object carrying a data format.</param>
   /// <returns><c>true</c> if the data format is unknown; otherwise, <c>false</c>.</returns>
   public static bool HasUnknownDataFormat(this IHasDataFormat value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.DataFormat.IsUnknown();
   }

   /// <summary>
   /// Determines whether the object's data format is custom or application-specific.
   /// </summary>
   /// <param name="value">The object carrying a data format.</param>
   /// <returns><c>true</c> if the data format is custom; otherwise, <c>false</c>.</returns>
   public static bool HasCustomDataFormat(this IHasDataFormat value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.DataFormat.IsCustom();
   }
}