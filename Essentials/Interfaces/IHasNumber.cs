namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has an optional numeric identifier.
/// </summary>
public interface IHasNumber
{
   /// <summary>
   /// Gets the optional numeric identifier of the object.
   /// </summary>
   int? Number { get; }
}