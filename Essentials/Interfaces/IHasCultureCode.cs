using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has a culture code.
/// </summary>
public interface IHasCultureCode
{
   /// <summary>
   /// Gets the culture code associated with the object.
   /// </summary>
   CultureCode CultureCode { get; }
}