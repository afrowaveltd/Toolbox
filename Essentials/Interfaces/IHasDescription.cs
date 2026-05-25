namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has an optional human-readable description.
/// </summary>
public interface IHasDescription
{
   /// <summary>
   /// Gets the description of the object, if available.
   /// </summary>
   string? Description { get; }
}