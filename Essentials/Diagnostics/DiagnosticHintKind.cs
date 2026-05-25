namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Describes the kind of diagnostic hint.
/// </summary>
public enum DiagnosticHintKind
{
   /// <summary>
   /// A general note related to the diagnostic message.
   /// </summary>
   Note = 0,

   /// <summary>
   /// Help text explaining how to fix or understand the issue.
   /// </summary>
   Help = 1,

   /// <summary>
   /// A suggested change or action.
   /// </summary>
   Suggestion = 2,

   /// <summary>
   /// An example that demonstrates the expected form or usage.
   /// </summary>
   Example = 3
}