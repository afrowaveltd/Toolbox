namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has an identifier.
/// </summary>
/// <typeparam name="TId">The identifier type.</typeparam>
public interface IHasId<out TId>
{
   /// <summary>
   /// Gets the object identifier.
   /// </summary>
   TId Id { get; }
}