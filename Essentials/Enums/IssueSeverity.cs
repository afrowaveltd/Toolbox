namespace Afrowave.Toolbox.Essentials.Enums;

/// <summary>
/// Describes the severity of an issue, diagnostic message, warning, or error.
/// </summary>
public enum IssueSeverity
{
   /// <summary>
   /// No severity was specified.
   /// </summary>
   None = 0,

   /// <summary>
   /// Very detailed information intended mainly for tracing internal behavior.
   /// </summary>
   Trace = 1,

   /// <summary>
   /// Diagnostic information useful during development or troubleshooting.
   /// </summary>
   Debug = 2,

   /// <summary>
   /// Informational message that does not indicate a problem.
   /// </summary>
   Information = 3,

   /// <summary>
   /// A warning. The operation may continue, but something may need attention.
   /// </summary>
   Warning = 4,

   /// <summary>
   /// An error. The requested operation likely failed or produced an invalid result.
   /// </summary>
   Error = 5,

   /// <summary>
   /// A critical error that may affect system stability, data integrity, security, or availability.
   /// </summary>
   Critical = 6,

   /// <summary>
   /// A fatal error after which the current process, operation, or component cannot safely continue.
   /// </summary>
   Fatal = 7
}