namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has optional detailed information.
/// </summary>
public interface IHasDetails
{
   /// <summary>
   /// Gets optional detailed information.
   /// </summary>
   string? Details { get; }
}