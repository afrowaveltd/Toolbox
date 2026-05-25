namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has a human-readable or logical name.
/// </summary>
public interface IHasName
{
   /// <summary>
   /// Gets the name of the object.
   /// </summary>
   string Name { get; }
}