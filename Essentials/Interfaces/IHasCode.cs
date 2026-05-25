namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has a stable code identifier.
/// </summary>
public interface IHasCode
{
   /// <summary>
   /// Gets the stable code identifier of the object.
   /// </summary>
   string Code { get; }
}