namespace Afrowave.Toolbox.Essentials.Enums;

/// <summary>
/// Describes the general format of data.
/// </summary>
public enum DataFormat
{
   /// <summary>
   /// The data format is unknown or was not specified.
   /// </summary>
   Unknown = 0,

   /// <summary>
   /// Generic text-based data without a more specific known format.
   /// </summary>
   GenericText = 1,

   /// <summary>
   /// Plain human-readable text.
   /// </summary>
   PlainText = 2,

   /// <summary>
   /// JSON data.
   /// </summary>
   Json = 3,

   /// <summary>
   /// AJIS data.
   /// </summary>
   Ajis = 4,

   /// <summary>
   /// XML data.
   /// </summary>
   Xml = 5,

   /// <summary>
   /// CSV data.
   /// </summary>
   Csv = 6,

   /// <summary>
   /// Markdown data.
   /// </summary>
   Markdown = 7,

   /// <summary>
   /// HTML data.
   /// </summary>
   Html = 8,

   /// <summary>
   /// YAML data.
   /// </summary>
   Yaml = 9,

   /// <summary>
   /// Generic binary data without a more specific known format.
   /// </summary>
   GenericBinary = 50,

   /// <summary>
   /// A custom or application-specific data format.
   /// </summary>
   Custom = 100
}