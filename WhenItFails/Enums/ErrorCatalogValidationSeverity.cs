namespace Afrowave.Toolbox.WhenItFails.Enums;

/// <summary>
/// Defines severity of an error catalog validation issue.
/// </summary>
public enum ErrorCatalogValidationSeverity
{
   /// <summary>
   /// Informational validation message.
   /// </summary>
   Information = 0,

   /// <summary>
   /// Warning that does not necessarily make the catalog unusable.
   /// </summary>
   Warning = 1,

   /// <summary>
   /// Error that makes the catalog invalid or unsafe to use.
   /// </summary>
   Error = 2
}