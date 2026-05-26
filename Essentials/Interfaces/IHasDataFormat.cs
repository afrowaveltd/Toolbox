using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has a data format.
/// </summary>
public interface IHasDataFormat
{
   /// <summary>
   /// Gets the data format associated with the object.
   /// </summary>
   DataFormat DataFormat { get; }
}