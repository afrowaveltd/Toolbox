using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="DataFormat"/> values.
/// </summary>
public static class DataFormatExtensions
{
   /// <summary>
   /// Determines whether the data format is text-based.
   /// </summary>
   /// <param name="format">The data format.</param>
   /// <returns><c>true</c> if the format is text-based; otherwise, <c>false</c>.</returns>
   public static bool IsTextBased(this DataFormat format)
   {
      return format is DataFormat.GenericText
          or DataFormat.PlainText
          or DataFormat.Json
          or DataFormat.Ajis
          or DataFormat.Xml
          or DataFormat.Csv
          or DataFormat.Markdown
          or DataFormat.Html
          or DataFormat.Yaml;
   }

   /// <summary>
   /// Determines whether the data format is binary.
   /// </summary>
   /// <param name="format">The data format.</param>
   /// <returns><c>true</c> if the format is binary; otherwise, <c>false</c>.</returns>
   public static bool IsBinary(this DataFormat format)
   {
      return format == DataFormat.GenericBinary;
   }

   /// <summary>
   /// Determines whether the data format is structured text.
   /// </summary>
   /// <param name="format">The data format.</param>
   /// <returns><c>true</c> if the format is structured text; otherwise, <c>false</c>.</returns>
   public static bool IsStructuredText(this DataFormat format)
   {
      return format is DataFormat.Json
          or DataFormat.Ajis
          or DataFormat.Xml
          or DataFormat.Csv
          or DataFormat.Markdown
          or DataFormat.Html
          or DataFormat.Yaml;
   }

   /// <summary>
   /// Determines whether the data format is unknown.
   /// </summary>
   /// <param name="format">The data format.</param>
   /// <returns><c>true</c> if the format is <see cref="DataFormat.Unknown"/>; otherwise, <c>false</c>.</returns>
   public static bool IsUnknown(this DataFormat format)
   {
      return format == DataFormat.Unknown;
   }

   /// <summary>
   /// Determines whether the data format is custom or application-specific.
   /// </summary>
   /// <param name="format">The data format.</param>
   /// <returns><c>true</c> if the format is <see cref="DataFormat.Custom"/>; otherwise, <c>false</c>.</returns>
   public static bool IsCustom(this DataFormat format)
   {
      return format == DataFormat.Custom;
   }
}